﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tool_backup
{

    public class network
    {
        public datagridview dgvManager;

        public string ipGet {  get; set; }

        public network(DataGridView dataGridView)
        {
            dgvManager = new datagridview(dataGridView);
        }

        bool IsSameText(string text1, string text2)
        {
            return string.Equals(text1, text2, StringComparison.OrdinalIgnoreCase);
        }


        public void ScanNetworks(string ipRange ,string macaddr, string usr , string keypath , string passphare)
        {
            ScanIpRange(ipRange,macaddr,usr,keypath,passphare);
        }

        private async void ScanIpRange(string ipRange, string macaddr, string usr, string keypath, string passphare)
        {
            var parts = ipRange.Split('.');
            if (parts.Length != 4)
            {
                MessageBox.Show("Invalid IP range format!");
                return;
            }

            var baseIp = $"{parts[0]}.{parts[1]}.{parts[2]}.";

            var tasks = new List<Task>();
            var scannedIps = new HashSet<string>();

            dgvManager.Clear();
            SemaphoreSlim semaphore = new SemaphoreSlim(51);

            for (int i = 0; i < 255; i++)
            {
                string currentIp = baseIp + (i + 1);

                if (scannedIps.Contains(currentIp))
                {
                    continue;
                }

                scannedIps.Add(currentIp);

                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        if (await PingIpAsync(currentIp))
                        {
                            string macAddress =     "";
                            string macAddressEth0 = "";
                            string sudoPassword = "Lumivn274@aihubcamera";

                            //handle MAC
                            using (var client = new ssh(currentIp, usr, keypath, passphare))
                            {
                                if (client.Connect())
                                {
                                    try
                                    {
                                        if (usr == "root")
                                        {
                                            macAddressEth0 = client.ExecuteCommand("cat /sys/class/net/eth0/address");
                                            macAddress = GetMacAddress(currentIp);

                                            //handle Ip
                                            ipGet = IsSameText(macAddressEth0.Trim(), macaddr) ? currentIp : ipGet;
                                        }
                                        else
                                        {
                                            macAddressEth0 = client.ExecuteCommand($"echo \"{sudoPassword}\" | sudo -S cat /sys/class/net/eth0/address");
                                            macAddress = GetMacAddress(currentIp);

                                            //handle Ip
                                            ipGet = IsSameText(macAddressEth0.Trim(), macaddr) ? currentIp : ipGet;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error executing command on {currentIp}: {ex.Message}");
                                    }
                                }
                            }
                            dgvManager.AddRow(currentIp, macAddress, macAddressEth0);
                        }
                        else
                        {
                            dgvManager.AddRow(currentIp, null, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error scanning IP {currentIp}: {ex.Message}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }
            try
            {
                await Task.WhenAll(tasks);
                if (!string.IsNullOrEmpty(ipGet))
                {
                    MessageBox.Show($"Địa chỉ ip của mac : {macaddr} là {ipGet}\n Nhấn \"Connect\" để tạo kết nối");
                }
                else
                {
                    MessageBox.Show($"Không tìm thấy ip");
                }
                
            }
            catch (Exception)
            {
                MessageBox.Show("Error Scanner");
            }
        }

        public string get_ip_scan_succesfully()
        {
            return ipGet;
        }


        private async Task<bool> PingIpAsync(string ipAddress)
        {
            using (var ping = new Ping())
            {
                try
                {
                    IPAddress ipAddr = IPAddress.Parse(ipAddress);
                    var reply = await ping.SendPingAsync(ipAddr, 50);
                    return reply.Status == IPStatus.Success;
                }
                catch
                {
                    return false;
                }
            }
        }

        private string GetMacAddress(string ipAddress)
        {
            var macAddress = "N/A";
            var startInfo = new ProcessStartInfo("arp", $"-a {ipAddress}")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(startInfo))
            {
                using (var reader = process.StandardOutput)
                {
                    string output = reader.ReadToEnd();
                    var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var line in lines)
                    {
                        if (line.Contains(ipAddress))
                        {
                            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            macAddress = parts.Length > 1 ? parts[1] : macAddress;
                            break;
                        }
                    }
                }
            }
            return macAddress;
        }
    }
}
