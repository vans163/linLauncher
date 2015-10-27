using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;

namespace loginForm
{
    public partial class Form1 : Form
    {
        ushort curPort = 16600;
        public Form1()
        {
            InitializeComponent();
            set_curport_label(curPort);
            listBox1.DisplayMember = "Name";
            populateServers();
            textBox1.Text = "C:\\Lineage Tikal";
        }

        void populateServers()
        {
            var zelgo = new LineageServer("Resurrection", "198.58.107.60", 46838) { Description = " 12x XP until 52 then 6x\n Adena 8x\n Drops 3x\n Karma 16x" };
            var zelgotest = new LineageServer("Resurrection Test", "24.99.243.145", 46838) { Description = "Extremely high rate server with character copy from Resurrection. Meant for testing." };
            listBox1.Items.Add(zelgo);
            listBox1.Items.Add(zelgotest);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int tries = 0;
            LineageServer selectedServer = (LineageServer)listBox1.SelectedItem;
            if (selectedServer == null)
                return;

            while (!Server.Start(curPort, selectedServer.IP, selectedServer.Port))
            {
                curPort++;

                tries++;
                if (tries > 50)
                {
                    MessageBox.Show("Tried to bind on over 50 ports, check your firewall");
                    System.Environment.Exit(1);
                }
            }

            ProcLauncher.CreateLinProcess(textBox1.Text, curPort, textBox2.Text, textBox3.Text);

            curPort++;
            set_curport_label(curPort);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LineageServer selectedServer = (LineageServer)listBox1.SelectedItem;
            richTextBox1.Text = selectedServer.Description;
            set_serverstatus_label(selectedServer.serverStatus);
        }

        void set_curport_label(ushort port)
        {
            label1.Text = string.Format("Current Port: {0}", port);
        }

        void set_serverstatus_label(LineageServer.ServerStatus status)
        {
            label2.Text = string.Format("Server is {0}", status);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(0);
        }
    }

    public class LineageServer
    {
        public string Name { get; set; }
        public string Description;
        public string IP;
        public ushort Port;
        public ServerStatus serverStatus = ServerStatus.Unknown;

        public LineageServer(string name, string ip, ushort port)
        {
            this.Name = name;
            this.IP = ip;
            this.Port = port;

            if (TestOnline(ip, port))
            {
                serverStatus = ServerStatus.Online;
            }
            else
            {
                serverStatus = ServerStatus.Offline;
            }
        }

        public bool TestOnline(string ip, ushort port)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    socket.Connect(ip, port);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }

        public enum ServerStatus
        {
            Online = 0,
            Offline = 1,
            Unknown = 99
        }
    }
}
