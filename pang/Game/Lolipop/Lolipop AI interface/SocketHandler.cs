using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Lolipop_AI_interface
{
    class SocketHandler
    {
        public void Start()
        {
            AppendLog("Your IP is: " + GetMyIpAddress().ToString());
            AppendLog("Your port is: " + port.ToString());
            AppendLog("Starting server...");
            InitializeSocket();
            AppendLog("Listening...");
            StartListening();
        }
        private void StartListening()
        {
            int msg_count = 0;
            bool stopping = false;
            Thread listeningThread = new Thread(() =>
              {
                  int pre_port = port;
                  AppendLog($"port {pre_port} started");
                  try
                  {
                      while (true)
                      {
                          Socket client = socket.Accept();
                          AppendLog(string.Format("Message #{0}:", ++msg_count));
                          IPEndPoint clientip = client.RemoteEndPoint as IPEndPoint;
                          AppendLog("Client's ip is: " + clientip);
                          NetworkStream stream = new NetworkStream(client);
                          StreamReader reader = new StreamReader(stream);
                          StreamWriter writer = new StreamWriter(stream);
                          bool stopped = false;
                          Thread transferingThread = new Thread(() =>
                            {
                                try
                                {
                                    while (true)
                                    {
                                        string s = reader.ReadLine();
                                        if (s.Length > 0)
                                        {
                                          //AppendLog(s);
                                          for (int i = 0; i < s.Length; i++) msgReceived?.Invoke(s[i], writer);
                                        }
                                    }
                                }
                                catch (Exception error)
                                {
                                    AppendLog("Connection Error:\r\n" + error.ToString());
                                    AppendLog("Listening...");
                                }
                              //client.Close();
                              stopped = true;
                            });
                          Thread transferingThreadMonitor = new Thread(() =>
                            {
                                while (!stopped)
                                {
                                    Thread.Sleep(100);
                                    if (stopping)
                                    {
                                        transferingThread.Abort();
                                        break;
                                    }
                                }
                            });
                          transferingThreadMonitor.IsBackground = true;
                          transferingThreadMonitor.Start();
                          transferingThread.Start();
                          if (stopping) break;
                      }
                  }
                  catch (Exception error)
                  {
                      AppendLog("Fatal Error:\r\n" + error.ToString());
                  }
                  AppendLog($"port {pre_port} stopped");
              });
            Thread listeningThreadMonitor = new Thread(() =>
            {
                int pre_port = port;
                while (true)
                {
                    Thread.Sleep(100);
                    if (port != pre_port)
                    {
                        stopping = true;
                        socket.Close();
                        AppendLog("new port will start in 1 sec...");
                        Thread.Sleep(1000);
                        Start();
                        return;
                    }
                }
            });
            listeningThreadMonitor.IsBackground = true;
            listeningThreadMonitor.Start();
            listeningThread.Start();
        }
        private void InitializeSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            socket.Listen(10);
        }
        private IPAddress GetMyIpAddress()
        {
            foreach (IPAddress ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipAddress;
                }
            }
            throw new Exception("Can't detect your IP address");
        }
        private void AppendLog(string log)
        {
            logAppended?.Invoke(log);
        }
        public SocketHandler(int _port)
        {
            port = _port;
            msgReceived += (msg, writer) => { ++dataConnectionCounter; };
        }
        private int port;
        private Socket socket;
        public delegate void logAppendedHandler(string log);
        public event logAppendedHandler logAppended;
        public delegate void msgReceivedHandler(char msg, StreamWriter writer);
        public event msgReceivedHandler msgReceived;
        public int dataConnectionCounter = 0;
    }
}