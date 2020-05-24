using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace CharMan
{
    public partial class Game : System.Windows.Forms.Form
    {
        public int MouseX
        {
            get
            {
                return Convert.ToInt32(((Convert.ToDouble(Cursor.Position.X) - Convert.ToDouble(this.Location.X) - 8 - Convert.ToDouble(this.Width - 15) / Convert.ToDouble(matrix.Width) / 4) / (Convert.ToDouble(this.Width - 16 - Convert.ToDouble(this.Width) / Convert.ToDouble(matrix.Width) / 2) / Convert.ToDouble(matrix.Width))) - ((Convert.ToDouble(Cursor.Position.X) - Convert.ToDouble(this.Location.X) - 8 - Convert.ToDouble(this.Width - 15) / Convert.ToDouble(matrix.Width) / 4) / (Convert.ToDouble(this.Width - 16 - Convert.ToDouble(this.Width) / Convert.ToDouble(matrix.Width) / 2) / Convert.ToDouble(matrix.Width))) % 1) + matrix.X;
            }
        }
        public int MouseY
        {
            get
            {
                return Convert.ToInt32(((Convert.ToDouble(Cursor.Position.Y) - Convert.ToDouble(this.Location.Y) - 31) / (Convert.ToDouble(this.Height - 39) / Convert.ToDouble(matrix.Height))) - ((Convert.ToDouble(Cursor.Position.Y) - Convert.ToDouble(this.Location.Y) - 31) / (Convert.ToDouble(this.Height - 39) / Convert.ToDouble(matrix.Height))) % 1) + matrix.Y;
            }
        }
        class Control
        {
            public static bool Right = false;
            public static bool Left = false;
            public static bool Up = false;
            public static bool Down = false;
            public static bool MouseClick = false;
        }
        class Symbol
        {
            public char S { get; set; }
            public Color C { get; set; }

            public Symbol()
            {
                S = ' ';
                C = Color.Black;
            }

            public Symbol(char s_)
            {
                S = s_;
                C = Color.Black;
            }

            public Symbol(char s_, Color c_)
            {
                S = s_;
                C = c_;
            }
        }
        class Frame
        {
            Symbol[,] frame;
            public int Height { get; }
            public int Width { get; }

            public Frame(int height, int width)
            {
                frame = new Symbol[height, width];
                Height = height;
                Width = width;
            }

            public void Add(string s, int h)
            {
                for (int i = 0; i < Width; i++)
                    frame[h, i] = new Symbol(s[i]);
            }

            public char this[int y, int x]
            {
                get
                {
                    return frame[y, x].S;
                }
            }
        }
        class Matrix
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; }
            public int Height { get; }
            public int Size { get; set; }
            public string Field { get; }
            public bool[,] View { get; }

            Symbol[,] matrix;

            public Matrix(int height, int width, int size)
            {
                Width = width;
                Height = height;
                Size = size;
                X = 0;
                Y = 0;

                Field = "";
                string l = "";
                for (int i = 0; i < Width; i++)
                    l += '█';
                for (int i = 1; i < Height; i++)
                    Field += '\n' + l;

                matrix = new Symbol[Height, Width];
                for (int i = 0; i < Height; i++)
                    for (int j = 0; j < Width; j++)
                        matrix[i, j] = new Symbol();
            }

            public void Paint(MObject obj)
            {
                int x2 = Math.Min(obj.X - obj.Center.X + obj.Width + obj.Parent.X - X, Width);
                int y2 = Math.Min(obj.Y - obj.Center.Y + obj.Height + obj.Parent.Y - Y, Height);
                int x;
                int y = -Math.Min(0, obj.Y - obj.Center.Y + obj.Parent.Y - Y);
                for (int i = Math.Max(obj.Y - obj.Center.Y + obj.Parent.Y - Y, 0); i < y2; i++)
                {
                    x = -Math.Min(0, obj.X - obj.Center.X + obj.Parent.X - X);
                    for (int j = Math.Max(obj.X - obj.Center.X + obj.Parent.X - X, 0); j < x2; j++)
                    {
                        if (obj.Char(y, x) != ' ')
                            if(obj.Char(y, x) == '')
                                matrix[i, j].S = ' ';
                            else
                                matrix[i, j].S = obj.Char(y, x);
                        x++;
                    }
                    y++;
                }
                for (int i = 0; i < obj.Posterity.Count; i++)
                    Paint(obj.Posterity[i]);
            }

            public char this[int y, int x]
            {
                get
                {
                    return matrix[y, x].S;
                }
                set
                {
                    matrix[y, x].S = value;
                }
            }

            public void Clear()
            {
                for (int i = 0; i < Height; i++)
                    for (int j = 0; j < Width; j++)
                    {
                        matrix[i, j] = new Symbol();
                        //if (i % 2 == 0 ^ j % 2 == 0)
                        //    matrix[i, j].S = '█';
                    }
            }

            public string Visual
            {
                get
                {
                    string result = "";
                    for (int i = 0; i < Height; i++)
                    {
                        for (int j = 0; j < Width; j++)
                            result += matrix[i, j].S;
                        result += "\n";
                    }
                    return result;
                }
            }
        }
        class Imate
        {
            Frame[] imate;
            bool[,] mask;
            int speed;
            public int Height { get; }
            public int Width { get; }
            public Point Center { get; }

            public Imate(string path_)
            {
                using (StreamReader sr = new StreamReader(path_))
                {
                    string line;

                    line = sr.ReadLine();
                    Center = new Point(Convert.ToInt32(line.Split(' ')[1]), Convert.ToInt32(line.Split(' ')[0]));

                    line = sr.ReadLine();
                    Height = Convert.ToInt32(line.Split(' ')[0]);
                    Width = Convert.ToInt32(line.Split(' ')[1]);

                    line = sr.ReadLine();
                    speed = Convert.ToInt32(line);

                    line = sr.ReadLine();
                    imate = new Frame[Convert.ToInt32(line)];

                    sr.ReadLine();
                    mask = new bool[Height, Width];
                    for (int i = 0; i < Height; i++)
                    {
                        line = sr.ReadLine();
                        for (int j = 0; j < Width; j++)
                            mask[i, j] = line[j] == '█';
                    }

                    for (int i = 0; i < imate.Length; i++)
                    {
                        sr.ReadLine();
                        imate[i] = new Frame(Height, Width);
                        for (int j = 0; j < Height; j++)
                            imate[i].Add(sr.ReadLine(), j);
                    }
                }
            }

            public char this[int x, int y]
            {
                get
                {
                    return imate[time / speed % imate.Length][x, y];
                }
            }

            public bool Bool(int x, int y)
            {
                return mask[x, y];
            }
        }

        Player player = new Player("DLight", 5, 10);
        static MObject None = new MObject();
        static List<MObject> Objects = new List<MObject>();
        static Matrix matrix = new Matrix(21, 51, 15);
        static MObject Spawner = new MObject(0, 0, "", None);
        static int time = 0;
        static Random random = new Random();
        static bool[,] Space = new bool[H, W];
        static int W = 100;
        static int H = 50;
        static int snail = 0;
        static int spider = 0;
        static SolidBrush brush = new SolidBrush(Color.White);

        public Game()
        {
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(Game_MouseWheel);
        }

        private void Game_Load(object sender, EventArgs e)
        {
            Objects.Add(player);
            Sword sword = new Sword(player);
            int v = 1;
            for (int i = 0; i < W; i += random.Next(5, 10))
            {
                Objects.Add(new Horisontal_Rock(i, v));
                if (v == 1)
                    v = 0;
                else
                    v = 1;
            }
            for (int i = 0; i < W; i += random.Next(5, 10))
            {
                if (v == H)
                    v = H + 1;
                else
                    v = H;
                Objects.Add(new Horisontal_Rock(i, v));
            }
            for (int i = 0; i < H; i += random.Next(4, 7))
            {
                if (v == 0)
                    v = 1;
                else
                    v = 0;
                Objects.Add(new Vertical_Rock(v, i));
            }
            for (int i = 0; i < H; i += random.Next(4, 7))
            {
                if (v == W)
                    v = W + 1;
                else
                    v = W;
                Objects.Add(new Vertical_Rock(v, i));
            }
        }

        class MObject
        {
            public List<MObject> Posterity { get; set; }
            public MObject Parent { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public string State { get; set; }

            public Dictionary<string, Imate> imates;

            public MObject(int x, int y, string type, MObject parent)
            {
                Posterity = new List<MObject>();
                Parent = parent;
                X = x;
                Y = y;
                if (type != "")
                {
                    State = "Default";

                    imates = new Dictionary<string, Imate>();

                    string[] D = Directory.GetFiles(Environment.CurrentDirectory + "\\imates\\" + type, "*.imate", SearchOption.AllDirectories);

                    for (int i = 0; i < D.Length; i++)
                        imates.Add(Path.GetFileNameWithoutExtension(D[i]), new Imate(D[i]));
                }
            }

            public bool DistanceTo(MObject mb, int d)
            {
                return Math.Sqrt(Math.Pow(mb.X - this.X, 2) + Math.Pow((mb.Y - this.Y) * 2, 2)) < d;
            }

            public MObject()
            {
                X = 0;
                Y = 0;
            }

            public virtual void AI(int ID, Player player, int MX, int MY)
            {
                
            }

            public void Spacing()
            {
                int x2 = Math.Min(X - Center.X + Width + Parent.X, W);
                int y2 = Math.Min(Y - Center.Y + Height + Parent.Y, H);
                int x;
                int y = -Math.Min(0, Y - Center.Y);
                for (int i = Math.Max(Y - Center.Y + Parent.Y, 0); i < y2; i++)
                {
                    x = -Math.Min(0, X - Center.X);
                    for (int j = Math.Max(X - Center.X + Parent.X, 0); j < x2; j++)
                    {
                        if (Bool(y, x))
                            Space[i, j] = true;
                        x++;
                    }
                    y++;
                }
            }

            public char Char(int y, int x)
            {
                return imates[State][y, x];
            }

            public bool Bool(int y, int x)
            {
                return imates[State].Bool(y, x);
            }

            public int Width
            {
                get
                {
                    return imates[State].Width;
                }
            }

            public int Height
            {
                get
                {
                    return imates[State].Height;
                }
            }

            public Point Center
            {
                get
                {
                    return imates[State].Center;
                }
            }
        }

        class Structure : MObject
        {
            protected Structure(int x, int y, string type) : base(x, y, "Structures\\" + type, None)
            {

            }
        }

        class Rock : Structure
        {
            protected Rock(int x, int y, string type) : base(x, y, "Rocks\\" + type)
            {

            }
        }

        class Vertical_Rock : Rock
        {
            public Vertical_Rock(int x, int y) : base(x, y, "Vertical")
            {

            }
        }

        class Horisontal_Rock : Rock
        {
            public Horisontal_Rock(int x, int y) : base(x, y, "Horisontal_" + Convert.ToString(random.Next(4)))
            {

            }
        }

        class Entity : MObject
        {
            public string Name { get; set; }
            public int HPMax { get; set; }
            public int HP { get; set; }
            public int Speed { get; set; }

            protected Entity(string name, int x, int y, int hp, int speed, string type, MObject parent) : base(x, y, "Entitys\\" + type, parent)
            {
                parent.Posterity.Add(this);
                HPMax = hp;
                Speed = speed;
                Name = name;
                HP = HPMax;
            }

            protected Entity(string name, int x, int y, int hp, int speed, string type) : base(x, y, "Entitys\\" + type, None)
            {
                HPMax = hp;
                Speed = speed;
                Name = name;
                HP = HPMax;
            }

            public bool Up
            {
                get
                {
                    return Free("Up");
                }
            }

            public bool Down
            {
                get
                {
                    return Free("Down");
                }
            }

            public bool Left
            {
                get
                {
                    return Free("Left");
                }
            }

            public bool Right
            {
                get
                {
                    return Free("Right");
                }
            }

            public bool Free(string direction)
            {
                bool result = true;
                int x2 = Math.Min(X - Center.X + Width, W);
                int y2 = Math.Min(Y - Center.Y + Height, H);
                int x;
                int y = -Math.Min(0, Y - Center.Y);
                for (int i = Math.Max(Y - Center.Y, 0); i < y2; i++)
                {
                    x = -Math.Min(0, X - Center.X);
                    for (int j = Math.Max(X - Center.X, 0); j < x2; j++)
                    {
                        if (direction == "Right" && Bool(y, x) && (j == W - 1 || Space[i, j + 1]) && (x == Width - 1 || !Bool(y, x + 1)))
                            result = false;
                        else if (direction == "Left" && Bool(y, x) && (j == 0 || Space[i, j - 1]) && (x == 0 || !Bool(y, x - 1)))
                            result = false;
                        else if (direction == "Down" && Bool(y, x) && (i == H - 1 || Space[i + 1, j]) && (y == Height - 1 || !Bool(y + 1, x)))
                            result = false;
                        else if (direction == "Up" && Bool(y, x) && (i == 0 || Space[i - 1, j]) && (y == 0 || !Bool(y - 1, x)))
                            result = false;
                        x++;
                    }
                    y++;
                }
                return result;
            }
        }

        class Player : Entity
        {
            public int reload = 0;
            public MObject Hit = new MObject(0, 0, "", None);

            public Player(string name, int x, int y) : base(name, x, y, 10, 10, "Player")
            {   
                
            }

            public override void AI(int ID, Player player, int MX, int MY)
            {
                if (HP == 0)
                {
                    brush.Color = Color.Black;
                    Objects.RemoveAt(ID);
                }

                if (Control.MouseClick && reload == -10)
                {
                    if (MX < X + 5 && MX > X - 5 && MY < Y + 2 && MY > Y - 2)
                        Hit = new MObject(MX, MY, "", None);
                    State = "Hit";
                    Posterity[0].State = "Hit";
                    reload = 10;
                }

                else if (reload < 1)
                {
                    State = "Default";
                    Posterity[0].State = "Default";

                    if (Control.Up && Up && !Control.Down)
                    {
                        State = "Walk_vertical";
                        if (time % (Speed * 2) == 0)
                        {
                            Y--;
                        }
                    }

                    if (Control.Down && Down && !Control.Up)
                    {
                        State = "Walk_vertical";
                        if (time % (Speed * 2) == 0)
                        {
                            Y++;
                        }
                    }

                    if (Control.Right && Right && !Control.Left)
                    {
                        State = "Walk_right";
                        if (time % Speed == 0)
                        {
                            X++;
                        }
                    }

                    if (Control.Left && Left && !Control.Right)
                    {
                        State = "Walk_left";
                        if (time % Speed == 0)
                        {
                            X--;
                        }
                    }

                    if (X - (matrix.Width - 1) / 2 < 0)
                        matrix.X = 0;
                    else if (X + matrix.Width / 2 >= W)
                        matrix.X = W - matrix.Width;
                    else
                        matrix.X = X - (matrix.Width - 1) / 2;

                    if (Y - matrix.Height / 2 < 0)
                        matrix.Y = 0;
                    else if (Y + matrix.Height / 2 >= H)
                        matrix.Y = H - matrix.Height;
                    else
                        matrix.Y = Y - (matrix.Height - 2) / 2 - 1;
                }
                if (reload > -10) reload--;
                Control.MouseClick = false;
            }
        }

        class Mob : Entity
        {
            protected Mob(string name, int x, int y, int hp, int speed, string type) : base(name, x, y, hp, speed, "Mobs\\" + type)
            {

            }
        }

        class Item : Entity
        {
            protected Item(string name, int x, int y, int hp, int speed, string type, Entity parent) : base(name, x, y, hp, speed, "Items\\" + type, parent)
            {
                
            }

            protected Item(string type, Entity parent) : base("Sword", 2, -1, 0, 0, "Items\\" + type, parent)
            {

            }
        }

        class Snail : Mob
        {
            private int freeze = 0;
            private string direction = "";
            private string act = "Stay";
            private int count;
            private int go;

            public Snail(string name, int x, int y) : base(name, x, y, 1, 100, "Snail")
            {
                
            }

            public override void AI(int ID, Player player, int MX, int MY)
            {
                if (player.Hit.X == X && player.Hit.Y == Y && player.HP < 10)
                {
                    Objects.RemoveAt(ID);
                    snail--;
                    player.HP += 2 - player.HP % 2;
                }

                if (act == "Out")
                {
                    if (freeze == 0)
                    {
                        if (count == 0)
                        {
                            act = "Stay";
                        }
                        else
                        {
                            count--;
                            if (State == "Default")
                            {
                                State = direction;
                                freeze = 25;
                            }
                            else
                            {
                                State = "Default";
                                freeze = 50;
                            }
                        }
                    }
                    else
                        freeze--;
                }

                else if (act == "Go")
                {
                    if (go > 0)
                    {
                        State = direction;
                        if (time % Speed == 0)
                        {
                            if (direction == "Left" && Left)
                                X--;
                            else if (direction == "Right" && Right)
                                X++;
                            else if (direction == "Up" && Up)
                                Y--;
                            else if (direction == "Down" && Down)
                                Y++;
                            go--;
                        }
                    }
                    else
                        act = "Stay";
                }

                else if (act == "Stay")
                {
                    State = "Default";
                    if (random.Next(500) == 0)
                    {
                        act = "Out";

                        int r = random.Next(0, 4);
                        if (r == 0)
                            direction = "Down";
                        else if (r == 1)
                            direction = "Up";
                        else if (r == 2)
                            direction = "Right";
                        else if (r == 3)
                            direction = "Left";

                        count = random.Next(4) * 2;
                        freeze = 30;
                    }

                    else if (random.Next(500) == 0)
                    {
                        act = "Go";

                        int r = random.Next(0, 4);
                        if (r == 0)
                            direction = "Down";
                        else if (r == 1)
                            direction = "Up";
                        else if (r == 2)
                            direction = "Right";
                        else if (r == 3)
                            direction = "Left";

                        go = random.Next(1, 4);
                        freeze = 30;
                    }
                }
            }
        }

        class Spider : Mob
        {
            int reload = 0;

            public Spider(string name, int x, int y) : base(name, x, y, random.Next(1, 4), 8, "Spider")
            {

            }

            public override void AI(int ID, Player player, int MX, int MY)
            {
                if (player.reload == 1 && DistanceTo(player.Hit, 3))
                    HP--;

                if (HP == 0)
                {
                    Objects.RemoveAt(ID);
                    spider--;
                }

                if (DistanceTo(player, 25))
                {
                    if (reload == 0 && DistanceTo(player, 4))
                    {
                        player.HP--;
                        reload = 100;
                    }
                    if (time % Speed == 0)
                    {
                        if (player.X != X && player.Y != Y)
                        {
                            if (random.Next(2) == 0)
                            {
                                if (player.X > X)
                                {
                                    if (Right)
                                    {
                                        State = "Walk_Right";
                                        X++;
                                    }
                                }
                                else
                                {
                                    if (Left)
                                    {
                                        State = "Walk_Left";
                                        X--;
                                    }
                                }
                            }
                            else
                            {
                                if (player.Y > Y)
                                {
                                    if (Down)
                                    {
                                        Y++;
                                        if (State == "Default")
                                            State = "Walk_Down";
                                    }
                                }
                                else
                                {
                                    if (Up)
                                    {
                                        Y--;
                                        if (State == "Default")
                                            State = "Walk_Up";
                                    }
                                }
                            }
                        }
                        else if (player.Y != Y)
                        {
                            if (player.Y > Y)
                            {
                                if (Down)
                                {
                                    State = "Walk_Down";
                                    Y++;
                                }
                            }
                            else
                            {
                                if (Up)
                                {
                                    State = "Walk_Up";
                                    Y--;
                                }
                            }
                        }
                        else if (player.X > X)
                        {
                            if (Right)
                            {
                                State = "Walk_Right";
                                X++;
                            }
                        }
                        else
                        {
                            if (Left)
                            {
                                State = "Walk_Left";
                                X--;
                            }
                        }
                    }
                }
                else
                    State = "Default";
                if (reload > 0)
                    reload--;
            }
        }

        class Sword : Item
        {
            public Sword(Entity parent) : base("Sword", parent)
            {

            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (time < 998)
                time++;
            else
                time = 0;

            for (int i = 0; i < Objects.Count; i++)
                for (int j = i; j < Objects.Count; j++)
                    if (Objects[j].Y < Objects[i].Y)
                    {
                        MObject swap = Objects[i];
                        Objects[i] = Objects[j];
                        Objects[j] = swap;
                    }

            if (time % 10 == 0 && random.Next(10) == 0 && spider < 10 && snail < 15)
            {
                Spawner.X = random.Next(5, W - 5);
                Spawner.Y = random.Next(5, H - 5);
                if (!Space[Spawner.Y, Spawner.X])
                    if (random.Next(3) != 1 && spider < 5)
                    {
                        Objects.Add(new Spider("", Spawner.X, Spawner.Y));
                        spider++;
                    }
                    else if (snail < 5)
                    {
                        Objects.Add(new Snail("", Spawner.X, Spawner.Y));
                        snail++;
                    }
            }

            pictureBox.Invalidate();
            matrix.Clear();
            Space = new bool[H, W];
            for (int i = 0; i < Objects.Count; i++)
            {
                Objects[i].Spacing();
            }
            for (int i = 0; i < Objects.Count; i++)
            {
                Objects[i].AI(i, player, MouseX, MouseY);
            }
            for (int i = 0; i < Objects.Count; i++)
            {
                matrix.Paint(Objects[i]);
            }

            matrix[matrix.Height - 3, 0] = '╔';
            matrix[matrix.Height - 2, 0] = '│';
            matrix[matrix.Height - 1, 0] = '╚';

            for (int i = 1; i < 10; i++)
            {
                matrix[matrix.Height - 3, i] = '─';
                matrix[matrix.Height - 2, i] = ' ';
                matrix[matrix.Height - 1, i] = '─';
            }

            matrix[matrix.Height - 3, 10] = '╗';
            matrix[matrix.Height - 2, 10] = '│';
            matrix[matrix.Height - 1, 10] = '╝';

            matrix[matrix.Height - 2, 1] = '♥';
            matrix[matrix.Height - 2, 2] = '╡';
            matrix[matrix.Height - 2, 8] = '╞';
            matrix[matrix.Height - 2, 9] = '@';

            for (int i = 0; i < 5; i++)
            {
                if (player.HP > i * 2 + 1)
                    matrix[matrix.Height - 2, 3 + i] = '█';
                else if (player.HP == i * 2 + 1)
                    matrix[matrix.Height - 2, 3 + i] = '▌';
                else matrix[matrix.Height - 2, 3 + i] = ' ';
            }
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (brush.Color == Color.Black)
                pictureBox.BackColor = Color.White;

            this.Width = Convert.ToInt32(e.Graphics.MeasureString(matrix.Field, new Font("Consolas", matrix.Size)).Width) + 15;
            this.Height = Convert.ToInt32(e.Graphics.MeasureString(matrix.Field, new Font("Consolas", matrix.Size)).Height) + 36;

            e.Graphics.DrawString(matrix.Visual, new Font("Consolas", matrix.Size), brush, 0, 0);
        }

        void Game_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (matrix.Size < 20)
                    matrix.Size++;
            }
            else
            {
                if (matrix.Size > 10)
                    matrix.Size--;
            }
        }

        private void Game_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A)
            {
                Control.Left = false;
            }
            if (e.KeyCode == Keys.D)
            {
                Control.Right = false;
            }
            if (e.KeyCode == Keys.W)
            {
                Control.Up = false;
            }
            if (e.KeyCode == Keys.S)
            {
                Control.Down = false;
            }
        }

        private void Game_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A)
            {
                Control.Left = true;
            }
            if (e.KeyCode == Keys.D)
            {
                Control.Right = true;
            }
            if (e.KeyCode == Keys.W)
            {
                Control.Up = true;
            }
            if (e.KeyCode == Keys.S)
            {
                Control.Down = true;
            }
            if (e.KeyCode == Keys.S)
            {
                Control.Down = true;
            }
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            Control.MouseClick = true;
        }
    }
}