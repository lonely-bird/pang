#include "pang.h"
#include "eric.h"
void init()
{
	int resume;
	printf("Load previous model.sav? [0/1]\n");
	scanf("%d", &resume);
    if(resume)
    {
        assert(freopen("model.sav","r",stdin));
        REP(i,H) REP(j,D) scanf("%lf",&model.W1[i][j]);
        REP(i,H) scanf("%lf",&model.W2[i]);
        fclose(stdin);
    }
    else
    {
        model.reset(1);
        grad_buffer.reset(0);
        rmsprop_cache.reset(0);
    }
}

Eric env;
void work()
{
    vector<int> observation = env.reset();
    vector<int> prev_x;
    vector<VI> xs;
    vector<VD> hs;
    vector<double> dlogps;
    vector<int> drs;
	double running_reward = 1; running_reward /= (running_reward - 1);
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
		vector<int> x = (prev_x.empty() ? con(cur_x, cur_x) : con(cur_x, prev_x));
        prev_x = cur_x;

        double aprob;VD h;
        tie(aprob,h) = policy_forward(x);
        int action = uni() < aprob;
		static int acnt = 0;
		if (++acnt % 100 == 0) printf("aprob = %.3f\n", aprob);

        xs.push_back(x);
        hs.push_back(h);
        dlogps.push_back(action - aprob);

        int reward;bool done;
        tie(observation,reward,done) = env.step(action);
        reward_sum += reward;

        drs.push_back(reward);
		//puts("b");
        if(!done)
        {
			puts("!done");
            episode_number++;
			printf("EPIS NUM = %d\n", episode_number);

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
                fprintf(stderr,"Episode %03lld avg. reward = %.3lf, Long-term avg. reward = %.3lf\n",\
                        episode_number/batch_size,reward_sum*1.0/batch_size,running_reward*1.0/batch_size);
                reward_sum = 0;

                if(episode_number % batch_size == 0)
                {
					FILE* pFile = fopen("model.sav", "w");
					assert(pFile);
                    REP(i,H) REP(j,D) fprintf(pFile,"%.16f ",model.W1[i][j]);
                    REP(i,H) fprintf(pFile,"%.16f ",model.W2[i]);
                    fclose(pFile);
                }
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
    work();
}
