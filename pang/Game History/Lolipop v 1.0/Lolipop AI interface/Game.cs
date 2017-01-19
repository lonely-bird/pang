using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Motivation;
using System.Windows.Forms;

namespace Lolipop_AI_interface
{
    class Game
    {
        Interval rangeY = new Interval(0, 1000);
        Interval obstacleDistance = new Interval(1000, 2000);
        Interval obstacleWidth = new Interval(100, 200);
        Interval obstacleY = new Interval(0, 500);
        Interval obstacleHeight = new Interval(250, 500);
        double gravity = -0.1;
        int obstacleCount = 1;

        public MyInputField inputField = new MyInputField();
        public Game()
        {
            inputField.AddField("重力", gravity.ToString()).TextChanged += (o, e) => { try { gravity = double.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
            inputField.AddField("可見障礙物數量", obstacleCount.ToString()).TextChanged+=(o,e)=> { try { obstacleCount = int.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
            inputField.AddField("存活區的範圍", rangeY.ToString()).TextChanged += (o, e) => { try { rangeY = Interval.Parse((o as MyTextBox).Text);  } catch (Exception) { MessageBox.Show("格式不正確"); }};
            inputField.AddField("障礙物距離的範圍", obstacleDistance.ToString()).TextChanged += (o, e) => { try { obstacleDistance = Interval.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
            inputField.AddField("障礙物寬度(通過時間)的範圍", obstacleWidth.ToString()).TextChanged += (o, e) => { try { obstacleWidth = Interval.Parse((o as MyTextBox).Text);  } catch (Exception) { MessageBox.Show("格式不正確"); }};
            inputField.AddField("障礙物通道底部高度的範圍", obstacleY.ToString()).TextChanged += (o, e) => { try { obstacleY = Interval.Parse((o as MyTextBox).Text);  } catch (Exception) { MessageBox.Show("格式不正確"); }};
            inputField.AddField("障礙物通道寬度的範圍", obstacleHeight.ToString()).TextChanged += (o, e) => { try { obstacleHeight = Interval.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
        }
        private static Random random = new Random();
        class Interval
        {
            public int minimum, maximum;
            public Interval(int mn, int mx)
            {
                Debug.Assert(mn <= mx);
                minimum = mn;
                maximum = mx;
            }
            public int getRandomValue()
            {
                return random.Next(minimum, maximum + 1);
            }
            public override string ToString()
            {
                return minimum.ToString() + "," + maximum.ToString();
            }
            public static Interval Parse(string s)
            {
                string[] a = s.Split(',');
                Debug.Assert(a.Length == 2);
                int mn=int.Parse(a[0]), mx=int.Parse(a[1]);
                return new Interval(mn, mx);
            }
        }
        class Obstacle
        {
            public int distance, width,upper_y,lower_y;
            public Obstacle(Game game)
            {
                distance = game.obstacleDistance.getRandomValue();
                width = game.obstacleWidth.getRandomValue();
                lower_y = game.obstacleY.getRandomValue();
                upper_y = lower_y + game.obstacleHeight.getRandomValue();
            }
        }
        Queue<Obstacle> obstacles = new Queue<Obstacle>();
        int gameState=0;
        double location,velocity;
        public void Reset()
        {
            gameState = 1;
            location = (double)(rangeY.minimum + rangeY.maximum) / 2.0;
            velocity = 0.0;
            obstacles.Clear();
            obstacles.Enqueue(new Obstacle(this));
        }
        public void Update(bool keyState)
        {
            if (gameState == 0) return;

            velocity += keyState ? -gravity : gravity;
            location += velocity;

            if (obstacleCount > 0 && obstacles.First().width == 0) obstacles.Dequeue();
            while (obstacles.Count < obstacleCount) obstacles.Enqueue(new Obstacle(this));
            Debug.Assert(obstacles.Count > 0);
            if (obstacles.First().distance == 0) obstacles.First().width--;
            else obstacles.First().distance--;

            if (location < rangeY.minimum || rangeY.maximum < location) gameState = 0;
            if (obstacles.First().distance == 0 && (location < obstacles.First().lower_y || obstacles.First().upper_y < location)) gameState = 0;
        }
        public string getFeedBack()
        {
            StringBuilder answer = new StringBuilder();
            answer.Append(gameState);
            answer.Append(' ');
            answer.Append(location);
            answer.Append(' ');
            answer.Append(velocity);
            answer.Append(' ');
            answer.Append(obstacleCount);
            for (int i = 0; i < obstacleCount; i++)
            {
                answer.Append(' ');
                answer.Append(obstacles.ElementAt(i).distance);
                answer.Append(' ');
                answer.Append(obstacles.ElementAt(i).width);
                answer.Append(' ');
                answer.Append(obstacles.ElementAt(i).lower_y);
                answer.Append(' ');
                answer.Append(obstacles.ElementAt(i).upper_y);
            }
            return answer.ToString();
        }
    }
}
