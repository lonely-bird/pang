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
using System.Threading;
using System.Runtime.InteropServices;

namespace QWOP_AI_interface_3
{
    public partial class Form1 : Form
    {
        private double FPS = 60.0;
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
                    TLPbtn = new MyTableLayoutPanel(1, 5, "A", $"AAAPS{Database.scopeSize.Width}");
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
                        CHBpressKey.Checked = true;
                        TLPbtn.AddControl(CHBpressKey, 0, 2);
                    }
                    {
                        LBL = new MyLabel("");
                        TLPbtn.AddControl(LBL, 0, 3);
                    }
                    {
                        PBX = new PictureBox();
                        PBX.Dock = DockStyle.Fill;
                        PBX.SizeMode = PictureBoxSizeMode.AutoSize;
                        TLPbtn.AddControl(PBX, 0, 4);
                    }
                    TLPmain.AddControl(TLPbtn, 1, 0);
                }
                this.Controls.Add(TLPmain);
            }
            socketHandler = new SocketHandler();
            socketHandler.logAppended += SocketHandler_logAppended;
            socketHandler.msgReceived += SocketHandler_msgReceived;
            {
                Thread thread = new Thread(() =>
                {
                    int pre_count = 0;
                    while (true)
                    {
                        Thread.Sleep(5000);
                        if (socketHandler.dataConnectionCounter != pre_count) SocketHandler_logAppended((pre_count = socketHandler.dataConnectionCounter).ToString() + " communications");
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
                          Thread.Sleep(500);
                          PBX.Invoke(new Action(() =>
                          {
                              var bmp = getFeedBackImage();
                              if (bmp == null) LBL.Text = "Unavailable";
                              else LBL.Text = (IsLive(bmp) ? "Alive" : "Dead");
                              var preImg = PBX.Image;
                              PBX.Image = bmp;
                              if (preImg != null) preImg.Dispose();
                          }));
                      }
                  });
                thread.IsBackground = true;
                thread.Start();
            }
<<<<<<< HEAD
            {
                Thread thread = new Thread(() =>
                  {
                  });
                thread.IsBackground = true;
                thread.Start();
            }
=======
            //{
            //    Thread thread = new Thread(() =>
            //      {
            //      });
            //    thread.IsBackground = true;
            //    thread.Start();
            //}
>>>>>>> c657fb1dae5a9228af561a12197509ae05216335
            //{
            //    Thread thread = new Thread(() =>
            //      {
            //          while (true)
            //          {
            //              if (pressQ) SendKeys.SendWait("Q");
            //              if (pressW) SendKeys.SendWait("W");
            //              if (pressO) SendKeys.SendWait("O");
            //              if (pressP) SendKeys.SendWait("P");
            //          }
            //      });
            //    thread.IsBackground = true;
            //    thread.Start();
            //}
        }
        private bool IsLive(Bitmap bmp)
        {
            int sum = 0;
            {
                BitmapData bd = bmp.LockBits(new Rectangle(new Point(0, 0), bmp.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* p = (byte*)bd.Scan0.ToPointer();
                    for (int i = 0; i < bd.Height*2/3; i++)
                    {
                        for (int j = bd.Width/4; j < bd.Width*3/4; j++)
                        {
                            byte* q = p + (j * 4);
                            sum += q[0];
                            sum += q[1];
                            sum += q[2];
                        }
                        p += bd.Stride;
                    }
                }
                bmp.UnlockBits(bd);
            }
            Do(() => { this.Text = sum.ToString(); });
            return sum < (600000 + 1100000) / 2;
        }
        private bool IsLive(int sum) { return sum < (1160000+1140000)/*(2685000 + 2090000)*/ / 2; }
        private void CHBpressKey_CheckedChanged(object sender, EventArgs e)
        {
            pressKey = (sender as MyCheckBox).Checked;
        }
        private Bitmap getFeedBackImage()
        {
            if (Database.scopeLocation == Database.failedPoint) return null;
            Bitmap bmp = new Bitmap(Database.feedBackSize.Width, Database.feedBackSize.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(Database.scopeLocation, new Point(0, 0), Database.feedBackSize);
                IntPtr dc1 = g.GetHdc();
                g.ReleaseHdc(dc1);
            }
            return bmp;
        }
        private string getFeedBack()
        {
            StringBuilder answer = new StringBuilder();
            bool isLive;
            {
                Bitmap bmp = getFeedBackImage();
                isLive = IsLive(bmp);
                BitmapData bd = bmp.LockBits(new Rectangle(new Point(0, 0), bmp.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* p = (byte*)bd.Scan0.ToPointer();
                    for (int i = 0; i < bd.Height; i++)
                    {
                        for (int j = 0; j < bd.Width; j++)
                        {
                            byte* q = p + (j * 4);
                            answer.Append(q[2]);
                            answer.Append(' ');
                            answer.Append(q[1]);
                            answer.Append(' ');
                            answer.Append(q[0]);
                            answer.Append(' ');
                        }
                        p += bd.Stride;
                    }
                }
                bmp.UnlockBits(bd);
                bmp.Dispose();
            }
            answer.Append(isLive ? 1 : 0);
            return answer.ToString();
        }
        private bool pressQ
        {
            set
            {
                keybd_event((byte)Keys.Q, 0, value ? 0 : 2, 0);
            }
        }
        private bool pressW
        {
            set
            {
                keybd_event((byte)Keys.W, 0, value ? 0 : 2, 0);
            }
        }
        private bool pressO
        {
            set
            {
                keybd_event((byte)Keys.O, 0, value ? 0 : 2, 0);
            }
        }
        private bool pressP
<<<<<<< HEAD
        {
            set
            {
                keybd_event((byte)Keys.P, 0, value ? 0 : 2, 0);
            }
        }
        private DateTime lastTimePressKey = DateTime.Now;
        private void SocketHandler_msgReceived(string msgStr, System.IO.StreamWriter writer)
        {
            if (!pressKey) return;
            //Log($"Receive: {msgStr}");
            Debug.Assert(msgStr.Length == 2);
            if (msgStr == "RR")
            {
                Log("Restarting the game...");
                SendKeys.SendWait("R");
                for (bool isLive = false; !isLive;)
                {
                    Bitmap bmp = getFeedBackImage();
                    isLive = IsLive(bmp);
                    bmp.Dispose();
                    Thread.Sleep(50);
                }
                Log("Game restarted");
                lastTimePressKey = DateTime.Now;
            }
            else
            {
                if (msgStr[0] != 'Q' && msgStr[0] != 'W') Log($"msgStr[0] must be Q or W! msgStr[0]: {msgStr[0]}");
                else if (msgStr[1] != 'O' && msgStr[1] != 'P') Log($"msgStr[1] must be O or P! msgStr[1]: {msgStr[1]}");
                for (int i = 0; i < msgStr.Length; i++)
                {
                    char msg = msgStr[i];
                    switch (msg)
                    {
                        case 'Q': pressQ = true; pressW = false; break;
                        case 'W': pressQ = false; pressW = true; break;
                        case 'O': pressO = true; pressP = false; break;
                        case 'P': pressO = false; pressP = true; break;
                        default: Log($"Invalid key: {msg}"); break;
                    }
                }
            }
=======
        {
            set
            {
                keybd_event((byte)Keys.P, 0, value ? 0 : 2, 0);
            }
        }
        private DateTime lastTimePressKey = DateTime.Now;
        private void SocketHandler_msgReceived(string msgStr, System.IO.StreamWriter writer)
        {
            if (!pressKey) return;
            //Log($"Receive: {msgStr}");
            Debug.Assert(msgStr.Length == 2);
            if (msgStr == "RR")
            {
                Log("Restarting the game...");
                SendKeys.SendWait("R");
                for (bool isLive = false; !isLive;)
                {
                    Bitmap bmp = getFeedBackImage();
                    isLive = IsLive(bmp);
                    bmp.Dispose();
                    Thread.Sleep(50);
                }
                Log("Game restarted");
                lastTimePressKey = DateTime.Now;
            }
            else
            {
                if (msgStr[0] != 'Q' && msgStr[0] != 'W') Log($"msgStr[0] must be Q or W! msgStr[0]: {msgStr[0]}");
                else if (msgStr[1] != 'O' && msgStr[1] != 'P') Log($"msgStr[1] must be O or P! msgStr[1]: {msgStr[1]}");
                for (int i = 0; i < msgStr.Length; i++)
                {
                    char msg = msgStr[i];
                    switch (msg)
                    {
                        case 'Q': pressQ = true; pressW = false; break;
                        case 'W': pressQ = false; pressW = true; break;
                        case 'O': pressO = true; pressP = false; break;
                        case 'P': pressO = false; pressP = true; break;
                        default: Log($"Invalid key: {msg}"); break;
                    }
                }
            }
>>>>>>> c657fb1dae5a9228af561a12197509ae05216335
            lastTimePressKey = lastTimePressKey.AddSeconds(1.0 / FPS);
            Thread.Sleep((int)Math.Max(0.0, (lastTimePressKey - DateTime.Now).TotalMilliseconds));
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
                                Database.scopeLocation = new Point(x, y+33);
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
        private MyTableLayoutPanel TLPmain,TLPbtn;
        private MyButton BTNstart,BTNscan;
        private MyCheckBox CHBpressKey;
        private MyTextBox TXBoutput;
        private PictureBox PBX;
        private MyLabel LBL;
        SocketHandler socketHandler;

        //键盘Hook结构函数
        [StructLayout(LayoutKind.Sequential)]
        public class KeyBoardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        #region DllImport
        //设置钩子
        //[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //抽掉钩子
        public static extern bool UnhookWindowsHookEx(int idHook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //调用下一个钩子
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);
        //取得模块句柄 
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        //寻找目标进程窗口
        [DllImport("USER32.DLL")]
        public static extern IntPtr FindWindow(string lpClassName,
            string lpWindowName);
        //设置进程窗口到最前 
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        //模拟键盘事件 
        [DllImport("User32.dll")]
        public static extern void keybd_event(Byte bVk, Byte bScan, Int32 dwFlags, Int32 dwExtraInfo);
        #endregion
        //keybd_event(VK_Q, 0, 0, 0);//按下小键盘7
        //keybd_event(VK_Q, 0, KEYEVENTF_KEYUP, 0); //松开小键盘7
    }
}