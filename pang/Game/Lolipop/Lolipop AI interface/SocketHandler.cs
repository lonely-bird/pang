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
        public static int port = 5;
        public void Start()
        {
            AppendLog("Your IP is: " + GetMyIpAddress().ToString());
            AppendLog("Your port is: " + port.ToString());
            AppendLog("Starting server...");
            InitializeSocket();
            AppendLog("Listening...");
            StartListening();
        }
        void StartListening()
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
        void InitializeSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            socket.Listen(10);
        }
        Socket socket;
        IPAddress GetMyIpAddress()
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
        public delegate void logAppendedHandler(string log);
        public event logAppendedHandler logAppended;
        public delegate void msgReceivedHandler(char msg, StreamWriter writer);
        public event msgReceivedHandler msgReceived;
        void AppendLog(string log)
        {
            logAppended?.Invoke(log);
        }
        public int dataConnectionCounter = 0;
        public SocketHandler()
        {
            msgReceived += (msg, writer) => { ++dataConnectionCounter; };
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