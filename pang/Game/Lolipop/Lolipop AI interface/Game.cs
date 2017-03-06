using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Motivation;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing;

namespace Lolipop_AI_interface
{
    class GameParameters
    {
        public double acceleration, velocity, location;
    }
    class Game
    {
        bool darryMode = false;
        class GameVisualizer
        {
            public void drawImage(Image bmp, int gameState, double location,Interval rangeY, Rectangle scope, Queue<GameObstacle>obstacles,double velocity,bool humanFriendly)
            {
                Color backgroundColor = Color.FromArgb(255, 255, 255)
                    ,podColor=Color.FromArgb(0,0,0)
                    ,obstacleColor=Color.FromArgb(255,0,0)
                    ,boundColor=Color.FromArgb(255,255,0);
                var locationOf = new Func<PointF, PointF>((PointF p) =>
                   {
                       float ratioX = (p.X - scope.X) / scope.Width
                       , ratioY = (p.Y - scope.Y) / scope.Height;
                       return new PointF(1+(bmp.Width-1)*ratioX,bmp.Height*ratioY);
                   });
                var rectangleOf = new Func<PointF, float, RectangleF>((PointF p, float l) =>
                    {
                        return new RectangleF(p.X - l, p.Y - l, l * 2, l * 2);
                    });
                var getRectangle = new Func<PointF, PointF, RectangleF>((PointF a, PointF b) =>
                {
                    Debug.Assert(a.X <= b.X && a.Y <= b.Y);
                    return new RectangleF(a.X, a.Y, b.X - a.X, b.Y - a.Y);
                });
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(backgroundColor);
                    g.DrawLine(new Pen(boundColor,1), locationOf(new PointF(scope.X, rangeY.maximum)), locationOf(new PointF(scope.X + scope.Width, rangeY.maximum)));
                    g.DrawLine(new Pen(boundColor,1), locationOf(new PointF(scope.X, rangeY.minimum)), locationOf(new PointF(scope.X + scope.Width, rangeY.minimum)));
                    int x = 0;
                    foreach (var o in obstacles)
                    {
                        {
                            var r = getRectangle(locationOf(new PointF(x + o.distance, rangeY.minimum)), locationOf(new PointF(x + o.distance + o.width, o.lower_y)));
                            g.FillRectangle(new SolidBrush(obstacleColor), r.X, r.Y, r.Width, r.Height);
                        }
                        {
                            var r = getRectangle(locationOf(new PointF(x + o.distance, o.upper_y)), locationOf(new PointF(x + o.distance + o.width, rangeY.maximum)));
                            g.FillRectangle(new SolidBrush(obstacleColor), r.X, r.Y, r.Width, r.Height);
                        }
                        x += o.distance + o.width;
                    }
                    {
                        var r = rectangleOf(locationOf(new PointF(0, (float)location)),humanFriendly?10: 1);
                        g.FillRectangle(new SolidBrush(podColor), r.X, r.Y, r.Width, r.Height);
                    }
                    {
                        PointF o = new PointF(0, (float)((scope.Height / 2.0) / scope.Height * bmp.Height));
                        PointF p = new PointF(0, (float)((scope.Height / 2.0 + velocity * 50.0) / scope.Height * bmp.Height));
                        //var r = rectangleOf(p, 1);
                        g.DrawLine(new Pen(podColor, 1), o,p);
                    }
                    //g.DrawLine(new Pen(Color.FromArgb(0, 0, 0)), new Point(), new Point(bmp.Width, bmp.Height));
                    if (gameState == 0) g.DrawString("Game Over", new Font("Consolas", 40, FontStyle.Bold), new SolidBrush(Color.FromArgb(0, 0, 255)), new PointF(0.2f * bmp.Width, 0.2f * bmp.Height));
                    g.Flush();
                }
            }
        }
        GameVisualizer visualizer = new GameVisualizer();
        public void drawImage(Image bmp,bool humanFriendly=false)
        {
            visualizer.drawImage(bmp,gameState, location,rangeY, new Rectangle(-1, rangeY.minimum - 1, obstacleCount * obstacleDistance.maximum + 3, rangeY.maximum - rangeY.minimum + 3), obstacles,velocity, humanFriendly);
        }
        class GameObstacle
        {
            public int distance, width, upper_y, lower_y;
            public GameObstacle(Game game)
            {
                distance = game.obstacleDistance.getRandomValue();
                width = game.obstacleWidth.getRandomValue();
                lower_y = game.obstacleY.getRandomValue();
                upper_y = lower_y + game.obstacleHeight.getRandomValue();
            }
        }
        Interval rangeY = new Interval(0, 1000);
        Interval obstacleDistance = new Interval(150, 250);
        Interval obstacleWidth = new Interval(100, 200);
        Interval obstacleY = new Interval(0, 500);
        Interval obstacleHeight = new Interval(250, 500);
        double gravity = -0.1,liftForce=0.1;
        double max_upward_speed = 15, alpha = 0.3;
        int obstacleCount = 1;
        public MyTableLayoutPanel controlPanel;
        MyInputField generalSettings;
        MyCheckBox imageFeedBack;
        MyCheckBox showImageFeedBack;
        Size imageFeedBackSize = new Size(10,40); //new Size(586, 705);
        public Game()
        {
            controlPanel = new MyTableLayoutPanel(3, 1, "AAS300", "A");
            {
                {
                    imageFeedBack = new MyCheckBox("Image Feed Back");
                    imageFeedBack.Checked = true;
                    controlPanel.AddControl(imageFeedBack, 0, 0);
                }
                {
                    showImageFeedBack = new MyCheckBox("Show Image Feed Back");
                    showImageFeedBack.Checked = false;
                    showImageFeedBack.CheckedChanged += ShowImageFeedBack_CheckedChanged;
                    controlPanel.AddControl(showImageFeedBack, 0, 0);
                }
                {
                    Panel pnl = new Panel();
                    {
                        pnl.Dock = DockStyle.Fill;
                        pnl.AutoScroll = true;
                        {
                            generalSettings = new MyInputField();
                            generalSettings.Dock = DockStyle.Top;
                            generalSettings.AddField("Image Feed Back Width", imageFeedBackSize.Width.ToString()).TextChanged += (o, e) => { try { imageFeedBackSize.Width = int.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
                            generalSettings.AddField("Image Feed Back Height", imageFeedBackSize.Height.ToString()).TextChanged += (o, e) => { try { imageFeedBackSize.Height = int.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
                            generalSettings.AddField("Port", SocketHandler.port.ToString()).TextChanged += (o, e) => { try { SocketHandler.port = int.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
                            generalSettings.AddField("darryMode", darryMode.ToString()).TextChanged += (o, e) => { try { darryMode = bool.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
                            generalSettings.AddField("max_upward_speed", max_upward_speed.ToString()).TextChanged += (o, e) => { try { max_upward_speed = double.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
                            generalSettings.AddField("alpha", alpha.ToString()).TextChanged += (o, e) => { try { alpha = double.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
                            generalSettings.AddField("重利", gravity.ToString()).TextChanged += (o, e) => { try { gravity = double.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
                            generalSettings.AddField("升力", liftForce.ToString()).TextChanged += (o, e) => { try { liftForce = double.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
                            generalSettings.AddField("可見障礙物數量", obstacleCount.ToString()).TextChanged += (o, e) => { try { obstacleCount = int.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
                            generalSettings.AddField("存活區的範圍", rangeY.ToString()).TextChanged += (o, e) => { try { rangeY = Interval.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
                            generalSettings.AddField("障礙物距離的範圍", obstacleDistance.ToString()).TextChanged += (o, e) => { try { obstacleDistance = Interval.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
                            generalSettings.AddField("障礙物寬度(通過時間)的範圍", obstacleWidth.ToString()).TextChanged += (o, e) => { try { obstacleWidth = Interval.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
                            generalSettings.AddField("障礙物通道底部高度的範圍", obstacleY.ToString()).TextChanged += (o, e) => { try { obstacleY = Interval.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
                            generalSettings.AddField("障礙物通道寬度的範圍", obstacleHeight.ToString()).TextChanged += (o, e) => { try { obstacleHeight = Interval.Parse((o as MyTextBox).Text); } catch (Exception) { MessageBox.Show("格式不正確"); } };
                            pnl.Controls.Add(generalSettings);
                        }
                    }
                    controlPanel.AddControl(pnl, 2, 0);
                }
            }
            Reset();
            Update(true);
        }
        private void ShowImageFeedBack_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as MyCheckBox).Checked)
            {
                Form f = new Form();
                f.ControlBox = false;
                f.Show();
                f.BackgroundImageLayout = ImageLayout.None;
                ImageFeedBackProduced += (Bitmap bmp) =>
                 {
                     f.Invoke(new Action(() => { f.Width = bmp.Width + 20;f.Height = bmp.Height + 50; f.BackgroundImage = bmp; }));
                 };
            }
            else ImageFeedBackProduced = null;
        }

        private static Random random = new Random();
        int newReward;
        int newDone
        {
            get { return gameState; }
        }
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
        Queue<GameObstacle> obstacles = new Queue<GameObstacle>();
        int gameState=0;
        double location,velocity,acceleration;
        public void Reset()
        {
            gameState = 1;
            newReward = 0;
            location = (double)(rangeY.minimum + rangeY.maximum) / 2.0;
            velocity = 0.0;
            obstacles.Clear();
            for (int i = 0; i < obstacleCount; i++) obstacles.Enqueue(new GameObstacle(this));
        }
        public void Update(bool keyState)
        {
            if (gameState == 0) return;
            newReward = 0;

            if (darryMode)
            {
                if (keyState) velocity = velocity * alpha + max_upward_speed * (1.0 - alpha);
                else
                {
                    acceleration = gravity;
                    velocity += acceleration;
                }
            }
            else
            {
                acceleration = keyState ? liftForce : gravity;
                velocity += acceleration;
            }
            location += velocity;

            if (obstacleCount > 0 && obstacles.First().width == 0)
            {
                obstacles.Dequeue();
                newReward++;
            }
            while (obstacles.Count < obstacleCount) obstacles.Enqueue(new GameObstacle(this));
            Debug.Assert(obstacles.Count > 0);
            if (obstacles.First().distance == 0) obstacles.First().width--;
            else obstacles.First().distance--;

            if ((location < rangeY.minimum || rangeY.maximum < location) || (obstacles.First().distance == 0 && (location < obstacles.First().lower_y || obstacles.First().upper_y < location)))
            {
                gameState = 0;
                newReward = -1;
            }
        }
        delegate void ImageFeedBackProducedEventHandler(Bitmap bmp);
        event ImageFeedBackProducedEventHandler ImageFeedBackProduced;
        public string getFeedBack()
        {
            if (imageFeedBack.Checked)
            {
                Bitmap bmp = new Bitmap(imageFeedBackSize.Width,imageFeedBackSize.Height);
                drawImage(bmp);
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                StringBuilder answer = new StringBuilder();
                unsafe
                {
                    byte* p = (byte*)bd.Scan0.ToPointer();
                    for (int i = 0; i < bmp.Height; i++)
                    {
                        for (int j = 0; j < bmp.Width; j++)
                        {
                            answer.Append(p[j * 4 + 2]);
                            answer.Append(' ');
                            answer.Append(p[j * 4 + 1]);
                            answer.Append(' ');
                            answer.Append(p[j * 4 + 0]);
                            answer.Append(' ');
                        }
                        p += bd.Stride;
                    }
                }
                bmp.UnlockBits(bd);
                ImageFeedBackProduced?.Invoke(bmp);
                answer.Remove(answer.Length - 1, 1);
                answer.Append(' ');
                answer.Append(newReward);
                answer.Append(' ');
                answer.Append(newDone);
                return answer.ToString();
            }
            else
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
}
