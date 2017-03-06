#include "pang.h"
#include "eric.h"
//#include <numeric>
void init()
{
	int resume;
	printf("Load previous model.sav? [0/1]\n");
	scanf("%d", &resume);
    if(resume)
    {
        assert(freopen("model.sav","r",stdin));
		REP(pangi,2)REP(i, H) REP(j, D) scanf("%lf", &Pang[pangi].model.W1[i][j]);
		REP(pangi, 2)REP(i, H) scanf("%lf", &Pang[pangi].model.W2[i]);
        fclose(stdin);
    }
    else
    {
		REP(pangi, 2)Pang[pangi].model.reset(1);
		REP(pangi, 2)Pang[pangi].grad_buffer.reset(0);
		REP(pangi, 2)Pang[pangi].rmsprop_cache.reset(0);
    }
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
    vector<double> observation = env.reset();
    vector<VD> xs[2];
    vector<VD> hs[2];
    vector<double> dlogps[2];
    vector<int> drs[2];
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
		vector<double> x = cur_x;// (prev_x.empty() ? con(cur_x, cur_x) : con(cur_x, prev_x));
		bool actions[2];
		REP(pangi, 2)
		{
			double aprob; VD h;
			tie(aprob, h) = Pang[pangi].policy_forward(x);
			int action = (uni() < emphasize(aprob));
			//if (Rand() % 1000000 < 1000000.0*explore_rate)action = Rand() & 1;
			if (uni() < explore_rate)action = uni() < 0.5;
			static int acnt = 0;
			if (++acnt % 5000 == 0) printf("(%.3f)", aprob);
			xs[pangi].push_back(x);
			hs[pangi].push_back(h);
			dlogps[pangi].push_back(action - aprob);
		}
        int reward;bool done;
        tie(observation,reward,done) = env.step(actions[0]?'Q':'W',actions[1]?'O':'P');
        reward_sum += reward;
		//printf("reward:%d, done:%d\n",reward, done);
		REP(pangi, 2)drs[pangi].push_back(min(reward,0));
		//puts("b");
        if(!done)
        {
			assert(reward == -1);
			//puts("!done");
            episode_number++;
			putchar('.');
			//printf("EPIS NUM = %d\n", episode_number);
			REP(pangi, 2)
			{
				vector<VD> epx; epx.swap(xs[pangi]);
				vector<VD> eph; eph.swap(hs[pangi]);
				vector<double> epdlogp; epdlogp.swap(dlogps[pangi]);
				vector<int> epr; epr.swap(drs[pangi]);

				//puts("a");
				vector<double> discounted_epr = discount_rewards(epr);
				{
					double mean = 0;
					for (auto &z : discounted_epr) mean += z;
					mean /= discounted_epr.size();
					for (auto &z : discounted_epr) z -= mean;
				}
				{
					double stdev = 0;
					for (auto &z : discounted_epr) stdev += z*z;
					stdev = sqrt(stdev / discounted_epr.size());
					for (auto &z : discounted_epr) z /= stdev;
				}

				//puts("b");
				//printf("%d %d\n", epdlogp.size(), discounted_epr.size());
				assert(epdlogp.size() == discounted_epr.size());
				REP(i, epdlogp.size()) epdlogp[i] *= discounted_epr[i];
				//puts("b1");
				//printf("%u %u %u\n", eph.size(), epdlogp.size(), epx.size());
				Model grad = Pang[pangi].policy_backward(eph, epdlogp, epx);
				//puts("b2");
				Pang[pangi].grad_buffer += grad;
			}
			//puts("c");
            if(episode_number % batch_size == 0)
            {
				REP(pangi, 2)
				{
					REP(i, H) REP(j, D)
						Pang[pangi].rmsprop_cache.W1[i][j] = decay_rate * Pang[pangi].rmsprop_cache.W1[i][j] + (1 - decay_rate) * Pang[pangi].grad_buffer.W1[i][j] * Pang[pangi].grad_buffer.W1[i][j];
					REP(i, H)
						Pang[pangi].rmsprop_cache.W2[i] = decay_rate * Pang[pangi].rmsprop_cache.W2[i] + (1 - decay_rate) * Pang[pangi].grad_buffer.W2[i] * Pang[pangi].grad_buffer.W2[i];

					REP(i, H) REP(j, D)
						Pang[pangi].model.W1[i][j] += learning_rate * Pang[pangi].grad_buffer.W1[i][j] / (sqrt(Pang[pangi].rmsprop_cache.W1[i][j]) + 1e-5);
					REP(i, H)
						Pang[pangi].model.W2[i] += learning_rate * Pang[pangi].grad_buffer.W2[i] / (sqrt(Pang[pangi].rmsprop_cache.W2[i]) + 1e-5);

					Pang[pangi].grad_buffer.reset(0);
				}
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
					REP(pangi, 2)REP(i,H) REP(j,D) fprintf(pFile,"%.16f ",Pang[pangi].model.W1[i][j]);
					REP(pangi, 2)REP(i,H) fprintf(pFile,"%.16f ",Pang[pangi].model.W2[i]);
                    fclose(pFile);
                }
				if (explore_rate >= 0.05)explore_rate *= 0.98;
            }

			//puts("d");
            observation = env.reset();
        }
    }
}
int main()
{
    init();
    work();
}
