using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Motivation;
using System.Diagnostics;
using System.IO.Ports;

namespace Client_Simulate
{
    public partial class Form1 : Form
    {
        private const int port = 6002, socketCount = 3;
        public static double FPS = 50.0;
        private List<SocketHandler> socketHandlers = new List<SocketHandler>();
        //private void Txb_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Enter)
        //    {
        //        this.Text = "Sending messages...";
        //        SendMessage(TXB.Text);
        //        this.Text = "Message sent!";
        //        TXB.Clear();
        //    }
        //}
        private void Do(Action a)
        {
            if (this.InvokeRequired) this.Invoke(a);
            else a.Invoke();
        }
        public Form1()
        {
            this.Shown += Form1_Shown;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Size = new Size(500, 500);
            this.FormClosing += Form1_FormClosing;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            {
                LBL = new MyLabel("");
                status = "Q, W, O: control\r\nP: restart all";
                this.Controls.Add(LBL);
            }
            {
                Thread thread = new Thread(() =>
                {
                    while (true)
                    {
                        StringBuilder s = new StringBuilder();
                        foreach (var sh in socketHandlers) s.AppendLine(sh.status);
                        s.AppendLine(status);
                        if (s.ToString() != LBL.Text) LBL.Invoke(new Action(() => { LBL.Text = s.ToString(); }));
                        Thread.Sleep(10);
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
            try
            {
                comport = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);
                comport.DataReceived += Comport_DataReceived;
                if (!comport.IsOpen)
                {
                    comport.Open();
                }
            }
            catch (Exception error)
            {
                status += $"\r\nArduino doesn't connected, restart the program to try again\r\nError:\r\n{error}";
            }
            for (int i = 0; i < socketCount; i++)
            {
                socketHandlers.Add(new SocketHandler(port + i));
            }
        }

        private void Comport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Byte[] buffer = new Byte[1024];
            Int32 length = (sender as SerialPort).Read(buffer, 0, buffer.Length);
            Array.Resize(ref buffer, length);
            LBL.Invoke(new Action(() =>
            {
                //LBL.Text = BitConverter.ToString(buffer);
                string s = BitConverter.ToString(buffer);
                this.Text = s;
                for (int i = 0, j = 0; i < s.Length; i++)
                {
                    if (s[i] == '0' || s[i] == '1')
                    {
                        if (j == 3 && s[i] == '1')
                        {
                            ResetAll();
                        }
                        else if (j < socketCount)
                        {
                            if (s[i] == '1') SetKeyState(j, 1);
                            else if (s[i] == '0') SetKeyState(j, 0);
                        }
                        j++;
                    }
                }
            }));
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            this.Text = $"KeyUp{e.KeyCode}";
            if (e.KeyCode == Keys.P)
            {
                ResetAll();
            }
            else if (e.KeyCode == Keys.Q)
            {
                SetKeyState(0, 0);
            }
            else if (e.KeyCode == Keys.W)
            {
                SetKeyState(1, 0);
            }
            else if (e.KeyCode == Keys.O)
            {
                SetKeyState(2, 0);
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            this.Text = $"KeyDown{e.KeyCode}";
            if (e.KeyCode == Keys.Q)
            {
                SetKeyState(0, 1);
            }
            else if (e.KeyCode == Keys.W)
            {
                SetKeyState(1, 1);
            }
            else if (e.KeyCode == Keys.O)
            {
                SetKeyState(2, 1);
            }
        }
        private void SetKeyState(int a, int v)
        {
            socketHandlers[a].keyState = v;
            for (int i = 0; i < socketCount; i++)
            {
                if (socketHandlers[i].keyState == -1) return;
            }
            for (int i = 0; i < socketCount; i++)
            {
                //socketHandlers[i].keyState = keyStates[i];
                socketHandlers[i].enabled = true;
            }
        }
        private void ResetAll()
        {
            for (int i = 0; i < socketCount; i++)
            {
                socketHandlers[i].SendMessage("R");
                socketHandlers[i].keyState = -1;
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        private SerialPort comport;
        private MyLabel LBL;
        private string status = "Not ready";
    }
}
