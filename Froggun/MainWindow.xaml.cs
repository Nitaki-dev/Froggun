using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
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
        private static Vector2 posJoueur = new Vector2();
        private static Vector2 vitesseJoueur = new Vector2();
        
        private static bool directionJoueur = false; // false = gauche, true = droite. à changer si possible
        private const float forceSaut = 15.0f;
        private const float vitesseMaxChute = 9.8f;
        private const float vitesseDeplacement = 8.0f;
        private const float friction = 0.8f;
        private const float gravite = 0.5f;
        
        private bool verrouillageMouvement = false;
        private bool estAuSol = false;
        private bool plongeVersSol = false;
        private bool deplacerGauche = false;
        private bool deplacerDroite = false;
        
        private const int nbAnts = 3;
        private const int nbFireflys = 3;
        private static List<Image> ants;
        private static List<Image> fireflys;
        private static BitmapImage imgAnt;
        private static BitmapImage imgFly;
        
        private static Vector2 posArme = new Vector2();
        private static int distancePisolet = 100;

        public MainWindow()
        {
            InitImage();
            InitializeComponent();
            InitialiserMinuterie();
            Minuterie();
            InitObjects();
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
                Console.WriteLine((int)(Canvas.GetLeft(ant)));
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
                (float)(posJoueur.X + (directionJoueur ? -player.ActualWidth / 2.0f : player.ActualWidth/2.0f)),
                (float)(posJoueur.Y + (player.ActualHeight/2.0f))
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

            inverseArme.ScaleX = -1;
            gun.RenderTransform = rotationArme;
            gun.RenderTransform = inverseArme;

            myTransformGroup.Children.Add(inverseArme);
            myTransformGroup.Children.Add(rotationArme);

            gun.RenderTransform = myTransformGroup;

            Canvas.SetTop(gun, posArme.Y);
            Canvas.SetLeft(gun, posArme.X);
        }

        private void Loop(object? sender, EventArgs e) 
        {
            int maxY = (int) grid.ActualHeight/2;

            // Inverse l'image du joueur si nécessaire
            if (directionJoueur) joueurFlip.ScaleX = -1; // droite
            else joueurFlip.ScaleX = 1; // gauche
            player.RenderTransform = joueurFlip;

            PivoterArme();

            // Vérifier l'état du joueur pour savoir si nous devons verrouiller son mouvement
            if (plongeVersSol) verrouillageMouvement = true;
            else verrouillageMouvement = false;

            if (verrouillageMouvement)
            {
                // verrouiller le mouvement du joueur
                if (plongeVersSol)
                {
                    vitesseJoueur.Y = vitesseMaxChute * 4.0f;
                    posJoueur.Y += vitesseJoueur.Y;

                    Canvas.SetLeft(player, posJoueur.X);
                    Canvas.SetTop(player, posJoueur.Y);

                    if (posJoueur.Y >= maxY - player.Height) plongeVersSol = false;
                }
            }
            else
            {
                // déplacer le joueur vers le bas
                if (vitesseJoueur.Y < vitesseMaxChute) vitesseJoueur.Y += gravite;
                else vitesseJoueur.Y = vitesseMaxChute;
                posJoueur.Y += vitesseJoueur.Y;

                // le joueur est sur le sol
                if (posJoueur.Y >= maxY - player.Height)
                {
                    // ne pas le bouger 
                    posJoueur.Y = maxY - (float)player.Height;
                    vitesseJoueur.Y = 0;
                    estAuSol = true;
                }

                else estAuSol = false;
                if (deplacerDroite && Canvas.GetLeft(player) < grid.ActualWidth) vitesseJoueur.X = vitesseDeplacement;  // bouger droite
                else if (deplacerGauche && Canvas.GetLeft(player) > 0) vitesseJoueur.X = -vitesseDeplacement; // bouger gauche

                else
                {
                    // réduire la vitesse du joueur en fonction de la friction
                    vitesseJoueur.X *= friction;
                    // si la vitesse (positive) est inférieure à 0.1f, arrêter le mouvement
                    if (Math.Abs(vitesseJoueur.X) < 0.1f) vitesseJoueur.X = 0;
                }
                if (Canvas.GetTop(player) < 0) posJoueur.Y += vitesseJoueur.Y;
                posJoueur.X += vitesseJoueur.X;

                Canvas.SetLeft(player, posJoueur.X);
                Canvas.SetTop(player, posJoueur.Y);
            }
        }

        private void keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                vitesseJoueur.Y = -forceSaut;
            }
            if (e.Key == Key.LeftCtrl)
            {
                if (!estAuSol) plongeVersSol = true;
            }

            if (e.Key == Key.D)
            {
                deplacerDroite = true;
                deplacerGauche = false;
                directionJoueur = true;
            }
            if (e.Key == Key.Q || e.Key == Key.A)
            {
                deplacerGauche = true;
                deplacerDroite = false; 
                directionJoueur = false;
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
        }
    }
}