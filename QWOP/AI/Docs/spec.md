void render():
    same as eric.py

tuple<vector<int>,int,bool> step(int action):
    action = 0 : don't press, 1 : press
    
    observation = a vector<int> of "simplified" pixels
    observation is sampled such that:
        1. width * depth == D (see "pang.h" to modify D)
        2. every pixel is either 0 or 1 (i.e. black or white)
        3. the 2-d screen is compressed into the 1-d vector<int>
    
    reward = just passed an obstacle : 1, normal: 0, dead: -1
    done = player is dead (game over) -> true, still playing -> false
    
    return make_tuple(observation,reward,done)

vector<int> reset():
    reset the game
    return the initial observation
