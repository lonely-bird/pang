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
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Drawing.Imaging;

namespace Lolipop_AI_interface
{
    public partial class Form1 : Form
    {
        private const int port = 8000;
        private const int panelCount = 4;
        private const double fps = 30;
        private static Random rand = new Random();
        public static int public_random { get { return rand.Next(int.MaxValue); } }
        private MyTableLayoutPanel TLP;
        public Form1()
        {
            this.Shown += Form1_Shown;
            //this.WindowState = FormWindowState.Maximized;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Size = new Size(800, 800);
            this.Location = new Point(400, 0);
            //this.TopMost = true;
            {
                TLP = new MyTableLayoutPanel((panelCount + 1) / 2, Math.Min(panelCount, 2), new Func<int, string>((int n) =>
                {
                    string ans = ""; for (int i = 0; i < n; i++) ans += "P"; return ans;
                })((panelCount + 1) / 2), panelCount == 1 ? "P" : "PP");
                for (int i = 0; i < panelCount; i++)
                {
                    TLP.AddControl(new GamePanel(port + i, fps), i / 2, i % 2);
                }
                this.Controls.Add(TLP);
            }
            this.FormClosing += Form1_FormClosing;
            //{
            //    Thread thread = new Thread(() =>
            //      {
            //          Random rand = new Random();
            //          while (true)
            //          {
            //              public_random = rand.Next(int.MaxValue);
            //              Thread.Sleep(20);
            //          }
            //      });
            //    thread.IsBackground = true;
            //    thread.Start();
            //}
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
