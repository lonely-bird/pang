/*************************************Global Variables***************************************/
const int H = 200;//quantity of hidden neurons
const int batch_size = 10; // every how many episodes to do a param update?
const double learning_rate = 1e-4;
const double dgamma = 0.99; // discount factor for reward
const double decay_rate = 0.99; // decay factor for RMSProp leaky sum of grad^2
const bool resume = false; // load previous model?
const bool render = false;
const int D = 80 * 80; //input dimen.
/*************************************Global Variables***************************************/
#include <cmath>
#include <random>
#include <cassert>
#include <vector>
#include <cstring>
#include <utility>
#include <tuple>
using namespace std;

#define REP(i,n) for(int i=0;i<((int)(n));i++)
typedef vector<double> VD;
typedef vector<int> VI;

VI operator -(const VI &a,const VI &b)
{
    auto c=a;
    REP(i,b.size()) c[i]-=b[i];
    return c;
}
double gauss()
{
    static mt19937 urng;//urng method can change
    static normal_distribution<double> dis;
    return dis(urng);
}
double uni()
{
    static mt19937 urng;//urng method can change
    static uniform_real_distribution<double> dis;
    return dis(urng);
}
struct Model
{
    double W1[H][D],W2[H];
    Model(bool rnd){reset(rnd);}
    void reset(bool rnd)
    {
        if(!rnd)
        {
            memset(W1,0,sizeof(W1));
            memset(W2,0,sizeof(W2));
        }
        else
        {
            REP(i,H) REP(j,D)
                W1[i][j]=gauss()/sqrt(D);
            REP(i,H)
                W2[i]=gauss()/sqrt(H);
        }
    }
    void operator +=(const Model &m)
    {
        REP(i,H) REP(j,D) W1[i][j]+=m.W1[i][j];
        REP(i,H) W2[i]+=m.W2[i];
    }
} model(0),grad_buffer(0),rmsprop_cache(0);

double sigmoid(const double &x){return 1/(1 + exp(-x));}

vector<int> prepro(const vector<int> &obs)
{
    assert((int)obs.size()==D);
    for(const int &z:obs) assert(0<=z && z<=1);
    return obs;
}

vector<double> discount_rewards(const vector<int> &r)
{
    vector<double> ret(r.size(),0);
    double add=0;
    for(int t=(int)r.size()-1;t>=0;t--)
    {
        if(r[t]) add=0;//refer to reward def. : pass a obstacle : 1, normal: 0, dead: -1
        add = add * dgamma + r[t];
        ret[t] = add;
    }
    return ret;
}

pair<double,vector<double> > policy_forward(const vector<int> &obs)
{
    vector<double> h(H,0);
    REP(i,H) REP(j,D)
        h[i]+=model.W1[i][j]*obs[j];
    for(auto &x:h) if(x<0)
        x=0;
    
    double logp=0;
    REP(i,H)
        logp+=model.W2[i]*h[i];
    double p = sigmoid(logp);
    return make_pair(p,h);
} 

Model policy_backward(const vector<VD> &eph,const vector<double> &epdlogp,const vector<VI> &epx)
{
    assert(eph.size() == epdlogp.size());
    const int Q=eph.size();
    Model ret(0);
    auto &dW1=ret.W1;
    auto &dW2=ret.W2;
    REP(j,Q) REP(i,H)
        dW2[i]+=eph[j][i]*epdlogp[j];

    vector<vector<double> > dht(H,vector<double>(Q,0));
    REP(i,H) REP(j,Q)
        dht[i][j]=(eph[j][i]<=0?0:epdlogp[j]*model.W2[i]);

    REP(i,H) REP(j,Q) REP(k,D)
        dW1[i][k]+=dht[i][j]*epx[j][k];
    return ret;
}
