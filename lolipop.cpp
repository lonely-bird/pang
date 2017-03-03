#include "emulator.h"
#include "learning.h"
#include <algorithm>
#include <cstdio>
#include <utility>
#include <vector>

std::valarray<double> to_features(const emulator::Image &img) {
    std::valarray<double> r(img.width() * img.height() * 2);
    for(int y = 0; y < img.height(); y++)
        for(int x = 0; x < img.width(); x++) {
            emulator::Color c = img[y][x];
            r[(y * img.width() + x) * 2 + 0] = c.red / 255.0;
            r[(y * img.width() + x) * 2 + 1] = c.green / 255.0;
        }
    return std::move(r);
}

int main() {
    constexpr int neuron_size = 200;
    constexpr int feature_size = 800;
    constexpr unsigned max_iteration = 1000000;
    emulator::Emulator emulator;
    learning::Learning learning(neuron_size, feature_size/*, seed*/);
    for(unsigned iteration = 0; iteration < max_iteration; iteration++) {
        int score = 0;
        while(!emulator.game_over()) {
            auto image = emulator.observe();
            auto features = to_features(image);
            auto action = learning.play(features);
            emulator.update(action);

            int new_score = emulator.score();
            learning.feedback(new_score - score);
            score = new_score;
        }
        emulator.reset();
        learning.game_over();

        printf("Iteration %d: Score=%d\n", iteration, score);
    }
}
