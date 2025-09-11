using NAudio.CoreAudioApi;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop.InputManagement.Structs;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Coop.Forms
{  
    public partial class Patrons : Form
    {
        public static Patrons Instance;

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private int refreshRate = 70;
        private int spacePress = 0;
        private int scrollSpeed = 4;
        private int volMinLoc;
        private int volMaxLoc;

        private float defVolume = 0.4f;

        private Random random = new Random();

        private Font nameFont = new Font("Impact", 40.0f, FontStyle.Bold, GraphicsUnit.Pixel, 0);
        private Font exitBtnFont = new Font("Impact", 20f, FontStyle.Bold, GraphicsUnit.Pixel, 0);
        private Font infoFont = new Font("Impact", 18f, FontStyle.Regular, GraphicsUnit.Pixel, 0);
        private Font creditsFont = new Font("Impact",25f, FontStyle.Regular, GraphicsUnit.Pixel, 0);
        private Font volFont = new Font("Franklin Gothic", 25f, FontStyle.Regular, GraphicsUnit.Pixel, 0);
        
        private Timer invalTimer;
       
        private Rectangle topBar;
        private Rectangle bottomBar;
        private Rectangle scrollingRect;
        private Rectangle swapColorRec;
        private Rectangle volIcon;
        private Rectangle volBarHor;
        private Rectangle volBarVer;

        private RectangleF exitBtn;

        private RectangleF[] bubbles = new RectangleF[4];

        private List<string> names = new List<string>();
        private List<Rectangle> namesRects = new List<Rectangle>();
        private List<Tuple<Point, int>> popOrigins = new List<Tuple<Point, int>>();

        private bool startScolling = false;
        private bool startEaster = false;
        private bool setVol = false;
        
        //private bool abortStartup = false;

        private IWavePlayer waveOut;
        private MediaFoundationReader audioFile;
        
        private string exitBtnText = "EXIT";
        private string patronsGitUri = "https://raw.githubusercontent.com/SplitScreen-Me/splitscreenme-patreon/refs/heads/main/tiers.txt";
        private static string cacheFile = Path.Combine(Application.StartupPath, $"webview\\cache\\patreon");
        private string volIconString = "♫";

        private Bitmap icon = Properties.Resources.icon_small;

        private SolidBrush randomBrush;
        private Point center;

        public Patrons()
        {
            InitializeComponent();

            DoubleBuffered = true;
            InitializeComponent();

            MaximizeBox = false;
            MinimizeBox = false;
            //ShowInTaskbar = false;     
            MinimumSize = Size;
            MaximumSize = Size;
            Size = new Size(640, 480);

            center = new Point(ClientRectangle.Width / 2, ClientRectangle.Height / 2);

            MouseMove += Mouse_Move;
            MouseDown += Input;
            MouseUp += Mouse_Up;

            KeyDown += SpeedUp;
            FormBorderStyle = FormBorderStyle.None;
            FormClosing += StopTimer;

            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.Black;        
            randomBrush = new SolidBrush(Color.GreenYellow);

            MakeScrollingNameList();           
        }

        private void MakeScrollingNameList()
        {
            try
            {
                names = GetGitPatrons()?.Split('\r')?.ToList();

                if(names == null)
                {
                    if(File.Exists(cacheFile))
                    {
                        names = File.ReadAllLines(cacheFile).ToList();
                    }
                    else
                    {
                        Dispose();
                        return;
                    }
                }
                else
                {
                    var cacheDir = Path.Combine(Application.StartupPath, $"webview\\cache"); 

                    if (!Directory.Exists(cacheDir))
                    {
                        Directory.CreateDirectory(cacheDir);
                    }

                    File.WriteAllText(cacheFile, string.Join("", names));
                }

                Graphics g = CreateGraphics();
                //names =  System.IO.File.ReadAllLines(Globals.NucleusInstallRoot + "\\tiers.txt").ToList();
                names.Insert(0, "\nPatreon's Supporters");

                for (int i = 0; i < names.Count(); i++)
                {
                    names[i] = names[i].Replace("<div>", "");
                    names[i] = names[i].Replace("</div>", "");

                    if (i > 0)//skip thanks string
                    {
                        names[i] = names[i].Replace("\n", "");
                    }

                    var size = g.MeasureString(names[i], nameFont);
                    scrollingRect = new Rectangle(0, 0, Width, (int)(scrollingRect.Height + size.Height));

                    var nameRect = Rectangle.Empty;

                    if (i == 0)
                    {
                        nameRect = new Rectangle((int)((scrollingRect.Width / 2) - (size.Width / 2)), (int)(scrollingRect.Bottom - size.Height), (int)size.Width, (int)size.Height);
                    }
                    else
                    {
                        nameRect = new Rectangle((int)((scrollingRect.Width / 2) - (size.Width / 2)), (int)(namesRects[i - 1].Top - size.Height), (int)size.Width, (int)size.Height);
                    }

                    namesRects.Add(nameRect);
                }

                string space = "\n";
                var sizeExt = g.MeasureString(space, creditsFont);
                var spaceRec = new Rectangle((int)((scrollingRect.Width / 2) - (sizeExt.Width / 2)), (int)(namesRects[namesRects.Count() - 1].Top - sizeExt.Height), (int)sizeExt.Width, (int)sizeExt.Height);
                names.Add(space);
                namesRects.Add(spaceRec);

                string credits = "Music Credits: https://mobygratis.com/";
                sizeExt = g.MeasureString(credits, creditsFont);
                var creditsRec = new Rectangle((int)((scrollingRect.Width / 2) - (sizeExt.Width / 2)), (int)(namesRects[namesRects.Count() - 1].Top - sizeExt.Height), (int)sizeExt.Width, (int)sizeExt.Height);
                names.Add(credits);
                namesRects.Add(creditsRec);

                string license = "https://mobygratis.com/license-agreement";
                sizeExt = g.MeasureString(license, creditsFont);
                names.Add(license);
                var licenseRec = new Rectangle((int)((scrollingRect.Width / 2) - (sizeExt.Width / 2)), (int)(namesRects[namesRects.Count() - 1].Top - sizeExt.Height), (int)sizeExt.Width, (int)sizeExt.Height);
                namesRects.Add(licenseRec);

                invalTimer = new System.Windows.Forms.Timer();
                invalTimer.Interval = refreshRate; //millisecond                   
                invalTimer.Tick += InvalTimer_Tick;
                invalTimer.Start();

                startScolling = true;
                PlayMusic();
                g.Dispose();

                Instance = this;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
      
        private void SpeedUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Space)
            {
                if(spacePress == 3)
                {
                    scrollSpeed = 3;
                    spacePress = 0;
                    return;
                }

                scrollSpeed *=2;
                spacePress++;
            }
        }

        private void Mouse_Move(object sender, MouseEventArgs m)
        {
            if (setVol && volBarVer.Contains(m.Location))
            {
                if(m.X > (volBarHor.Left) && m.X < volBarHor.Right)
                {
                    volBarVer = new Rectangle(m.X - (volBarVer.Width / 2), (int)(volBarHor.Top + (volBarHor.Height / 2) - 8), 16, 16);
                    float test = ((float)volMaxLoc - (float)volMinLoc);
                    var currentLoc = (float)volMinLoc - ((float)volBarVer.X + ((float)volBarVer.Width / 2));

                    test = (test + (currentLoc)) / 100f;

                    if(waveOut != null)
                    {
                        waveOut.Volume = 1.0f - test;
                    }                          
                }               
            }
        }

        private void Mouse_Up(object sender, MouseEventArgs m)
        {
            setVol = false;
        }

        private void Input(object sender, MouseEventArgs m)
        {
            if (m.Button == MouseButtons.Left)
            {
                if (exitBtn.Contains(m.Location))
                {
                    this.Close();
                }
                else if(volBarVer.Contains(m.Location))
                {
                    setVol = true;
                }
                else if(topBar.Contains(m.Location))
                {
                    User32Interop.ReleaseCapture();
                    IntPtr nucHwnd = User32Interop.FindWindow(null, Text);
                    User32Interop.SendMessage(nucHwnd, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, (IntPtr)0);
                }

                for (int i = 0; i < bubbles.Count(); i++)
                {
                    if (bubbles[i].Contains(m.Location))
                    {
                        Random rand = new Random();
                        popOrigins.Add(Tuple.Create(new Point((int)bubbles[i].Location.X + (int)bubbles[i].Width / 2, (int)bubbles[i].Location.Y + (int)bubbles[i].Height / 2), (int)bubbles[i].Width));
                        int randX = rand.Next(80, ClientRectangle.Width - 80);
                        int randY = rand.Next(80, ClientRectangle.Height - 80);
                        int randS = rand.Next(1, 5);

                        double angle = i * (2 * Math.PI / bubbles.Count());
                        int length = bubbles.Count();
                        int x = (int)(length * Math.Cos(angle));
                        int y = (int)(length * Math.Sin(angle));

                        bubbles[i] = new RectangleF(randX + x, randY + y, randS, randS);
                        randomBrush = new SolidBrush(Color.FromArgb(255, random.Next(256), random.Next(256), random.Next(256)));
                    }
                }
            }
        }

        private void InvalTimer_Tick(object Object, EventArgs eventArgs)
        {
            try
            {
                Invoke((MethodInvoker)delegate
                {
                    scrollingRect = new Rectangle(scrollingRect.X, scrollingRect.Y + 2, scrollingRect.Width, scrollingRect.Height);

                    for (int i = 0; i < namesRects.Count(); i++)
                    {
                        namesRects[i] = new Rectangle(namesRects[i].X, namesRects[i].Y + scrollSpeed, namesRects[i].Width, namesRects[i].Height);                      
                    }

                    Invalidate();

                    //Close the form and trigger the easter
                    if (namesRects.Last().Top > ClientRectangle.Bottom)
                    {
                        startEaster = true;

                        Invoke((MethodInvoker)delegate
                        {
                            //this.Hide();
                            /*this.*/
                            Close();
                        });
                    }
                });
            }
            catch
            { }
        }

        private void PlayMusic()
        {
            if(waveOut == null)
            {
                waveOut = new WaveOutEvent();
                audioFile = new MediaFoundationReader(Globals.NucleusInstallRoot + "\\gui\\audio\\colts field 132.mp3");
                waveOut.Init(audioFile);
                waveOut.Volume = 0.5f;
            }

            waveOut.Init(audioFile);
            waveOut.Play();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //var raysCount = 200;

            //for (int i = 0; i < raysCount; i++)
            //{
            //    // distribute lines evenly around the circle
            //    double angle = i * (2 * Math.PI / raysCount);
            //    int length = Width; // add a little randomness
            //    int x = center.X + (int)(length * Math.Cos(angle));
            //    int y = center.Y + (int)(length * Math.Sin(angle));

            //    using (Pen p = new Pen(Color.Blue))
            //    {
            //        g.DrawLine(p, center, new Point(x, y));
            //    }
            //}

            if (swapColorRec == Rectangle.Empty)
            {
                swapColorRec = new Rectangle(0, (ClientRectangle.Height / 2) - 50, Width, 100);
            }
           
            if (startScolling)
            {
                //loop the music
                if (waveOut?.PlaybackState == PlaybackState.Stopped)
                {
                    PlayMusic();
                }

                Random rand = new Random();

                for (int i = 0; i < bubbles.Count(); i++)
                {
                    bool maxSize = bubbles[i].Width >= 20.0F;
                    
                    var intersect = bubbles.Any(b => b != bubbles[i] && (b.IntersectsWith(bubbles[i])) /*|| bubbles[i].IntersectsWith(swapColorRec)*/);

                    if (maxSize || (bubbles[i] == RectangleF.Empty) || intersect)
                    {
                        if(bubbles[i] != RectangleF.Empty)
                        {
                            popOrigins.Add(Tuple.Create(new Point((int)bubbles[i].Location.X + (int)bubbles[i].Width / 2, (int)bubbles[i].Location.Y + (int)bubbles[i].Height / 2), (int)bubbles[i].Width));
                            bubbles[i] = RectangleF.Empty;
                            continue;

                        }
                       
                        int randX = rand.Next(80, ClientRectangle.Width - 80);
                        int randY = rand.Next(80, ClientRectangle.Height - 80);
                        int randS = rand.Next(1, 5);

                        double angle = i * (2 * (Math.PI / (i+1)));
                        int length = bubbles.Count();
                        int x = (int)(length * Math.Cos(angle));
                        int y = (int)(length * Math.Sin(angle));

                        bubbles[i] = new RectangleF(randX + x, randY + y, randS, randS);
                       
                    }

                    bool isOnLeft = bubbles[i].X < center.X;
                    bool isOnRight = bubbles[i].X > center.X;
                    bool isOnTop = bubbles[i].Y < center.Y;
                    bool isOnBot = bubbles[i].Y > center.Y;

                    //RectangleF reflection = RectangleF.Empty;

                    bubbles[i].Width += 0.2f;
                    bubbles[i].Height += 0.2f;

                    if (isOnLeft)
                    {
                        bubbles[i].X -= 0.2f;
                    }
                    else
                    {
                        bubbles[i].X += 0.2f;
                    }

                    bubbles[i].Y += 0.2f;

                    //PointF bubC = new PointF(bubbles[i].X + (bubbles[i].Width / 2), bubbles[i].Y + (bubbles[i].Height/ 2)); 

                    //if (isOnLeft && isOnTop)
                    //{
                    //    var x = bubbles[i].X + ((bubbles[i].Width / 2)  ); 
                    //    var y = bubbles[i].Y +  ((bubbles[i].Height / 2) );
                    //    reflection = new RectangleF(x , y, bubbles[i].Width / 4, bubbles[i].Height / 4);
                    //}
                    //else if(isOnLeft && isOnBot)
                    //{
                    //    var x = bubbles[i].X + ((bubbles[i].Width / 2));
                    //    var y = bubbles[i].Y /*+ ((bubbles[i].Height / 2) )*/;
                    //    reflection = new RectangleF(x, y, bubbles[i].Width / 4, bubbles[i].Height / 4);
                    //}
                    //else if (isOnRight && isOnTop)
                    //{
                    //    var x = bubbles[i].X ;
                    //    var y = bubbles[i].Y + ((bubbles[i].Height / 2));

                    //    reflection = new RectangleF(x, y, bubbles[i].Width / 4, bubbles[i].Height / 4);
                    //}
                    //else if (isOnRight && isOnBot)
                    //{
                    //    var x = bubbles[i].X + 3;
                    //    var y = bubbles[i].Y;

                    //    reflection = new RectangleF(x, y, bubbles[i].Width / 4, bubbles[i].Height / 4);
                    //}
                    RectangleF reflection = new RectangleF(bubbles[i].X + 1, bubbles[i].Y + 3, bubbles[i].Width / 5, bubbles[i].Height / 4);

                    //using (Brush b = new SolidBrush(Color.White))
                    //{
                    g.FillEllipse(randomBrush, reflection);
                    //}

                    using (Pen p = new Pen(Color.White, 1))
                    {
                        g.DrawEllipse(p, bubbles[i]);              
                    }
                }

                for (int i = 0; i < popOrigins.Count(); i++)
                {
                    var popOrigin = popOrigins[i];

                    for (int j = 0; j < 12; j++)
                    {
                        double angle = (j + 1) * (2 * Math.PI / 12);
                        int length = popOrigin.Item2 + random.Next(0, 5);
                        int x = popOrigin.Item1.X + (int)(length * Math.Cos(angle));
                        int y = popOrigin.Item1.Y + (int)(length * Math.Sin(angle));

                        g.FillEllipse(Brushes.White, new Rectangle(x, y, 3, 3));
                    }
                }

                for (int i = 0; i < namesRects.Count(); i++)
                {
                    var rec = namesRects[i];
                    if (namesRects[i].Top >= ClientRectangle.Bottom || namesRects[i].Bottom <= ClientRectangle.Top)
                    {
                        continue;
                    }

                    if (i == 0)
                    {
                        //thanks
                        using (Brush b = new SolidBrush(Color.Sienna))
                        {
                            g.DrawString(names[i], nameFont, b, rec.X, rec.Y);

                        }
                    }
                    else if (i == names.Count() - 1 || i == names.Count() - 2)
                    {
                        //credits
                        using (Brush b = new SolidBrush(Color.Yellow))
                        {
                            g.DrawString(names[i], creditsFont, b, rec.X, rec.Y);
                        }
                    }
                    else if (swapColorRec.Contains(rec))
                    {
                        //Highlight
                        g.DrawString(names[i], nameFont, randomBrush, rec.X, rec.Y);
                    }
                    else
                    {
                        //Regular
                        using (Brush b = new SolidBrush(Color.FromArgb(255, 80, 80, 80)))
                        {
                            g.DrawString(names[i], nameFont, b, rec.X, rec.Y);
                        }
                    }                
                }

                if (exitBtn == Rectangle.Empty)
                {
                    SizeF btnSize = g.MeasureString(exitBtnText, exitBtnFont);
                    topBar = new Rectangle(-2, -2, ClientRectangle.Width + 3 , (int)btnSize.Height + 10);
                    bottomBar = new Rectangle(-2, ClientRectangle.Bottom - ((int)btnSize.Height + 8), ClientRectangle.Width + 3, (int)btnSize.Height + 8);
                    exitBtn = new RectangleF((ClientRectangle.Width - btnSize.Width) - 5, 4, btnSize.Width - 1, btnSize.Height - 3);
                    volBarHor = new Rectangle((int)(exitBtn.Left - 115), (int)(exitBtn.Top + exitBtn.Height / 2) - 3, 100, 6);
                    volMinLoc = volBarHor.Left;
                    volMaxLoc = volBarHor.Right;

                    SizeF iconSize = g.MeasureString(volIconString, volFont);
                    var defLoc = volBarHor.X + (defVolume * 100) + 8;
                    volBarVer = new Rectangle((int)(defLoc), (int)(volBarHor.Top + (volBarHor.Height / 2) - (16 / 2)), 16, 16);
                    volIcon = new Rectangle((int)(volBarHor.Left - iconSize.Width) - 4, (int)(exitBtn.Top + volIcon.Height / 2) - 3, (int)iconSize.Width, (int)iconSize.Height);

                    PictureBox pict = new PictureBox();

                    pict.SizeMode = PictureBoxSizeMode.StretchImage;
                    pict.Location = new Point(center.X - 20, 2);
                    pict.Image = Properties.Resources.dancer;
                    float ratio = (float)pict.Image.Height / (float)pict.Image.Width;

                    pict.Size = new Size((int)exitBtn.Height, (int)(exitBtn.Height * ratio));
                    pict.BackColor = Color.Transparent;
                    Controls.Add(pict);
                }

                using (Brush b = new SolidBrush(Color.FromArgb(255, 20, 20, 20)))
                {
                    g.FillRectangle(b, topBar);
                    g.FillRectangle(b, bottomBar);
                }

                using (Pen p = new Pen(Color.White, 2))
                {
                    g.DrawRectangles(p, new RectangleF[] { exitBtn });
                }

                using (Pen p = new Pen(Color.Gray, 3))
                {
                    g.DrawRectangle(p, volBarHor);
                }

                using (Brush b = new SolidBrush(Color.White))
                {
                    g.DrawString(exitBtnText, exitBtnFont, b, exitBtn.X, exitBtn.Y);
                    g.FillRectangle(b, volBarHor);
                    g.FillEllipse(randomBrush, volBarVer);
                    g.DrawString(volIconString, volFont, b, volIcon.X, volIcon.Y);
                    SizeF infoSize = g.MeasureString("SPACE BAR >>", infoFont);

                    g.DrawString("SPACE BAR >>", infoFont, b, new Rectangle((int)(bottomBar.X + ((bottomBar.Width / 2) - (infoSize.Width / 2))), (int)(bottomBar.Y + ((bottomBar.Height / 2) - (infoSize.Height / 2))), (int)infoSize.Width, (int)infoSize.Height));
                }

                using (Pen p = new Pen(Color.Gray, 2))
                {                  
                    g.DrawEllipse(p, volBarVer);
                }
            }

            popOrigins.Clear();
        }

        private void StopTimer(object sender, EventArgs e)
        {
            invalTimer?.Stop();
            
            audioFile?.Dispose();
            waveOut?.Stop();
            waveOut?.Dispose();
            Instance = null;

            if (startEaster)
            {
                Easter easter = new Easter();
                easter.Show();
            }
        }

        public string GetGitPatrons()
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.DefaultConnectionLimit = 9999;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(patronsGitUri);
                request.Timeout = 2500;//lower values make the timeout too short for some "big" handlers (GTAIV)             
                request.Method = "Get";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
