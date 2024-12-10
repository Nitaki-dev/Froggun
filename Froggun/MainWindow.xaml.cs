using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Froggun.MainWindow;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        private static DispatcherTimer minuterie = new DispatcherTimer();
        private static DispatcherTimer tempsRestant = new DispatcherTimer();

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
        private const int nbFireflys = 3;
        private static List<Image> ants;
        private static List<Image> fireflys;
        private static BitmapImage imgAnt;
        private static BitmapImage imgFly;
        private static Vector2 posSouris = new Vector2();
        private double moveSpeed = 20.0;

        private static Vector2 posGun = new Vector2();

        private static Vector2 posLangue = new Vector2();
        private static Vector2 posArme = new Vector2();
        private static int distancePisolet = 30;
        private static bool tirLangue;

        public MainWindow()
        {
            InitImage();
            InitializeComponent();
            parametre fentreNiveau = new parametre();
            fentreNiveau.ShowDialog();
            if (fentreNiveau.DialogResult==false)
                Application.Current.Shutdown();
            if (fentreNiveau.DialogResult == true)  // Si la fenêtre a été fermée correctement (avec DialogResult = true)
            {
                    // Récupérer le résultat de la fenêtre du jeu
                    string resultat = fentreNiveau.Resultat;
                if (resultat == "parametre")
                {
                    controle fentreControle = new controle();
                    fentreControle.ShowDialog();
                    if (fentreControle.DialogResult == true)
                    {
                        if (fentreControle.Resultat == "choixTouche")
                        {
                            controle fentreChoixTouche = new controle();
                            fentreChoixTouche.ShowDialog();
                        }
                        else if (fentreControle.Resultat == "aide")
                        {
                            controle fentreAide = new controle();
                            fentreAide.ShowDialog();
                        }
                    }

                    
                }
            }

            InitialiserMinuterie();
            Minuterie();
            InitObjects();

            RenderOptions.SetBitmapScalingMode(canvas.Background, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(player, BitmapScalingMode.NearestNeighbor);
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
        }
        
        private void tempsEnMoins(object? sender, EventArgs e)
        {
            temps--;
        }
        
        private void InitImage()
        {
            imgAnt = new BitmapImage(new Uri("pack://application:,,/img/ant.png"));
            imgFly = new BitmapImage(new Uri("pack://application:,,,/img/fly.png"));

            imgFrogFront = new BitmapImage(new Uri("pack://application:,,,/img/frog_front.png"));
            imgFrogBack = new BitmapImage(new Uri("pack://application:,,,/img/frog_back.png"));
            imgFrogSide = new BitmapImage(new Uri("pack://application:,,,/img/frog_side.png"));
        }

        private void InitObjects()
        {
            ants = new List<Image>();
            fireflys = new List<Image>();
            for (int i = 0; i < nbFireflys; i++)
            {
                Image fly = new Image();
                fly.Source = imgFly;
                fly.Width = 50;
                fly.Height = 50;
                Canvas.SetLeft(fly, alea.Next(0, 1920));
                Canvas.SetTop(fly, alea.Next(0, 300));
                canvas.Children.Add(fly);
                fireflys.Add(fly);
            }

            for (int i = 0; i < nbAnts; i++)
            {
                Image ant = new Image();
                ant.Source = imgAnt;
                ant.Width = 50;
                ant.Height = 50;
                Canvas.SetLeft(ant, alea.Next(0, 1920));
                Canvas.SetTop(ant, alea.Next(600,1080));
                canvas.Children.Add(ant);
                ants.Add(ant);
            }
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

        private void Loop(object? sender, EventArgs e) 
        {
            int maxY = (int) grid.ActualHeight/2;

            //fix direction:
            if      (deplacerBas) directionJoueur = Directions.down;
            else if (deplacerHaut) directionJoueur = Directions.up;
            else if (deplacerDroite) directionJoueur = Directions.right;
            else if (deplacerGauche) directionJoueur = Directions.left;
            else if (deplacerBas && deplacerGauche) directionJoueur = Directions.diagDownLeft;
            else if (deplacerBas && deplacerDroite) directionJoueur = Directions.diagDownRight;
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

            if (deplacerDroite && Canvas.GetLeft(player) < grid.ActualWidth) vitesseJoueur.X = vitesseDeplacement;  // bouger vers la droite
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

            DoubleAnimation grow = new DoubleAnimation
            {
                From = playerTongue.Width,
                To = 300,
                Duration = TimeSpan.FromMilliseconds(100)
            };

            DoubleAnimation shrink = new DoubleAnimation
            {
                From = 300,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(50)
            };
            
            // lorsque l'animation est compléter
            shrink.Completed += (s, e) =>
            {
                tirLangue = false;
            };
            
            grow.Completed += (s, e) => { // s = object? sender  e = EventArgs event
                playerTongue.BeginAnimation(Rectangle.WidthProperty, shrink);
            };

            playerTongue.BeginAnimation(Rectangle.WidthProperty, grow);
        }

        private void keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                vitesseJoueur.Y = -forceSaut;
            }

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

            if (e.Key == Key.E)
            {
                ShootTung();
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

        }

        private void rightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShootTung();
        }
    }
}