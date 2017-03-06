#ifndef LOLIPOP_LEARNING_H
#define LOLIPOP_LEARNING_H
#include <cassert>
#include <iostream>
#include <memory>
#include <random>
#include <vector>
namespace learning {
class Vector {
    unsigned m_size;
    std::unique_ptr<double[]> m_data;
public:
    Vector(unsigned size);
    Vector(const Vector &rhs);
    Vector(const std::vector<double> &rhs);
    Vector &operator = (const Vector &rhs);
    unsigned size() const { return m_size; }
    const double *data() const { return m_data.get(); }
    double *data() { return m_data.get(); }
    double operator [] (unsigned index) const;
    double &operator [] (unsigned index);
    explicit operator std::vector<double>() const;
    ~Vector();
};

class Matrix {
    unsigned m_rows, m_columns;
    std::unique_ptr<double[]> m_data;
public:
    Matrix(unsigned r, unsigned c);
    Matrix(const Matrix &rhs);
    Matrix(const std::vector<std::vector<double>> &rhs);
    Matrix &operator = (const Matrix &rhs);
    unsigned rows() const { return m_rows; }
    unsigned columns() const { return m_columns; }
    unsigned size() const { return m_rows * m_columns; }
    const double *data() const { return m_data.get(); }
    double *data() { return m_data.get(); }
    const double *operator [] (unsigned row) const;
    double *operator [] (unsigned row);
    explicit operator std::vector<std::vector<double>>() const;
    ~Matrix();
};

class Model {
    unsigned m_neurons, m_features;
    Matrix m_w1;
    Vector m_w2;
public:
    Model(unsigned n, unsigned f) :
        m_neurons(n), m_features(f), m_w1(n, f), m_w2(n) {}
    Model(const Matrix &w1, const Vector &w2) :
            m_neurons(w1.columns()), m_features(w1.rows()),
            m_w1(w1), m_w2(w2) {
        assert(w2.size() == m_neurons);
    }
    unsigned neurons() const { return m_neurons; }
    unsigned features() const { return m_features; }
    Matrix &w1() { return m_w1; }
    const Matrix &w1() const { return m_w1; }
    Vector &w2() { return m_w2; }
    const Vector &w2() const { return m_w2; }
};

class Learning {
    unsigned m_neurons, m_features;
    Model m_model, m_gradbuf, m_rmsprop;
    std::mt19937 m_engine;
    unsigned m_episode;
    std::vector<std::vector<double>> m_saved_feature, m_saved_mid;
    std::vector<double> m_saved_pd, m_saved_reward;

    using seed_type = decltype(m_engine)::result_type;
    constexpr static seed_type default_seed = decltype(m_engine)::default_seed;
public:
    Learning(unsigned n, unsigned f, seed_type seed = default_seed);
    bool play(const std::vector<double> &feature);
    void feedback(double reward);
    void game_over();
    // TODO method for display
private:
    void batch_work();
    friend std::istream &operator >> (std::istream &in, Learning &);
    friend std::ostream &operator << (std::ostream &, const Learning &);
};
std::istream &operator >> (std::istream &, Learning &);
std::ostream &operator << (std::ostream &, const Learning &);
}
#endif // LOLIPOP_LEARNING_H
