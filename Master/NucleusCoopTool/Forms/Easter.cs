using NAudio.CoreAudioApi;
using NAudio.Wave;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Media;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Nucleus.Coop.Forms
{

    internal class Entity
    {
       public Point Location { get; set; }
       public Point Center { get; set; }
       private SizeF size;
       public Bitmap Sprite { get; set; }
       
       public SizeF Size
       {
            get => size;
            set
            {
                size = value;
                Center = new Point((int)(Location.X + (size.Width / 2)), (int)(Location.Y + (size.Height / 2)));
            }
       }

        public Entity()
        {

        }
    }

    internal class Ennemy : Entity
    {
        public Ennemy()
        {
        }
    }

    internal class Prop : Entity
    {
        public Prop()
        {
        }
    }

    public partial class Easter : Form
    {
        private Timer invalTimer;
        private Bitmap icon = Properties.Resources.icon_small;
        private Bitmap tree = new Bitmap(Globals.NucleusInstallRoot + "\\cloud.png"); 
        private Bitmap smoke = new Bitmap(Globals.NucleusInstallRoot + "\\smoke.png");
        private Bitmap ground = new Bitmap(Globals.NucleusInstallRoot + "\\ground3.png");
        private ImageAttributes smokeAtt;
        //private float angle = 0; // rotation angle
        private string[] names;
        private SolidBrush particlesBrush;
        private int particlesMax = 10;
        private List<RectangleF> particles  = new List<RectangleF>();
        private RectangleF[] props;
        private int propsMax = 10;
        private int minDefSize = 20;
        private int maxDefSize = 30;
        private int propSizeMax = 80;
        private int propSizeMin = 50;
        //private int maxDefSize = 30;
        private Stopwatch watch = new Stopwatch();
        private Point center;
        private long ellapsed = 0;
        private int currentName = 0;
        private int top = 50;
        private int bottom;

        private Font nameFont = new Font("Gabriola", 35.0f, FontStyle.Bold, GraphicsUnit.Point, 0);
        private Font gameoverFont = new Font("Impact", 40f, FontStyle.Bold, GraphicsUnit.Point, 0);
        private Font scoreFont = new Font("Impact", 15f, FontStyle.Bold, GraphicsUnit.Point, 0);
        private Font startFont = new Font("Impact", 20f, FontStyle.Bold, GraphicsUnit.Point, 0);
        private Size startSize;
        private RectangleF startBtn;

        private Timer timer;
        private int radius = 0;
        private readonly int maxRadius = 20;
        private SolidBrush namesBrush;
        private SolidBrush randomBrush;
        private RectangleF background;
        private RectangleF background2;
        private ColorMatrix smokeMatrix;

        private readonly int points = 10; // number of rays
        private Random random = new Random();
        private int prevYLoc;
        private List<Tuple<Point,int>> popOrigins = new List<Tuple<Point,int>>();
        private Point mousePos;
        private int score = 0;
        private int stepSpeedScore = 50;
        private int stepEnnemyScore = 70;
        private string scoreString = "Score: ";
        private float speed = 3.0f;//3
        private float speedStep = 1.0f;
        private bool missed;
        private bool gameover = false;
        private int hp = 10;

        private string hpString = "♥: ";
        private bool drawHit = false;

        private List<RectangleF> holes = new List<RectangleF>();
        private bool gameoverWatch = false;
        private IWavePlayer waveOut;
        private AudioFileReader audioFile;
        private string startButtonText = "START";
        private bool playing;
       
        public Easter()
        {
            DoubleBuffered = true;
            InitializeComponent();
            this.FormClosing += StopTimer;
            MaximizeBox = false;
            MinimizeBox = false;
            this.Text = "Cheap War";

            Size = new Size(480, 650);
            MinimumSize = Size;
            MaximumSize = Size;

            StartPosition = FormStartPosition.CenterScreen;
            //clientArea = this.ClientSize;
            BackColor = Color.Black;
            invalTimer = new System.Windows.Forms.Timer();
            invalTimer.Interval = 50; //millisecond                   
            invalTimer.Tick += InvalTimer_Tick;
            invalTimer.Start();

            this.MouseDown += Input;
            center = new Point(Width / 2, Height / 2);
            namesBrush = new SolidBrush(Color.FromArgb(random.Next(150, 256), random.Next(256), random.Next(256), random.Next(256)));

            Cursor = Cursors.Cross;
            MouseMove += GetCursorLoc;

             smokeMatrix = new ColorMatrix(new[]
            {
                new float[] {0.3f, 0.3f, 0.3f, 0, 0}, // Red contribution
                new float[] {0.3f, 0.3f, 0.3f, 0, 0}, // Green contribution
                new float[] {0.3f, 0.3f, 0.3f, 0, 0}, // Blue contribution
                new float[] {0,    0,    0,    1, 0}, // Alpha unchanged
                new float[] {-0.2f, -0.2f, -0.2f, 0, 1} // Slightly dark bias
             });

            smokeAtt = new ImageAttributes();
            smokeAtt.SetColorMatrix(smokeMatrix);

            if (waveOut == null)
            {
                waveOut = new WaveOutEvent(); // you can use DirectSoundOut or WasapiOut too
                audioFile = new AudioFileReader(Globals.NucleusInstallRoot + "\\music.wav"); // replace with your file
                waveOut.Init(audioFile);
                waveOut.Volume = 0.2f;
            }
            waveOut.Play();

            bottom = this.ClientRectangle.Height - 50;
            //BackgroundImage = ground;
        }

        private void GameOver()
        {
            gameover = true;
            playing = false;
            holes.Clear();
            particles.Clear();
            popOrigins.Clear();
            props = null;
            speed = 3.0f;    
            hp = 10;      
            ellapsed = 0;
            stepSpeedScore = 50;
            stepEnnemyScore = 70;
            speedStep = 1.0f;
        }

        private void GetCursorLoc(object sender, MouseEventArgs m)
        {
            mousePos = m.Location;
        }


        private SoundPlayer spShoot = new SoundPlayer(Globals.NucleusInstallRoot + "\\shoot.wav");
        private SoundPlayer spMusic = new SoundPlayer(Globals.NucleusInstallRoot + "\\music.wav");

        private void Input(object sender, MouseEventArgs m)
        {           
            if (m.Button == MouseButtons.Left)
            {
                if(!playing || (playing && gameover))
                {
                    if(startBtn.Contains(m.Location))
                    {
                        particles = new List<RectangleF>();
                        GenParticles(particlesMax);
                        GenProps();
                        playing = true;
                        gameover = false;
                        score = 0;
                        return;
                    }

                }

                if(particles == null)
                {
                    return;
                }

                var shooted = particles?.Where(p => p.Contains(m.Location)).FirstOrDefault();

                System.Threading.Tasks.Task.Run(() =>
                {
                    if(playing)
                    spShoot.Play();
                });

                if (shooted == RectangleF.Empty)
                {
                    holes.Add(new RectangleF(m.Location.X - ((Cursor.Size.Width * 2) /2), m.Location.Y - ((Cursor.Size.Height * 2) / 2), Cursor.Size.Width * 2, Cursor.Size.Height * 2));
                    missed = true;
                    return;
                }

                for (int i = 0; i < particles.Count(); i++)
                {
                    if (particles[i].Contains(m.Location))
                    {
                        popOrigins.Add(Tuple.Create(new Point((int)particles[i].Location.X + (int)particles[i].Width / 2, (int)particles[i].Location.Y + (int)particles[i].Height / 2), (int)particles[i].Width));
                        Random rand = new Random();
                        int randMultX = rand.Next(-ClientRectangle.Width / 2, ClientRectangle.Width / 2);
                        int randMultY = rand.Next(-ClientRectangle.Height / 2, ClientRectangle.Height / 2);

                        int randX = rand.Next(maxDefSize, Width - maxDefSize);
                        int randY = rand.Next(-500, top);
                        int randS = rand.Next(minDefSize, maxDefSize);
                        
                        particles[i] = new RectangleF(randX,randY,randS,randS);
                        score++;

                        drawHit = true;
                        Invalidate();
                    }
                    
                }
            }
        }

        public List<Rectangle> bullets = new List<Rectangle>();

        private void GenParticles(int amount)
        {
            Random rand = new Random();
            for (int i = 0; i < amount; i++)
            {
                int randS = rand.Next(minDefSize, maxDefSize);
                int randX = rand.Next(randS, ClientRectangle.Width - maxDefSize);
                int randY = rand.Next(-500, top);

                while (particles.Any(p => (new RectangleF(randX, randY, randS, randS).IntersectsWith(p))) ||
                         (randX + randS > ClientRectangle.Width) || (randX < 0))
                {

                    randX = rand.Next(randS, ClientRectangle.Width - maxDefSize);
                    randY = rand.Next(-500, top);
                }
              
                particles.Add(new RectangleF(randX, randY, randS, randS));
            }
        }


        private void GenProps()
        {
            Random rand = new Random();
            props = new RectangleF[propsMax];

            for (int i = 0; i < propsMax; i++)
            {
                int randX = rand.Next(propSizeMax, ClientRectangle.Width - propSizeMax);
                int randY = rand.Next(-ClientRectangle.Height, top);
                int randS = rand.Next(propSizeMin, propSizeMax);
                var spawchance = rand.Next(1, 15);

                if(spawchance > 5 && spawchance < 10)
                {
                    continue;
                }

                while (props.Any(p => (new RectangleF(randX - maxDefSize, randY - maxDefSize, propSizeMax + maxDefSize, propSizeMax + maxDefSize).IntersectsWith(p))) ||
                          props.Any(p => p != props[i] && p.Width == propSizeMax))
                {
                    randX = randX = rand.Next(propSizeMax, ClientRectangle.Width - maxDefSize);
                    randY = rand.Next(-ClientRectangle.Height, top);
                    //propSize -= 1;
                }

                props[i] = new RectangleF(randX, randY, randS, randS);
            }
        }

        public void Draw(Graphics g)
        {
            Random rand = new Random();
            
            if(gameover)
            {
                SizeF playBtnSize = g.MeasureString("Try again", startFont);
                startBtn = new RectangleF(10, 10, playBtnSize.Width, playBtnSize.Height);
                g.DrawString("Try again", startFont, Brushes.White, 10, 10);

                var btnBorder = new Rectangle((int)10, (int)10, (int)playBtnSize.Width, (int)playBtnSize.Height);
                using (Pen p = new Pen(Color.White, 2))
                {
                    g.DrawRectangle(p, btnBorder);
                }

                if(!gameoverWatch)
                {
                    watch = new Stopwatch();
                    watch.Start();
                    gameoverWatch = true;
                }

                SizeF tagSize = g.MeasureString("GAME OVER", gameoverFont);
                g.DrawString($"GAME OVER", gameoverFont, Brushes.Red, (center.X - (tagSize.Width / 2)) - 0.1f, (center.Y - (tagSize.Height / 2)) - 0.1f);

                SizeF finalScore = g.MeasureString(scoreString + score, startFont);
                g.DrawString(scoreString + score, startFont, Brushes.Yellow, (center.X - (finalScore.Width / 2)) - 0.1f, finalScore.Height + (center.Y - (finalScore.Height / 2)) - 0.1f);

                if (watch.Elapsed.TotalSeconds >= 3)
                {
                    gameoverWatch = false;
                }
                
                return;
            }

            if (!playing)
            {
                //top banner
                using (Brush b = new SolidBrush(Color.DarkOrange))
                {
                    g.FillRectangle(b, new Rectangle(0, 0, Width, top));
                }

                SizeF playBtnSize = g.MeasureString(startButtonText, startFont);
                startBtn = new RectangleF(10, 10, playBtnSize.Width, playBtnSize.Height);
                g.DrawString(startButtonText, startFont, Brushes.White, 10, 10);

                var btnBorder = new Rectangle((int)10, (int)10, (int)playBtnSize.Width, (int)playBtnSize.Height);
                using (Pen p = new Pen(Color.White, 2))
                {
                    g.DrawRectangle(p, btnBorder);
                }

                watch.Start();


                return;
            }

            if (background== RectangleF.Empty)
            {
                background = new RectangleF(0, top , Width, bottom);
                background2 = new RectangleF(0, (background.Top + 2) - background.Height, Width, bottom);
            }

            background = new RectangleF(0, (float)(background.Y + 1.0f), Width, bottom );
            background2 = new RectangleF(0, (float)(background2.Y + 1.0f), Width, bottom );
         
            if (background.Top > bottom + 10)
            {
                background = new RectangleF(0, (float)((background2.Top + 2)- background.Height), Width, bottom);
            }
           
            if (background2.Top > bottom  + 10)
            {
                background2 = new RectangleF(0, (float)((background.Top + 2) - background2.Height), Width, bottom );
            }

            g.DrawImage(ground, background);
            g.DrawImage(ground, background2);

            if (score > stepSpeedScore)
            {
                speed += speedStep;
                stepSpeedScore += stepSpeedScore;
                Console.WriteLine("Speedup " + speed);
            }

            if (score > stepEnnemyScore)
            {
                GenParticles(2);
                stepEnnemyScore += stepEnnemyScore;
                Console.WriteLine("Gen ennemies" + particles.Count);
            }

            using (Pen p = new Pen(Color.Red, 2))
            {
                //sight
                g.DrawEllipse(p, new Rectangle((mousePos.X - Cursor.Current.Size.Width / 2) - 1, (mousePos.Y - Cursor.Current.Size.Height / 2) - 1, Cursor.Current.Size.Width, Cursor.Current.Size.Height));
            }

            if (drawHit)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(80, 230, 255, 61)))
                {
                    //red flash
                    g.FillRectangle(brush, ClientRectangle);
                }

                drawHit = false;
            }

            if (missed)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(80, 255, 172, 62)))
                {
                    //red flash
                    g.FillRectangle(brush, ClientRectangle);
                }

                missed = false;
            }

            if (particles == null)
            {

            }

            for (int i = 0; i < particles.Count(); i++)
            {
                var shoutRand = rand.Next(0,10);
                bool ncShout = shoutRand == 5 || shoutRand == 8;          

                particles[i] = new RectangleF(particles[i].X, particles[i].Y + speed, particles[i].Width, particles[i].Height);

                bool minSize = particles[i].Width > maxDefSize;

                if (particles[i].Top + (particles[i].Height/2) >= bottom)
                {
                    int randS = rand.Next(minDefSize, maxDefSize);
                    popOrigins.Add(Tuple.Create(new Point((int)particles[i].Location.X + (int)particles[i].Width / 2, (int)particles[i].Location.Y + (int)particles[i].Height / 2), (int)particles[i].Width));
                    int randX = rand.Next(randS, ClientRectangle.Width - randS);
                    int randY = rand.Next(-500, top);

                    while (particles.Any(p => (new RectangleF(randX, randY, randS, randS).IntersectsWith(p))) ||
                         (randX + randS > ClientRectangle.Width) ||( randX < 0))
                    {
                        randX = rand.Next(randS, ClientRectangle.Width - maxDefSize);
                        randY = rand.Next(-500, top);
                    }
               
                    particles[i] = new RectangleF(randX, randY, randS, randS);

                    hp--;

                    using (Brush brush  = new SolidBrush(Color.FromArgb(80, 255, 0, 0)))
                    {
                          g.FillRectangle(brush, ClientRectangle);
                    }

                    if(hp == 0)
                    {
                        //gameover = true;
                        GameOver();
                        return;
                    }
                }

                if (particles[i].Width > 1)
                {
                    if(ncShout)
                    {
                        //bullets.Add(new Rectangle((int)(particles[i].X + particles[i].Width / 2), (int)(particles[i].Y + particles[i].Height / 2), 2, 2));
                    }

                    using (Brush brush = new SolidBrush(Color.FromArgb(255, random.Next(256), random.Next(256), random.Next(256))))
                    {
                        //g.FillEllipse(brush, particles[i]);
                        //particlesBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
                    }

                    
                    using (Pen p = new Pen(Color.White,3))
                    {
                        //g.DrawEllipse(p, particles[i]);
                    }

                    g.DrawImage(icon, particles[i]);
                    //g.DrawIcon(icon, new Rectangle((int)particles[i].X, (int)particles[i].Y, (int)particles[i].Width, (int)particles[i].Height));
                }
            }




            for (int i = 0; i < props?.Count(); i++)
            {
                props[i].Y += 1.0f;

                if (props[i].Top >= bottom)
                {
                    int randS = rand.Next(propSizeMin, propSizeMax);
                    //int randMultX = rand.Next(-ClientRectangle.Width / 2, (ClientRectangle.Width / 2) - maxDefSize);
                    //int randMultY = rand.Next(-ClientRectangle.Height / 2, ClientRectangle.Height / 2);

                    int randX = rand.Next(randS, ClientRectangle.Width - randS);
                    int randY = rand.Next(-ClientRectangle.Height, top);

                    var spawchance = rand.Next(1, 10);

                    if (spawchance > 5 && spawchance < 10)
                    {
                        continue;
                    }

                    props[i] = new RectangleF(randX, randY, randS, randS);

                    //hp--;

                    //using (Brush brush = new SolidBrush(Color.FromArgb(80, 255, 0, 0)))
                    //{
                    //    g.FillRectangle(brush, ClientRectangle);
                    //}

                    //if (hp == 0)
                    //{
                    //    GameOver();
                    //    return;
                    //}
                }

                g.DrawImage(tree, props[i]);
            }

            if (props?.Count() < propsMax)
            {
                GenProps();
            }

            List<Tuple<Point, int>> toRemove = new List<Tuple<Point, int>>();
            for (int i = 0; i < popOrigins.Count; i++)
            {
             
                var popOrigin = popOrigins[i];
                for (int j = 0; j < points; j++)
                {

                    // distribute lines evenly around the circle
                    double angle = j * (2 * Math.PI / points);
                    int length = popOrigin.Item2 + random.Next(-5, 5); // add a little randomness
                    int x = popOrigin.Item1.X + (int)(length * Math.Cos(angle));
                    int y = popOrigin.Item1.Y + (int)(length * Math.Sin(angle));

                    //using (Brush brush  = new SolidBrush(Color.FromArgb(255, random.Next(256), random.Next(256), random.Next(256))))
                    //{
                    g.FillEllipse(Brushes.Red, new Rectangle(x, y, 3, 3));

                    //g.DrawString("HIT!", startFont, Brushes.YellowGreen, popOrigins[i].Item1.X + 20, popOrigins[i].Item1.Y);
                    //}


                    //popOrigins[i] = Tuple.Create(new Point(x, y), popOrigin.Item2);

                    //if (x < popOrigin.Item1.X - maxRadius || x > popOrigin.Item1.X + maxRadius || y < popOrigin.Item1.Y - maxRadius || y > popOrigin.Item1.Y + maxRadius)
                    //{
                    //    toRemove.Add(popOrigin);
                    //}
                }
            }


            //List<RectangleF> holesToRemove = new List<RectangleF>();
            for (int i = 0; i < holes?.Count; i++)
            {
                var hole = holes[i];

                if (holes[i].Top >= bottom)
                {
                    holes.Remove(holes[i]);
                    continue;
                }


                holes[i] = new RectangleF(holes[i].X + 0.5f, holes[i].Y - 0.5f, hole.Width, hole.Height);
                //g.FillEllipse(Brushes.DarkGray, holes[i]);


                var randMatrix = rand.Next(0, 4);
                if (randMatrix == 2)
                {
                    g.DrawImage(smoke, new Rectangle((int)holes[i].X, (int)holes[i].Y, (int)holes[i].Width, (int)holes[i].Height), 0, 0, smoke.Width, smoke.Height, GraphicsUnit.Pixel, smokeAtt);
                }
                else
                {
                    g.DrawImage(smoke, holes[i]);
                }
            }

            popOrigins.Clear();

            //top banner
            //bottom banner
            using (Brush b = new SolidBrush(Color.FromArgb(255, 20, 20, 20)))
            {
                g.FillRectangle(b, new Rectangle(0, 0, Width, top));
                g.FillRectangle(b, new Rectangle(0,  bottom, Width, bottom ));
            }

            string _score = $"{scoreString}{score}";
            SizeF scoreSize = g.MeasureString(_score, scoreFont);
            g.DrawString(_score, scoreFont, Brushes.WhiteSmoke, 3,3);

            string _hp = $"{hpString}{hp}";
            SizeF _hpSize = g.MeasureString(_hp, scoreFont);
            g.DrawString(_hp, scoreFont, Brushes.WhiteSmoke, scoreSize.Width + 10, 3);

           
        }

        private void InvalTimer_Tick(object Object, EventArgs eventArgs)
        {
            try
            {
                Invoke((MethodInvoker)delegate
                {
                    radius += 5; // grow outward
                    if (radius > maxRadius)
                    {
                        radius = 0; // reset for looping firework
                    }

                    Invalidate();
                });
            }
            catch
            {

            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //Graphics g = e.Graphics;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            //if(!playing)
            //{

                //return;
            //}
               
            Draw(e.Graphics);
            
        }

        private void StopTimer(Object sender, EventArgs e)
        {
            invalTimer?.Stop();
            waveOut?.Stop();
            audioFile.Position = 0; // reset position if you want replay
            waveOut?.Dispose();
            audioFile?.Dispose();
        }

    }
}
