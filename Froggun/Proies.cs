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
        public double Vitesse { get; set; }
        public double delayAvantNouvellePosition { get; set; }
        public int distanceMaxNouvellePosition { get; set; }
        private string chemainImage { get; set; }
        public int[] indexAnimation { get; set; }

        public Rectangle Hitbox { get; set; }
        public Image Image { get; set; }
        private int currentFrameIndex { get; set; }
        private double animationTimeElapsed { get; set; }
        private const double AnimationFrameDuration = 100;
        private BitmapImage[] frameImages;
        public Canvas canvas { get; set; }

        public double objectifX;
        public double objectifY;
        private DispatcherTimer timerMouvement;

        public Proies(TypeProies type, double x, double y, double largeur, double hauteur, double vitesse, double newPosDelay, int newPosOffset, Canvas canvas)
        {
            X = x;
            Y = y;
            this.largeur = largeur;
            this.hauteur = hauteur;
            Vitesse = vitesse;
            distanceMaxNouvellePosition = newPosOffset;
            this.canvas = canvas;

            switch (type)
            {
                case TypeProies.Fly:
                    indexAnimation = new int[] { 1, 2 };
                    chemainImage = "img/Proies";
                    break;
            }

            currentFrameIndex = 0;

            Image = new Image { Width = largeur, Height = hauteur };
            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);

            this.Hitbox = new Rectangle
            {
                Width = largeur - 10,
                Height = hauteur - 10,
                Stroke = Brushes.Red,
                StrokeThickness = 1
            };

            Canvas.SetLeft(Hitbox, x + 5);
            Canvas.SetTop(Hitbox, y + 5);

            Canvas.SetLeft(Image, X);
            Canvas.SetTop(Image, Y);
            canvas.Children.Add(Image);
            
            ChargementImage();

            // https://learn.microsoft.com/fr-fr/dotnet/api/system.windows.media.compositiontarget.rendering?view=windowsdesktop-9.0
            CompositionTarget.Rendering += OnRendering;

            timerMouvement = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(newPosDelay) };
            timerMouvement.Tick += TrouverNouvellePosition;
            timerMouvement.Start();
        }

        private void ChargementImage()
        {
            // charge chaques images 
            frameImages = new BitmapImage[indexAnimation.Length];
            for (int i = 0; i < indexAnimation.Length; i++) frameImages[i] = new BitmapImage(new Uri($"pack://application:,,/{chemainImage}/{indexAnimation[i]}.png"));
        }

        // cette fonction est appeller automatiquement a chaque fois que l'image est dessiner sur l'ecran
        private void OnRendering(object sender, EventArgs e)
        {
            animationTimeElapsed += 16;

            if (animationTimeElapsed >= AnimationFrameDuration)
            {
                currentFrameIndex++;
                if (currentFrameIndex >= indexAnimation.Length)
                    currentFrameIndex = 0;

                animationTimeElapsed = 0;
                int frame = indexAnimation[currentFrameIndex];

                Image.Source = frameImages[currentFrameIndex];
            }
        }

        public void TrouverNouvellePosition(object? sender, EventArgs e)
        {
            // nouvelle position aleatoir
            Random random = new Random();
            objectifX = X + random.Next(-distanceMaxNouvellePosition, distanceMaxNouvellePosition);
            objectifY = Y + random.Next(-distanceMaxNouvellePosition, distanceMaxNouvellePosition);
            if (objectifY < 0) objectifY = 10;
            if (objectifX < 0) objectifX = 10;

            if (objectifY > canvas.ActualHeight - largeur) objectifY = 10;
            if (objectifX > canvas.ActualWidth - largeur) objectifX = 10;
        }

        public static void ReccomencerProies(List<Proies> proies, Canvas canvas)
        {
            // tous les tuer
            for (int i = 0; i < proies.Count; i++)
            {
                proies[i].Image.Visibility = Visibility.Hidden;
                canvas.Children.Remove(proies[i].Image);
                proies.RemoveAt(i);
            }
        }

        public static void UpdateProies(Canvas canvas, List<Proies> proies, Rect joueur)
        {
            foreach (Proies proie in proies)
            {
                //Console.WriteLine(proie.ToString());
                
                // debugging
                if (Double.IsNaN(proie.X) || Double.IsNaN(proie.Y))
                {
                    proie.X = 10; proie.Y = 10;
                    Canvas.SetLeft(proie.Hitbox, 10);
                    Canvas.SetTop(proie.Hitbox, 10);
                }

                // mouvement de la proie
                Vector2 direction = new Vector2(
                    (float)(proie.objectifX - proie.X),
                    (float)(proie.objectifY - proie.Y)
                );

                direction = (direction.Length() !=0) ? Vector2.Normalize(direction) : new Vector2(0,0);
                double newX = proie.X + direction.X * proie.Vitesse;
                double newY = proie.Y + direction.Y * proie.Vitesse;

                // Prevent moving off-screen (adjust position if out of bounds)
                newX = Math.Max(0, Math.Min(newX, canvas.ActualWidth - proie.largeur - 20));
                newY = Math.Max(0, Math.Min(newY, canvas.ActualHeight - proie.hauteur - 20));

                proie.X = newX;
                proie.Y = newY;

                // Update image position
                Canvas.SetLeft(proie.Image, proie.X);
                Canvas.SetTop(proie.Image, proie.Y);

                // Update bounding box
                Canvas.SetLeft(proie.Hitbox, proie.X + 5);
                Canvas.SetTop(proie.Hitbox, proie.Y + 5);
            }
        }

        //method pour debug
        public override string? ToString()
        {
            return " objX:" + Math.Round(objectifX) + "  objY:" + Math.Round (objectifY) + 
                "     X" + Math.Round(this.X) + "    Y" + Math.Round(this.Y) + 
                "     Hitbox:  w:" + Math.Round(this.Hitbox.Width) +      " h:" + Math.Round(this.Hitbox.Height) + 
                             " x:" + Math.Round(Canvas.GetLeft(Hitbox)) + " y:" + Math.Round(Canvas.GetTop(Hitbox));
        }
    }
}