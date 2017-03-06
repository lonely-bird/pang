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

namespace Lolipop_AI_interface___client_simulate
{
    public partial class Form1 : Form
    {
        int port = 5;
        void SendMessage(string msg)
        {
            Debug.Assert(msg == "R" || msg == "0" || msg == "1");
            LBL.Invoke(new Action(() => { LBL.Text = $"Score: {score}"; }));
            //this.Invoke(new Action(() => { this.Text = "Send:" + msg; }));
            writer.WriteLine(msg);
            writer.Flush();
            try
            {
                string[] s=reader.ReadLine().Split(' ');
                if (msg == "R") score = 0;
                else
                {
                    score +=Math.Max( int.Parse(s[s.Length - 2]),0);
                }
            } catch(Exception error) { this.Invoke(new Action(() => { this.Text = error.ToString(); })); }
        }
        NetworkStream stream;
        StreamReader reader;
        StreamWriter writer;
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
        int score = 0;
        private SerialPort comport;

        public Form1()
        {
            this.Size = new Size(500, 500);
            this.FormClosing += Form1_FormClosing;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            {
                LBL = new MyLabel("R: restart\r\nQ: control");
                this.Controls.Add(LBL);
            }
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(GetMyIpAddress(), port));
            stream = new NetworkStream(socket);
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            Thread thread = new Thread(() =>
              {
                  for (DateTime startTime = DateTime.Now; ;)
                  {
                      if (keyState == 0) SendMessage("1");
                      if (keyState == 1) SendMessage("0");
                      startTime = startTime.AddSeconds(1.0 / FPS);
                      Thread.Sleep((int)Math.Max(0.0,(startTime-DateTime.Now).TotalMilliseconds));
                  }
              });
            thread.IsBackground = true;
            thread.Start();
            comport = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);
            comport.DataReceived += Comport_DataReceived;
            if (!comport.IsOpen)
            {
                comport.Open();
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
                string s= BitConverter.ToString(buffer);
                this.Text = s;
                for (int i=0,j=0;i<s.Length ;i++)
                {
                    if(s[i]=='0'||s[i]=='1')
                    {
                        if(j==3&&s[i]=='1')
                        {
                            SendMessage("R");
                            keyState = -1;
                        }
                        else if(j==0)
                        {
                            if (s[i] == '1') keyState = 1;
                            else if (s[i] == '0') keyState = 0;
                        }
                        j++;
                    }
                }
            }));
        }

        MyLabel LBL;
        double FPS = 200.0;
        int keyState = -1;
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            this.Text = $"KeyUp{e.KeyCode}";
            if (e.KeyCode == Keys.R)
            {
                SendMessage("R");
                keyState = -1;
            }
            else if (e.KeyCode == Keys.Q)
            {
                keyState = 0;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            this.Text = $"KeyDown{e.KeyCode}";
            if (e.KeyCode == Keys.Q)
            {
                keyState = 1;
            }
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
