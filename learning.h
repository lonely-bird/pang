#ifndef LOLIPOP_LEARNING_H
#define LOLIPOP_LEARNING_H
#include <algorithm>
#include <cassert>
#include <random>
#include <utility>
#include <valarray>
#include <vector>
namespace learning {
template<typename T>
class Matrix {
    int m_rows, m_columns;
    std::valarray<T> m_data;
    struct CRow {
        const std::valarray<T> &ref;
        int offset;
        T operator [] (int column) const {
            return ref[offset + column];
        }
    };
    struct Row {
        std::valarray<T> &ref;
        int offset;
        T operator [] (int column) const {
            return ref[offset + column];
        }
        T &operator [] (int column) {
            return ref[offset + column];
        }
    };
public:
    Matrix(int r, int c) : m_rows(r), m_columns(c), m_data(r * c) {}
    int columns() const {
        return m_columns;
    }
    int rows() const {
        return m_rows;
    }
    const std::valarray<T> &data() const {
        return m_data;
    }
    std::valarray<T> &data() {
        return m_data;
    }
    CRow operator [] (int row) const {
        assert(0 <= row);
        assert(row < m_rows);
        return CRow { m_data, row * m_columns };
    }
    Row operator [] (int row) {
        assert(0 <= row);
        assert(row < m_rows);
        return Row { m_data, row * m_columns };
    }
    Matrix &operator = (T rhs) {
        m_data = rhs;
        return *this;
    }
};

class Model {
    int m_neurons, m_features;
    Matrix<double> m_w1;
    std::valarray<double> m_w2;
public:
    Model(int n, int f) : m_neurons(n), m_features(f), m_w1(n, f), m_w2(n) {}
    Model(const Matrix<double> &w1, const std::valarray<double> &w2) :
            m_neurons(w1.columns()), m_features(w1.rows()),
            m_w1(w1), m_w2(w2) {
        assert((int) w2.size() == m_neurons);
    }
    int neurons() const {
        return m_neurons;
    }
    int features() const {
        return m_features;
    }
    Matrix<double> &w1() {
        return m_w1;
    }
    const Matrix<double> &w1() const {
        return m_w1;
    }
    std::valarray<double> &w2() {
        return m_w2;
    }
    const std::valarray<double> &w2() const {
        return m_w2;
    }
    void reset();
    template<typename Engine>
    void randomize(Engine &eng) {
        std::normal_distribution<double> gauss;
        for(int i = 0; i < m_neurons; i++)
            for(int j = 0; j < m_features; j++)
                m_w1[i][j] = gauss(eng) / sqrt(m_features);
        for(int i = 0; i < m_neurons; i++)
            m_w2[i] = gauss(eng) / sqrt(m_neurons);
    }
};

class Learning {
    int m_neurons, m_features;
    Model m_model, m_gradbuf, m_rmsprop;
    std::mt19937 m_engine;
    int m_episode = 0;
    std::vector<std::valarray<double>> m_saved_feature, m_saved_mid;
    std::vector<double> m_saved_pd, m_saved_reward;

    using seed_type = decltype(m_engine)::result_type;
    constexpr static seed_type default_seed = decltype(m_engine)::default_seed;
public:
    Learning(int n, int f, seed_type seed = default_seed);
    // TODO method for load/save
    bool play(std::valarray<double> feature);
    void feedback(double reward);
    void game_over();
    // TODO method for display
private:
    void batch_work();
};
}
#endif // LOLIPOP_LEARNING_H
