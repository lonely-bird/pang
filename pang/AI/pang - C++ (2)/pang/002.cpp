#include "pang.h"
#include "eric.h"
//#include <numeric>
#include<direct.h>
void printLocation()
{
	char *buffer;
	if ((buffer = _getcwd(NULL, 0)) == NULL)
	{
		perror("getcwd error");
	}
	else
	{
		printf("%s\n", buffer);
		free(buffer);
	}
}
void init()
{
	printLocation();
	int resume = 1;
	printf("Load previous model.sav? [0/1]\n");
	//scanf("%d", &resume);
	if (resume)
	{
		{
			auto tmp = freopen("model.sav", "r", stdin);
			assert(tmp);
		}
		char c = getchar();
		if (c == 'A')
		{
			REP(i, H) REP(j, D)
			{
				unsigned long long *t = (unsigned long long*)&model.W1[i][j];
				scanf("%llu", t);// &model.W1[i][j]);
			}
			REP(i, H)
			{
				unsigned long long *t = (unsigned long long*)&model.W2[i];
				scanf("%llu", t);// &model.W2[i]);
			}
			fclose(stdin);
		}
		else
		{
			freopen("model.sav", "r", stdin);
			REP(i, H) REP(j, D) scanf("%lf", &model.W1[i][j]);
			REP(i, H) scanf("%lf", &model.W2[i]);
			fclose(stdin);
		}
		puts("model.sav loaded!");
		//puts("d");
	}
	else
	{
		model.reset(1);
		grad_buffer.reset(0);
		rmsprop_cache.reset(0);
		puts("randomized model created!");
	}
	//puts("a");
}

Eric env;
//unsigned int Rand()
//{
//	static unsigned int seed = 0x20170227;
//	seed *= 0xdefaced;
//	seed += 94441;
//	return seed += seed >> 20;
//}
double explore_rate = 0.0;
#include<cmath>
double emphasize(double v)
{
	return v;
	static double pv = 0.0, pa = 0.0;
	const double p = 2.0;
	double ans;
	if (v < 0.5)ans = pow(v, p) / pow(0.5, p) / 2.0;
	else ans = 1.0 - pow(1.0 - v, p) / pow(0.5, p) / 2.0;
	assert((pv < v) == (pa < ans));
	pv = v, pa = ans;
	return ans;
}
void work()
{
	//puts("c");
	vector<int> observation = env.reset();
	vector<int> prev_x;
	vector<VI> xs;
	vector<VD> hs;
	vector<double> dlogps;
	vector<int> drs;
	double running_reward = numeric_limits<double>::infinity();// 1; running_reward /= (running_reward - 1);
	long long reward_sum = 0;
	long long episode_number = 0;

	auto con = [&](vector<int> a, vector<int> b)
	{
		a.insert(a.end(), b.begin(), b.end());
		return a;
	};
	while (1)
	{
		//puts("a");
		if (render) env.render();

		auto cur_x = prepro(observation);
		vector<int> x = cur_x;// (prev_x.empty() ? con(cur_x, cur_x) : con(cur_x, prev_x));
		prev_x = cur_x;

		double aprob; VD h;
		tie(aprob, h) = policy_forward(x);
		int action = (uni() < emphasize(aprob));
		//if (Rand() % 1000000 < 1000000.0*explore_rate)action = Rand() & 1;
		if (uni() < explore_rate)action = uni() < 0.5;
		static int acnt = 0;
		//if (++acnt % 5000 == 0) printf("(%.3f)", aprob);
		/*
		xs.push_back(x);
		hs.push_back(h);
		dlogps.push_back(action - aprob);*/

		int reward; bool done;
		tie(observation, reward, done) = env.step(action, aprob);
		reward_sum += reward;

		//drs.push_back(min(reward,0));
		//puts("b");
		if (!done)
		{
			//puts("!done");
			episode_number++;
			putchar('.');
			//puts("d");
			Sleep(1000);
			observation = env.reset();
			prev_x.clear();
		}
	}
}
int main()
{
	init();
	//puts("b");
	work();
}
