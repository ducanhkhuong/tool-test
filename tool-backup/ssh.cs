﻿using Renci.SshNet;
using Renci.SshNet.Common;
using Serilog;
using System.IO;
using System;

namespace tool_backup
{
    public class ssh : IDisposable
    {
        private SshClient client;

        // not key
        private string username_notkey;
        private string ip_notkey;
        private string password_notkey;

        // key
        private string username;
        private string ip;
        private string keyFilePath;
        private string passphrase;

        //giao diện giải phóng tài nguyên
        private bool _disposed = false;

        // kết nối khi có key
        public ssh(string ip, string username, string keyFilePath, string passphrase)
        {
            this.ip = ip;
            this.username = username;
            this.keyFilePath = keyFilePath;
            this.passphrase = passphrase;
            PrivateKeyFile keyFile = new PrivateKeyFile(keyFilePath, passphrase);
            PrivateKeyFile[] keyFiles = new[] { keyFile };
            AuthenticationMethod[] authMethods = new AuthenticationMethod[]
            {
                new PrivateKeyAuthenticationMethod(username, keyFiles)
            };
            ConnectionInfo connectionInfo = new ConnectionInfo(ip, username, authMethods);
            this.client = new SshClient(connectionInfo);
        }


        // kết nối khi không có key
        public ssh(string ip_notkey, string username_notkey, string password_notkey)
        {
            this.ip_notkey = ip_notkey;
            this.username_notkey = username_notkey;
            this.password_notkey = password_notkey;
            AuthenticationMethod[] authMethods = new AuthenticationMethod[]
            {
                new PasswordAuthenticationMethod(username_notkey, password_notkey)
            };
            ConnectionInfo connectionInfo = new ConnectionInfo(ip_notkey, username_notkey, authMethods);
            this.client = new SshClient(connectionInfo);
        }

        //kết nối
        public bool Connect()
        {
            try
            {
                client.Connect();
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        //ngắt kết nối
        public void Disconnect()
        {
            if (client != null && client.IsConnected)
            {
                client.Disconnect();
            }
        }

        //thực thi ExcuteCommand
        public string ExecuteCommand(string commandText)
        {
            if (client == null)
            {
                return "ExecuteCommand failed: client is null";
            }

            if (!client.IsConnected)
            {
                return "ExecuteCommand failed: client is not connected";
            }

            try
            {
                var command = client.CreateCommand(commandText);
                command.Execute();
                return command.Result;
            }
            catch (Exception ex)
            {
                return $"ExecuteCommand failed: {ex.Message}";
            }
        }


        //cmd output
        public void ExecuteCommand(string commandText, Action<string> logAction)
        {
            if (client != null && client.IsConnected)
            {
                try
                {
                    var command = client.RunCommand(commandText);
                    if (!string.IsNullOrEmpty(command.Error))
                    {
                        logAction($"Error:\n{command.Error}");
                    }
                }
                catch (Exception ex)
                {
                    logAction($"Error executing command: {ex.Message}");
                }
            }
            else
            {
                logAction("SSH not connected.");
            }
        }


        //check connect
        public bool IsConnected => client != null && client.IsConnected;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ;
                }
                _disposed = true;
            }
        }


        ~ssh()
        {
            Dispose(false);
        }
    }
}
