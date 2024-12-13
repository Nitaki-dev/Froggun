using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Media;
using System.IO;
using System.Diagnostics;

namespace Froggun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Random alea = new Random();
        
        private bool pause = false;
        private static DispatcherTimer minuterie = new DispatcherTimer();
        private static DispatcherTimer pauseVagues = new DispatcherTimer();

        private static ScaleTransform joueurFlip = new ScaleTransform();
        private static Vector2 posJoueur = new Vector2(50.0f, 50.0f);
        private static Vector2 vitesseJoueur = new Vector2();

        private static BitmapImage imgFrogFront;
        private static BitmapImage imgFrogBack;
        private static BitmapImage imgFrogSide;

        public enum Directions
        {
            left, //0
            right, //1
            up, //2
            down, //3
            diagUpLeft, //4
            diagUpRight, //5
            diagDownLeft, //6
            diagDownRight //7
        }
        Directions directionJoueur = Directions.right;

        private const float vitesseDeplacement = 8.0f;
        private const float friction = 0.4f;
        //private const float friction = 0.8f;

        private bool deplacerGauche = false;
        private bool deplacerDroite = false;
        private bool deplacerHaut = false;
        private bool deplacerBas = false;
        
        private static BitmapImage imgAnt;
        private static BitmapImage imgFly;

        private float currentAngle;

        private static bool tirLangue, expensionLangue;
        private static readonly int expensionLangueVitesse = 60, retractionLangueVitesse = 80;
        private static Vector2 posArme = new Vector2();
        private static int distancePisolet = 20;

        private static BitmapImage imageBalle;
        private static double vitesseBalle = 30.0f;

        private List<Balle> Balles = new List<Balle>(); 
        private List<Ennemis> ennemis = new List<Ennemis>();
        private List<Proies> proies = new List<Proies>();
        //private Player player;
        public static string difficulte;

        int pauseEntreVagues = 5; // en secondes
        int pauseCounter = 0;
        int waveCount = 0;
        private bool isTimerRunning = false;

        //public SoundPlayer musique;
        //public Stream audioStream;
        
        public MainWindow()
        {
            InitImage();
            InitializeComponent(); 

            // Création de la fenêtre parametre avec un Canvas
            parametre fentreNiveau = new parametre();
            fentreNiveau.ShowDialog();  // Affichage de la fenêtre parametre

            // Si la fenêtre parametre est fermée avec DialogResult == false, fermer l'application
            if (fentreNiveau.DialogResult == false)
            {
                Application.Current.Shutdown();
            }
            else
            {
                string resultat = fentreNiveau.Resultat; // Récupérer le résultat de la fenêtre parametre
                                                         // Vérification si le résultat est "parametre", ce qui signifie que le processus doit continuer
                do
                {
                    if (resultat == "parametre")
                    {
                        do
                        {
                            // Affichage du Canvas pour la fenêtre controle
                            choixTouche fentrechoixTouche = new choixTouche();
                            fentrechoixTouche.ShowDialog();  // Affiche la fenêtre controle de manière modale

                            // Si la fenêtre controle est fermée avec DialogResult == false, revenir à la fenêtre parametre
                            if (fentrechoixTouche.DialogResult == false)
                            {
                                // Création d'une nouvelle instance de la fenêtre parametre, on ne peut pas réutiliser l'ancienne
                                fentreNiveau = new parametre();  // Nouvelle instance de parametre
                                fentreNiveau.ShowDialog();  // Réaffiche la fenêtre parametre

                                // Si l'utilisateur choisit "jouer", sortir de la boucle
                                resultat = fentreNiveau.Resultat; // Mettre à jour le résultat
                                if (resultat == "jouer")
                                {
                                    break;  // Quitter la boucle et lancer le jeu
                                }

                                // Si la fenêtre parametre est fermée à nouveau avec DialogResult == false, fermer l'application
                                if (fentreNiveau.DialogResult == false)
                                {
                                    Application.Current.Shutdown();
                                    return;
                                }
                            }
                        }
                        while (resultat == "parametre");  // Continue la boucle si le résultat est encore "parametre"
                    }
                    else if (resultat == "aide")
                    {
                        do
                        {
                            // Affichage du Canvas pour la fenêtre controle
                            aide fentreaide = new aide();
                            fentreaide.ShowDialog();  // Affiche la fenêtre controle de manière modale

                            // Si la fenêtre controle est fermée avec DialogResult == false, revenir à la fenêtre parametre
                            if (fentreaide.DialogResult == false)
                            {
                                // Création d'une nouvelle instance de la fenêtre parametre, on ne peut pas réutiliser l'ancienne
                                fentreNiveau = new parametre();  // Nouvelle instance de parametre
                                fentreNiveau.ShowDialog();  // Réaffiche la fenêtre parametre

                                // Si l'utilisateur choisit "jouer", sortir de la boucle
                                resultat = fentreNiveau.Resultat; // Mettre à jour le résultat
                                if (resultat == "jouer")
                                {
                                    break;  // Quitter la boucle et lancer le jeu
                                }

                                // Si la fenêtre parametre est fermée à nouveau avec DialogResult == false, fermer l'application
                                if (fentreNiveau.DialogResult == false)
                                {
                                    Application.Current.Shutdown();
                                    return;
                                }
                            }
                        }
                        while (resultat == "aide");  // Continue la boucle si le résultat est encore "parametre"
                    }
                } while (resultat != "jouer");
                choixDifficulte fentreDifficulte = new choixDifficulte();
                fentreDifficulte.ShowDialog();  // Affiche la fenêtre controle de manière modale
                difficulte = fentreDifficulte.Resultat;
            }
            lab_Pause.Visibility = Visibility.Collapsed;
            
            InitialiserMinuterie();
            RenderOptions.SetBitmapScalingMode(canvas.Background, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(player, BitmapScalingMode.NearestNeighbor);
            //Measure(new Size(Width, Height));
            //Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));
        }

        void StartWave()
        {
            if (isTimerRunning) return;
            isTimerRunning = true;

            pauseVagues = new DispatcherTimer();
            pauseVagues.Interval = TimeSpan.FromSeconds(1);
            pauseVagues.Tick += NouvelleVague;
            pauseCounter = 0;
            pauseVagues.Start();
        }

        private void NouvelleVague(object? sender, EventArgs e)
        {
            labelWave.Content = $"New wave in {pauseEntreVagues - pauseCounter}!";

            pauseCounter++;
            if (pauseCounter < pauseEntreVagues) return;


            waveCount++;
            // \operatorname{ceil}\left(\sqrt{\left(x\right)}^{3}\right) // LaTeX !!
            int spiderCount = (int) Math.Ceiling(Math.Pow(Math.Sqrt(waveCount),3.0)) % 10;

            labelWave.Content = $"Wave {waveCount}";

            for (int i = 0; i < spiderCount; i++)
            {
                int hautBasGaucheDroite = alea.Next(0, 3);
                if (hautBasGaucheDroite == 0)
                {
                    Ennemis spider = new Ennemis(TypeEnnemis.Spider, alea.Next(0, 100), alea.Next(0, 600), 100, 100, 8, canvas);
                    ennemis.Add(spider);

                    Proies fly = new Proies(TypeProies.Fly, alea.Next(0, 100), alea.Next(0, 600), 50, 50, 3, 500, 200, canvas);
                    proies.Add(fly);
                }
                else if (hautBasGaucheDroite == 1)
                {
                    Ennemis spider = new Ennemis(TypeEnnemis.Spider, alea.Next(0, 1200), alea.Next(50, 150), 100, 100, 8, canvas);
                    ennemis.Add(spider);

                    Proies fly = new Proies(TypeProies.Fly, alea.Next(0, 1200), alea.Next(50, 150), 50, 50, 3, 500, 200, canvas);
                    proies.Add(fly);
                }

                else if (hautBasGaucheDroite == 2)
                {
                    Ennemis spider = new Ennemis(TypeEnnemis.Spider, alea.Next(1100, 1200), alea.Next(0, 600), 100, 100, 8, canvas);
                    ennemis.Add(spider);

                    Proies fly = new Proies(TypeProies.Fly, alea.Next(1100, 1200), alea.Next(0, 600), 50, 50, 3, 500, 200, canvas);
                    proies.Add(fly);
                }
                else
                {
                    Ennemis spider = new Ennemis(TypeEnnemis.Spider, alea.Next(0, 1200), alea.Next(500, 600), 100, 100, 8, canvas);
                    ennemis.Add(spider);

                    Proies fly = new Proies(TypeProies.Fly, alea.Next(0, 1200), alea.Next(500, 600), 50, 50, 3, 500, 200, canvas);
                    proies.Add(fly);
                }
            }
            Console.WriteLine(spiderCount);
            Console.WriteLine(ennemis.Count);
            /*
            //do
            //{
            //    trier = true;
            //    for (int i = 0; i < spiderCount - 1; i++)
            //    {
            //        for (int j = spiderCount - 1; j > i; j--)
            //        {
            //            if (ennemis[i].BoundingBox.IntersectsWith(ennemis[j].BoundingBox))
            //            {
            //                trier=false;
            //                int hautBasGaucheDroite2 = alea.Next(0, 3);
            //                if (hautBasGaucheDroite2 == 0)
            //                {
            //                    ennemis[i].X = alea.Next(0, 100);
            //                    ennemis[i].Y = alea.Next(0, 600);
            //                }
            //                else if (hautBasGaucheDroite2 == 1)
            //                {
            //                    ennemis[i].X = alea.Next(0, 1200);
            //                    ennemis[i].Y = alea.Next(50, 150);
            //                }
            //                else if (hautBasGaucheDroite2 == 2)
            //                {
            //                    ennemis[i].X = alea.Next(1100, 1200);
            //                    ennemis[i].Y = alea.Next(0, 600);
            //                }
            //                else
            //                {
            //                    ennemis[i].X = alea.Next(0, 1200);
            //                    ennemis[i].Y = alea.Next(500, 600);
            //                }
            //            }
            //        }
            //    }
            //} while (trier == false);
            */
            pauseVagues.Stop(); 
            isTimerRunning = false;
        }

        void InitialiserMinuterie()
        {
            minuterie = new DispatcherTimer();
            minuterie.Interval = TimeSpan.FromMilliseconds(16.6666667);
            minuterie.Tick += Loop;
            minuterie.Start();
        }

        private void InitImage()
        {
            imgAnt = new BitmapImage(new Uri("pack://application:,,/img/ant.png"));
            imgFly = new BitmapImage(new Uri("pack://application:,,,/img/ennemis/LL/1.png"));

            imgFrogFront = new BitmapImage(new Uri("pack://application:,,,/img/frog_front.png"));
            imgFrogBack = new BitmapImage(new Uri("pack://application:,,,/img/frog_back.png"));
            imgFrogSide = new BitmapImage(new Uri("pack://application:,,,/img/frog_side.png"));

            imageBalle = new BitmapImage(new Uri("pack://application:,,,/img/balle.png"));
        }

        private void UpdateMousePosition()
        {
            // Get the mouse position once and calculate the direction to player center
            Point mousePos = Mouse.GetPosition(canvas);
            Vector2 posCentreJoueur = new Vector2(
                (float)(posJoueur.X + player.Width / 2.0f),
                (float)(posJoueur.Y + player.Height / 2.0f)
            );

            // Calculate direction vector and angle once
            Vector2 directionSouris = Vector2.Normalize(new Vector2((float)mousePos.X, (float)mousePos.Y) - posCentreJoueur);
            currentAngle = (float)(Math.Atan2(directionSouris.Y, directionSouris.X) * (180 / Math.PI));

            // Update positions for both the weapon and tongue
            UpdateWeaponPosition(mousePos, posCentreJoueur, directionSouris);
            UpdateTonguePosition(mousePos, posCentreJoueur, directionSouris);
        }

        private void UpdateWeaponPosition(Point mousePos, Vector2 posCentreJoueur, Vector2 directionSouris)
        {
            // Calculate weapon position around the player
            float distanceJoueurSouris = Vector2.Distance(posCentreJoueur, new Vector2((float)mousePos.X, (float)mousePos.Y));
            posArme = posCentreJoueur + (directionSouris * distancePisolet);

            // Apply transforms for the weapon
            TransformGroup transformGroup = new TransformGroup();
            ScaleTransform inverseArme = new ScaleTransform();
            RotateTransform rotationArme = new RotateTransform(currentAngle);

            // Flip the weapon image if the mouse is to the left
            inverseArme.ScaleY = directionSouris.X > 0 ? 1 : -1;

            transformGroup.Children.Add(inverseArme);
            transformGroup.Children.Add(rotationArme);
            gun.RenderTransform = transformGroup;

            // Set the position of the weapon
            Canvas.SetTop(gun, posArme.Y);
            Canvas.SetLeft(gun, posArme.X);
        }

        private void UpdateTonguePosition(Point mousePos, Vector2 posCentreJoueur, Vector2 directionSouris)
        {
            // Set tongue rotation based on the calculated angle
            RotateTransform rotationArme = new RotateTransform(currentAngle);
            if (!tirLangue) playerTongue.RenderTransform = rotationArme;

            // Set the position of the tongue
            Canvas.SetTop(playerTongue, directionSouris.X > 0 ? posCentreJoueur.Y : posCentreJoueur.Y + playerTongue.Height / 2.0f);
            Canvas.SetLeft(playerTongue, posCentreJoueur.X);
        }

        public static bool TryGetIntersection(Line line1, Line line2, out Point intersection)
        {
            // explication: https://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
            Point p = new Point(line1.X1, line1.Y1);
            Point r = new Point(line1.X2 - line1.X1, line1.Y2 - line1.Y1);
            Point q = new Point(line2.X1, line2.Y1);
            Point s = new Point(line2.X2 - line2.X1, line2.Y2 - line2.Y1);

            double rCrossS = CrossProduct(r, s);
            Point qMinusP = new Point(q.X - p.X, q.Y - p.Y);
            double qMinusPCrossS = CrossProduct(qMinusP, s);
            double qMinusPCrossR = CrossProduct(qMinusP, r);

            if (rCrossS == 0)
            {
                if (qMinusPCrossR == 0)
                {
                    double t0 = (qMinusP.X * r.X + qMinusP.Y * r.Y) / (r.X * r.X + r.Y * r.Y);
                    double t1 = t0 + (s.X * r.X + s.Y * r.Y) / (r.X * r.X + r.Y * r.Y);

                    if ((t0 >= 0 && t0 <= 1) || (t1 >= 0 && t1 <= 1))
                    {
                        intersection = new Point(p.X + t0 * r.X, p.Y + t0 * r.Y);
                        return true;
                    }
                }
                intersection = new Point();
                return false;
            }

            if (rCrossS != 0)
            {
                double t = qMinusPCrossS / rCrossS;
                double u = qMinusPCrossR / rCrossS;
                
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                {
                    intersection = new Point(p.X + t * r.X, p.Y + t * r.Y);
                    return true;
                }
            }

            intersection = new Point();
            return false;
        }
        
        private static double CrossProduct(Point v1, Point v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }

        private Point RotatePoint(Point point, Point center, double a)
        {
            double radians = a * (Math.PI / 180); // Convert angle to radians
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            // Translate point to origin
            double x = point.X - center.X;
            double y = point.Y - center.Y;

            // Rotate point
            double rotatedX = x * cos - y * sin + center.X;
            double rotatedY = x * sin + y * cos + center.Y;

            return new Point(rotatedX, rotatedY);
        }

        private void Loop(object? sender, EventArgs e) 
        {
            if (pause) return;
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            // if no enemies start a wave
            if (ennemis.Count <= 0 && proies.Count <= 0)
            {
                StartWave();
            }

            Rect playerRect = new Rect(posJoueur.X, posJoueur.Y, player.Width, player.Height);
            Player.UpdatePlayer();
            Ennemis.UpdateEnnemis(ennemis, playerRect, Balles, canvas);
            Proies.UpdateProies(proies, playerRect);

            CheckOutofboundsBullets();
            CheckEatingFly();

            //fix direction:
            if (deplacerBas)                         directionJoueur = Directions.down;
            else if (deplacerHaut)                   directionJoueur = Directions.up;
            else if (deplacerDroite)                 directionJoueur = Directions.right;
            else if (deplacerGauche)                 directionJoueur = Directions.left;
            else if (deplacerBas && deplacerGauche)  directionJoueur = Directions.diagDownLeft;
            else if (deplacerBas && deplacerDroite)  directionJoueur = Directions.diagDownRight;
            else if (deplacerHaut && deplacerGauche) directionJoueur = Directions.diagUpLeft;
            else if (deplacerHaut && deplacerDroite) directionJoueur = Directions.diagUpRight;
            Player.SetPlayerImage(directionJoueur);
            // Inverse l'image du joueur si nécessaire
            bool doitFlip = (directionJoueur == Directions.left || directionJoueur == Directions.diagUpLeft || directionJoueur == Directions.diagDownLeft);
            if (doitFlip) joueurFlip.ScaleX = 1;
            else joueurFlip.ScaleX = -1;

            player.RenderTransformOrigin = new Point(0.5, 0.5);
            player.RenderTransform = joueurFlip;

            if (directionJoueur == Directions.left || directionJoueur == Directions.right) player.Source = imgFrogSide;
            if (directionJoueur == Directions.up || directionJoueur == Directions.diagUpLeft || directionJoueur == Directions.diagUpRight) player.Source = imgFrogBack;
            if (directionJoueur == Directions.down || directionJoueur == Directions.diagDownLeft || directionJoueur == Directions.diagDownRight) player.Source = imgFrogFront;

            UpdateMousePosition();

            if (deplacerHaut && Canvas.GetTop(player) > 0) vitesseJoueur.Y = -vitesseDeplacement;  // bouger vers le haut
            else if (deplacerBas && Canvas.GetTop(player) < grid.ActualHeight-player.ActualHeight) vitesseJoueur.Y = vitesseDeplacement; // bouger vers le bas 
            else
            {
                // réduire la vitesse du joueur en fonction de la friction
                vitesseJoueur.Y *= friction;
                // si la vitesse (positive) est inférieure à 0.1f, arrêter le mouvement
                if (Math.Abs(vitesseJoueur.Y) < 0.1f) vitesseJoueur.Y = 0;
            }
            posJoueur.Y += vitesseJoueur.Y;

            if      (deplacerDroite && Canvas.GetLeft(player) < grid.ActualWidth-player.ActualWidth) vitesseJoueur.X = vitesseDeplacement;  // bouger vers la droite
            else if (deplacerGauche && Canvas.GetLeft(player) > 0)                                   vitesseJoueur.X = -vitesseDeplacement; // bouger vers la gauche
            else
            {
                // réduire la vitesse du joueur en fonction de la friction
                vitesseJoueur.X *= friction;
                // si la vitesse (positive) est inférieure à 0.1f, arrêter le mouvement
                if (Math.Abs(vitesseJoueur.X) < 0.1f) vitesseJoueur.X = 0;
            }
                
            posJoueur.X += vitesseJoueur.X;
            Canvas.SetLeft(player, posJoueur.X);
            Canvas.SetTop(player, posJoueur.Y);

            //stopwatch.Stop();
            //Console.WriteLine($"Loop execution time: {stopwatch.Elapsed} ");
        }

        private void CheckOutofboundsBullets()
        {
            for (int i = 0; i < Balles.Count; i++)
            {
                Balle balle = Balles[i];
                balle.UpdatePositionBalles();
                if (balle.X < -balle.BalleImage.ActualWidth || balle.Y < -balle.BalleImage.ActualHeight
                 || balle.X > grid.ActualWidth || balle.Y > grid.ActualHeight)
                {
                    Balles.RemoveAt(i);
                    canvas.Children.Remove(balle.BalleImage);
                }
            }
        }

        private void CheckEatingFly()
        {
            if (expensionLangue)
            {
                if (playerTongue.Width < 300)
                {
                    // create two lines from start to end of tongue
                    var rotation = (RotateTransform)playerTongue.RenderTransform;
                    Point tcentre = new Point(Canvas.GetLeft(playerTongue), Canvas.GetTop(playerTongue) + playerTongue.Height / 2.0f);

                    Point t11 = new Point(Canvas.GetLeft(playerTongue), Canvas.GetTop(playerTongue));
                    Point t12 = new Point(Canvas.GetLeft(playerTongue) + playerTongue.Width, Canvas.GetTop(playerTongue));

                    Point t21 = new Point(Canvas.GetLeft(playerTongue), Canvas.GetTop(playerTongue) + playerTongue.Height);
                    Point t22 = new Point(Canvas.GetLeft(playerTongue) + playerTongue.Width, Canvas.GetTop(playerTongue) + playerTongue.Height);

                    Point point_start_1 = RotatePoint(t11, tcentre, rotation.Angle);
                    Point point_start_2 = RotatePoint(t21, tcentre, rotation.Angle);
                    Point point_end_1 = RotatePoint(t22, tcentre, rotation.Angle);
                    Point point_end_2 = RotatePoint(t12, tcentre, rotation.Angle);

                    Line line_frog_1 = new Line
                    {
                        X1 = point_start_1.X,
                        Y1 = point_start_1.Y,
                        X2 = point_end_1.X,
                        Y2 = point_end_1.Y,
                        StrokeThickness = 2,
                        Stroke = Brushes.Red
                    };

                    Line line_frog_2 = new Line
                    {
                        X1 = point_start_2.X,
                        Y1 = point_start_2.Y,
                        X2 = point_end_2.X,
                        Y2 = point_end_2.Y,
                        StrokeThickness = 2,
                        Stroke = Brushes.Red
                    };

                    //canvas.Children.Add(line_frog_1);
                    //canvas.Children.Add(line_frog_2);

                    foreach (var proie in proies.ToList())
                    {
                        /*
                         *   A--------B 
                         *   |        |
                         *   |        |
                         *   C--------D
                         */

                        Point intersection;
                        Line line_AB = new Line
                        {
                            X1 = proie.BoundingBox.X,
                            Y1 = proie.BoundingBox.Y,
                            X2 = proie.BoundingBox.X + proie.BoundingBox.Width,
                            Y2 = proie.BoundingBox.Y,
                            StrokeThickness = 2,
                            Stroke = Brushes.Red
                        };
                        Line line_BD = new Line
                        {
                            X1 = proie.BoundingBox.X + proie.BoundingBox.Width,
                            Y1 = proie.BoundingBox.Y,
                            X2 = proie.BoundingBox.X + proie.BoundingBox.Width,
                            Y2 = proie.BoundingBox.Y + proie.BoundingBox.Height,
                            StrokeThickness = 2,
                            Stroke = Brushes.Red
                        };
                        Line line_DC = new Line
                        {
                            X1 = proie.BoundingBox.X + proie.BoundingBox.Width,
                            Y1 = proie.BoundingBox.Y + proie.BoundingBox.Height,
                            X2 = proie.BoundingBox.X,
                            Y2 = proie.BoundingBox.Y + proie.BoundingBox.Height,
                            StrokeThickness = 2,
                            Stroke = Brushes.Red
                        };
                        Line line_CA = new Line
                        {
                            X1 = proie.BoundingBox.X,
                            Y1 = proie.BoundingBox.Y + proie.BoundingBox.Height,
                            X2 = proie.BoundingBox.X,
                            Y2 = proie.BoundingBox.Y,
                            StrokeThickness = 2,
                            Stroke = Brushes.Red
                        };

                        //canvas.Children.Add(line_AB);
                        //canvas.Children.Add(line_BD);
                        //canvas.Children.Add(line_DC);
                        //canvas.Children.Add(line_CA);

                        if (TryGetIntersection(line_frog_1, line_AB, out intersection)
                         || TryGetIntersection(line_frog_1, line_BD, out intersection)
                         || TryGetIntersection(line_frog_1, line_DC, out intersection)
                         || TryGetIntersection(line_frog_1, line_CA, out intersection)

                         || TryGetIntersection(line_frog_2, line_AB, out intersection)
                         || TryGetIntersection(line_frog_2, line_BD, out intersection)
                         || TryGetIntersection(line_frog_2, line_DC, out intersection)
                         || TryGetIntersection(line_frog_2, line_CA, out intersection))
                        {
                            Console.WriteLine($"ate a {proie.type}");
                            expensionLangue = false;
                            line_frog_1.Stroke = Brushes.Green;
                            line_frog_2.Stroke = Brushes.Green;
                            canvas.Children.Remove(proie.Image);
                            proies.Remove(proie);
                        }
                    }
                    if (expensionLangue) playerTongue.Width += expensionLangueVitesse;
                }
                else
                {
                    expensionLangue = false;
                }
            }
            else
            {
                if (playerTongue.Width > 0)
                {
                    if (playerTongue.Width <= retractionLangueVitesse) playerTongue.Width = 0;
                    else playerTongue.Width -= retractionLangueVitesse;
                }
                else tirLangue = false;
            }

        }

        private void ShootTung()
        {
            //SonLangue();
            if (tirLangue) return;
            else tirLangue = true;
            expensionLangue = true;
        }
        private void SonGun()
        {
            //// Charger le fichier audio depuis les ressources
            //Uri audioUri = new Uri("/son/coupdefeu.wav", UriKind.RelativeOrAbsolute);
            //Stream audioStream = Application.GetResourceStream(audioUri).Stream;
            //// Créer un objet SoundPlayer pour lire le son
            //SoundPlayer musique = new SoundPlayer(audioStream);
            //musique.Play();
        }
        private void SonLangue()
        {
            //// Charger le fichier audio depuis les ressources
            //Uri audioUri = new Uri("/son/langue.wav", UriKind.RelativeOrAbsolute);
            //Stream audioStream = Application.GetResourceStream(audioUri).Stream;
            //// Créer un objet SoundPlayer pour lire le son
            //SoundPlayer musique = new SoundPlayer(audioStream);
            //musique.Play();
        }
        
        private void ShootGun()
        {
            //SonGun();
            Console.WriteLine("test");
            double a = currentAngle * Math.PI / 180.0;
            Balle balle = new Balle(posArme.X, posArme.Y, a, vitesseBalle, 10, canvas, imageBalle);
            Balles.Add(balle);
        }

        public void AfficheScore(int score)
        {
            labelScore.Content = $"Score : {score} ";
        }

        public void AfficheCombo(double combo)
        {
            labelScore.Content = $"Combo : {Math.Round(combo, 2)} ";
        }

        private void keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D && !pause)
            {
                deplacerDroite = true;
                deplacerGauche = false;
                directionJoueur = Directions.right;
            }
            if ((e.Key == Key.Q || e.Key == Key.A) && !pause)
            {
                deplacerGauche = true;
                deplacerDroite = false;
                directionJoueur = Directions.left;
            }
            if (e.Key == Key.S && !pause)
            {
                deplacerBas = true;
                deplacerHaut = false;
                directionJoueur = Directions.down;
            }
            if ((e.Key == Key.Z || e.Key == Key.W) && !pause)
            {
                deplacerHaut = true;
                deplacerBas = false;
                directionJoueur = Directions.up;
            }
            if (e.Key == Key.Escape || e.Key == Key.Space )
            {
                pause=!pause;
                if (pause)
                    lab_Pause.Visibility = Visibility.Visible;
                else
                    lab_Pause.Visibility = Visibility.Collapsed;
            }
        }

        private void keyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D)
            {
                deplacerDroite = false;
            }
            if (e.Key == Key.Q || e.Key == Key.A)
            {
                deplacerGauche = false;
            }
            if (e.Key == Key.S)
            {
                deplacerBas = false;
            }
            if (e.Key == Key.Z || e.Key == Key.W)
            {
                deplacerHaut = false;
            }

        }

        private void leftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (pause) return;
            Console.WriteLine("test");
            ShootGun();
        }

        private void rightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (pause) return;
            ShootTung();
        }
    }
}