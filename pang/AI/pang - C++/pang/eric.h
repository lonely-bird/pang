#include <WinSock2.h>
struct Socket
{
	SOCKADDR_IN addr;
	SOCKET sConnect;
	char Buffer[80*80*9],Recv[80*80*3];
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
		DLLVersion = MAKEWORD(2, 2);
		return !WSAStartup(DLLVersion, &wsaData);
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
		if (!InitWinsock()) { printf("Initwinsock Error!\n"); while(1); }

		InitSocketData();
		return !connect(sConnect, (SOCKADDR*)&addr, sizeof(addr));
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
	bool ParseFeedback(const char* const s, const int& len, tuple<vector<int>, int, bool>& result)
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

		vector<int> TMP;
		for (int i = 0; i < buffer.size(); i += 3)
			if (buffer[i] == 235 && buffer[i + 1] == 235 && buffer[i + 2] == 235) TMP.push_back(0);
			else TMP.push_back(1);
		result = std::make_tuple(TMP, reward, (bool)done);
		return true;
	}
public:
	void SendAction(const int& Action)
	{
		char Send[5];
		if (Action >= 'A' && Action <= 'Z') Send[0] = Action;
		else Send[0] = Action + '0';
		for(int i=0;MessagePartitionSymbol[i];i++)
			Send[i+1]=MessagePartitionSymbol[i];
		if (!send(sConnect, Send, (int)strlen(Send), 0)) { printf("Action Sending Error!\n"); while(1); }
	}

	tuple<vector<int>, int, bool> ReceiveRewards()
	{
		int BufferLen = 0;
		for (;;)
		{
			ZeroMemory(Recv, 5);
			if (!recv(sConnect, Recv, sizeof(Recv), 0)) { printf("Message Receiving Error!\n"); while(1); }

			bool HavePartitionSymbol = 0;
			for (int i = 0; Recv[i] > 0; i++)
			{
				Buffer[BufferLen++] = Recv[i];
			}
			for(int i=0;i<BufferLen;i++)
				if (Buffer[i] == MessagePartitionSymbol[0] &&
						Buffer[i+1]== MessagePartitionSymbol[1]) {HavePartitionSymbol = true; break;}
			if (HavePartitionSymbol) break;
		}
		tuple<vector<int>, int, bool> result;
		if (!ParseFeedback(Buffer, BufferLen, result)) {printf("FeedBackParsing Error!\n"); while(1);}
		return result;
	}
	Socket()
	{
		if (!Connect()) { printf("Connection Error!\n"); while(1); }
	}
	~Socket()
	{
		if (!closesocket(sConnect)) { printf("Disconnection Error!\n"); while(1); }
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
