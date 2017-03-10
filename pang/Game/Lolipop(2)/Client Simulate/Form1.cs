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
        private const int port = 6000, aiPort=7000, socketCount = 1;
        public static double FPS = 200.0;
        private List<MessageSender> socketHandlers = new List<MessageSender>();
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
            this.Size = new Size(400, 800);
            this.Location = new Point(0, 0);
            InitializeControls();
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
            {
                Thread thread = new Thread(() =>
                  {
                      while (true)
                      {
                          PBX.Invoke(new Action(() => { PBX.Image = (AIaction == '0' ? Properties.Resources.buttonLight : Properties.Resources.buttonDark); }));
                          TB.Invoke(new Action(() =>
                          {
                              int target = ((int)Math.Round(AIpressButtonPossibility * 1000.0) - TB.Value);
                              if (target > 0) TB.Value += (int)Math.Ceiling(target * 0.2);
                              if (target < 0) TB.Value += (int)Math.Floor(target * 0.2);
                              Application.DoEvents();
                          }));
                          Thread.Sleep(20);
                      };
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
            for (int i = 1; i <= socketCount; i++)
            {
                socketHandlers.Add(new MessageSender(port + i));
            }
            socketHandlers.Add(new MessageSender(port));
            MessageReceiver receiver = new MessageReceiver(aiPort);
            receiver.msgReceived += Receiver_msgReceived;
            receiver.Start();
        }
        
        private void InitializeControls()
        {
            this.FormClosing += Form1_FormClosing;
            {
                TLP = new MyTableLayoutPanel(5, 1, "PAAAA", "P");
                {
                    {
                        LBL = new MyLabel("");
                        status = "Q, W, O: control\r\nP: restart all";
                        TLP.AddControl(LBL, 0, 0);
                    }
                    {
                        MyTableLayoutPanel tlp = new MyTableLayoutPanel(1, 2, "A", "AA");
                        {
                            PBX = new PictureBox();
                            PBX.Dock = DockStyle.Fill;
                            PBX.SizeMode = PictureBoxSizeMode.AutoSize;
                            PBX.Image = Properties.Resources.buttonDark;
                            tlp.AddControl(PBX, 0, 0);
                        }
                        {
                            Panel pnl = new Panel();
                            pnl.Dock = DockStyle.Fill;
                            pnl.AutoSize = true;
                            pnl.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                            PictureBox pbx = new PictureBox();
                            pbx.Dock = DockStyle.Fill;
                            pbx.SizeMode = PictureBoxSizeMode.Zoom;
                            pbx.Image = Properties.Resources.computer;
                            MyLabel lbl = new MyLabel("");
                            lbl.Font = new Font("Consolas", 8, FontStyle.Bold);
                            lbl.Dock = DockStyle.None;
                            lbl.ForeColor = Color.FromArgb(64, 0, 0, 0);
                            lbl.BackColor = Color.Transparent;
                            lbl.Parent = pbx;
                            //{
                            //    Bitmap bmp = new Bitmap(1, 1);
                            //    bmp.SetPixel(0, 0, Color.Transparent);
                            //    lbl.BackgroundImage = bmp;
                            //}
                            //pnl.Controls.Add(lbl);
                            pnl.Controls.Add(pbx);
                            tlp.AddControl(pnl, 0, 1);
                            Thread thread = new Thread(() =>
                              {
                                  Thread.Sleep(3000);
                                  while (true)
                                  {
                                      Thread.Sleep(20);
                                      StringBuilder s = new StringBuilder();
                                      for (int i = 0; i < 10; i++)
                                      {
                                          for (int j = 0; j < 30; j++)
                                          {
                                              s.Append(random.Next(0, 2) == 0 ? '0' : '1');
                                          }
                                          s.Append("\r\n");
                                      }
                                      lbl.Invoke(new Action(() => { lbl.Text = s.ToString(); }));
                                  }
                              });
                            thread.IsBackground = true;
                            thread.Start();
                        }
                        TLP.AddControl(tlp, 1, 0);
                    }
                    {
                        TB = new TrackBar();
                        TB.Dock = DockStyle.Fill;
                        TB.Minimum = 0;
                        TB.Maximum = 1000;
                        TLP.AddControl(TB, 2, 0);
                    }
                    {
                        TXB = new MyTextBox(false);
                        TXB.KeyDown += Form1_KeyDown;
                        TXB.KeyUp += Form1_KeyUp;
                        TXB.TextChanged += (object s, EventArgs e1) => { TXB.Text = null; };
                        TLP.AddControl(TXB, 3, 0);
                    }
                    {
                        IFD = new MyInputField();
                        IFD.AddField("Play speed (FPS)", FPS.ToString()).TextChanged += (object s, EventArgs e1) => {
                            double t;
                            if (!double.TryParse((s as TextBox).Text, out t)) MessageBox.Show("格式不正確");
                            else FPS = t;
                        };
                        TLP.AddControl(IFD, 4, 0);
                    }
                }
                this.Controls.Add(TLP);
            }
        }
        private void ExtractInfo(string msg,out char action,out double possibility)
        {
            action = msg[0];
            //status = $"msg: {msg}";
            if (action != '0' && action != '1')
            {
                action = (char)('0' + random.Next(0, 2));
            }
            Debug.Assert(action == '0' || action == '1');
            possibility = 1.0-double.Parse(msg.Substring(1));
        }
        private static Random random = new Random();
        private double AIreactPeriod = 0.0;
        private Queue<DateTime> AIlastReactTime=new Queue<DateTime>();
        private char AIaction='0';
        private double AIpressButtonPossibility=0.5;
        private void Receiver_msgReceived(string msg, StreamWriter writer)
        {
            ExtractInfo(msg, out AIaction, out AIpressButtonPossibility);
            SetKeyState(socketCount, 1-(AIaction - '0'));
            string feedBack;
            while ((feedBack = socketHandlers[socketCount].answer/*Invoking to avoid incompleted string?*/) == null) status = "Trying to get AI's feedback...";
            //status = "feed back received";
            writer.WriteLine(feedBack);
            writer.Flush();
            //status = "feed back returned to AI";
            AIlastReactTime.Enqueue(DateTime.Now);
            while ((DateTime.Now- AIlastReactTime.First()).TotalSeconds > 0.5) AIlastReactTime.Dequeue();
            AIreactPeriod = (AIlastReactTime.Last()-AIlastReactTime.First()).TotalSeconds/(AIlastReactTime.Count-1);
            status = $"AI speed: {(1.0 / AIreactPeriod).ToString("F1")} Hz";//+msg;
        }
        bool IsSerialFormatCorrect(string s)
        {
            if (s.Length < 17) return false;
            for(int i=0;i<4;i++)
            {
                if (s[i * 3 + 0] != '3') return false;
                if (s[i * 3 + 1] != '0'&& s[i * 3 + 1] != '1') return false;
                if (s[i * 3 + 2] != '-') return false;
            }
            if (s.Substring(4 * 3, 5) != "0D-0A") return false;
            return true;
        }
        private void Comport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Byte[] buffer = new Byte[1024];
            Int32 length = (sender as SerialPort).Read(buffer, 0, buffer.Length);
            Array.Resize(ref buffer, length);
            string s = BitConverter.ToString(buffer);
            if (!IsSerialFormatCorrect(s)) return;
            LBL.Invoke(new Action(() =>
            {
                //LBL.Text = BitConverter.ToString(buffer);
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
            for (int i = 0; i <= socketCount; i++)
            {
                //socketHandlers[i].keyState = keyStates[i];
                socketHandlers[i].enabled = true;
            }
        }
        private void ResetAll()
        {
            for (int i = 0; i <= socketCount; i++) socketHandlers[i].keyState = -1;
            Thread.Sleep(50);
            for (int i = 0; i <= socketCount; i++)
            {
                socketHandlers[i].SendMessage("R");
            }
            socketHandlers[socketCount].keyState = 0;
            AIlastReactTime.Clear();
            AIlastReactTime.Enqueue(DateTime.Now);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        private SerialPort comport;
        private MyLabel LBL;
        private MyInputField IFD;
        private MyTextBox TXB;
        private MyTableLayoutPanel TLP;
        private PictureBox PBX;
        private TrackBar TB;
        private string status = "Not ready";
    }
}
