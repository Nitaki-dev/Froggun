using System.Numerics;
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

namespace Froggun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Random alea = new Random();
        
        private int score = 0;
        private int temps = 60;
        private int health = 5;
        private double speedFactorFirefly = 0.5;
        private double speedFactorAraignee = 0.8;
        private static DispatcherTimer minuterie = new DispatcherTimer();
        private static DispatcherTimer tempsRestant = new DispatcherTimer();
        private static DispatcherTimer minVagues = new DispatcherTimer();
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
            InitialiserMinuterie();
            Minuterie();
            InitObjects();
            MinuterieVagues();
            RenderOptions.SetBitmapScalingMode(canvas.Background, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(player, BitmapScalingMode.NearestNeighbor);
        }
        void MinuterieVagues()
        {
            minVagues = new DispatcherTimer();
            minVagues.Interval = TimeSpan.FromSeconds(12);
            minVagues.Tick += NouvelleVague;
            minVagues.Start();
            NouvelleVague(this, EventArgs.Empty);
        }
        private void NouvelleVague(object? sender, EventArgs e)
        {
            for (int i = 0; i < alea.Next(1,5); i++)
            {
                int hautOuBas = alea.Next(0, 1);
                if (hautOuBas == 0)
                {
                    Ennemis spider = new Ennemis(TypeEnnemis.Spider, 100, 100, alea.Next(100, 1100), alea.Next(50, 200), 100, 100, 8, canvas);
                    ennemis.Add(spider);
                }
                else
                {
                    Ennemis spider = new Ennemis(TypeEnnemis.Spider, 100, 100, alea.Next(100, 1100), alea.Next(500, 600), 100, 100, 8, canvas);
                    ennemis.Add(spider);
                }
            }
        }
        void InitialiserMinuterie()
        {
            minuterie = new DispatcherTimer();
            minuterie.Interval = TimeSpan.FromMilliseconds(16.6666667);
            //minuterie.Tick += EnnemiesAttack;
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
            string imageDirectory = "img/ennemis/LL";
            int[] animationFrames = new int[] { 1, 2, 3, 1, 4, 5 };

            Proies fly1 = new Proies(TypeProies.Fly,
                600, 600, //position
                50, 50,   // taille
                3, 500, 100, // vitesse, tail max du prochain pas, et delai entre chaque pas
                new Vector2(0,0), new Vector2((float)grid.ActualWidth, (float)grid.ActualHeight), canvas); // zone où la proie peut navigeur

            proies.Add(fly1);

            //string imageDirectory1 = "img/ennemis/Food1";
            //int[] animationFrames1 = new int[] { 1, 2 };
            //Ennemis Food = new Ennemis(300, 200, 64, 64, 8, canvas, imageDirectory1, animationFrames1);

            //ennemis.Add(Food);
        }

        private void InitScore(int ajout)
        {
            score += ajout;
        }

        private void PivoterArme() {
            posArme.X = (float)Mouse.GetPosition(canvas).X;
            posArme.Y = (float)Mouse.GetPosition(canvas).Y;

            // Centre du joueur
            Vector2 posCentreJoueur = new Vector2(
                (float)(posJoueur.X + player.Width / 2.0f),
                (float)(posJoueur.Y + player.Height / 6.0f)
            );

            // Trouve la position de l'arme autour du joueur
            Vector2 directionSouris = Vector2.Normalize(posArme - posCentreJoueur);
            float distanceJoueurSouris = Vector2.Distance(posCentreJoueur, posArme);
            posArme = posCentreJoueur + (directionSouris * distancePisolet);
            
            // Trouve l'angle de l'arme du joueur
            float angle = (float) (Math.Atan2(directionSouris.Y, directionSouris.X) * (180 / Math.PI));

            // Initialisation des transformes de l'image de l'arme du joueur
            TransformGroup myTransformGroup = new TransformGroup();
            ScaleTransform inverseArme = new ScaleTransform();
            RotateTransform rotationArme = new RotateTransform(angle);

            // Inverser l'image de l'arme si à gauche
            if (directionSouris.X > 0) inverseArme.ScaleY = 1;
            else inverseArme.ScaleY = -1;

            //inverseArme.ScaleX = -1;
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

        //public bool DoesRectIntersectLine(double x0, double y0, double x1, double y1, Rect rect)
        //{
        //    x0 = Math.Round(x0);
        //    y0 = Math.Round(y0);
        //    x1 = Math.Round(x1);
        //    y1 = Math.Round(y1);
            
        //    Point point1 = new Point(x0, y0);
        //    Point point2 = new Point(x1, y1);

        //    LineGeometry lineGeometry = new LineGeometry(point1, point2);

        //    Rect geometryRect = rect;

        //    return lineGeometry.Bounds.IntersectsWith(geometryRect);
        //}


        //private Point RotatePoint(Point point, Point center, double angle)
        //{
        //    double radians = angle * (Math.PI / 180); // Convert angle to radians
        //    double cos = Math.Cos(radians);
        //    double sin = Math.Sin(radians);

        //    // Translate point to origin
        //    double x = point.X - center.X;
        //    double y = point.Y - center.Y;

        //    // Rotate point
        //    double rotatedX = x * cos - y * sin + center.X;
        //    double rotatedY = x * sin + y * cos + center.Y;

        //    return new Point(rotatedX, rotatedY);
        //}

        private void Loop(object? sender, EventArgs e) 
        {
            Rect playerRect = new Rect(posJoueur.X, posJoueur.Y, player.Width, player.Height);

            Ennemis.UpdateEnnemis(ennemis, playerRect, Balles);
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

            //if (expensionLangue)
            //{
            //    if (playerTongue.Width < 300)
            //    {
            //        playerTongue.Width = 300;
            //        var rotation = (RotateTransform)playerTongue.RenderTransform;
            //        Point t1 = new Point(Canvas.GetLeft(playerTongue), Canvas.GetTop(playerTongue));
            //        Point t2 = new Point(Canvas.GetLeft(playerTongue)+playerTongue.Width, Canvas.GetTop(playerTongue)+playerTongue.Height);

            //        Point point1 = RotatePoint(t1, t1, rotation.Angle);
            //        Point point2 = RotatePoint(t2, t1, rotation.Angle);

            //        // Define the rectangle
            //        Rect rect = new Rect(100, 100, 150, 100);

            //        // Add line to canvas
            //        Line line = new Line
            //        {
            //            X1 = point1.X,
            //            Y1 = point1.Y,
            //            X2 = point2.X,
            //            Y2 = point2.Y,
            //            Stroke = Brushes.White,
            //            StrokeThickness = 2
            //        };
            //        canvas.Children.Add(line);

            //        // Add rectangle to canvas
            //        Rectangle rectangle = new Rectangle
            //        {
            //            Width = rect.Width,
            //            Height = rect.Height,
            //            Stroke = Brushes.Red,
            //            StrokeThickness = 2,
            //            Fill = Brushes.Transparent
            //        };
            //        Canvas.SetLeft(rectangle, rect.X);
            //        Canvas.SetTop(rectangle, rect.Y);
            //        canvas.Children.Add(rectangle);

            //        // Check if rectangle intersects with the line
            //        if (DoesRectIntersectLine(point1.X,point1.Y, point2.X, point2.Y, rect))
            //        {
            //            line.Stroke = Brushes.Green; // Change the line color if intersected
            //        }

            //        //foreach (var proie in proies.ToList())
            //        //{
            //        //    bool test = DoesRectIntersectLine(Canvas.GetLeft(playerTongue), Canvas.GetTop(playerTongue), playerTongue.Width, playerTongue.Height, proie.BoundingBox);
            //        //    Console.WriteLine(test);
            //        //    //proies.Remove(proie);
            //        //    //canvas.Children.Remove(proie.Image);
            //        //    //expensionLangue = false;
            //        //}

            //        //if (expensionLangue) playerTongue.Width += expensionLangueVitesse;
            //    } else
            //    {
            //        expensionLangue = false;
            //    }
            //} 
            //else
            //{
            //    if (playerTongue.Width > 0)
            //    {
            //        if (playerTongue.Width <= retractionLangueVitesse) playerTongue.Width = 0;
            //        else playerTongue.Width -= retractionLangueVitesse;
            //    }
            //    else tirLangue = false;
            //}

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

        private void keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D)
            {
                deplacerDroite = true;
                deplacerGauche = false;
                directionJoueur = Directions.right;
            }
            if (e.Key == Key.Q || e.Key == Key.A)
            {
                deplacerGauche = true;
                deplacerDroite = false;
                directionJoueur = Directions.left;
            }
            if (e.Key == Key.S)
            {
                deplacerBas = true;
                deplacerHaut = false;
                directionJoueur = Directions.down;
            }
            if (e.Key == Key.Z || e.Key == Key.W)
            {
                deplacerHaut = true;
                deplacerBas = false;
                directionJoueur = Directions.up;
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
            ShootGun();
        }

        private void rightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShootTung();
        }
    }
}