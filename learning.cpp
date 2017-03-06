#include "learning.h"
#include <cmath>
#include <algorithm>
namespace learning {
Vector::Vector(unsigned size) :
        m_size(size), m_data(new double[size]()) {
}
Vector::Vector(const Vector &rhs) :
        m_size(rhs.size()), m_data(new double[m_size]()) {
    std::copy_n(rhs.data(), rhs.size(), data());
}
Vector::Vector(const std::vector<double> &rhs) :
        m_size(rhs.size()), m_data(new double[m_size]()) {
    std::copy(rhs.begin(), rhs.end(), data());
}
Vector &Vector::operator = (const Vector &rhs) {
    if(this != &rhs) {
        if(m_size != rhs.size()) {
            m_size = rhs.size();
            m_data.reset(new double[m_size]());
        }
        std::copy_n(rhs.data(), rhs.size(), data());
    }
    return *this;
}
Vector::~Vector() {
}
Vector::operator std::vector<double> () const {
    return std::vector<double>(data(), data() + size());
}
double Vector::operator [] (unsigned index) const {
    assert(index < size());
    return data()[index];
}
double &Vector::operator [] (unsigned index) {
    assert(index < size());
    return data()[index];
}
Vector &operator += (Vector &lhs, const Vector &rhs) {
    assert(lhs.size() == rhs.size());
    for(unsigned i = 0; i < lhs.size(); i++)
        lhs[i] += rhs[i];
    return lhs;
}
Vector &operator += (Vector &lhs, double rhs) {
    for(unsigned i = 0; i < lhs.size(); i++)
        lhs[i] += rhs;
    return lhs;
}
Vector &operator -= (Vector &lhs, const Vector &rhs) {
    assert(lhs.size() == rhs.size());
    for(unsigned i = 0; i < lhs.size(); i++)
        lhs[i] -= rhs[i];
    return lhs;
}
Vector &operator -= (Vector &lhs, double rhs) {
    return lhs += -rhs;
}
Vector &operator *= (Vector &lhs, const Vector &rhs) {
    assert(lhs.size() == rhs.size());
    for(unsigned i = 0; i < lhs.size(); i++)
        lhs[i] *= rhs[i];
    return lhs;
}
Vector &operator *= (Vector &lhs, double rhs) {
    for(unsigned i = 0; i < lhs.size(); i++)
        lhs[i] *= rhs;
    return lhs;
}
Vector &operator /= (Vector &lhs, const Vector &rhs) {
    assert(lhs.size() == rhs.size());
    for(unsigned i = 0; i < lhs.size(); i++)
        lhs[i] /= rhs[i];
    return lhs;
}
Vector &operator /= (Vector &lhs, double rhs) {
    return lhs *= 1.0 / rhs;
}
double sum(const Vector &lhs) {
    double result = 0;
    for(unsigned i = 0; i < lhs.size(); i++)
        result += lhs[i];
    return result;
}
double sum_square(const Vector &lhs) {
    double result = 0;
    for(unsigned i = 0; i < lhs.size(); i++)
        result += lhs[i] * lhs[i];
    return result;
}
double inner_product(const Vector &lhs, const Vector &rhs) {
    assert(lhs.size() == rhs.size());
    double result = 0;
    for(unsigned i = 0; i < lhs.size(); i++)
        result += lhs[i] * rhs[i];
    return result;
}

Matrix::Matrix(unsigned row, unsigned col) :
        m_rows(row), m_columns(col), m_data(new double[row * col]()) {
}
Matrix::Matrix(const Matrix &rhs) :
        m_rows(rhs.rows()), m_columns(rhs.columns()),
        m_data(new double[m_rows * m_columns]()) {
    std::copy_n(rhs.data(), rhs.size(), data());
}
Matrix::Matrix(const std::vector<std::vector<double>> &rhs) :
        m_rows(rhs.size()), m_columns(rhs.front().size()),
        m_data(new double[m_rows * m_columns]()) {
    for(unsigned i = 0; i < m_rows; i++)
        std::copy(rhs[i].begin(), rhs[i].end(), (*this)[i]);
}
Matrix &Matrix::operator = (const Matrix &rhs) {
    if(this != &rhs) {
        unsigned old_size = m_rows * m_columns;
        m_rows = rhs.rows();
        m_columns = rhs.columns();
        if(m_rows * m_columns != old_size)
            m_data.reset(new double[m_rows * m_columns]());
        std::copy_n(rhs.data(), rhs.size(), data());
    }
    return *this;
}
Matrix::~Matrix() {
}
Matrix::operator std::vector<std::vector<double>> () const {
    std::vector<std::vector<double>> v;
    v.reserve(rows());
    for(unsigned i = 0; i < rows(); i++)
        v.emplace_back((*this)[i], (*this)[i] + columns());
    return std::move(v);
}
const double *Matrix::operator [] (unsigned row) const {
    assert(row < rows());
    return data() + row * columns();
}
double *Matrix::operator [] (unsigned row) {
    assert(row < rows());
    return data() + row * columns();
}
Matrix &operator += (Matrix &lhs, const Matrix &rhs) {
    assert(lhs.rows() == rhs.rows());
    assert(lhs.columns() == rhs.columns());
    for(unsigned i = 0; i < lhs.size(); i++)
        lhs.data()[i] += rhs.data()[i];
    return lhs;
}
Matrix &operator -= (Matrix &lhs, const Matrix &rhs) {
    assert(lhs.rows() == rhs.rows());
    assert(lhs.columns() == rhs.columns());
    for(unsigned i = 0; i < lhs.size(); i++)
        lhs.data()[i] -= rhs.data()[i];
    return lhs;
}
Matrix &operator *= (Matrix &lhs, double rhs) {
    for(unsigned i = 0; i < lhs.size(); i++)
        lhs.data()[i] *= rhs;
    return lhs;
}
Vector operator * (const Matrix &lhs, const Vector &rhs) {
    assert(lhs.columns() == rhs.size());
    Vector result(lhs.rows());
    for(unsigned i = 0; i < lhs.rows(); i++)
        for(unsigned j = 0; j < lhs.columns(); j++)
            result[i] += lhs[i][j] * rhs[j];
    return std::move(result);
}
Vector operator * (const Vector &lhs, const Matrix &rhs) {
    assert(lhs.size() == rhs.rows());
    Vector result(rhs.columns());
    for(unsigned i = 0; i < rhs.columns(); i++)
        for(unsigned j = 0; j < rhs.rows(); j++)
            result[i] += lhs[j] * rhs[j][i];
    return std::move(result);
}
Matrix operator * (const Matrix &lhs, const Matrix &rhs) {
    assert(lhs.columns() == rhs.rows());
    Matrix result(lhs.rows(), rhs.columns());
    for(unsigned i = 0; i < lhs.rows(); i++)
        for(unsigned k = 0; k < lhs.columns(); k++)
            for(unsigned j = 0; j < rhs.columns(); j++)
                result[i][j] += lhs[i][k] * rhs[k][j];
    return std::move(result);
}

void reset(Model &m) {
    for(unsigned i = 0; i < m.w1().size(); i++)
        m.w1().data()[i] = 0.0;
    for(unsigned i = 0; i < m.w2().size(); i++)
        m.w2()[i] = 0.0;
}
template<typename Engine>
void randomize(Model &m, Engine &eng) {
    std::normal_distribution<double> gauss;
    for(unsigned i = 0; i < m.w1().rows(); i++)
        for(unsigned j = 0; j < m.w1().columns(); j++)
            m.w1()[i][j] = gauss(eng) / sqrt(m.features());
    for(unsigned i = 0; i < m.w2().size(); i++)
        m.w2()[i] = gauss(eng) / sqrt(m.neurons());
}

constexpr int batch_size = 10;
constexpr double learning_rate = 1e-4;
constexpr double dgamma = 0.99;
constexpr double decay_rate = 0.99;

constexpr double sigmoid(double x) {
    return 1 / (1 + exp(-x));
}

void discount_rewards(std::vector<double> &r) {
    double add = 0;
    for(int i = (int) r.size() - 1; i >= 0; i--) {
        if(r[i])
            add = 0;
        add = add * dgamma + r[i];
        r[i] = add;
    }
}
void normalize(Vector &r) {
    double mean = sum(r) / r.size();
    r -= mean;
    double stdev = std::sqrt(sum_square(r) / r.size());
    if(stdev)
        r /= stdev;
}

Learning::Learning(unsigned n, unsigned f, seed_type seed) :
        m_neurons(n), m_features(f), m_model(n, f),
        m_gradbuf(n, f), m_rmsprop(n, f), m_engine(seed),
        m_episode(0), m_saved_feature(), m_saved_mid(),
        m_saved_pd(), m_saved_reward() {
    randomize(m_model, m_engine);
}
bool Learning::play(const std::vector<double> &orig_feature) {
    Vector feature(orig_feature);
    Vector mid = m_model.w1() * feature;
    for(unsigned i = 0; i < m_neurons; i++)
        if(mid[i] < 0)
            mid[i] = 0;

    double p = sigmoid(inner_product(m_model.w2(), mid));
    bool action = std::bernoulli_distribution(p)(m_engine);

    m_saved_feature.push_back(orig_feature);
    m_saved_mid.push_back(std::vector<double>(mid));
    m_saved_pd.push_back((action ? 1.0 : 0.0) - p);

    return action;
}
void Learning::feedback(double reward) {
    m_saved_reward.push_back(reward);
}
void Learning::game_over() {
    m_episode++;

    discount_rewards(m_saved_reward);

    Matrix epx(m_saved_feature);
    Matrix eph(m_saved_mid);
    Vector epd(m_saved_pd);
    Vector epr(m_saved_reward);
    m_saved_feature.clear();
    m_saved_mid.clear();
    m_saved_pd.clear();
    m_saved_reward.clear();

    normalize(epr);
    epd *= epr;

    // policy backward
    Matrix dht(eph.columns(), epx.rows());
    for(unsigned i = 0; i < dht.rows(); i++)
        for(unsigned j = 0; j < dht.columns(); j++)
            dht[i][j] = (eph[j][i] <= 0 ? 0 : epd[j] * m_model.w2()[i]);

    m_gradbuf.w1() += dht * epx;
    m_gradbuf.w2() += epd * eph;

    if(m_episode % batch_size == 0)
        batch_work();
}

inline void decay(double &lhs, double rhs) {
    lhs = lhs * decay_rate + rhs * rhs * (1 - decay_rate);
}
void decay(Vector &lhs, const Vector &rhs) {
    assert(lhs.size() == rhs.size());
    for(unsigned i = 0; i < lhs.size(); i++)
        decay(lhs[i], rhs[i]);
}
void decay(Matrix &lhs, const Matrix &rhs) {
    assert(lhs.rows() == rhs.rows());
    assert(lhs.columns() == rhs.columns());
    for(unsigned i = 0; i < lhs.size(); i++)
        decay(lhs.data()[i], rhs.data()[i]);
}
inline void learn(double &lhs, double x, double y) {
    assert(y >= 0.0);
    lhs += learning_rate * x / (std::sqrt(y) + 1e-5);
}
void learn(Vector &lhs, const Vector &x, const Vector &y) {
    assert(lhs.size() == x.size() && lhs.size() == y.size());
    for(unsigned i = 0; i < lhs.size(); i++)
        learn(lhs[i], x[i], y[i]);
}
void learn(Matrix &lhs, const Matrix &x, const Matrix &y) {
    assert(lhs.rows() == x.rows() && x.rows() == y.rows());
    assert(lhs.columns() == x.columns() && x.columns() == y.columns());
    for(unsigned i = 0; i < lhs.size(); i++)
        learn(lhs.data()[i], x.data()[i], y.data()[i]);
}
void Learning::batch_work() {
    decay(m_rmsprop.w1(), m_gradbuf.w1());
    decay(m_rmsprop.w2(), m_gradbuf.w2());

    learn(m_model.w1(), m_gradbuf.w1(), m_rmsprop.w1());
    learn(m_model.w2(), m_gradbuf.w2(), m_rmsprop.w2());

    reset(m_gradbuf);
}

std::istream &operator >> (std::istream &in, Model &x) {
    auto gethex = [&in](double &z) {
        std::string s;
        if(in >> s)
            z = stod(s);
    };
    Matrix &w1 = x.w1();
    Vector &w2 = x.w2();

    for(unsigned i = 0; i < w1.rows(); i++)
        for(unsigned j = 0; j < w1.columns(); j++)
            gethex(w1[i][j]);
    for(unsigned i = 0; i < w2.size(); i++)
        gethex(w2[i]);
    return in;
}
std::ostream &operator << (std::ostream &out, const Model &x) {
    const Matrix &w1 = x.w1();
    const Vector &w2 = x.w2();

    out << std::hexfloat;
    for(unsigned i = 0; i < w1.rows(); i++)
        for(unsigned j = 0; j < w1.columns(); j++)
            out << w1[i][j] << (j == w1.columns() - 1 ? '\n' : ' ');
    for(unsigned i = 0; i < w2.size(); i++)
        out << w2[i] << (i == w2.size() - 1 ? '\n' : ' ');
    return out << std::defaultfloat;
}
std::istream &operator >> (std::istream &in, Learning &x) {
    return in >> x.m_model >> x.m_rmsprop;
}
std::ostream &operator << (std::ostream &out, const Learning &x) {
    return out << x.m_model << x.m_rmsprop;
}
}


