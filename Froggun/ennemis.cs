using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using System.Numerics;
using System.Windows.Media;

namespace Froggun
{
    public enum TypeEnnemis
    {
        Spider,
        Ant,
        Fly
    }

    internal class Ennemis
    {
        public TypeEnnemis type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Speed { get; set; }
        private string imagePath { get; set; }
        public int[] AnimationIndex { get; set; }

        public Rect BoundingBox { get; set; }
        public Image Image { get; set; }
        private int currentFrameIndex { get; set; }
        private DispatcherTimer animationTimer { get; set; }
        public Ennemis(TypeEnnemis type, double x, double y, double width, double height, double speed, Canvas canvas, string path, int[] animationIndex, Rect BoundingBox = new Rect())
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Speed = speed;
            imagePath = path;
            AnimationIndex = animationIndex;

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

            BoundingBox = new Rect(X, Y, Width, Height);

            animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            currentFrameIndex++;
            if (currentFrameIndex >= AnimationIndex.Length) currentFrameIndex = 0; 
            
            int frame = AnimationIndex[currentFrameIndex];
            BitmapImage newImageSource = GetImageSourceForFrame(frame);
            Image.Source = newImageSource;
        }

        private BitmapImage GetImageSourceForFrame(int frame)
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri($"pack://application:,,/{imagePath}/{frame}.png"));
            return bitmapImage;
        }

        public static void UpdateEnnemis(List<Ennemis> ennemis, Vector2 posJoueur)
        {
            foreach (var ennemi in ennemis)
            {
                ennemi.BoundingBox = new Rect(
                    (int)ennemi.X,
                    (int)ennemi.Y,
                    (int)ennemi.Width,
                    (int)ennemi.Height
                );

                Vector2 direction = new Vector2(
                    (float)(posJoueur.X - ennemi.X),
                    (float)(posJoueur.Y - ennemi.Y)
                );

                direction = Vector2.Normalize(direction);

                //check if the thing can move here
                // todo...

                ennemi.X = (ennemi.X + direction.X * ennemi.Speed);
                ennemi.Y = (ennemi.Y + direction.Y * ennemi.Speed);

                Canvas.SetLeft(ennemi.Image, ennemi.X);
                Canvas.SetTop(ennemi.Image, ennemi.Y);
            }
        }
    }
}
