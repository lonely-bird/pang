<a name="___top"></a>
# Pang
*A Deep Reinforcement Learning Project of Solving a Flappy Bird/Lolipop-Like Game*

* * *
*   [Socket Format](#Socket_Format)
*   [Global Variables](#Global_Variables)
*   [How to Save a Model](#How_to_Save_a_Model)
*   [How to Load a Model](#How_to_Load_a_Model)

* * *
<a name="Socket_Format"></a>      []({{{1)
# [Socket Format &#9650;](#___top "click to go to top of document")

**AI Socket Format**
<pre>
void render():
    Render the current status onto the screen
    Will be useful when preparing for the final presentation (e.g. call render() only when our AI performs well)
    No need to implement this as for now

tuple<vector<int>,int,bool> step(int action):
    action = 0 : don't press, 1 : press
    
    observation := a vector<int> of "simplified" pixels
        1. observation.size() == D (see "pang.h" to modify D)
        2. every pixel is either 0 or 1 (i.e. black or white)
        3. the 2-d screen is compressed into an 1-d vector<int>
    
    reward = just passed an obstacle : 1, normal: 0, dead: -1
    done = player is dead (game over) -> true, still playing -> false
    
    return make_tuple(observation,reward,done)

vector<int> reset():
    reset the game
    return the initial observation
</pre>

**Game Socket Format**
<pre>
TODO: Make BY do this.
</pre>

[](1}}})
<a name="Global_Variables"></a>      []({{{1)
# [Global Variables &#9650;](#___top "click to go to top of document")

**AI Global Variables**
<pre>
const int H = 200;// quantity of hidden neurons
const int batch_size = 10; // per how many episodes does the model update?
const double learning_rate = 1e-4;// higher -> learns fast, lower -> learns more stably
const double dgamma = 0.99; // discount factor for reward
const double decay_rate = 0.99; // decay factor for RMSProp leaky sum of grad^2
const bool resume = false; // set this to true to load previous model
const bool render = false; // set this to true to write onto screen
const int D = 20 * 20; // size of observation
</pre>

**Game Global Variables**
<pre>
TODO: Make BY do this.
</pre>

[](1}}})
<a name="How_to_Save_a_Model"></a>      []({{{1)
# [How to Save a Model &#9650;](#___top "click to go to top of document")
The AI (002.cpp) auto-saves the under-training model per 10 batches.
The file name is "model.sav".

[](1}}})
<a name="How_to_Load_a_Model"></a>      []({{{1)
# [How to Load a Model &#9650;](#___top "click to go to top of document")
Step 1: Name the model as "model.sav"
Step 2: Put it in the same directory with the AI (002.cpp)
Step 3: Set "resume = true" in "pang.h"
Step 4: Good to go!

[](1}}})
