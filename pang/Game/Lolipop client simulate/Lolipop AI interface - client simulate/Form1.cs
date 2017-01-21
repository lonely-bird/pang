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

namespace Lolipop_AI_interface___client_simulate
{
    public partial class Form1 : Form
    {
        int port = 5;
        void SendMessage(string msg)
        {
            writer.WriteLine(msg);
            writer.Flush();
        }
        NetworkStream stream;
        StreamReader reader;
        StreamWriter writer;
        private void Txb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.Text = "Sending messages...";
                SendMessage(TXB.Text);
                this.Text = "Message sent!";
                TXB.Clear();
            }
        }
        private void Do(Action a)
        {
            if (this.InvokeRequired) this.Invoke(a);
            else a.Invoke();
        }
        MyTextBox TXB;
        public Form1()
        {
            this.Size = new Size(750, 500);
            this.FormClosing += Form1_FormClosing;
            {
                TXB = new MyTextBox(false);
                TXB.Multiline = false;
                TXB.KeyDown += Txb_KeyDown;
                this.Controls.Add(TXB);
            }
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(GetMyIpAddress(), port));
            stream = new NetworkStream(socket);
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    string s = reader.ReadLine();
                    if (s.Length > 0)
                    {
                        MessageBox.Show(s);
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        IPAddress GetMyIpAddress()
        {
            //return IPAddress.Parse("192.168.43.19").MapToIPv4();
            foreach (IPAddress ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    //MessageBox.Show(ipAddress.Address.ToString());
                    return ipAddress;
                }
            }
            throw new Exception("Can't detect your IP address");
        }
    }
    /*class ChatBox
    {
        int port = 20;

        public static void Main(String[] args)
        {
            ChatBox chatBox = new ChatBox();
            if (args.Length == 0)
                chatBox.ServerMain();
            else
                chatBox.ClientMain(args[0]);
        }

        public void ServerMain()
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            newsock.Bind(ipep);
            newsock.Listen(10);
            Socket client = newsock.Accept();
            new TcpListener(client); // create a new thread and then receive message.
            newsock.Close();
        }

        public void ClientMain(String ip)
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ip), port);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Connect(ipep);
            new TcpListener(server);
            server.Shutdown(SocketShutdown.Both);
            server.Close();
        }

    }

    public class TcpListener
    {
        Socket socket;
        Thread inThread, outThread;
        NetworkStream stream;
        StreamReader reader;
        StreamWriter writer;

        public TcpListener(Socket s)
        {
            socket = s;
            stream = new NetworkStream(s);
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            inThread = new Thread(new ThreadStart(inLoop));
            inThread.Start();
            outThread = new Thread(new ThreadStart(outLoop));
            outThread.Start();
            inThread.Join(); // 等待 inThread 執行續完成，才離開此函數。 
                             // (注意、按照 inLoop 的邏輯，這個函數永遠不會跳出，因為 inLoop 是個無窮迴圈)。
        }

        public void inLoop()
        {
            while (true)
            {
                String line = reader.ReadLine();
                Console.WriteLine("收到：" + line);
            }
        }

        public void outLoop()
        {
            while (true)
            {
                String line = Console.ReadLine();
                writer.WriteLine(line);
                writer.Flush();
            }
        }
    }*/
}
