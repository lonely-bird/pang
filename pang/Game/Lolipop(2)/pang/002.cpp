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
	int resume=1;
	printf("Load previous model.sav? [0/1]\n");
	//scanf("%d", &resume);
    if(resume)
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
	if (v < 0.5)ans= pow(v, p) / pow(0.5, p)/2.0;
	else ans= 1.0 - pow(1.0 - v, p) / pow(0.5, p)/2.0;
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
    long long episode_number=0;

	auto con = [&](vector<int> a, vector<int> b)
	{
		a.insert(a.end(), b.begin(), b.end());
		return a;
	};
    while(1)
    {
		//puts("a");
        if(render) env.render();

        auto cur_x = prepro(observation);
		vector<int> x = cur_x;// (prev_x.empty() ? con(cur_x, cur_x) : con(cur_x, prev_x));
        prev_x = cur_x;

        double aprob;VD h;
        tie(aprob,h) = policy_forward(x);
        int action = (uni() < emphasize(aprob));
		//if (Rand() % 1000000 < 1000000.0*explore_rate)action = Rand() & 1;
		if (uni() < explore_rate)action = uni() < 0.5;
		static int acnt = 0;
		if (++acnt % 5000 == 0) printf("(%.3f)", aprob);
/*
        xs.push_back(x);
        hs.push_back(h);
        dlogps.push_back(action - aprob);*/

        int reward;bool done;
		tie(observation, reward, done) = env.step(action, aprob);
        reward_sum += reward;

        //drs.push_back(min(reward,0));
		//puts("b");
        if(!done&&false)
        {
			//puts("!done");
            episode_number++;
			putchar('.');
			//printf("EPIS NUM = %d\n", episode_number);

            vector<VI> epx;epx.swap(xs);
            vector<VD> eph;eph.swap(hs);
            vector<double> epdlogp;epdlogp.swap(dlogps);
            vector<int> epr;epr.swap(drs);

			//puts("a");
            vector<double> discounted_epr = discount_rewards(epr);
            {
                double mean=0;
                for(auto &z:discounted_epr) mean+=z;
                mean/=discounted_epr.size();
                for(auto &z:discounted_epr) z-=mean;
            }
            {
                double stdev=0;
                for(auto &z:discounted_epr) stdev+=z*z;
                stdev=sqrt(stdev/discounted_epr.size());
                for(auto &z:discounted_epr) z/=stdev;
            }

			//puts("b");
            REP(i,epdlogp.size()) epdlogp[i] *= discounted_epr[i];
			//puts("b1");
			//printf("%u %u %u\n", eph.size(), epdlogp.size(), epx.size());
            Model grad=policy_backward(eph,epdlogp,epx);
			//puts("b2");
            grad_buffer += grad;

			//puts("c");
            if(episode_number % batch_size == 0)
            {
                REP(i,H) REP(j,D)
                    rmsprop_cache.W1[i][j] = decay_rate * rmsprop_cache.W1[i][j] + (1 - decay_rate) * grad_buffer.W1[i][j] * grad_buffer.W1[i][j];
                REP(i,H)
                    rmsprop_cache.W2[i] = decay_rate * rmsprop_cache.W2[i] + (1 - decay_rate) * grad_buffer.W2[i] * grad_buffer.W2[i];

                REP(i,H) REP(j,D)
                    model.W1[i][j] += learning_rate * grad_buffer.W1[i][j] / (sqrt(rmsprop_cache.W1[i][j]) + 1e-5);
                REP(i,H)
                    model.W2[i] += learning_rate * grad_buffer.W2[i] / (sqrt(rmsprop_cache.W2[i]) + 1e-5);

                grad_buffer.reset(0);

                running_reward = isinf(running_reward) ? reward_sum : running_reward * 0.99 + reward_sum * 0.01;
				printf("AI : %.3lf s, Game : %.3lf s\n", timeConsumedByAI / 1000.0, timeConsumedByGame / 1000.0);
                fprintf(stderr,"Episode %03lld avg. reward = %.3lf, Long-term avg. reward = %.3lf, explore rate = %.3f\n",\
                        episode_number/batch_size,reward_sum*1.0/batch_size,running_reward*1.0/batch_size,explore_rate);
				{
					FILE* rec = fopen("rec.txt", "a");
					for (int i = 0; i < 10; i++)fprintf(rec, ".");
					fprintf(rec, "AI : %.3lf s, Game : %.3lf s\n", timeConsumedByAI / 1000.0, timeConsumedByGame / 1000.0);
					fprintf(rec, "Episode %03lld avg. reward = %.3lf, Long-term avg. reward = %.3lf\n", \
						episode_number / batch_size, reward_sum*1.0 / batch_size, running_reward*1.0 / batch_size);
					fclose(rec);
				}
				timeConsumedByAI = timeConsumedByGame = 0.0;
                reward_sum = 0;
                if(episode_number % batch_size == 0)
                {
					FILE* pFile = fopen("model.sav", "w");
					assert(pFile);
					fprintf(pFile, "A");
					REP(i, H) REP(j, D)fprintf(pFile, "%llu ", *((unsigned long long*)&model.W1[i][j])); //fprintf(pFile,"%.50f ",model.W1[i][j]);
					REP(i, H)fprintf(pFile, "%llu ", *((unsigned long long*)&model.W2[i])); //fprintf(pFile,"%.50f ",model.W2[i]);
					fclose(pFile);
                }
				model.Perturbate();
				if (explore_rate >= 0.05)explore_rate *= 0.98;
            }

			//puts("d");
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
