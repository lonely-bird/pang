using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Motivation;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace QWOP_AI_interface_3
{
    public partial class Form1 : Form
    {
        private void scanPointsOverAndOver()
        {
            socketHandler.Start();
        }
        private void Log(string text)
        {
            Do(() => { TXBoutput.AppendText(text+Environment.NewLine); });
        }
        private Task scanPointsOverAndOverAsync()
        {
            return Task.Factory.StartNew(() => { scanPointsOverAndOver(); });
        }
        private async void start()
        {
            try
            {
                scanScopeLocation();
                await scanPointsOverAndOverAsync();
                Do(() => { BTNstart.Text = "Running..."; });
            }
            catch (Exception error)
            {
                Log("Error:");
                Log(error.ToString());
                Do(() => { BTNstart.Enabled = true; });
            }
        }
        private void BTNstart_Click(object sender, EventArgs e)
        {
            BTNstart.Enabled = false;
            start();
        }
        private void Do(Action a)
        {
            if (this.InvokeRequired) this.Invoke(a);
            else a.Invoke();
        }
        public Form1()
        {
            this.Size = new Size(700, 500);
            this.FormClosing += Form1_FormClosing;
            {
                TLPmain = new MyTableLayoutPanel(2, 1, "PA", "P");
                {
                    TXBoutput = new MyTextBox(true);
                    TLPmain.AddControl(TXBoutput, 0, 0);
                }
                {
                    TLPbtn = new MyTableLayoutPanel(1, 3, "P", "PPP");
                    {
                        BTNstart = new MyButton("Start");
                        BTNstart.Click += BTNstart_Click;
                        TLPbtn.AddControl(BTNstart, 0, 0);
                    }
                    {
                        BTNscan = new MyButton("Scan");
                        BTNscan.Click += BTNscan_Click;
                        TLPbtn.AddControl(BTNscan, 0, 1);
                    }
                    {
                        CHBpressKey = new MyCheckBox("Press Key");
                        CHBpressKey.CheckedChanged += CHBpressKey_CheckedChanged;
                        TLPbtn.AddControl(CHBpressKey, 0, 2);
                    }
                    TLPmain.AddControl(TLPbtn, 1, 0);
                }
                this.Controls.Add(TLPmain);
            }
            socketHandler = new SocketHandler();
            socketHandler.logAppended += SocketHandler_logAppended;
            socketHandler.msgReceived += SocketHandler_msgReceived;
            System.Threading.Thread thread = new System.Threading.Thread(() =>
            {
                int pre_count = 0;
                while (true)
                {
                    System.Threading.Thread.Sleep(5000);
                    if (socketHandler.dataConnectionCounter != pre_count) SocketHandler_logAppended((pre_count = socketHandler.dataConnectionCounter).ToString() + " communications");
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void CHBpressKey_CheckedChanged(object sender, EventArgs e)
        {
            pressKey = (sender as MyCheckBox).Checked;
        }

        private string getFeedBack()
        {
            StringBuilder answer = new StringBuilder();
            int sum = 0;
            using (Bitmap bmp = new Bitmap(Database.scopeSize.Width, Database.scopeSize.Height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(Database.scopeLocation, new Point(0, 0), Database.scopeSize);
                    IntPtr dc1 = g.GetHdc();
                    g.ReleaseHdc(dc1);
                }
                BitmapData bd = bmp.LockBits(new Rectangle(new Point(0, 0), bmp.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                unsafe
                {
                    answer.Append(bd.Width);
                    answer.Append(' ');
                    answer.Append(bd.Height);
                    byte* p = (byte*)bd.Scan0.ToPointer();
                    for (int i = 0; i < bd.Height; i++)
                    {
                        for (int j = 0; j < bd.Width; j++)
                        {
                            byte* q = p + (j * 4);
                            answer.Append(' ');
                            answer.Append(q[2]);
                            answer.Append(' ');
                            answer.Append(q[1]);
                            answer.Append(' ');
                            answer.Append(q[0]);
                            sum += q[2];
                            sum += q[1];
                            sum += q[0];
                        }
                        p += bd.Stride;
                    }
                }
                bmp.UnlockBits(bd);
            }
            Do(() => { this.Text = sum.ToString(); });
            bool isLive = (sum < (2685000 + 2090000) / 2);
            answer.Append(' ');
            answer.Append(isLive ? 1 : 0);
            return answer.ToString();
        }
        private void SocketHandler_msgReceived(char msg, System.IO.StreamWriter writer)
        {
            switch (msg)
            {
                case 'R':
                case 'Q':
                case 'W':
                case 'O':
                case 'P': break;
                default: throw new Exception($"key={msg}");
            }
            if(pressKey)SendKeys.SendWait(msg.ToString());
            writer.WriteLine(getFeedBack());
            writer.Flush();
        }

        private void SocketHandler_logAppended(string log)
        {
            Log(log);
        }

        private void BTNscan_Click(object sender, EventArgs e)
        {
            try
            {
                scanScopeLocation();
            }
            catch(Exception error)
            {
                Log("Error:");
                Log(error.ToString());
            }
        }

        private void scanScopeLocation()
        {
            Log("Scanning scope location...");
            Database.scopeLocation = Database.failedPoint;
            bool answer = false;
            using (Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
                    IntPtr dc1 = g.GetHdc();
                    g.ReleaseHdc(dc1);
                }
                BitmapData bd = bmp.LockBits(new Rectangle(new Point(0, 0), bmp.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* p = (byte*)bd.Scan0.ToPointer();
                    var isColor = new Func<int, int, int, int, int, bool>((int y, int x, int r, int g, int b) =>
                    {
                        byte* q = p + (bd.Stride * y + x * 4);
                        return q[2] == r && q[1] == g && q[0] == b;
                    });
                    //var isBlack = new Func<int, int, bool>((int y, int x) => { return isColor(y, x, 0, 0, 0); });
                    var isWhite = new Func<int, int, bool>((int y, int x) => { return isColor(y, x, 255, 255, 255); });
                    var isValid = new Func<int, int, bool>((int y, int x) =>
                    {
                        for (int i = 0; i < Database.scopeSize.Height; i++)
                        {
                            if (!(isWhite(y + i, x - 1) && !isWhite(y + i, x))) return false;
                            if (!(isWhite(y + i, x + Database.scopeSize.Width) && !isWhite(y + i, x + Database.scopeSize.Width - 1))) return false;
                        }
                        for (int j = 0; j < Database.scopeSize.Width; j++)
                        {
                            if (!(isWhite(y - 1, x + j) && !isWhite(y, x + j))) return false;
                            if (!(isWhite(y + Database.scopeSize.Height, x + j) && !isWhite(y + Database.scopeSize.Height - 1, x + j))) return false;
                        }
                        return true;
                    });
                    for (int y = 1; y + Database.scopeSize.Height < Screen.PrimaryScreen.Bounds.Height && !answer; y++)
                    {
                        for (int x = 1; x + Database.scopeSize.Width < Screen.PrimaryScreen.Bounds.Width && !answer; x++)
                        {
                            if (isValid(y, x))
                            {
                                Database.scopeLocation = new Point(x, y);
                                answer = true;
                            }
                        }
                    }
                }
                bmp.UnlockBits(bd);
            }
            if (answer) Log($"Found scopeLocation={Database.scopeLocation}");
            else throw new Exception($"Can't find any rectangle of size {Database.scopeSize}");
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }
        bool pressKey = false;
        private MyTableLayoutPanel TLPmain;
        private MyTableLayoutPanel TLPbtn;
        private MyButton BTNstart;
        private MyButton BTNscan;
        private MyCheckBox CHBpressKey;
        private MyTextBox TXBoutput;
        SocketHandler socketHandler;
    }
}