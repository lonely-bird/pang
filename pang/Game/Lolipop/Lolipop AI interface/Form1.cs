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
        SocketHandler socketHandler = new SocketHandler(5);
        Game game = new Game();
        MyTableLayoutPanel TLP;
        Bitmap bmp = new Bitmap(750, 750);
        public Form1()
        {
            //MessageBox.Show(Color.FromArgb(127,127,127).ToString());
            this.Size = new Size(1200, 750);
            this.FormClosing += Form1_FormClosing;
            {
                MyTableLayoutPanel tlp = new MyTableLayoutPanel(1, 2, "A", "PP");
                {
                    TLP = new MyTableLayoutPanel(2, 1, "AP", "P");
                    {
                        TLP.AddControl(Game.controlPanel, 0, 0);
                    }
                    {
                        TXB = new MyTextBox(true);
                        TLP.AddControl(TXB, 1, 0);
                    }
                    tlp.AddControl(TLP, 0, 0);
                }
                {
                    PictureBox pbx = new PictureBox();
                    pbx.Image = new Bitmap(pbx.Width, pbx.Height);
                    pbx.Dock = DockStyle.Fill;
                    pbx.SizeMode = PictureBoxSizeMode.Zoom;
                    /*pbx.SizeChanged += (sender, e) =>
                    {
                        SocketHandler_logAppended(pbx.Size.ToString());
                        pbx.Image.Dispose();
                        pbx.Image = new Bitmap(pbx.Width, pbx.Height);
                    };*/
                    new Thread(() =>
                    {
                        while (true)
                        {
                            Thread.Sleep(20);
                            Do(() =>
                            {
                                var bmp = new Bitmap(game.imageFeedBackSize.Width,game.imageFeedBackSize.Height); //new Bitmap(pbx.Width, pbx.Height);
                                this.Text = bmp.Size.ToString();
                                game.drawImage(bmp,false);
                                //{
                                //    BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                                //    unsafe
                                //    {
                                //        byte *p=(byte*)bd.Scan0.ToPointer
                                //    }
                                //}
                                var img = pbx.Image;
                                pbx.Image = bmp;
                                img.Dispose();
                            });
                        }
                    }).Start();
                    tlp.AddControl(pbx, 0, 1);
                }
                this.Controls.Add(tlp);
            }
            socketHandler.logAppended += SocketHandler_logAppended;
            socketHandler.msgReceived += SocketHandler_msgReceived;
            socketHandler.Start();
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void SocketHandler_msgReceived(char msg,StreamWriter writer)
        {
            Do(() =>
            {
                /*
                 * R:restart
                 * 0:release
                 * 1:press
                 */
                switch (msg)
                {
                    case 'R': game.Reset(); break;
                    case '0': game.Update(false); break;
                    case '1': game.Update(true); break;
                    default: throw new ArgumentException();
                }
                string s = game.getFeedBack();
                //SocketHandler_logAppended("sending... msg = " + s);
                writer.WriteLine(s);
                writer.Flush();
            });
        }
        private void Do(Action a)
        {
            if (this.InvokeRequired) this.Invoke(a);
            else a.Invoke();
        }
        private void SocketHandler_logAppended(string log)
        {
            if(TXB.InvokeRequired) TXB.Invoke(new Action(() => { TXB.AppendText(log + "\r\n"); }));
            else TXB.AppendText(log + "\r\n");
        }

        MyTextBox TXB;
    }
}
