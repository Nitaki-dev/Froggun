//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Froggun
//{
//    using System;
//    using System.Windows;
//    using System.Windows.Controls;
//    using System.Windows.Media.Imaging;
//    using System.Windows.Threading;
//    using System.IO;
//    using System.Numerics;
//    using System.Windows.Media;

//    namespace Froggun
//    {
//        public enum TypeProie
//        {
//            Spider,
//            Ant
//        }

//        internal class Proie
//        {
//            public TypeProie type { get; set; }
//            public double X { get; set; }
//            public double Y { get; set; }
//            public double Width { get; set; }
//            public double Height { get; set; }
//            public double Speed { get; set; }
//            public double NewPositionSpeed { get; set; }
//            private string imagePath { get; set; }
//            public int[] AnimationIndex { get; set; }

//            public Rect BoundingBox { get; set; }
//            public Image Image { get; set; }
//            private int currentFrameIndex { get; set; }
//            private DispatcherTimer animationTimer { get; set; }


//            public Proie(TypeProie type, double x, double y, double width, double height, double speed, double newPositionSpeed, Canvas canvas, string path, Rect BoundingBox = new Rect())
//            {
//                X = x;
//                Y = y;
//                Width = width;
//                Height = height;
//                Speed = speed;
//                NewPositionSpeed = newPositionSpeed;

//                switch (type)
//                {
//                    case TypeProie.Spider:
//                        AnimationIndex = new int[] { 1, 2, 3, 1, 4, 5 };
//                        imagePath = "img/ennemis/LL";
//                        break;
//                    case TypeProie.Ant:
//                        AnimationIndex = new int[] { 1, 2, 3, 1, 4, 5 };
//                        imagePath = "img/ennemis/LL";
//                        break;
//                }

//                currentFrameIndex = 0;

//                Image = new Image
//                {
//                    Width = width,
//                    Height = height
//                };

//                RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);

//                Canvas.SetLeft(Image, X);
//                Canvas.SetTop(Image, Y);
//                canvas.Children.Add(Image);

//                BoundingBox = new Rect(X, Y, Width, Height);

//                animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
//                animationTimer.Tick += AnimationTimer_Tick;
//                animationTimer.Start();
//            }

//            private void AnimationTimer_Tick(object sender, EventArgs e)
//            {
//                currentFrameIndex++;
//                if (currentFrameIndex >= AnimationIndex.Length) currentFrameIndex = 0;

//                int frame = AnimationIndex[currentFrameIndex];
//                BitmapImage newImageSource = GetImageSourceForFrame(frame);
//                Image.Source = newImageSource;
//            }

//            private BitmapImage GetImageSourceForFrame(int frame)
//            {
//                BitmapImage bitmapImage = new BitmapImage(new Uri($"pack://application:,,/{imagePath}/{frame}.png"));
//                return bitmapImage;
//            }

//            public static void UpdateProies(List<Proie> proies, Rect joueur)
//            {
//                foreach (var proie in proies)
//                {
//                    proie.BoundingBox = new Rect(
//                        (int)proie.X,
//                        (int)proie.Y,
//                        (int)proie.Width,
//                        (int)proie.Height
//                    );

//                    Vector2 direction = new Vector2(
//                        (float)(joueur.X - proie.X),
//                        (float)(joueur.Y - proie.Y)
//                    );

//                    direction = Vector2.Normalize(direction);

//                    //check if the thing can move here
//                    // todo...

//                    proie.X += (direction.X * proie.Speed);
//                    proie.Y += (direction.Y * proie.Speed);

//                    if (joueur.IntersectsWith(proie.BoundingBox))
//                    {
//                        //health++;
//                    }


//                    Canvas.SetLeft(proie.Image, proie.X);
//                    Canvas.SetTop(proie.Image, proie.Y);
//                }
//            }
//        }
//    }

//}
