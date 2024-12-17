using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Numerics;
using System.Windows.Media;
using static Froggun.MainWindow;
using System.Windows.Controls.Ribbon.Primitives;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public double largeur { get; set; }
        public double hauteur { get; set; }
        public double vitesse { get; set; }
        public double delayNouvPos { get; set; }
        public int ecartMaxNouvPos { get; set; }
        private string cheminImage { get; set; }
        public int[] indexAnimation { get; set; }

        public Rectangle BoundingBox { get; set; }
        public Image Image { get; set; }
        private int currentFrameIndex { get; set; }
        private DispatcherTimer animationTimer { get; set; }
        public Canvas canvas { get; set; }

        public double cibleX;
        public double cibleY;
        private DispatcherTimer minuterieMouvement;

        public Proies(TypeProies type, double x, double y, double largeur, double hauteur, double vitesse, double newPosDelay, int newPosOffset, Canvas canvas)
        {
            X = x;
            Y = y;
            this.largeur = largeur;
            this.hauteur = hauteur;
            this.vitesse = vitesse;
            ecartMaxNouvPos = newPosOffset;
            this.canvas = canvas;

            switch (type)
            {
                case TypeProies.Fly:
                    indexAnimation = new int[] { 1, 2 };
                    cheminImage = "img/ennemis/Food1";
                    break;
            }

            currentFrameIndex = 0;

            Image = new Image
            {
                Width = largeur,
                Height = hauteur
            };

            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);

            Canvas.SetLeft(Image, X);
            Canvas.SetTop(Image, Y);
            canvas.Children.Add(Image);

            this.BoundingBox = new Rectangle {
                Width = largeur-10,
                Height = hauteur-10,
                Stroke = Brushes.Red,
                StrokeThickness = 1
            };

            Canvas.SetLeft(BoundingBox, x + 5);
            Canvas.SetTop(BoundingBox, y + 5);

            animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();

            minuterieMouvement = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(newPosDelay) };
            minuterieMouvement.Tick += GenerateRandomTarget;
            minuterieMouvement.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            currentFrameIndex++;
            if (currentFrameIndex >= indexAnimation.Length) currentFrameIndex = 0;

            int frame = indexAnimation[currentFrameIndex];
            BitmapImage newImageSource = GetImageSourceForFrame(frame);
            Image.Source = newImageSource;
        }

        private BitmapImage GetImageSourceForFrame(int frame)
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri($"pack://application:,,/{cheminImage}/{frame}.png"));
            return bitmapImage;
        }

        public static void ReccomencerProies(List<Proies> proies, Canvas canvas)
        {
            //for (int i = 0; i < proies.Count; i++)
            //{
            //   // proies[i].IsAlive = false;
            //    proies[i].Image.Visibility = Visibility.Hidden;
            //    canvas.Children.Remove(proies[i].Image);
            //    //canvas.Children.Remove(proies[i].healthBarEmpty);
            //    //canvas.Children.Remove(proies[i].healthBar);
            //    proies.RemoveAt(i);
            //}
        }
        
        public void GenerateRandomTarget(object? sender, EventArgs e)
        {
            Random random = new Random();
            cibleX = X + random.Next(-ecartMaxNouvPos, ecartMaxNouvPos);
            cibleY = Y + random.Next(-ecartMaxNouvPos, ecartMaxNouvPos);
        }

        public static void UpdateProies(Canvas canvas, List<Proies> proies, Rect joueur)
        {
            foreach (Proies proie in proies)
            {
                // Update bounding box
                Canvas.SetLeft(proie.BoundingBox, proie.X + 5);
                Canvas.SetTop(proie.BoundingBox, proie.Y + 5);

                // Move the enemy
                Vector2 direction = new Vector2(
                    (float)(proie.cibleX - proie.X),
                    (float)(proie.cibleY - proie.Y)
                );

                direction = Vector2.Normalize(direction);
                double newX = proie.X + direction.X * proie.vitesse;
                double newY = proie.Y + direction.Y * proie.vitesse;

                // Prevent moving off-screen (adjust position if out of bounds)
                newX = Math.Max(0, Math.Min(newX, canvas.ActualWidth - proie.largeur - 20));
                newY = Math.Max(0, Math.Min(newY, canvas.ActualHeight - proie.hauteur - 20));

                proie.X = newX;
                proie.Y = newY;

                // Update image position
                Canvas.SetLeft(proie.Image, proie.X);
                Canvas.SetTop(proie.Image, proie.Y);
            }
        }
    }
}