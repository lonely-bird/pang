#include<cassert>
#include<cstdio>
#include<queue>
#include "game.h"
const bool useBuildInGame = false;
struct Socket
{
	SOCKADDR_IN addr;
	SOCKET sConnect;
	char Buffer[10000000/*D*80*/];
	queue<char>Recv;
	static const int PORT = 6001;
	static const char* Server_Address;
	static const int TransitDataSize = 1024;
	static const char* MessagePartitionSymbol;
	static const char FeedbackPartitionSymbol = ' ';
private:
	bool InitWinsock()
	{
		if (useBuildInGame)return true;
		WSAData wsaData;
		WORD DLLVersion;
		DLLVersion = MAKEWORD(2, 2);
		return !WSAStartup(DLLVersion, &wsaData);
	}
	void InitSocketData()
	{
		if (useBuildInGame)return;
		sConnect = socket(AF_INET, SOCK_STREAM, NULL);

		addr.sin_addr.s_addr = inet_addr(Server_Address);
		addr.sin_family = AF_INET;
		addr.sin_port = htons(PORT);
	}
	bool Connect()
	{
		if (useBuildInGame)return true;
		if (!InitWinsock()) { printf("Initwinsock Error!\n"); assert(0); }

		InitSocketData();
		return !connect(sConnect, (SOCKADDR*)&addr, sizeof(addr));
	}
	int StringToInteger(const char* const s, const int& len, int& p)
	{
		while (s[p]<'0' || '9'<s[p])p++;
		int num = 0;
		for (; /*p<len*/'0'<=s[p]&&s[p]<='9'; p++)
		{
			/*if (s[p] == FeedbackPartitionSymbol||s[p]=='\0') break;
			if (!('0' <= s[p] && s[p] <= '9'))printf("s[%d]=%c\n", p, s[p]);
			assert('0' <= s[p] && s[p] <= '9');*/
			num = num * 10 + s[p] - '0';
		}
		return num;
	}
	bool ParseFeedback(const char* const s, const int& len, tuple<vector<int>, int, bool>& result)
	{
		vector<int> buffer;
		for (int p = 0; s[p] != MessagePartitionSymbol[0];)
		{
			int num = StringToInteger(s, len, p);
			buffer.push_back(num);
		}
		/*for (int i = 1200-20; i < buffer.size(); i++)printf("%d ", buffer[i]);
		printf("buffer.size=%d\n", (int)buffer.size());*/
		/*printf("s=%s\n", s);
		printf("%d %d\n", (int)buffer.size(), INP * 3 + 2);*/
		//printf("buffer.size()=%d\n", (int)buffer.size());
		assert((int)buffer.size() == INP * 3 + 2);
		bool done = (buffer.back()!=0);
		buffer.pop_back();
		int reward = buffer.back();
		buffer.pop_back();
		assert(buffer.size() % 3 == 0);
		vector<int> TMP;
		for (int i = 0; i < (int)buffer.size(); i += 3)
		{
			TMP.push_back(buffer[i] < 255);
			TMP.push_back(buffer[i + 1] < 255);
		}
		//printf("TMP.size=%d\n", (int)TMP.size());
		/*static vector<int>pre;
		int n = TMP.size();
		if (pre.empty())
		{
			for (int i = 0; i < n; i++)TMP.push_back(TMP[i]),pre.push_back(TMP[i]);
		}
		else
		{
			assert(pre.size() == TMP.size());
			for (int i = 0; i < n; i++)TMP.push_back(pre[i]);
			pre.clear();
			for (int i = 0; i < n; i++)pre.push_back(TMP[i]);
		}*/
		if (done != 1) reward *= -1;

		result = std::make_tuple(TMP, reward, done);
		if (reward != 0)
		{
			if(reward>0)printf("%d", reward);
			else assert(reward == -1);
		}
		/*if (done != 1)printf("done=%d\n", done);*/
		return true;
	}
public:
	string feedBackFromBuildInGame;
	void SendAction(const int& Action,const double &prefer)
	{
		string Send = "";
		if (Action >= 'A' && Action <= 'Z') Send+= (char)Action;
		else Send += (char)(Action + '0');
		{
			static char tmp[100];
			sprintf(tmp, "%.3f", prefer);
			//Send += tmp;
		}
		for (int i = 0; MessagePartitionSymbol[i]; i++)
			Send+=MessagePartitionSymbol[i];
		if (useBuildInGame)
		{
			feedBackFromBuildInGame = Game::Do(Send[0]);
			return;
		}
		//printf("send: %s", Send);
		if (!send(sConnect, Send.c_str(), (int)Send.size(), 0)) { printf("Action Sending Error!\n"); assert (0); }
	}

	tuple<vector<int>, int, bool> ReceiveRewards()
	{
		int BufferLen = 0;
		if (useBuildInGame)
		{
			BufferLen = (int)feedBackFromBuildInGame.size();
			for (int i = 0; i < BufferLen; i++)
			{
				Buffer[i] = feedBackFromBuildInGame[i];
			}
			Buffer[BufferLen++] = MessagePartitionSymbol[0];
			Buffer[BufferLen++] = MessagePartitionSymbol[1];
		}
		else
		{
			//puts("receiving...");
			for (;;)
			{
				//ZeroMemory(Recv, D*5);
				//if (!recv(sConnect, Recv, sizeof(Recv), 0)) { printf("Message Receiving Error!\n"); assert(0); }
				static char r[1024];
				int n;
				if (!(n=recv(sConnect, r, sizeof(r), 0))) { printf("Message Receiving Error!\n"); assert(0); }
				// wait a long time here?
				//printf("receive: %s\n", Recv);
				bool HavePartitionSymbol = 0;
				for (int i = 0; i<n; i++)
				{
					Recv.push(r[i]);
				}
				/*for (int i = 0; Recv[i] > 0; i++)
				{
					Buffer[BufferLen++] = Recv[i];
				}*/
					/*for (int i = 0; i < BufferLen; i++)
					if (Buffer[i] == MessagePartitionSymbol[0] &&
					Buffer[i+1]== MessagePartitionSymbol[1])
					{
					HavePartitionSymbol = true;
					printf("i=%d,BufferLen=%d\n", i, BufferLen);
					assert(i + 1 == BufferLen - 1);
					break;
					}*/
				while (!Recv.empty())
				{
					Buffer[BufferLen++] = Recv.front(); Recv.pop();
					if (BufferLen >= 2 && Buffer[BufferLen - 2] == MessagePartitionSymbol[0] &&
						Buffer[BufferLen - 1] == MessagePartitionSymbol[1])
					{
						HavePartitionSymbol = true;
						//printf("i=%d,BufferLen=%d\n", i, BufferLen);
						//assert(i + 1 == BufferLen - 1);
						break;
					}
				}
				if (HavePartitionSymbol) break;
			}
		}
		//puts("received");
		tuple<vector<int>, int, bool> result;
		if (!ParseFeedback(Buffer, BufferLen, result)) {printf("FeedBackParsing Error!\n"); assert(0);}
		//puts("parsed");
		return result;
	}
	Socket()
	{
		while (!Connect()) { printf("Connection Error!\n"); Sleep(500); }
		printf("Connected!\n");
	}
	~Socket()
	{
		if (!closesocket(sConnect)) { printf("Disconnection Error!\n"); assert(0); }
	}
};
const char* Socket::Server_Address = "127.0.0.1";
const char* Socket::MessagePartitionSymbol = "\r\n";
double timeConsumedByAI = 0.0, timeConsumedByGame = 0.0;
struct Eric
{
	Socket soc;
public:
	clock_t st;
	void render()
    {
		return;
	}
    tuple<vector<int>,int,bool> step(const int& action,const double &prefer)
    {
		clock_t t1 = clock();

		soc.SendAction(action,prefer);
		auto answer= soc.ReceiveRewards();

		clock_t t2 = clock();
		static int cnt = 0;
		//if (++cnt % 100 == 0) printf("AI : %.3lfms, Game : %.3lfms\n", 1000.0*(t1 - st) / CLOCKS_PER_SEC, 1000.0*(t2 - t1) / CLOCKS_PER_SEC);
		timeConsumedByAI += 1000.0*(t1 - st) / CLOCKS_PER_SEC, timeConsumedByGame += 1000.0*(t2 - t1) / CLOCKS_PER_SEC;
		st = t2;

		return answer;
	}
    vector<int> reset()
    {
		return std::get<0>(step('R', 0.5));
    }
};
