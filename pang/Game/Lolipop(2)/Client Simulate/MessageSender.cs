using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Client_Simulate
{
    class MessageSender
    {
        public int keyState = -1;
        public MessageSender(int _port)
        {
            port = _port;
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            {
            retry_index:;
                try { socket.Connect(new IPEndPoint(GetMyIpAddress(), port)); }
                catch (Exception) { goto retry_index; }
            }
            stream = new NetworkStream(socket);
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            {
                Thread thread = new Thread(() =>
                {
                    for (DateTime startTime = DateTime.Now; ;)
                    {
                        if (keyState == 0) SendMessage("1");
                        if (keyState == 1) SendMessage("0");
                        startTime = startTime.AddSeconds(1.0 / Form1.FPS);
                        Thread.Sleep((int)Math.Max(0.0, (startTime - DateTime.Now).TotalMilliseconds));
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
        }
        public string answer = null;
        public void SendMessage(string msg)
        {
            Debug.Assert(msg == "R" || msg == "0" || msg == "1");
            if (msg == "R")
            {
                status = "Not ready";
                enabled = false;
            }
            else
            {
                status = $"Score: {score}";
                if (!enabled) return;
            }
            //this.Invoke(new Action(() => { this.Text = "Send:" + msg; }));
            writer.WriteLine(msg);
            writer.Flush();
            try
            {
                answer = reader.ReadLine();
                string[] s = answer.Split(' ');
                if (msg == "R") score = 0;
                else
                {
                    score += Math.Max(int.Parse(s[s.Length - 2]), 0);
                }
            }
            catch (Exception error)
            {
                status = error.ToString();
                answer = null;
            }
            return;
        }
        IPAddress GetMyIpAddress()
        {
            //var ans= IPAddress.Parse("192.168.56.1").MapToIPv4();
            ////System.Windows.Forms.MessageBox.Show(ans.ToString());
            //return ans;
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
        //public delegate void StatusChangedEventHandler(string status);
        //public StatusChangedEventHandler StatusChanged;
        public bool enabled = false;
        public string status = "Not ready";
        private int port;
        private int score;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
    }
}
