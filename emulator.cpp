#include "emulator.h"
#include <random>
#include <algorithm>
#include <cassert>
#include <utility>
namespace emulator {
void clip(int &val, int min, int max) {
    if(val < min)
        val = min;
    else if(val > max)
        val = max;
}
void clip(int &val1, int &val2, int min, int max) {
    if(val1 < min)
        val1 = min;
    if(val2 > max)
        val2 = max;
}

Image::Image(int w, int h, Color c) :
    m_width(w), m_height(h), m_data(w * h, c) {}
const Color *Image::operator [] (int y) const {
    return m_data.data() + y * width();
}
Color *Image::operator [] (int y) {
    return m_data.data() + y * width();
}
void Image::fill_rectangle(int x1, int y1, int x2, int y2, Color color) {
    clip(x1, x2, 0, width() - 1);
    clip(y1, y2, 0, height() - 1);
    for(int x = x1; x <= x2; x++)
        for(int y = y1; y <= y2; y++)
            m_data[y * width() + x] = color;
}

constexpr Interval<int> range_y { 0, 1000 };
constexpr Interval<int> obstacle_distance { 100, 200 };
constexpr Interval<int> obstacle_width { 10, 20 };
constexpr Interval<int> obstacle_y { 0, 500 };
constexpr Interval<int> obstacle_height { 250, 500 };
constexpr double gravity = -0.1, lift_force = 0.1;
constexpr double max_upward_speed = 15, alpha = 0.3;
constexpr int obstacle_count = 1;
constexpr int image_width = 10, image_height = 40;
constexpr Color background_color {255, 255, 255};
constexpr Color pod_color {0, 0, 0};
constexpr Color obstacle_color {255, 0, 0};

template<typename Engine>
Obstacle random_obstacle(Engine &m_engine) {
    Obstacle ob;
    ob.distance = obstacle_distance.random(m_engine);
    ob.width = obstacle_width.random(m_engine);
    ob.y.min = obstacle_y.random(m_engine);
    ob.y.max = ob.y.min + obstacle_height.random(m_engine);
    return ob;
}

Emulator::Emulator() {
    reset();
}
bool Emulator::game_over() const {
    return !m_alive;
}
int Emulator::score() const {
    return m_score;
}
Image Emulator::observe() const {
    static_assert(obstacle_count == 1);
    constexpr int vw = obstacle_distance.max;
    constexpr int vh = range_y.max - range_y.min;
    constexpr int w = image_width;
    constexpr int h = image_height;
    Image img(w, h, background_color);

    // draw m_obstacles
    int offset = 0;
    for(Obstacle o : m_obstacles) {
        int x1 = (offset + o.distance) * w / vw;
        int x2 = (offset + o.distance + o.width) * w / vw;
        int y1 = o.y.min * h / vh;
        int y2 = o.y.max * h / vh;

        img.fill_rectangle(x1, 0, x2, y1, obstacle_color);
        img.fill_rectangle(x1, y2, x2, h, obstacle_color);

        offset += o.distance + o.width;
    }

    // draw the pod
    {
        int y = m_location * h / vh;
        img.fill_rectangle(0, y, 0, y, pod_color);
    }

#if 0
    // draw m_velocity
    {
        int y = h / 2 + (m_velocity * 50) * h / vh;
        img.fill_rectangle(0, y, 0, y, pod_color);
    }
#endif

    return std::move(img);
}
void Emulator::reset() {
    m_alive = true;
    m_score = 0;
    m_location = (range_y.min + range_y.max) * 0.5;
    m_velocity = 0.0;
    m_obstacles.clear();
    for(int i = 0; i < obstacle_count; i++)
        m_obstacles.push_back(random_obstacle(m_eng));
}
void Emulator::update(bool key_pressed) {
    if(!m_alive)
        return;

#if 0
    double acceleration = key_pressed ? lift_force : gravity;
    m_velocity += acceleration;
    m_location += m_velocity;
#else
    m_location += key_pressed ? 5 : -5;
#endif

    while(!m_obstacles.empty() && m_obstacles.front().width <= 0) {
        m_obstacles.erase(m_obstacles.begin());
        m_score++;
    }
    while(m_obstacles.size() < obstacle_count)
        m_obstacles.push_back(random_obstacle(m_eng));

    auto &ob = m_obstacles.front();
    if(ob.distance == 0)
        ob.width--;
    else
        ob.distance--;

    if (!range_y.contains(m_location)) {
        m_alive = false;
        m_score--;
    } else if (ob.distance == 0 && !ob.y.contains(m_location)) {
        m_alive = false;
        m_score--;
    }
}
}
