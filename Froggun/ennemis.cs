using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using System.Numerics;
using System.Windows.Media;
using static Froggun.MainWindow;

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

        public Rect BoundingBox { get; set; }
        public Image Image { get; set; }
        private int currentFrameIndex { get; set; }
        private DispatcherTimer animationTimer { get; set; }


        public Ennemis(TypeEnnemis type, double x, double y, double width, double height, double speed, Canvas canvas, double SpeedMultiplier = 1.0, Rect BoundingBox = new Rect())
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Speed = speed;
            //imagePath = path;
            //animationIndex = animationIndex;
            isSlowed = false;

            switch (type)
            {
                case TypeEnnemis.Spider:
                    animationIndex = new int[] { 1, 2, 3, 1, 4, 5 };
                    imagePath = "img/ennemis/LL";
                    break;
                case TypeEnnemis.Ant:
                    animationIndex = new int[] { 1, 2, 3, 1, 4, 5 };
                    imagePath = "img/ennemis/LL";
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

        public static void UpdateEnnemis(List<Ennemis> ennemis, Rect joueur, List<Balle> balles)
        {

            foreach (var ennemi in ennemis)
            {
                foreach (var balle in balles)
                {
                    Rect rImgBalle = new Rect(
            balle.X,
            balle.Y,
            25,
            25
        );
                    if (ennemi.BoundingBox.IntersectsWith(rImgBalle))
                    {

                    }
                }

                if (ennemi.isSlowed) ennemi.SpeedMultiplier = 0.5;
                else ennemi.SpeedMultiplier = 0.25;

                ennemi.BoundingBox = new Rect(
                    (int)ennemi.X,
                    (int)ennemi.Y,
                    (int)ennemi.Width,
                    (int)ennemi.Height
                );

                Vector2 direction = new Vector2(
                    (float)(joueur.X - ennemi.X),
                    (float)(joueur.Y - ennemi.Y)
                );

                direction = Vector2.Normalize(direction);
                double newX = ennemi.X + direction.X * ennemi.Speed * ennemi.SpeedMultiplier;
                double newY = ennemi.Y + direction.Y * ennemi.Speed * ennemi.SpeedMultiplier;

                bool canMove = true; 
                foreach (var autreEnnemi in ennemis)
                {
                    if (autreEnnemi == ennemi)
                        continue;
                }

                if (canMove)
                {
                    ennemi.X = newX;
                    ennemi.Y = newY;
                }

                if (joueur.IntersectsWith(ennemi.BoundingBox))
                {
                    //health--;
                    if (!ennemi.isSlowed) ennemi.SlowDown(3);
                }

                Canvas.SetLeft(ennemi.Image, ennemi.X);
                Canvas.SetTop(ennemi.Image, ennemi.Y);
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
