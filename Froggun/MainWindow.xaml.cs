using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Froggun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static DispatcherTimer minuterie = new DispatcherTimer();

        private static Vector2 positionJoueur = new Vector2();
        private static Vector2 vitesseJoueur = new Vector2();
        private const float gravite = 0.5f;
        private const float forceSaut = 15.0f;
        private const float vitesseMaxChute = 9.8f;
        private const float vitesseDeplacement = 8.0f;
        private const float friction = 0.8f;
        private bool estAuSol = false;
        private bool plongeVersSol = false;
        private bool verrouillageMouvement = false;
        private bool deplacerGauche = false;
        private bool deplacerDroite = false;


        public MainWindow()
        {
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

        private void Loop(object? sender, EventArgs e)
        {
            int maxY = (int) grid.ActualHeight;
            Console.WriteLine($"{plongeVersSol} {verrouillageMouvement} {estAuSol} {Math.Round(vitesseJoueur.X, 1)}  {Math.Round(vitesseJoueur.Y, 1)}    //    {positionJoueur.X}  {positionJoueur.Y}");

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
                if (deplacerDroite) vitesseJoueur.X = vitesseDeplacement;  // bouger droite
                else if (deplacerGauche) vitesseJoueur.X = -vitesseDeplacement; // bouger gauche

                else
                {
                    // réduire la vitesse du joueur en fonction de la friction
                    vitesseJoueur.X *= friction;
                    // si la vitesse (obligée d'être positive) est inférieure à 0.1f, arrêter le mouvement
                    if (Math.Abs(vitesseJoueur.X) < 0.1f) vitesseJoueur.X = 0;
                }

                positionJoueur.Y += vitesseJoueur.Y;
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
            }
            if (e.Key == Key.Q || e.Key == Key.A)
            {
                deplacerGauche = true;
                deplacerDroite = false;
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