﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using System.IO;
using tool_backup;
using System.Threading;
using Renci.SshNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Renci.SshNet.Sftp;
using System.Net.Mail;

namespace tool_backup
{
    public partial class Form1 : Form
    {
        public manager manager;
        public option optionManager;

        //SETUP
        public Form1()
        {
            InitializeComponent();

            manager = new manager(dataGridView1, treeView1, Device_Download);

            optionManager = new option();

            //type-device
            optionManager.AddItem("MT7688", "MT7688");
            optionManager.AddItem("AI-V2", "AI-V2");
            optionManager.AddItem("AI-V3", "AI-V3");
            comboBoxOptions.DataSource = optionManager.GetItems();
            comboBoxOptions.SelectedIndexChanged += comboBoxOptions_SelectedIndexChanged;
        }

        //FORM-LOAD
        private void Form1_Load(object sender, EventArgs e)
        {
            manager.form1_load(Log_app, "Team : OS", "Version : 1.0.0");
            manager.autoload_disconected(ConnectDevice_Status);
        }

        //CHECK-OPTION
        private void comboBoxOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = (ComboBoxItem)comboBoxOptions.SelectedItem;
            if (selectedItem != null)
            {
                string selectedValue = optionManager.GetSelectedValue(selectedItem);
                manager.LoadConfig("setting.json", selectedValue, ConnectDevice_Username, ConnectDevice_Passphare, Device_Download, Log_app);
            }
        }

        //CHECK-STATUS-IP
        private void ConnectDevice_CheckIp_CheckedChanged(object sender, EventArgs e)
        {
            manager.get_ip_status(ConnectDevice_CheckIp, ConnectDevice_Ip);
        }

        //CHECK-INPUT-IP
        private void ConnectDevice_Ip_TextChanged(object sender, EventArgs e)
        {
            manager.get_ip_input(ConnectDevice_CheckIp, ConnectDevice_Ip);
        }

        //OPEN-KEY-FILE
        private void ConnectDevice_CheckKeyfile_CheckedChanged(object sender, EventArgs e)
        {
            manager.get_key_file(ConnectDevice_CheckKeyfile, ConnectDevice_KeyFile,Log_app);
        }

        //DOWNLOAD
        private void SCP_Download_Click(object sender, EventArgs e)
        {
            manager.download(Local_Upload, Device_Upload, Local_Download, Log_app);
        }

        //UPLOAD
        private void SCP_Upload_Click(object sender, EventArgs e)
        {
            manager.upload(Local_Download, Device_Download, Local_Upload, Device_Upload, Log_app);
        }

        //SCAN IP & NETWORK
        private void Scan_btn_network_Click(object sender, EventArgs e)
        {
            manager.scan_ip_mac(Scan_IP_textbox, Scan_Search, ConnectDevice_KeyFile, progressBar1, Log_app, progressbar_value);
        }

        //SEARCH BY MAC
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            manager.search_by_mac(Scan_Search);
        }

        //CONNECT
        private void Btn_Connect_Click(object sender, EventArgs e)
        {
            manager.connect(ConnectDevice_CheckIp, ConnectDevice_Status, ConnectDevice_Ip, Log_app);
        }

        //DISCONECT
        private void Btn_Exit_Click(object sender, EventArgs e)
        {
            manager.disconnect(ConnectDevice_Status, Log_app);
        }

        //EXIT FORM
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            manager.exit(e,Log_app);
        }
    }
}
