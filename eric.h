#pragma comment(lib, "Ws2_32.lib")
#include <WinSock2.h>
struct Socket
{
	SOCKADDR_IN addr;
	SOCKET sConnect;
	static const int PORT = 5;
	static const char* Server_Address;
	static const int TransitDataSize = 1024;
	static const char* MessagePartitionSymbol;
	static const char FeedbackPartitionSymbol = ' ';
private:
	bool InitWinsock()
	{
		WSAData wsaData;
		WORD DLLVersion;
		DLLVersion = MAKEWORD(2, 1);
		return WSAStartup(DLLVersion, &wsaData);
	}
	void InitSocketData()
	{
		sConnect = socket(AF_INET, SOCK_STREAM, NULL);

		addr.sin_addr.s_addr = inet_addr(Server_Address);
		addr.sin_family = AF_INET;
		addr.sin_port = htons(PORT);
	}
	bool Connect()
	{
		if (!InitWinsock()) { printf("Initwinsock Error!\n"); exit(EXIT_FAILURE); }

		InitSocketData();
		return connect(sConnect, (SOCKADDR*)&addr, sizeof(addr));
	}
	int StringToInteger(const char* const s, const int& len, int& p)
	{
		int num = 0;
		for (; p<len; p++)
		{
			if (s[p] == FeedbackPartitionSymbol) break;
			num = num * 10 + s[p] - '0';
		}
		return num;
	}
	bool ParseFeedBack(const char* const s, const int& len, tuple<vector<int>, int, bool>& result)
	{
		vector<int> buffer;
		for (int p = 0; p<len;p++)
		{
			int num = StringToInteger(s, len, p);
			buffer.push_back(num);
		}

		int done = buffer[buffer.size() - 1];
		buffer.pop_back();
		int reward = buffer[buffer.size() - 1];
		buffer.pop_back();

		result = std::make_tuple(buffer, reward, (bool)done);
		return true;
	}
public:
	void SendAction(const int& Action)
	{
		char Send[5];
		Send[0] = Action + '0';
		for(int i=0;MessagePartitionSymbol[i];i++)
			Send[i+1]=MessagePartitionSymbol[i];
		if (!send(sConnect, Send, (int)strlen(Send), 0)) { printf("Action Sending Error!\n"); exit(EXIT_FAILURE); }
	}

	tuple<vector<int>, int, bool> ReceiveRewards()
	{
		int BufferLen = 0;
		char Recv[5], Buffer[100];
		for (;;)
		{
			ZeroMemory(Recv, 5);
			if (!recv(sConnect, Recv, sizeof(Recv), 0)) { printf("Message Receiving Error!\n"); exit(EXIT_FAILURE); }

			bool HavePartitionSymbol = 0;
			for (int i = 0; Recv[i]; i++)
			{
				Buffer[BufferLen++] = Recv[i];
				if (Recv[i] == MessagePartitionSymbol[0] &&
						Recv[i+1]== MessagePartitionSymbol[1]) HavePartitionSymbol = true;
			}
			if (HavePartitionSymbol) break;
		}
		tuple<vector<int>, int, bool> result;
		if (!ParseFeedback(Buffer, BufferLen, result)) {printf("FeedBackParsing Error!\n"); exit(EXIT_FAILURE);}
		return result;
	}
	Socket()
	{
		if (!Connect()) { printf("Connection Error!\n"); exit(EXIT_FAILURE); }
	}
	~Socket()
	{
		if (!closesocket(sConnect)) { printf("Disconnection Error!\n"); exit(EXIT_FAILURE); }
	}
};
const char* Socket::Server_Address = "127.0.0.1";
const char* Socket::MessagePartitionSymbol = "\r\n";
struct Eric
{
	Socket soc;
public:
	void render()
    {
		return;
	}
    tuple<vector<int>,int,bool> step(const int& action)
    {
		soc.SendAction(action);
		return soc.ReceiveRewards();
	}
    vector<int> reset()
    {
		return std::get<0>(step('R'));
    }
};
