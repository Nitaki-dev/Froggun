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
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Speed { get; set; }
        public int[] AnimationIndex { get; set; }
        public Image Image { get; set; }
        private int currentFrameIndex;
        private DispatcherTimer animationTimer;
        private string imagePath;

        public Ennemis(double x, double y, double width, double height, double speed, Canvas canvas, string path, int[] animationIndex)
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

        public void Tick()
        {
            
        }
    }
}
