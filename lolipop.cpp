#include "emulator.h"
#include "learning.h"
#include <algorithm>
#include <cstdio>
#include <cstring>
#include <ctime>
#include <chrono>
#include <map>
#include <ncurses.h>
#include <fstream>
#include <string>
#include <thread>
#include <utility>
#include <vector>

std::vector<double> to_features(const emulator::Image &img) {
    std::vector<double> r;
    for(int y = 0; y < img.height(); y++)
        for(int x = 0; x < img.width(); x++) {
            emulator::Color c = img[y][x];
            r.push_back(c.red / 255.0);
            r.push_back(c.green / 255.0);
        }
    return std::move(r);
}

void prepare_live(WINDOW *w) {
    box(w, 0, 0);
    if(!has_colors()) {
        mvwprintw(w, 1, 1, "Your terminal does not support color.");
        wrefresh(w);
        return;
    }
    use_default_colors();
    start_color();
    init_pair(1, COLOR_RED, COLOR_RED);
    init_pair(2, COLOR_WHITE, COLOR_WHITE);
    init_pair(3, COLOR_BLACK, COLOR_BLACK);
    wrefresh(w);
}

void paint_image(WINDOW *w, const emulator::Image &img) {
    if(!has_colors())
        return;

    constexpr emulator::Color red { 255, 0, 0 };
    constexpr emulator::Color white { 255, 255, 255 };
    constexpr emulator::Color black { 0, 0, 0 };
    for(int y = 0; y < img.height(); y++)
        for(int x = 0; x < img.width(); x++) {
            emulator::Color c = img[y][x];
            int color_pair;
            if(c == red) {
                color_pair = 1;
            } else if(c == white) {
                color_pair = 2;
            } else if(c == black) {
                color_pair = 3;
            } else {
                assert(false);
            }
            mvwaddch(w, x + 1, y + 1, ' ' | COLOR_PAIR(color_pair));
        }
    wrefresh(w);
}

std::string make_save_name() {
    std::string s("model-");
    s += std::to_string(std::time(nullptr));
    s += ".sav";
    return s;
}

int main(int argc, char **argv) {
    std::string load_name;
    std::string save_name = make_save_name();

    for(int i = 1; i < argc; ) {
        const char *s = argv[i];
        auto is = [s](const char *t) {
            return std::strcmp(s, t) == 0;
        };
        if(is("-l") || is("--load")) {
            if(i + 1 >= argc) {
                std::cerr << "Argument expected after " << argv[i] << std::endl;
                return 1;
            }
            load_name = argv[i + 1];
            i += 2;
        } else if(is("-s") || is("--save")) {
            if(i + 1 >= argc) {
                std::cerr << "Argument expected after " << argv[i] << std::endl;
                return 1;
            }
            save_name = argv[i + 1];
            i += 2;
        } else {
            std::cerr << "Unknown argument " << argv[i] << std::endl;
            return 1;
        }
    }

    initscr();
    constexpr int neuron_size = 200;
    constexpr int feature_size = 800;
    constexpr unsigned max_iteration = std::numeric_limits<unsigned>::max();
    constexpr unsigned save_iteration = 100;
    emulator::Emulator emulator;
    learning::Learning learning(neuron_size, feature_size/*, seed*/);
    std::map<int, int> score_stat;

    if(!load_name.empty()) {
        std::ifstream(load_name) >> learning;
    }

    refresh();
    WINDOW *w_info = newwin(LINES, COLS - 42, 0, 0);
    wrefresh(w_info);

    WINDOW *w_live = newwin(12, 42, 0, COLS - 42);
    prepare_live(w_live);

    for(unsigned iteration = 0; iteration < max_iteration; iteration++) {
        mvwprintw(w_info, 0, 0, "Iteration %d\n", iteration);
        wrefresh(w_info);

        int score = 0;
        while(!emulator.game_over()) {
            auto image = emulator.observe();
            auto features = to_features(image);
            auto action = learning.play(features);
            emulator.update(action);

            int new_score = emulator.score();
            learning.feedback(new_score - score);
            score = new_score;

            paint_image(w_live, image);

            // std::this_thread::sleep_for(std::chrono::milliseconds(17));
        }
        emulator.reset();
        learning.game_over();

        score_stat[score]++;
        wmove(w_info, 2, 0);
        for(auto p : score_stat)
            wprintw(w_info, "Scored %d : %d\n", p.first, p.second);
        wrefresh(w_info);

        if((iteration + 1) % save_iteration == 0)
            std::ofstream(save_name) << learning;
    }
    endwin();
}
