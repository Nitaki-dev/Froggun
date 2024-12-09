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
        private static DispatcherTimer minuterie = new DispatcherTimer();

        private static ScaleTransform joueurFlip = new ScaleTransform();
        private static Vector2 positionJoueur = new Vector2();
        private static Vector2 vitesseJoueur = new Vector2();
        private static bool directionJoueur = false; // false = gauche, true = droite. à changer si possible
        private const float gravite = 0.5f;
        private const float forceSaut = 15.0f;
        private const float vitesseMaxChute = 9.8f;
        private const float vitesseDeplacement = 8.0f;
        private const float friction = 0.8f;
        private static Random alea = new Random();
        private bool estAuSol = false;
        private bool plongeVersSol = false;
        private bool verrouillageMouvement = false;
        private bool deplacerGauche = false;
        private bool deplacerDroite = false;
        private const int nbAnts = 3;
        private const int nbFireflys = 3;
        private static List<Image> ants;
        private static List<Image> fireflys;
        private static BitmapImage imgAnt;
        private static BitmapImage imgFly;
        private static Vector2 posSouris = new Vector2();

        private static Vector2 posGun = new Vector2();

        private static int distancePisolet = 100;
        private static ScaleTransform gunFlip = new ScaleTransform();
       

        private int mouseX;
        private int mouseY;

        public MainWindow()
        {
            InitImage();
            InitializeComponent();
            InitialiserMinuterie();
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
            imgFly = new BitmapImage(new Uri("pack://application:,,/img/fly.png"));
        }
        private void InitObjects()
        {
            ants = new List<Image>();
            fireflys = new List<Image>();
            for (int i = 0; i < nbFireflys; i++)
            {
                Image fly = new Image();
                fly.Source = imgFly;
                Canvas.SetLeft(fly, alea.Next(0, (int)(this.ActualWidth)));
                Canvas.SetTop(fly, alea.Next(100, 200));
                canvas.Children.Add(fly);
                fireflys.Add(fly);
            }
            for (int i = 0; i < nbAnts; i++)
            {
                Image ant = new Image();
                ant.Source = imgAnt;
                Canvas.SetLeft(ant, alea.Next(0, (int)(this.ActualWidth)));
                Canvas.SetTop(ant, alea.Next(100,200));
                canvas.Children.Add(ant);
                ants.Add(ant);
            }
        }
        private void Loop(object? sender, EventArgs e) 
        {
            int maxY = (int) grid.ActualHeight/2;

            if (directionJoueur) joueurFlip.ScaleX = -1; // droite
            else joueurFlip.ScaleX = 1; // gauche


            player.RenderTransform = joueurFlip;

            mouseX = (int)Mouse.GetPosition(canvas).X;
            mouseY = (int)Mouse.GetPosition(canvas).Y;
            posGun.X = mouseX;
            posGun.Y = mouseY;

            Vector2 posJoueurTemp = new Vector2(
                (float)(positionJoueur.X + (directionJoueur ? -player.ActualWidth / 2.0f : player.ActualWidth/2.0f)),
                (float)(positionJoueur.Y + (player.ActualHeight/2.0f))
            );

            Vector2 directionSouris = Vector2.Normalize(posGun - posJoueurTemp);
            float distanceJoueurSouris = Vector2.Distance(posJoueurTemp, posGun);
            posGun = posJoueurTemp + (directionSouris * distancePisolet);
            
            float angle = (float) (Math.Atan2(directionSouris.Y, directionSouris.X) * (180 / Math.PI));
            RotateTransform rotateTransform = new RotateTransform(angle);
            gun.RenderTransform = rotateTransform;
            gunFlip.ScaleX = -1;

            if (directionSouris.X > 0) gunFlip.ScaleY = 1;
            else gunFlip.ScaleY = -1;

            gun.RenderTransform = gunFlip;

            TransformGroup myTransformGroup = new TransformGroup();
            myTransformGroup.Children.Add(gunFlip);
            myTransformGroup.Children.Add(rotateTransform);

            gun.RenderTransform = myTransformGroup;

            Canvas.SetTop(gun, posGun.Y);
            Canvas.SetLeft(gun, posGun.X);

            // Vérifier l'état du joueur pour savoir si nous devons verrouiller son mouvement
            if (plongeVersSol) verrouillageMouvement = true;
            else verrouillageMouvement = false;

            if (verrouillageMouvement)
            {
                // verrouiller le mouvement du joueur
                if (plongeVersSol)
                {
                    vitesseJoueur.Y = vitesseMaxChute * 4.0f;
                    positionJoueur.Y += vitesseJoueur.Y;

                    Canvas.SetLeft(player, positionJoueur.X);
                    Canvas.SetTop(player, positionJoueur.Y);

                    if (positionJoueur.Y >= maxY - player.Height) plongeVersSol = false;
                }
            }
            else
            {
                // déplacer le joueur vers le bas
                if (vitesseJoueur.Y < vitesseMaxChute) vitesseJoueur.Y += gravite;
                else vitesseJoueur.Y = vitesseMaxChute;
                // Appliquer la vitesse verticale  
                positionJoueur.Y += vitesseJoueur.Y;

                // le joueur est sur le sol
                if (positionJoueur.Y >= maxY - player.Height)
                {
                    // ne pas bouger 
                    positionJoueur.Y = maxY - (float)player.Height;
                    estAuSol = true;
                    vitesseJoueur.Y = 0;
                }

                else estAuSol = false;
                if (deplacerDroite && Canvas.GetLeft(player) < grid.ActualWidth) vitesseJoueur.X = vitesseDeplacement;  // bouger droite
                else if (deplacerGauche && Canvas.GetLeft(player) > 0) vitesseJoueur.X = -vitesseDeplacement; // bouger gauche

                else
                {
                    // réduire la vitesse du joueur en fonction de la friction
                    vitesseJoueur.X *= friction;
                    // si la vitesse (obligée d'être positive) est inférieure à 0.1f, arrêter le mouvement
                    if (Math.Abs(vitesseJoueur.X) < 0.1f) vitesseJoueur.X = 0;
                }
                if (Canvas.GetTop(player) < 0) positionJoueur.Y += vitesseJoueur.Y;
                positionJoueur.X += vitesseJoueur.X;

                Canvas.SetLeft(player, positionJoueur.X);
                Canvas.SetTop(player, positionJoueur.Y);
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