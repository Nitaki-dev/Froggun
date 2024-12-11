using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using System.Numerics;
using System.Windows.Media;
using static Froggun.MainWindow;
using System.Windows.Shapes;

namespace Froggun
{
    public enum TypeEnnemis
    {
        Spider,
        Ant
    }

    internal class Ennemis
    {
        public TypeEnnemis type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Speed { get; set; }
        public double SpeedMultiplier { get; set; }
        public bool isSlowed { get; set; }
        private string imagePath { get; set; }
        public int[] animationIndex { get; set; }

        public double PV { get; set; }
        public double maxPV{ get; set; }

        public Rect BoundingBox { get; set; }
        public Image Image { get; set; }
        private int currentFrameIndex { get; set; }
        private DispatcherTimer animationTimer { get; set; }
        private int Health { get; set; }
        public bool IsAlive { get; private set; }
        public bool hasCollided { get; set; }
        private Rectangle healthBar { get; set; }


        public Ennemis(TypeEnnemis type, double PointVie, double MaxPointVie, double x, double y, double width, double height, double speed, Canvas canvas, double SpeedMultiplier = 1.0, Rect BoundingBox = new Rect())
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Speed = speed;
            PV = PointVie;
            maxPV = MaxPointVie;
            healthBar = new Rectangle
            {
                Fill = Brushes.Green, 
                Width = width,  
                Height = 10,  
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Canvas.SetLeft(healthBar, X);
            Canvas.SetTop(healthBar, Y - 15); 
            canvas.Children.Add(healthBar);


            //imagePath = path;
            //animationIndex = animationIndex;
            isSlowed = false;
            IsAlive = true;
            switch (type)
            {
                case TypeEnnemis.Spider:
                    animationIndex = new int[] { 1, 2, 3, 1, 4, 5 };
                    imagePath = "img/ennemis/LL";
                    Health = 3;
                    break;
                case TypeEnnemis.Ant:
                    animationIndex = new int[] { 1, 2, 3, 1, 4, 5 };
                    imagePath = "img/ennemis/LL";
                    Health = 2;
                    break;
            }
          

            currentFrameIndex = 0;

            

            Image = new Image
            {
                Width = width,
                Height = height
            };
            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);

            Canvas.SetLeft(Image, X);
            Canvas.SetTop(Image, Y);
            canvas.Children.Add(Image);

            BoundingBox = new Rect(X+5, Y+5, Width-10, Height-10);

            animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            currentFrameIndex++;
            if (currentFrameIndex >= animationIndex.Length) currentFrameIndex = 0; 
            
            int frame = animationIndex[currentFrameIndex];
            BitmapImage newImageSource = GetImageSourceForFrame(frame);
            Image.Source = newImageSource;
        }

        private BitmapImage GetImageSourceForFrame(int frame)
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri($"pack://application:,,/{imagePath}/{frame}.png"));
            return bitmapImage;
        }

        public static void UpdateEnnemis(List<Ennemis> ennemis, Rect joueur, List<Balle> balles, Canvas canvas)
        {
            
            for (int i=0; i<ennemis.Count; i++)
            {
                if (!ennemis[i].IsAlive) continue;
                ennemis[i].hasCollided = false;
                for (int j = 0; j < balles.Count; j++)
                {
                    if (balles[j].hasHit) continue;
                    Rect rImgBalle = new Rect(
            balles[j].X,
            balles[j].Y,
            25,
            25
        );
                    if (ennemis[i].BoundingBox.IntersectsWith(rImgBalle) && !ennemis[i].hasCollided)
                    {
                        ennemis[i].Health--;
                        balles[j].hasHit = true;
                        canvas.Children.Remove(balles[j].BalleImage);
                        balles.RemoveAt(j);
                        j--;
                        break;
                    }
                }
                if (ennemis[i].Health <= 0)
                {
                    ennemis[i].Die(canvas);
                }
                if (ennemis[i].isSlowed) ennemis[i].SpeedMultiplier = 0.5;
                else ennemis[i].SpeedMultiplier = 0.25;

                ennemis[i].BoundingBox = new Rect(
                    (int)ennemis[i].X,
                    (int)ennemis[i].Y,
                    (int)ennemis[i].Width,
                    (int)ennemis[i].Height
                );

                Vector2 direction = new Vector2(
                    (float)(joueur.X - ennemis[i].X),
                    (float)(joueur.Y - ennemis[i].Y)
                );

                direction = Vector2.Normalize(direction);
                double newX = ennemis[i].X + direction.X * ennemis[i].Speed * ennemis[i].SpeedMultiplier;
                double newY = ennemis[i].Y + direction.Y * ennemis[i].Speed * ennemis[i].SpeedMultiplier;

                bool canMove = true; 
                foreach (var autreEnnemi in ennemis)
                {
                    if (autreEnnemi == ennemis[i])
                        continue;
                }

                if (canMove)
                {
                    ennemis[i].X = newX;
                    ennemis[i].Y = newY;
                    
                }

                if (joueur.IntersectsWith(ennemis[i].BoundingBox))
                {
                    //health--;
                    if (!ennemis[i].isSlowed) ennemis[i].SlowDown(3);
                }

                Canvas.SetLeft(ennemis[i].Image, ennemis[i].X);
                Canvas.SetTop(ennemis[i].Image, ennemis[i].Y);
                Canvas.SetLeft(ennemis[i].healthBar, ennemis[i].X);
                Canvas.SetTop(ennemis[i].healthBar, ennemis[i].Y - 15);
            }
            for (int i = 0; i < ennemis.Count - 1; i++)
            {
                for (int j = ennemis.Count - 1; j > i; j--)
                {
                    if (ennemis[i].BoundingBox.IntersectsWith(ennemis[j].BoundingBox))
                    {
                        Vector2 direction = new Vector2(
                    (float)(joueur.X - ennemis[i].X),
                    (float)(joueur.Y - ennemis[i].Y));
                        direction = Vector2.Normalize(direction);
                        float collisionPushback = 3.0f;
                        ennemis[i].X += direction.X * collisionPushback;
                        ennemis[i].Y += direction.Y * collisionPushback;
                        ennemis[j].X -= direction.X * collisionPushback;
                        ennemis[j].Y -= direction.Y * collisionPushback;


                    }
                }
            }
        }
        public void Die(Canvas canvas)
        {
            IsAlive = false;
            Image.Visibility = Visibility.Hidden;
            canvas.Children.Remove(healthBar);
        }
        public void SlowDown(int durationInSeconds)
        {
            isSlowed = true;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(durationInSeconds);
            timer.Tick += (s,e) =>
            {
                isSlowed = false;
                timer.Stop();
            };

            timer.Start();
        }
    }
}
