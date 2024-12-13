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
using System.Windows.Automation;
using System.Media;

namespace Froggun
{
    public enum TypeEnnemis
    {
        Spider,
        Squit
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
        private int Health { get; set; }
        public double maxHealth{ get; set; }
        public bool IsAlive { get; private set; }
        public bool hasCollided { get; set; }
        private Rectangle healthBarEmpty { get; set; }
        private Rectangle healthBar { get; set; }

        public static int killStreak = 0;

        public static double scoreMultiplier = 1;

        private static DispatcherTimer killStreakTimer = new DispatcherTimer();

        private static bool isTimerRunning = false;


        public Ennemis(TypeEnnemis type, double x, double y, double width, double height, double speed, Canvas canvas, double SpeedMultiplier = 1.0, Rect BoundingBox = new Rect())
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Speed = speed;

            healthBarEmpty = new Rectangle
            {
                Fill = Brushes.Gray,
                Width = width,
                Height = 10,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            Canvas.SetLeft(healthBarEmpty, X);
            Canvas.SetTop(healthBarEmpty, Y - 15);

            canvas.Children.Add(healthBarEmpty);

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

            isSlowed = false;
            IsAlive = true;
            switch (type)
            {
                case TypeEnnemis.Spider:
                    animationIndex = new int[] { 1, 2, 3, 1, 4, 5 };
                    imagePath = "img/ennemis/LL";
                    if (difficulte == "facile") Health = 150;
                    else if (difficulte == "moyen") Health = 200;
                    else if (difficulte == "difficile") Health = 250;
                    else if (difficulte == "extreme") Health = 300;
                    maxHealth = Health;
                    break;
                case TypeEnnemis.Squit:
                    animationIndex = new int[] { 1, 2, 3, 4 };
                    imagePath = "img/ennemis/Squit";
                    if (difficulte == "facile") Health = 300;
                    else if (difficulte == "moyen") Health = 400;
                    else if (difficulte == "difficile") Health = 450;
                    else if (difficulte == "extreme") Health = 550;
                    maxHealth = Health;
                    break;
            }
          
            currentFrameIndex = 0;
            Image = new Image { Width = width, Height = height };
            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);

            Canvas.SetLeft(Image, X);
            Canvas.SetTop(Image, Y);
            canvas.Children.Add(Image);

            BoundingBox = new Rect(X+5, Y+5, Width-10, Height-10);

            animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
            InitializeKillStreakTimer();
        }
        void InitializeKillStreakTimer()
        {
            killStreakTimer.Interval = TimeSpan.FromSeconds(5);
            killStreakTimer.Tick += (sender, e) =>
            {
                Console.WriteLine("Kill streak expired.");
                killStreak = 0;
                scoreMultiplier = 1;
                isTimerRunning = false;
                killStreakTimer.Stop();
            };
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

        public static void UpdateEnnemis(List<Ennemis> ennemis, Rect joueurr, List<Balle> balles, Canvas canvas, ref Joueur joueur, ref double score)
        {
            // Cache bounding box and other frequently used values
            Rect rJoueur = joueurr;
            for (int i = 0; i < ennemis.Count; i++)
            {
                Ennemis ennemi = ennemis[i];
                if (!ennemi.IsAlive) continue;

                ennemi.hasCollided = false;

                // Handle collision with balls
                for (int j = 0; j < balles.Count; j++)
                {
                    Balle balle = balles[j];
                    if (balle.hasHit) continue;

                    Rect rImgBalle = new Rect(balle.X, balle.Y, 25, 25);

                    if (ennemi.BoundingBox.IntersectsWith(rImgBalle) && !ennemi.hasCollided)
                    {
                        ennemi.Health -= 50;
                        if (ennemi.Health <= 0)
                            ennemi.Die(ennemis, ennemi, canvas, ref score);

                        if (ennemi.Health > 0)
                        {
                            ennemi.healthBar.Width = ennemi.healthBar.ActualWidth * (ennemi.Health / ennemi.maxHealth);
                        }

                        balle.hasHit = true;
                        canvas.Children.Remove(balle.BalleImage);
                        balles.RemoveAt(j);
                        j--; // Adjust the index after removal
                        break;
                    }
                }

                // Skip if enemy is dead
                if (ennemi.Health <= 0) continue;

                // Adjust movement speed based on status (isSlowed)
                ennemi.SpeedMultiplier = ennemi.isSlowed ? 0.5 : 0.25;
                if (difficulte == "facile")
                {
                    ennemi.SpeedMultiplier = ennemi.isSlowed ? 0.5 : 0.25;
                }
                else if (difficulte == "moyen")
                {
                    ennemi.SpeedMultiplier = ennemi.isSlowed ? 0.6 : 0.3;
                }
                else if (difficulte == "difficile")
                {
                    ennemi.SpeedMultiplier = ennemi.isSlowed ? 0.7 : 0.35;
                }
                else if (difficulte == "extreme")
                {
                    ennemi.SpeedMultiplier = ennemi.isSlowed ? 0.8 : 0.4;
                }
                ennemi.BoundingBox = new Rect(ennemi.X, ennemi.Y, ennemi.Width, ennemi.Height);  // Update the bounding box (optimized for reuse)
                // Calculate direction towards the player (only do this if necessary)
                Vector2 direction = new Vector2((float)(rJoueur.X - ennemi.X), (float)(rJoueur.Y - ennemi.Y));
                direction = Vector2.Normalize(direction);

                double newX = ennemi.X + direction.X * ennemi.Speed * ennemi.SpeedMultiplier;
                double newY = ennemi.Y + direction.Y * ennemi.Speed * ennemi.SpeedMultiplier;

                // Check if enemy can move (avoid unnecessary checks)
                bool canMove = true;
                for (int j = 0; j < ennemis.Count; j++)
                {
                    if (i == j) continue; // Skip self
                    if (ennemis[i].BoundingBox.IntersectsWith(ennemis[j].BoundingBox))
                    {
                        canMove = false;
                        break; // No need to check further
                    }
                }

                if (canMove)
                {
                    ennemi.X = newX;
                    ennemi.Y = newY;
                }

                // Check if the enemy collides with the player
                if (rJoueur.IntersectsWith(ennemi.BoundingBox) && !ennemi.isSlowed)
                {
                    ennemi.SlowDown(3); // Slow down effect
                    joueur.hit(1);
                }

                // Efficiently update canvas positions
                Canvas.SetLeft(ennemi.Image, ennemi.X);
                Canvas.SetTop(ennemi.Image, ennemi.Y);
                Canvas.SetLeft(ennemi.healthBarEmpty, ennemi.X);
                Canvas.SetTop(ennemi.healthBarEmpty, ennemi.Y - 15);
                Canvas.SetLeft(ennemi.healthBar, ennemi.X);
                Canvas.SetTop(ennemi.healthBar, ennemi.Y - 15);
            }

            // Collision handling between enemies
            for (int i = 0; i < ennemis.Count - 1; i++)
            {
                Ennemis ennemiA = ennemis[i];
                for (int j = i + 1; j < ennemis.Count; j++)
                {
                    Ennemis ennemiB = ennemis[j];

                    if (ennemiA.BoundingBox.IntersectsWith(ennemiB.BoundingBox))
                    {
                        // Resolve collision by pushing enemies apart
                        Vector2 direction = new Vector2((float)(rJoueur.X - ennemiA.X), (float)(rJoueur.Y - ennemiA.Y));
                        direction = Vector2.Normalize(direction);

                        float collisionPushback = 3.0f;
                        ennemiA.X += direction.X * collisionPushback;
                        ennemiA.Y += direction.Y * collisionPushback;

                        ennemiB.X -= direction.X * collisionPushback;
                        ennemiB.Y -= direction.Y * collisionPushback;
                    }
                }
            }
        }
        private void SonMortEnnemie()
        {
            // Charger le fichier audio depuis les ressources
            Uri audioUri = new Uri("/son/mortEnnemie.wav", UriKind.RelativeOrAbsolute);
            Stream audioStream = Application.GetResourceStream(audioUri).Stream;
            // Créer un objet SoundPlayer pour lire le son
            SoundPlayer musique = new SoundPlayer(audioStream);
            musique.Play();
        }

        public void Die(List<Ennemis> ennemis, Ennemis e, Canvas canvas, ref double score)
        {
            //SonMortEnnemie();
            IsAlive = false;
            Image.Visibility = Visibility.Hidden;
            canvas.Children.Remove(Image);
            canvas.Children.Remove(healthBarEmpty);
            canvas.Children.Remove(healthBar);
            ennemis.Remove(e);
            AddKillToStreak();
            score += 100 * scoreMultiplier;
        }
        public static void AddKillToStreak()
        {
            if (!isTimerRunning)
            {
                killStreakTimer.Start();
                isTimerRunning = true;
            }
            killStreak++;
            scoreMultiplier = 1 + (killStreak - 1) * 0.5;
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
