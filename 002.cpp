#include "pang.h"
#include "eric.h"

void init()
{
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

void work()
{
    Eric env;
    vector<int> observation = env.reset();
    vector<int> prev_x;
    vector<VI> xs;
    vector<VD> hs;
    vector<double> dlogps;
    vector<int> drs;
    double running_reward = nan("");
    long long reward_sum = 0;
    long long episode_number=0;
    
    while(1)
    {
        if(render) env.render();

        const auto &cur_x = prepro(observation);
        vector<int> x = (prev_x.empty()?vector<int>(D,0):cur_x-prev_x);
        prev_x = cur_x;

        double aprob;VD h;
        tie(aprob,h) = policy_forward(x);
        int action = uni() < aprob;

        xs.pb(x);
        hs.pb(h);
        dlogps.pb(action - aprob);

        int reward;bool done;
        tie(observation,reward,done) = env.step(action);
        reward_sum += reward;

        drs.pb(reward);

        if(done)
        {
            episode_number++;

            vector<VI> epx;epx.swap(xs);
            vector<VD> eph;eph.swap(hs);
            vector<double> epdlogp;epdlogp.swap(dlogps);
            vector<int> epr;epr.swap(drs);

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

            REP(i,SZ(epdlogp)) epdlogp[i] *= discounted_epr[i];
            Model grad=policy_backward(eph,epdlogp,epx);
            grad_buffer += grad;

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

                running_reward = isnan(running_reward) ? reward_sum : running_reward * 0.99 + reward_sum * 0.01;
                fprintf(stderr,"Episode %03lld avg. reward = %.3lf, Long-term avg. reward = %.3lf\n",\
                        episode_number/batch_size,reward_sum*1.0/batch_size,running_reward*1.0/batch_size);
                reward_sum = 0;

                if(episode_number % (10 * batch_size) == 0)
                {
                    assert(freopen("model.sav","w",stdout));
                    REP(i,H) REP(j,D) printf("%.16f ",model.W1[i][j]);
                    REP(i,H) printf("%.16f ",&model.W2[i]);
                    fclose(stdout);
                }
            }

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
