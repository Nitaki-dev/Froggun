﻿using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using System.Windows.Media.Media3D;
using System.Reflection.PortableExecutable;

namespace Froggun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Random alea = new Random();
        
        private int score = 0;
        private bool pause = false;
        private int temps = 60;
        private int health = 5;
        private double speedFactorFirefly = 0.5;
        private double speedFactorAraignee = 0.8;
        private static DispatcherTimer minuterie = new DispatcherTimer();
        private static DispatcherTimer tempsRestant = new DispatcherTimer();
        private static DispatcherTimer pauseVagues = new DispatcherTimer();
        private Rect playerR = new Rect();

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

        private const float forceSaut = 15.0f;
        private const float vitesseMaxChute = 9.8f;
        private const float vitesseDeplacement = 8.0f;
        private const float friction = 0.4f;
        private const float gravite = 0.5f;
        
        private bool verrouillageMouvement = false;
        private bool estAuSol = false;
        private bool plongeVersSol = false;
        private bool deplacerGauche = false;
        private bool deplacerDroite = false;
        private bool deplacerHaut = false;
        private bool deplacerBas = false;
        
        private const int nbAnts = 3;
        private const int nbFireflys = 5;
        private static List<Image> ants;
        private static List<Image> fireflys;
        private static BitmapImage imgAnt;
        private static BitmapImage imgFly;
        List<Rect> rectanglesfireflys = new List<Rect>();
        List<Rect> rectanglesants = new List<Rect>();
        private static Vector2 posSouris = new Vector2();
        private double moveSpeed = 20.0;

        private static Vector2 posGun = new Vector2();

        private static Vector2 posLangue = new Vector2();
        private static bool tirLangue, expensionLangue;
        private static readonly int expensionLangueVitesse = 60, retractionLangueVitesse = 80;
        private static Vector2 posArme = new Vector2();
        private static int distancePisolet = 20;

        private static Vector2 dirBalle = new Vector2();
        private static BitmapImage imageBalle;
        private static double vitesseBalle = 30.0f;

        private List<Balle> Balles = new List<Balle>(); 
        private List<Ennemis> ennemis = new List<Ennemis>();
        private List<Proies> proies = new List<Proies>();
        public static string difficulte;

        int pauseEntreVagues = 1; // en secondes
        //int pauseEntreVagues = 5; // en secondes
        int pauseCounter = 0;
        int waveCount = 0;
        private bool isTimerRunning = false;

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
            Minuterie();
            InitObjects();
            RenderOptions.SetBitmapScalingMode(canvas.Background, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(player, BitmapScalingMode.NearestNeighbor);
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
                int hautOuBas = alea.Next(0, 1);
                if (hautOuBas == 0)
                {
                    Ennemis spider = new Ennemis(TypeEnnemis.Spider, alea.Next(100, 1100), alea.Next(50, 200), 100, 100, 8, canvas);
                    ennemis.Add(spider);
                }
                else
                {
                    Ennemis spider = new Ennemis(TypeEnnemis.Spider, alea.Next(100, 1100), alea.Next(500, 600), 100, 100, 8, canvas);
                    ennemis.Add(spider);
                }
            }
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

        private void Minuterie()
        {
            tempsRestant = new DispatcherTimer();
            tempsRestant.Interval = TimeSpan.FromSeconds(1);
            tempsRestant.Tick += tempsEnMoins;
            tempsRestant.Start();
            playerR = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);

        }

        private void tempsEnMoins(object? sender, EventArgs e)
        {
            temps--;
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

        private void InitObjects()
        {
            Proies fly1 = new Proies(TypeProies.Fly,
                600, 600, //position
                50, 50,   // taille
                3, 500, 200, // vitesse, tail max du prochain pas, et delai entre chaque pas
                new Vector2(0, 0), new Vector2((float)1700, (float)600), canvas); // zone où la proie peut navigeur

            proies.Add(fly1);
        }

        private void InitScore(int ajout)
        {
            score += ajout;
        }

        private void PivoterArme()
        {
            posArme.X = (float)Mouse.GetPosition(canvas).X;
            posArme.Y = (float)Mouse.GetPosition(canvas).Y;

            // Centre du joueur
            Vector2 posCentreJoueur = new Vector2(
                (float)(posJoueur.X + player.Width / 2.0f),
                (float)(posJoueur.Y + player.Height / 2.0f)
            );

            // Trouve la position de l'arme autour du joueur
            Vector2 directionSouris = Vector2.Normalize(posArme - posCentreJoueur);
            float distanceJoueurSouris = Vector2.Distance(posCentreJoueur, posArme);
            posArme = posCentreJoueur + (directionSouris * distancePisolet);

            // Trouve l'angle de l'arme du joueur
            float angle = (float)(Math.Atan2(directionSouris.Y, directionSouris.X) * (180 / Math.PI));

            // Initialisation des transformes de l'image de l'arme du joueur
            TransformGroup myTransformGroup = new TransformGroup();
            ScaleTransform inverseArme = new ScaleTransform();
            RotateTransform rotationArme = new RotateTransform(angle);

            // Inverser l'image de l'arme si à gauche
            if (directionSouris.X > 0) inverseArme.ScaleY = 1;
            else inverseArme.ScaleY = -1;

            gun.RenderTransform = rotationArme;
            gun.RenderTransform = inverseArme;

            myTransformGroup.Children.Add(inverseArme);
            myTransformGroup.Children.Add(rotationArme);

            gun.RenderTransform = myTransformGroup;

            Canvas.SetTop(gun, posArme.Y);
            Canvas.SetLeft(gun, posArme.X);
        }

        private void PivoterLangue()
        {
            posLangue.X = (float)Mouse.GetPosition(canvas).X;
            posLangue.Y = (float)Mouse.GetPosition(canvas).Y;

            Vector2 posCentreJoueur = new Vector2(
                (float)(posJoueur.X + player.Width / 2.0f),
                (float)(posJoueur.Y + player.Height / 2.0f)
            );

            Vector2 directionSouris = Vector2.Normalize(posLangue - posCentreJoueur);

            float angle = (float)(Math.Atan2(directionSouris.Y, directionSouris.X) * (180 / Math.PI));
            RotateTransform rotationArme = new RotateTransform(angle);
            
            if (!tirLangue) playerTongue.RenderTransform = rotationArme;
            
            Canvas.SetTop(playerTongue, directionSouris.X > 0 ? posCentreJoueur.Y : posCentreJoueur.Y + playerTongue.Height/2.0f);
            Canvas.SetLeft(playerTongue, posCentreJoueur.X);
        }

        public static bool TryGetIntersection(Line line1, Line line2, out Point intersection)
        {
            // explication de https://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
            Point p = new Point(line1.X1, line1.Y1);
            Point r = new Point(line1.X2 - line1.X1, line1.Y2 - line1.Y1); // r = (X2 - X1, Y2 - Y1)
            Point q = new Point(line2.X1, line2.Y1);
            Point s = new Point(line2.X2 - line2.X1, line2.Y2 - line2.Y1); // s = (X2 - X1, Y2 - Y1)

            // Calculate cross products (2D vectors)
            double rCrossS = CrossProduct(r, s);
            Point qMinusP = new Point(q.X - p.X, q.Y - p.Y);
            double qMinusPCrossS = CrossProduct(qMinusP, s);
            double qMinusPCrossR = CrossProduct(qMinusP, r);

            // Case 1: r × s == 0 (collinear or parallel)
            if (rCrossS == 0)
            {
                // If (q - p) × r == 0, the lines are collinear
                if (qMinusPCrossR == 0)
                {
                    // Check if the segments overlap
                    double t0 = (qMinusP.X * r.X + qMinusP.Y * r.Y) / (r.X * r.X + r.Y * r.Y);
                    double t1 = t0 + (s.X * r.X + s.Y * r.Y) / (r.X * r.X + r.Y * r.Y);

                    // If the segments overlap, return the intersection point
                    if ((t0 >= 0 && t0 <= 1) || (t1 >= 0 && t1 <= 1))
                    {
                        intersection = new Point(p.X + t0 * r.X, p.Y + t0 * r.Y); // Use t0 as the intersection point
                        return true;
                    }
                }
                // If the lines are parallel and not collinear
                intersection = new Point();
                return false;
            }

            // Case 2: r × s != 0 (lines are not parallel)
            if (rCrossS != 0)
            {
                double t = qMinusPCrossS / rCrossS;
                double u = qMinusPCrossR / rCrossS;

                // Check if the intersection point lies within the bounds of both segments
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                {
                    intersection = new Point(p.X + t * r.X, p.Y + t * r.Y);
                    return true;
                }
            }

            // Case 3: Lines are parallel but do not intersect
            intersection = new Point();
            return false;
        }
        
        private static double CrossProduct(Point v1, Point v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }

        private Point RotatePoint(Point point, Point center, double angle)
        {
            double radians = angle * (Math.PI / 180); // Convert angle to radians
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
        { if (pause) return;

            // if no enemies start a wave
            if (ennemis.Count <= 0 && proies.Count <= 0)
            {
                //StartWave();
            }

            Rect playerRect = new Rect(posJoueur.X, posJoueur.Y, player.Width, player.Height);

            Ennemis.UpdateEnnemis(ennemis, playerRect, Balles, canvas);
            Proies.UpdateProies(proies, playerRect);

            for (int i = 0; i < Balles.Count; i++)
            {
                Balle balle = Balles[i];
                balle.UpdatePositionBalles();
                if (balle.X < -balle.BalleImage.ActualWidth || balle.Y < -balle.BalleImage.ActualHeight
                 || balle.X > grid.ActualWidth || balle.Y > grid.ActualHeight) {
                    Balles.RemoveAt(i);
                    canvas.Children.Remove(balle.BalleImage);
                }
            }

            if (expensionLangue)
            {
                if (playerTongue.Width < 300)
                {
                    Vector2 posCentreJoueur = new Vector2(
                        (float)(posJoueur.X + player.Width / 2.0f),
                        (float)(posJoueur.Y + player.Height / 2.0f)
                    );

                    Line test = new Line
                    {
                        X1 = posCentreJoueur.X - 1,
                        Y1 = posCentreJoueur.Y - 1,
                        X2 = posCentreJoueur.X + 1,
                        Y2 = posCentreJoueur.Y + 1,
                        Stroke = Brushes.Red,
                        StrokeThickness = 2
                    };

                    // create two lines from start to end of tongue
                    var rotation = (RotateTransform)playerTongue.RenderTransform;
                    Point tcentre = new Point(Canvas.GetLeft(playerTongue), Canvas.GetTop(playerTongue) + playerTongue.Height/2.0f);

                    Point t11 = new Point(Canvas.GetLeft(playerTongue), Canvas.GetTop(playerTongue));
                    Point t12 = new Point(Canvas.GetLeft(playerTongue)+playerTongue.Width, Canvas.GetTop(playerTongue));

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
                            X2 = proie.BoundingBox.Y,
                            Y2 = proie.BoundingBox.Y,
                            StrokeThickness = 2,
                            Stroke = Brushes.Red
                        };

                        //canvas.Children.Add(line_AB);
                        //canvas.Children.Add(line_BD);
                        //canvas.Children.Add(line_DC);
                        //canvas.Children.Add(line_CA);

                        //canvas.Children.Add(line_frog_1);
                        //canvas.Children.Add(line_frog_2);

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

            //fix direction:
            if      (deplacerBas)                    directionJoueur = Directions.down;
            else if (deplacerHaut)                   directionJoueur = Directions.up;
            else if (deplacerDroite)                 directionJoueur = Directions.right;
            else if (deplacerGauche)                 directionJoueur = Directions.left;
            else if (deplacerBas && deplacerGauche)  directionJoueur = Directions.diagDownLeft;
            else if (deplacerBas && deplacerDroite)  directionJoueur = Directions.diagDownRight;
            else if (deplacerHaut && deplacerGauche) directionJoueur = Directions.diagUpLeft;
            else if (deplacerHaut && deplacerDroite) directionJoueur = Directions.diagUpRight;

            // Inverse l'image du joueur si nécessaire
            bool doitFlip = (directionJoueur == Directions.left || directionJoueur == Directions.diagUpLeft || directionJoueur == Directions.diagDownLeft);
            if (doitFlip) joueurFlip.ScaleX = 1;
            else joueurFlip.ScaleX = -1;

            player.RenderTransformOrigin = new Point(0.5, 0.5);
            player.RenderTransform = joueurFlip;

            if (directionJoueur == Directions.left || directionJoueur == Directions.right) player.Source = imgFrogSide;
            if (directionJoueur == Directions.up || directionJoueur == Directions.diagUpLeft || directionJoueur == Directions.diagUpRight) player.Source = imgFrogBack;
            if (directionJoueur == Directions.down || directionJoueur == Directions.diagDownLeft || directionJoueur == Directions.diagDownRight) player.Source = imgFrogFront;

            PivoterArme();
            PivoterLangue();

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

            if (deplacerDroite && Canvas.GetLeft(player) < grid.ActualWidth-player.ActualWidth) vitesseJoueur.X = vitesseDeplacement;  // bouger vers la droite
            else if (deplacerGauche && Canvas.GetLeft(player) > 0) vitesseJoueur.X = -vitesseDeplacement; // bouger vers la gauche
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
            
        }

        private void ShootTung()
        {
            if (tirLangue) return;
            else tirLangue = true;
            expensionLangue = true;
        }

        private void ShootGun()
        {
            Vector2 dirBalle = new Vector2((float)Mouse.GetPosition(canvas).X, (float)Mouse.GetPosition(canvas).Y);

            // Centre du joueur
            Vector2 posCentreJoueur = new Vector2(
                (float)(posJoueur.X + player.Width / 2.0f),
                (float)(posJoueur.Y + player.Height / 6.0f)
            );

            Vector2 directionSouris = Vector2.Normalize(dirBalle - posCentreJoueur);
            double angle = Math.Atan2(directionSouris.Y, directionSouris.X);

            Balle balle = new Balle(posArme.X, posArme.Y, angle, vitesseBalle, 10, canvas, imageBalle);
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
            ShootGun();
        }

        private void rightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (pause) return;
            ShootTung();
        }
    }
}