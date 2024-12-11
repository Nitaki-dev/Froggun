﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Numerics;
using System.Windows.Media;
using static Froggun.MainWindow;
using System.Windows.Controls.Ribbon.Primitives;

namespace Froggun
{
    public enum TypeProies
    {
        Fly
    }

    internal class Proies
    {
        public TypeProies type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Speed { get; set; }
        public double newPosDelay { get; set; }
        public int newPosMaxDiff { get; set; }
        private string imagePath { get; set; }
        public int[] AnimationIndex { get; set; }

        public Rect BoundingBox { get; set; }
        public Image Image { get; set; }
        private int currentFrameIndex { get; set; }
        private DispatcherTimer animationTimer { get; set; }

        public double targetX;
        public double targetY;
        public Vector2 minOffset { get; set; }
        public Vector2 maxOffset { get; set; }
        private DispatcherTimer movementTimer;


        public Proies(TypeProies type, double x, double y, double width, double height, double speed, double newPosDelay, int newPosOffset, Vector2 MinOffset, Vector2 MaxOffset, Canvas canvas, Rect BoundingBox = new Rect())
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Speed = speed;
            newPosMaxDiff = newPosOffset;
            minOffset = MinOffset;
            maxOffset = MaxOffset;

            switch (type)
            {
                case TypeProies.Fly:
                    AnimationIndex = new int[] { 1, 2 };
                    imagePath = "img/ennemis/Food1";
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

            BoundingBox = new Rect(X, Y, Width, Height);

            animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();

            movementTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(newPosDelay) };
            movementTimer.Tick += GenerateRandomTarget;
            movementTimer.Start();
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

        private void GenerateRandomTarget(object? sender, EventArgs e)
        {
            Random random = new Random();
            targetX = X + random.Next(newPosMaxDiff);
            targetY = Y + random.Next(newPosMaxDiff);
        }

        public static void UpdateProies(List<Proies> proies, Rect joueur)
        {
            foreach (var proie in proies)
            {
                proie.BoundingBox = new Rect(
                    (int)proie.X-5,
                    (int)proie.Y-5,
                    (int)proie.Width+10,
                    (int)proie.Height+10
                );

                // Move the enemy
                Vector2 direction = new Vector2(
                    (float)(proie.targetX - proie.X),
                    (float)(proie.targetY - proie.Y)
                );

                direction = Vector2.Normalize(direction);
                double newX = proie.X + direction.X * proie.Speed;
                double newY = proie.Y + direction.Y * proie.Speed;

                if (newX > proie.minOffset.X && newX < proie.maxOffset.X && newY > proie.maxOffset.Y && newY < proie.maxOffset.Y)
                {
                    proie.X = newX;
                    proie.Y = newY;
                    Canvas.SetLeft(proie.Image, proie.X);
                    Canvas.SetTop(proie.Image, proie.Y);
                }
            }
        }
    }
}