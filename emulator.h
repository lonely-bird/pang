#ifndef LOLIPOP_EMULATOR_H
#define LOLIPOP_EMULATOR_H
#include <vector>
#include <random>
namespace emulator {
struct Color {
    unsigned char red, green, blue;
    bool operator == (const Color &rhs) const {
        return red == rhs.red && green == rhs.green && blue == rhs.blue;
    }
};
class Image {
    int m_width, m_height;
    std::vector<Color> m_data;
public:
    Image(int w, int h, Color color);
    int width() const { return m_width; }
    int height() const { return m_height; }
    const Color *operator [] (int y) const;
    Color *operator [] (int y);
    void draw_line(int x1, int y1, int x2, int y2, Color color);
    void fill_rectangle(int x1, int y1, int x2, int y2, Color color);
};
template<typename T>
struct Interval {
    T min, max;
    bool contains(T x) const {
        return min <= x && x <= max;
    }
    template<typename Engine>
    T random(Engine &engine) const {
        return std::uniform_int_distribution<T>(min, max)(engine);
    }
};
struct Obstacle {
    int distance, width;
    Interval<int> y; 
};
class Emulator {
    bool m_alive;
    int m_score;
    std::vector<Obstacle> m_obstacles;
    double m_location, m_velocity;
    std::mt19937 m_eng;
public:
    Emulator();
    bool game_over() const;
    Image observe() const;
    int score() const;
    void reset();
    void update(bool key_pressed);
};
}
#endif // LOLIPOP_EMULATOR_H
