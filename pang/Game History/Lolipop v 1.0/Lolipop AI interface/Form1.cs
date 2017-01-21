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

namespace Lolipop_AI_interface
{
    public partial class Form1 : Form
    {
        SocketHandler socketHandler = new SocketHandler();
        Game game = new Game();
        MyTableLayoutPanel TLP;
        public Form1()
        {
            this.Size = new Size(750, 750);
            this.FormClosing += Form1_FormClosing;
            {
                TLP = new MyTableLayoutPanel(2, 1, "AP", "P");
                {
                    TLP.AddControl(game.inputField, 0, 0);
                }
                {
                    TXB = new MyTextBox(true);
                    TLP.AddControl(TXB, 1, 0);
                }
                this.Controls.Add(TLP);
            }
            socketHandler.logAppended += SocketHandler_logAppended;
            socketHandler.msgReceived += SocketHandler_msgReceived;
            socketHandler.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void SocketHandler_msgReceived(char msg,StreamWriter writer)
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
        }
        private void Do(Action a)
        {
            if (this.InvokeRequired) this.Invoke(a);
            else a.Invoke();
        }
        private void SocketHandler_logAppended(string log)
        {
            Do(() => { TXB.AppendText(log + "\r\n"); });
        }

        MyTextBox TXB;
    }
}
