using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

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
        private static Vector2 posArme = new Vector2();
        private static int distancePisolet = 30;
        private static bool tirLangue;

        private static Vector2 dirBalle = new Vector2();
        private static BitmapImage imageBalle;
        private static double vitesseBalle = 30.0f;

        private List<Balle> Balles = new List<Balle>();
        
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
                                    // Logique pour lancer le jeu ici
                                    MessageBox.Show("Lancement du jeu !");  // Remplacez ceci par le code de lancement réel du jeu
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
                                    // Logique pour lancer le jeu ici
                                    MessageBox.Show("Lancement du jeu !");  // Remplacez ceci par le code de lancement réel du jeu
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
            //minuterie.Tick += EnnemiesAttack;
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
            Ennemis spider = new Ennemis(200, 200, 100, 100, 2.5, canvas, imageDirectory, animationFrames);

            string imageDirectory1 = "img/ennemis/Food1";
            int[] animationFrames1 = new int[] { 1, 2 };
            Ennemis Food = new Ennemis(300, 200, 64, 64, 2.5, canvas, imageDirectory1, animationFrames1);

            // TODO: FIX MOVEMENT


            // ants = new List<Image>();
            // fireflys = new List<Image>();
            // for (int i = 0; i < nbFireflys; i++)
            // {

            //     Image fly = new Image();
            //     fly.Source = imgFly;
            //     fly.Width = 50;
            //     fly.Height = 50;
            //     Canvas.SetLeft(fly, alea.Next(200, 1300));
            //     Canvas.SetTop(fly, alea.Next(0, 300));
            //     canvas.Children.Add(fly);
            //     fireflys.Add(fly);
            //     Rect newRect = new Rect((int)Canvas.GetLeft(fly), (int)Canvas.GetTop(fly), (int)fly.Width, (int)fly.Height);
            //     rectanglesfireflys.Add(newRect);
            //     RenderOptions.SetBitmapScalingMode(fly, BitmapScalingMode.NearestNeighbor);
            // }
            // bool trier;
            // do
            // {
            //     trier = true;
            //     for (int i = 0; i < nbFireflys - 1; i++)
            //     {
            //         for (int j = nbFireflys - 1; j > i; j--)
            //         {
            //             if (rectanglesfireflys[i].IntersectsWith(rectanglesfireflys[j]))
            //             {
            //                 trier = false;
            //                 Canvas.SetLeft(fireflys[i], alea.Next(200, 1300));
            //                 Canvas.SetTop(fireflys[i], alea.Next(0, 300));
            //             }

            //         }
            //     }
            // } while (trier == false);


            // for (int i = 0; i < nbAnts; i++)
            // {
            //     Image ant = new Image();
            //     ant.Source = imgAnt;
            //     ant.Width = 50;
            //     ant.Height = 50;
            //     Canvas.SetLeft(ant, alea.Next(200, 1300));
            //     Canvas.SetTop(ant, alea.Next(0, 300));
            //     canvas.Children.Add(ant);
            //     ants.Add(ant);
            //     Rect newRect = new Rect((int)Canvas.GetLeft(ant), (int)Canvas.GetTop(ant), (int)ant.Width, (int)ant.Height);
            //     rectanglesants.Add(newRect);
            //     RenderOptions.SetBitmapScalingMode(ant, BitmapScalingMode.NearestNeighbor);
            // }
            // do
            // {
            //     trier = true;
            //     for (int i = 0; i < nbAnts - 1; i++)
            //     {
            //         for (int j = nbAnts - 1; j > i; j--)
            //         {
            //             if (rectanglesants[i].IntersectsWith(rectanglesants[j]))
            //             {
            //                 trier = false;
            //                 Canvas.SetLeft(ants[i], alea.Next(200, 1500));
            //                 Canvas.SetTop(ants[i], alea.Next(0, 300));
            //                 //Console.WriteLine(rectanglesants[i]);
            //                 //Console.WriteLine(rectanglesants[j]);
            //             }

            //         }
            //     }
            // } while (trier == false);
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
            for (int i = 0; i < Balles.Count; i++) {
                Balle balle = Balles[i];

                balle.UpdatePositionBalles();

                if (balle.X < -balle.BalleImage.ActualWidth || balle.Y < -balle.BalleImage.ActualHeight
                 || balle.X > grid.ActualWidth || balle.Y > grid.ActualHeight)
                    Balles.RemoveAt(i);
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

            Balle balle = new Balle(posArme.X, posArme.Y, angle, vitesseBalle, canvas, imageBalle);
            Balles.Add(balle);
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

        //private void EnnemiesAttack(object? sender, EventArgs e)
        //{
            
        //    Rect playerR = new Rect((int)Canvas.GetLeft(player), (int)Canvas.GetTop(player), (int)player.Width, (int)player.Height);
        //    for (int i = 0; i < nbFireflys; i++)
        //    {
        //        double speedFactor = 0.5;
        //        double xFirefly = Canvas.GetLeft(fireflys[i]);
        //        double yFirefly = Canvas.GetTop(fireflys[i]);
        //        Vector2 directionFirefly = new Vector2(
        //            (float)(Canvas.GetLeft(player) - xFirefly),
        //            (float)(Canvas.GetTop(player) - yFirefly)
        //        );
        //        directionFirefly = Vector2.Normalize(directionFirefly);
        //        Canvas.SetLeft(fireflys[i], xFirefly + directionFirefly.X * vitesseDeplacement * speedFactor);
        //        Canvas.SetTop(fireflys[i], yFirefly + directionFirefly.Y * vitesseDeplacement * speedFactor);

        //        rectanglesfireflys[i] = new Rect(
        //            (int)Canvas.GetLeft(fireflys[i]),
        //            (int)Canvas.GetTop(fireflys[i]),
        //            (int)fireflys[i].Width,
        //            (int)fireflys[i].Height
        //        );
        //    }
        //    for (int i = 0; i < nbAnts; i++)
        //    {
        //        double speedFactor = 0.8;
        //        double xAnt = Canvas.GetLeft(ants[i]);
        //        double yAnt = Canvas.GetTop(ants[i]);
        //        Vector2 directionAnt = new Vector2(
        //            (float)(Canvas.GetLeft(player) - xAnt),
        //            (float)(Canvas.GetTop(player) - yAnt)
        //        );

        //        directionAnt = Vector2.Normalize(directionAnt);
        //        Canvas.SetLeft(ants[i], xAnt + directionAnt.X * vitesseDeplacement * speedFactor);
        //        Canvas.SetTop(ants[i], yAnt + directionAnt.Y * vitesseDeplacement * speedFactor);

        //        rectanglesants[i] = new Rect(
        //            (int)Canvas.GetLeft(ants[i]),
        //            (int)Canvas.GetTop(ants[i]),
        //            (int)ants[i].Width,
        //            (int)ants[i].Height
        //        );
        //    }
        //}

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