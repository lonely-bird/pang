using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using Motivation;

namespace Lolipop_AI_interface
{
    public partial class GamePanel : Panel
    {
        private int port;
        public GamePanel(int _port,double fps)
        {
            this.Dock = DockStyle.Fill;
            port = _port;
            socketHandler = new SocketHandler(port);
            game = new Game(socketHandler,new Random( Form1.public_random));
            //MessageBox.Show(Color.FromArgb(127,127,127).ToString());
            //this.Size = new Size(1200, 750);
            {
                TLPmain = new MyTableLayoutPanel(1, 2, "A", "S1P");
                {
                    TLPctrl = new MyTableLayoutPanel(2, 1, "PP", "P");
                    TLPctrl.AutoSize = false;
                    {
                        TLPctrl.AddControl(game.controlPanel, 0, 0);
                    }
                    {
                        TXB = new MyTextBox(true);
                        TLPctrl.AddControl(TXB, 1, 0);
                    }
                    TLPmain.AddControl(TLPctrl, 0, 0);
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
                    bool humanFriendly = true;
                    pbx.Click += (object sender, EventArgs e) =>
                      {
                          humanFriendly ^= true;
                      };
                    pbx.DoubleClick += Pbx_DoubleClick;
                    {
                        Thread thread=new Thread(() =>
                        {
                            while (true)
                            {
                                Thread.Sleep((int)Math.Ceiling(1000.0 / fps));
                                var bmp = (humanFriendly ? new Bitmap(pbx.Width, pbx.Height) : new Bitmap(game.imageFeedBackSize.Width, game.imageFeedBackSize.Height));
                                game.drawImage(bmp, humanFriendly);
                                Do(() =>
                                {
                                    //this.Text = bmp.Size.ToString();
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
                        });
                        thread.IsBackground = true;
                        thread.Start();
                    }
                    TLPmain.AddControl(pbx, 0, 1);
                }
                this.Controls.Add(TLPmain);
            }
            socketHandler.logAppended += SocketHandler_logAppended;
            socketHandler.msgReceived += SocketHandler_msgReceived;
            socketHandler.Start();
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
        }
        private void Pbx_DoubleClick(object sender, EventArgs e)
        {
            TLPmain.ColumnStyles[0] = (TLPmain.ColumnStyles[0].SizeType == SizeType.Absolute ?
                new ColumnStyle(SizeType.Percent, 1) : new ColumnStyle(SizeType.Absolute, 1));
        }
        private void SocketHandler_msgReceived(char msg, StreamWriter writer)
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
                    case 'R': game.Reset(new Random(Form1.public_random)); break;
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
            if (TXB.InvokeRequired) TXB.Invoke(new Action(() => { TXB.AppendText(log + "\r\n"); }));
            else TXB.AppendText(log + "\r\n");
        }
        MyTextBox TXB;
        MyTableLayoutPanel TLPmain,TLPctrl;
        SocketHandler socketHandler;
        Game game;
    }
}
