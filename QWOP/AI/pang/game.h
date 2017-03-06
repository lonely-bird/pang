#pragma once
#include<cassert>
#include<deque>
using namespace std;
class Random
{
private:
	unsigned long long seed = 0x20170226;
public:Random() {}
private:unsigned long long Next()
{
	seed *= 0xdefaced;
	seed += 0x164952;
	return seed += seed >> 20;
}
public: int Next(int mn, int mx)
{
	auto v = Next();
	v %= (mx - mn + 1);
	return (int)(mn + v);
}
};
class Game
{
	const bool imageFeedBackChecked = true;
	bool darryMode = false;
	class GameObstacle
	{
	public:
		int distance, width, upper_y, lower_y;
		GameObstacle(Game game)
		{
			distance = game.obstacleDistance.getRandomValue();
			width = game.obstacleWidth.getRandomValue();
			lower_y = game.obstacleY.getRandomValue();
			upper_y = lower_y + game.obstacleHeight.getRandomValue();
		}
	};
	class Size
	{
	public:
		int Width, Height;
		Size(int width, int height) :Width(width), Height(height) {}
	};
	class Interval
	{
	public: int minimum, maximum;
	public: Interval(int mn, int mx)
	{
		assert(mn <= mx);
		minimum = mn;
		maximum = mx;
	}
	public: int getRandomValue()
	{
		return random.Next(minimum, maximum + 1);
	}
	};
	class Rectangle
	{
	public:int X, Y, Width, Height;
	public:Rectangle(int x,int y,int w,int h):X(x),Y(y),Width(w),Height(h){}
	};
	class Color
	{
	public:short R, G, B;
	public:Color(int r,int g,int b):R(r),G(g),B(g){}
	};
	class PointF
	{
	public:float X, Y;
	public:PointF(float x, float y):X(x),Y(y){}
	};
	class RectangleF
	{
	public:float X, Y, Width, Height;
	public:RectangleF(float x, float y, float w, float h) :X(x), Y(y), Width(w), Height(h) {}
	};
	class Pen
	{
	public:Color c;
	public:int w;
	public:Pen(Color _c, int _w) :c(_c), w(_w) {}
	};
	class Bitmap
	{
	private:short *s;
	public: int Width, Height;
	public:Bitmap(int w, int h) :Width(w), Height(h) { /*printf("w=%d,h=%d\n", w, h);*/ s = new short[w*h * 4]; }
		   //~Bitmap() { delete[] s; }
	public:short *Scan0() { return s; }
	public:int Stride() { return Width * 4; }
	public:void Clear(Color c)
	{
		for (int i = 0; i < Height; i++)for (int j = 0; j < Width; j++)
		{
			int k = (i*Width + j) * 4;
			s[k + 0] = c.B;
			s[k + 1] = c.G;
			s[k + 2] = c.R;
			s[k + 3] = 255;
		}
	}
	private:void DrawDot(Pen p, int j, int i)
	{
		assert(p.w == 1);
		if (j < 0)j = 0;
		if (j >= Width)j = Width - 1;
		if (i < 0)i = 0;
		if (i >= Height)i = Height - 1;
		assert(0 <= j&&j < Width);
		assert(0 <= i&&i < Height);
		int k = (i*Width + j) * 4;
		s[k + 0] = p.c.B;
		s[k + 1] = p.c.G;
		s[k + 2] = p.c.R;
		s[k + 3] = 255;
	}
	public:void DrawRectangle(Pen p, int x, int y, int w, int h)
	{
		DrawLine(p, PointF(x, y), PointF(x + Width - 1, y));
		DrawLine(p, PointF(x, y), PointF(x, y + Height - 1));
		DrawLine(p, PointF(x + Width - 1, y), PointF(x + Width - 1, y + Height - 1));
		DrawLine(p, PointF(x, y + Height - 1), PointF(x + Width - 1, y + Height - 1));
	}
	public:void DrawLine(Pen p, PointF a,PointF b)
	{
		int x1 = (int)(a.X + 0.5), y1 = (int)(a.Y + 0.5), x2 = (int)(b.X + 0.5), y2 = (int)(b.Y + 0.5);
		if (x1 == x2)
		{
			for (int i = y1; i <= y2; i++)DrawDot(p, x1, i);
		}
		else if (y1 == y2)
		{
			for (int i = x1; i <= x2; i++)DrawDot(p, i, y1);
		}
		else assert(0);
	}
	};
	class GameVisualizer
	{
	public: void drawImage(Bitmap bmp, int gameState, double location, Interval rangeY, Rectangle scope, deque<GameObstacle>obstacles)
		{
			Color backgroundColor = Color(255, 255, 255)
				, podColor = Color(0, 0, 0)
				, obstacleColor = Color(255, 0, 0)
				, boundColor = Color(255, 255, 0);
			auto locationOf = [&](PointF p) -> PointF
			{
				float ratioX = (p.X - scope.X) / scope.Width, ratioY = (p.Y - scope.Y) / scope.Height;
				return PointF(bmp.Width*ratioX, bmp.Height*ratioY);
			};
			auto rectangleOf = [&](PointF p, float l) ->RectangleF
			{
				return RectangleF(p.X - l, p.Y - l, l * 2, l * 2);
			};
			auto getRectangle = [&](PointF a, PointF b) ->RectangleF
			{
				assert(a.X <= b.X && a.Y <= b.Y);
				return RectangleF(a.X, a.Y, b.X - a.X, b.Y - a.Y);
			};
			{
				bmp.Clear(backgroundColor);
				bmp.DrawLine(Pen(boundColor, 1), locationOf(PointF(scope.X, rangeY.maximum)), locationOf(PointF(scope.X + scope.Width, rangeY.maximum)));
				bmp.DrawLine(Pen(boundColor, 1), locationOf(PointF(scope.X, rangeY.minimum)), locationOf(PointF(scope.X + scope.Width, rangeY.minimum)));
				int x = 0;
				for(auto o :obstacles)
				{
					{
						auto r = getRectangle(locationOf(PointF(x + o.distance, rangeY.minimum)), locationOf(PointF(x + o.distance + o.width, o.lower_y)));
						bmp.DrawRectangle(Pen(obstacleColor, 1), r.X, r.Y, r.Width, r.Height);
					}
					{
						auto r = getRectangle(locationOf(PointF(x + o.distance, o.upper_y)), locationOf(PointF(x + o.distance + o.width, rangeY.maximum)));
						bmp.DrawRectangle(Pen(obstacleColor, 1), r.X, r.Y, r.Width, r.Height);
					}
					x += o.distance + o.width;
				}
				{
					auto r = rectangleOf(locationOf(PointF(0, (float)location)), 1);
					bmp.DrawRectangle(Pen(podColor, 1), r.X, r.Y, r.Width, r.Height);
				}
				//g.DrawLine(new Pen(Color.FromArgb(0, 0, 0)), new Point(), new Point(bmp.Width, bmp.Height));
				//if (gameState == 0) g.DrawString("Game Over", new Font("Consolas", 40, FontStyle.Bold), new SolidBrush(Color.FromArgb(0, 0, 255)), new PointF(0.2f * bmp.Width, 0.2f * bmp.Height));
				//g.Flush();
			}
		}
	};
	GameVisualizer visualizer = GameVisualizer();
public: void drawImage(Bitmap bmp)
	{
		visualizer.drawImage(bmp, gameState, location, rangeY, Rectangle(-1, rangeY.minimum - 1, obstacleCount * obstacleDistance.maximum + 3, rangeY.maximum - rangeY.minimum + 3), obstacles);
	}
	Interval rangeY = Interval(0, 1000);
	Interval obstacleDistance = Interval(150, 250);
	Interval obstacleWidth = Interval(100, 200);
	Interval obstacleY = Interval(0, 500);
	Interval obstacleHeight = Interval(250, 500);
	double gravity = -0.1, liftForce = 0.1;
	double max_upward_speed = 15, alpha = 0.3;
	int obstacleCount = 1;
	Size imageFeedBackSize = Size(10, 40); //new Size(586, 705);
public: Game()
{
	Reset();
	Update(true);
}
private: static Random random;
		 int newReward;
		 int newDone()
		 {
			 return gameState;
		 }
		 deque<GameObstacle> obstacles = deque<GameObstacle>();
		 int gameState = 0;
		 double location, velocity, acceleration;
public: void Reset()
{
	gameState = 1;
	newReward = 0;
	location = (double)(rangeY.minimum + rangeY.maximum) / 2.0;
	velocity = 0.0;
	while (!obstacles.empty())obstacles.pop_front();
	for (int i = 0; i < obstacleCount; i++) obstacles.push_back(GameObstacle(*this));
}
public: void Update(bool keyState)
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

	if (obstacleCount > 0 && obstacles.front().width == 0)
	{
		obstacles.pop_front();
		newReward++;
	}
	while (obstacles.size() < obstacleCount) obstacles.push_back(GameObstacle(*this));
	assert(obstacles.size() > 0);
	if (obstacles.front().distance == 0) obstacles.front().width--;
	else obstacles.front().distance--;

	if ((location < rangeY.minimum || rangeY.maximum < location) || (obstacles.front().distance == 0 && (location < obstacles.front().lower_y || obstacles.front().upper_y < location)))
	{
		gameState = 0;
		newReward = -1;
	}
}
		private:string toString(int a)
		{
			static char s[100];
			sprintf(s, "%d", a);
			return s;
		}
public: string getFeedBack()
{
	if (imageFeedBackChecked)
	{
		Bitmap bmp = Bitmap(imageFeedBackSize.Width, imageFeedBackSize.Height);
		drawImage(bmp);
		string answer = "";
		short* p = bmp.Scan0();
		for (int i = 0; i < bmp.Height; i++)
		{
			for (int j = 0; j < bmp.Width; j++)
			{
				answer += toString(p[j * 4 + 2]);
				answer += ' ';
				answer += toString(p[j * 4 + 1]);
				answer += ' ';
				answer += toString(p[j * 4 + 0]);
				answer += ' ';
			}
			p += bmp.Stride();
		}
		answer+= toString(newReward);
		answer+=' ';
		answer+= toString(newDone());
		//printf("feedback=%s\n", answer.c_str());
		return answer;
	}
	else
	{
		assert(0);
		string answer = "";
		answer += gameState;
		answer += ' ';
		answer += location;
		answer += ' ';
		answer += velocity;
		answer += ' ';
		answer += obstacleCount;
		for (int i = 0; i < obstacleCount; i++)
		{
			answer += ' ';
			answer += obstacles[i].distance;
			answer += ' ';
			answer += obstacles[i].width;
			answer += ' ';
			answer += obstacles[i].lower_y;
			answer += ' ';
			answer += obstacles[i].upper_y;
		}
		return answer;
	}
}

public:static string Do(char msg)
{
	static Game game = Game();
	/*
	* R:restart
	* 0:release
	* 1:press
	*/
	switch (msg)
	{
	case 'R': game.Reset(); break;
	case '0': game.Update(false); break;
	case '1': game.Update(true); break;
	default: assert(0);
	}
	string s = game.getFeedBack();
	//SocketHandler_logAppended("sending... msg = " + s);
	return s;
}
};
Random Game::random = Random();