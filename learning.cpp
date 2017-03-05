#include "learning.h"
#include <cmath>
namespace learning {
template<typename T>
Matrix<T> &operator += (Matrix<T> &lhs, const Matrix<T> &rhs) {
    assert(lhs.rows() == rhs.rows());
    assert(lhs.columns() == rhs.columns());
    lhs.data() += rhs.data();
    return lhs;
}
template<typename T>
Matrix<T> operator + (const Matrix<T> &lhs, const Matrix<T> &rhs) {
    Matrix<T> result = lhs;
    result += rhs;
    return std::move(result);
}
template<typename T>
Matrix<T> &operator -= (Matrix<T> &lhs, const Matrix<T> &rhs) {
    assert(lhs.rows() == rhs.rows());
    assert(lhs.columns() == rhs.columns());
    lhs.data() -= rhs.data();
    return lhs;
}
template<typename T>
Matrix<T> operator - (const Matrix<T> &lhs, const Matrix<T> &rhs) {
    Matrix<T> result = lhs;
    result -= rhs;
    return std::move(result);
}
template<typename T>
Matrix<T> &operator *= (Matrix<T> &lhs, T rhs) {
    lhs.data() *= rhs;
    return lhs;
}
template<typename T>
Matrix<T> operator * (const Matrix<T> &lhs, T rhs) {
    Matrix<T> result = lhs;
    result *= rhs;
    return std::move(result);
}
template<typename T>
std::valarray<T> operator * (const Matrix<T> &lhs,
        const std::valarray<T> &rhs) {
    assert(lhs.columns() == (int) rhs.size());
    std::valarray<T> result(lhs.rows());
    for(int i = 0; i < lhs.rows(); i++)
        for(int j = 0; j < lhs.columns(); j++)
            result[i] += lhs[i][j] * rhs[j];
    return std::move(result);
}
template<typename T>
std::valarray<T> operator * (const std::valarray<T> &lhs,
        const Matrix<T> &rhs) {
    assert((int) lhs.size() == rhs.rows());
    std::valarray<T> result(rhs.columns());
    for(int i = 0; i < rhs.columns(); i++)
        for(int j = 0; j < rhs.rows(); j++)
            result[i] += lhs[j] * rhs[j][i];
    return std::move(result);
}
template<typename T>
Matrix<T> operator * (const Matrix<T> &lhs, const Matrix<T> &rhs) {
    assert(lhs.columns() == rhs.rows());
    Matrix<T> result(lhs.rows(), rhs.columns());
    for(int i = 0; i < lhs.rows(); i++)
        for(int k = 0; k < lhs.columns(); k++)
            for(int j = 0; j < rhs.columns(); j++)
                result[i][j] += lhs[i][k] * rhs[k][j];
    return std::move(result);
}

template<typename T>
T inner_product(const std::valarray<T> &lhs, const std::valarray<T> &rhs) {
    assert(lhs.size() == rhs.size());
    T result = 0;
    for(int i = 0; i < (int) lhs.size(); i++)
        result += lhs[i] * rhs[i];
    return result;
}

template<typename T>
Matrix<T> to_matrix(const std::vector<std::valarray<T>> v) {
    if(v.empty())
        return Matrix<T>(0, 0);
    Matrix<T> result(v.size(), v[0].size());
    for(int i = 0; i < result.rows(); i++)
        for(int j = 0; j < result.columns(); j++)
            result[i][j] = v[i][j];
    return result;
}
template<typename T>
std::valarray<T> to_valarray(const std::vector<T> v) {
    return std::valarray<T>(v.data(), v.size());
}

void Model::reset() {
    m_w1 = 0;
    m_w2 = 0;
}
#if 0
Model &Model::operator += (Model &lhs, const Model &rhs) {
    lhs.w1() += rhs.w1();
    lhs.w2() += rhs.w2();
    return lhs;
}
#endif

constexpr int batch_size = 10; // every how many episodes to do a param update?
constexpr double learning_rate = 1e-4;
constexpr double dgamma = 0.99; // discount factor for reward
constexpr double decay_rate = 0.99; // decay factor for RMSProp leaky sum of grad^2

constexpr double sigmoid(double x) {
    return 1 / (1 + exp(-x));
}

void discount_rewards(std::valarray<double> &r) {
    double add = 0;
    for(int i = (int) r.size() - 1; i >= 0; i--) {
        if(r[i]) add=0;
        add = add * dgamma + r[i];
        r[i] = add;
    }
}
void normalize(std::valarray<double> &r) {
    double mean = r.sum() / r.size();
    r -= mean;
    double stdev = std::sqrt(inner_product(r, r) / r.size());
    if(stdev)
        r /= stdev;
}

Learning::Learning(int n, int f, seed_type seed) :
        m_neurons(n), m_features(f), m_model(n, f),
        m_gradbuf(n, f), m_rmsprop(n, f), m_engine(seed),
        m_episode(0), m_saved_feature(), m_saved_mid(),
        m_saved_pd(), m_saved_reward() {
    m_model.randomize(m_engine);
}
bool Learning::play(std::valarray<double> feature) {
    std::valarray<double> mid = m_model.w1() * feature;
    for(int i = 0; i < m_neurons; i++)
        if(mid[i] < 0)
            mid[i] = 0;

    double p = sigmoid(inner_product(m_model.w2(), mid));
    bool action = std::bernoulli_distribution(p)(m_engine);

    m_saved_feature.push_back(feature);
    m_saved_mid.push_back(mid);
    m_saved_pd.push_back((action ? 1.0 : 0.0) - p);

    return action;
}
void Learning::feedback(double reward) {
    assert(reward >= -1.0 && reward <= 1.0);
    m_saved_reward.push_back(reward);
}
void Learning::game_over() {
    m_episode++;

    Matrix<double> epx = to_matrix(m_saved_feature);
    Matrix<double> eph = to_matrix(m_saved_mid);
    std::valarray<double> epd = to_valarray(m_saved_pd);
    std::valarray<double> epr = to_valarray(m_saved_reward);

    discount_rewards(epr);
    normalize(epr);
    epd *= epr;

    // policy backward
    Matrix<double> dht(eph.columns(), epx.rows());
    for(int i = 0; i < dht.rows(); i++)
        for(int j = 0; j < dht.columns(); j++)
            dht[i][j] = (eph[j][i] <= 0 ? 0 : epd[j] * m_model.w2()[i]);

    m_gradbuf.w1() += dht * epx;
    m_gradbuf.w2() += epd * eph;

    // cleanup
    m_saved_feature.clear();
    m_saved_mid.clear();
    m_saved_pd.clear();
    m_saved_reward.clear();

    if(m_episode % batch_size == 0)
        batch_work();
}

void decay(double &lhs, double rhs) {
    lhs = lhs * decay_rate + rhs * rhs * (1 - decay_rate);
}
void decay(std::valarray<double> &lhs, const std::valarray<double> &rhs) {
    assert(lhs.size() == rhs.size());
    for(int i = 0; i < (int) lhs.size(); i++)
        decay(lhs[i], rhs[i]);
}
void decay(Matrix<double> &lhs, const Matrix<double> &rhs) {
    decay(lhs.data(), rhs.data());
}
void learn(double &lhs, double x, double y) {
    assert(y >= 0.0);
    lhs += learning_rate * x / (std::sqrt(y) + 1e-5);
}
void learn(std::valarray<double> &lhs, const std::valarray<double> &x,
        const std::valarray<double> &y) {
    assert(lhs.size() == x.size());
    assert(lhs.size() == y.size());
    for(int i = 0; i < (int) lhs.size(); i++)
        learn(lhs[i], x[i], y[i]);
}
void learn(Matrix<double> &lhs, const Matrix<double> &x,
        const Matrix<double> &y) {
    learn(lhs.data(), x.data(), y.data());
}
void Learning::batch_work() {
    decay(m_rmsprop.w1(), m_gradbuf.w1());
    decay(m_rmsprop.w2(), m_gradbuf.w2());

    learn(m_model.w1(), m_gradbuf.w1(), m_rmsprop.w1());
    learn(m_model.w2(), m_gradbuf.w2(), m_rmsprop.w2());

    m_gradbuf.reset();
}
}


