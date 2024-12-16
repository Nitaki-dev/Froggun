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
using System.Net.Sockets;
using System.Security.Authentication;
using System.Diagnostics;

namespace Froggun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static int fenetreGauche = 100;
        public static int fenetreHaut = 100;
        public const int WINDOW_WIDTH = 1280, WINDOW_HEIGHT = 720;

        private static Random alea = new Random();

        private bool pause = false;
        private static DispatcherTimer minuterie = new DispatcherTimer();
        private static DispatcherTimer pauseVagues = new DispatcherTimer();

        private float currentAngle;
        private static bool tirLangue, expensionLangue;
        private static readonly int expensionLangueVitesse = 60, retractionLangueVitesse = 80;
        private static Vector2 posArme = new Vector2();
        private static int distancePisolet = 50;

        private static BitmapImage imageBalle;
        private static double vitesseBalle = 30.0f;

        private static BitmapImage imageVie5;
        private static BitmapImage imageVie4;
        private static BitmapImage imageVie3;
        private static BitmapImage imageVie2;
        private static BitmapImage imageVie1;
        private static BitmapImage imageVie0;

        private static BitmapImage imgFrogFront;
        private static BitmapImage imgFrogBack;
        private static BitmapImage imgFrogSide;
        private static BitmapImage imgFrogFrontHit; 
        private static BitmapImage imgFrogBackHit;
        private static BitmapImage imgFrogSideHit;

        private static BitmapImage[] imgExplosion = new BitmapImage[6];

        private static BitmapImage imgMantis;

        private List<Balle> Balles = new List<Balle>();
        private List<Ennemis> ennemis = new List<Ennemis>();
        private List<Proies> proies = new List<Proies>();
        public static string difficulte;
        public static int bulletOffset = 0;
        int pauseEntreVagues = 1; // en secondes
        int pauseCounter = 0;
        int waveCount = 0;
        private bool isTimerRunning = false;

        public bool fightingBoss = false;
        public bool bossSpawned = false;

        public SoundPlayer musique;
        public Stream audioStream;
        private MediaPlayer musiqueDeFond;
        private MediaPlayer musiqueDeJeu;

        private Joueur joueur;

        private MantisBosses mantis;
        public MainWindow()
        {
            InitializeComponent();

            // Création de la fenêtre parametre avec un Canvas
            parametre fentreNiveau = new parametre();
            fentreNiveau.Left = fenetreGauche;
            fentreNiveau.Top = fenetreHaut;
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
                            fentrechoixTouche.Left = fenetreGauche;
                            fentrechoixTouche.Top = fenetreHaut;
                            fentrechoixTouche.ShowDialog();  // Affiche la fenêtre controle de manière modale

                            // Si la fenêtre controle est fermée avec DialogResult == false, revenir à la fenêtre parametre
                            if (fentrechoixTouche.DialogResult == false)
                            {
                                // Création d'une nouvelle instance de la fenêtre parametre, on ne peut pas réutiliser l'ancienne
                                fentreNiveau = new parametre();  // Nouvelle instance de parametre
                                fentreNiveau.Left = fenetreGauche;
                                fentreNiveau.Top = fenetreHaut;
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
                            fentreaide.Left = fenetreGauche;
                            fentreaide.Top = fenetreHaut;
                            fentreaide.ShowDialog();  // Affiche la fenêtre controle de manière modale

                            // Si la fenêtre controle est fermée avec DialogResult == false, revenir à la fenêtre parametre
                            if (fentreaide.DialogResult == false)
                            {
                                // Création d'une nouvelle instance de la fenêtre parametre, on ne peut pas réutiliser l'ancienne
                                fentreNiveau = new parametre();  // Nouvelle instance de parametre
                                fentreNiveau.Left = fenetreGauche;
                                fentreNiveau.Top = fenetreHaut;
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
                fentreDifficulte.Left = fenetreGauche;
                fentreDifficulte.Top = fenetreHaut;
                fentreDifficulte.ShowDialog();  // Affiche la fenêtre controle de manière modale
                difficulte = fentreDifficulte.Resultat;
                //InitMusique(false);
                //InitMusiqueJeux(true);
                this.Left = fenetreGauche;
                this.Top = fenetreHaut;
            }

            lab_Pause.Visibility = Visibility.Collapsed;
            lab_Defaite.Visibility = Visibility.Collapsed;

            InitImage();
            //InitMusique(true);

            Rect playerRect = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            joueur = new Joueur(player, playerRect, 640 - (int)player.Width/2, 360 - (int)player.Height / 2, grid, imgFrogFront, imgFrogSide, imgFrogBack, imgFrogFrontHit, imgFrogSideHit, imgFrogBackHit);

            InitialiserMinuterie();
            RenderOptions.SetBitmapScalingMode(canvas.Background, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(player, BitmapScalingMode.NearestNeighbor);
            Measure(new Size(WINDOW_WIDTH, WINDOW_HEIGHT));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));

            ChangeBackground("img/arena/arena_unaltered.png");
        }

        private void InitMusique(bool jouer)
        {
            if (jouer)
            {
                musiqueDeFond = new MediaPlayer();
                musiqueDeFond.Open(new Uri("son/intro.mp3", UriKind.Relative));
                musiqueDeFond.MediaEnded += RelanceMusique;
                musiqueDeFond.Play();
            }
            else
            {
                musiqueDeFond.Stop();
            }

        }
        
        private void RelanceMusique(object? sender, EventArgs e)
        {
            musiqueDeFond.Position = TimeSpan.Zero;
            musiqueDeFond.Play();
        }

        private void InitMusiqueJeux(bool jouer)
        {
            if (jouer)
            {
                musiqueDeJeu = new MediaPlayer();
                musiqueDeJeu.Open(new Uri("son/son_jeu.mp3", UriKind.Relative));
                musiqueDeJeu.MediaEnded += RelanceMusiqueJeux;
                musiqueDeJeu.Volume = 1.0;
                musiqueDeJeu.Play();
            }
            else
            {
                musiqueDeJeu.Stop();
            }

        }
        
        private void RelanceMusiqueJeux(object? sender, EventArgs e)
        {
            musiqueDeJeu.Position = TimeSpan.Zero;
            musiqueDeJeu.Play();
        }

        void NouvelleVague()
        {
            if (isTimerRunning || fightingBoss) return;
            isTimerRunning = true;
            
            //if (difficulte == "facile" || difficulte == "moyen") pauseEntreVagues = 5;
            //else pauseEntreVagues = 10;

            pauseVagues = new DispatcherTimer();
            pauseVagues.Interval = TimeSpan.FromSeconds(1);
            pauseVagues.Tick += Vague;
            pauseCounter = 0;
            pauseVagues.Start();
        }
        
        private void Vague(object? sender, EventArgs e)
        {
            if (pause || fightingBoss)
            {
                pauseVagues.Stop();
                return;
            }

            pauseCounter++;
            labelWave.Content = $"Prochainne vague dans {pauseEntreVagues - pauseCounter}!";
            if (pauseCounter < pauseEntreVagues) return;

            labelWave.Content = $"Vague {waveCount+1}";
            if (difficulte == "facile" && !AreAllEnemiesDestroyed()) return;
            waveCount++;
            
            int nbrPetitEnnemis;
            int nbrMoyenEnnemis;
            int nbrGrandEnnemis;

            // TODO: make this not terrible (redo difficulty spawnrates for the 6th time)
            if (waveCount <= 5)
            {
                nbrPetitEnnemis = waveCount * 3;
                nbrMoyenEnnemis = 0;
                nbrGrandEnnemis = 0;
            }
            else if (waveCount <= 8)
            {
                nbrPetitEnnemis = (waveCount-3)*2;
                nbrMoyenEnnemis = (waveCount - 7) * 2;
                nbrGrandEnnemis = 0;
            }
            else
            {
                nbrPetitEnnemis = waveCount;
                nbrMoyenEnnemis = (waveCount - 5);
                nbrGrandEnnemis = (waveCount - 8) * 1;
            }

            // poisson disk sampling (voir Sampling.cs)
            Sampler sampler = new Sampler(1260, 680, 50);
            List<Point> allPoints = sampler.GeneratePoints();

            // https://stackoverflow.com/questions/273313/randomize-a-listt
            alea = new Random();
            allPoints = allPoints.OrderBy(_ => alea.Next()).ToList();

            // creations de nouvelles listes pour chaques type d'ennemis
            List<Point> petits = allPoints.Take(nbrPetitEnnemis).ToList();
            List<Point> moyens = allPoints.Skip(nbrPetitEnnemis).Take(nbrMoyenEnnemis).ToList(); //skip les petits ennemis
            List<Point> grands = allPoints.Skip(nbrPetitEnnemis + nbrMoyenEnnemis).Take(nbrGrandEnnemis).ToList(); //skip les petits et moyens ennemis

            // creation des ennemis
            foreach (Point point in petits) ennemis.Add(new Ennemis(TypeEnnemis.Firefly, point.X, point.Y, 50, 50, 12, canvas));
            foreach (Point point in moyens) ennemis.Add(new Ennemis(TypeEnnemis.Spider, point.X, point.Y, 100, 100, 8, canvas));
            foreach (Point point in grands) ennemis.Add(new Ennemis(TypeEnnemis.Squit, point.X, point.Y, 150, 150, 13, canvas));

            //proies.Add(new Proies(TypeProies.Fly, point.X, point.Y, 50, 50, 3, 500, 200, canvas));

            pauseVagues.Stop();
            isTimerRunning = false;
        }

        private bool AreAllEnemiesDestroyed()
        {
            return ennemis.Count == 0 && proies.Count == 0;
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
            imgFrogFront = new BitmapImage(new Uri("pack://application:,,,/img/frog_front.png"));
            imgFrogBack = new BitmapImage(new Uri("pack://application:,,,/img/frog_back.png"));
            imgFrogSide = new BitmapImage(new Uri("pack://application:,,,/img/frog_side.png"));

            imgFrogFrontHit = new BitmapImage(new Uri("pack://application:,,,/img/frog_front_hit.png"));
            imgFrogBackHit = new BitmapImage(new Uri("pack://application:,,,/img/frog_back_hit.png"));
            imgFrogSideHit = new BitmapImage(new Uri("pack://application:,,,/img/frog_side_hit.png"));

            imageBalle = new BitmapImage(new Uri("pack://application:,,,/img/balle.png"));

            imageVie5 = new BitmapImage(new Uri("pack://application:,,,/img/vie/health5.png"));
            imageVie4 = new BitmapImage(new Uri("pack://application:,,,/img/vie/health4.png"));
            imageVie3 = new BitmapImage(new Uri("pack://application:,,,/img/vie/health3.png"));
            imageVie2 = new BitmapImage(new Uri("pack://application:,,,/img/vie/health2.png"));
            imageVie1 = new BitmapImage(new Uri("pack://application:,,,/img/vie/health1.png"));
            imageVie0 = new BitmapImage(new Uri("pack://application:,,,/img/vie/health0.png"));

            imgExplosion[0] = new BitmapImage(new Uri("pack://application:,,,/img/explosion/img_explosion0.png"));
            imgExplosion[1] = new BitmapImage(new Uri("pack://application:,,,/img/explosion/img_explosion1.png"));
            imgExplosion[2] = new BitmapImage(new Uri("pack://application:,,,/img/explosion/img_explosion2.png"));
            imgExplosion[3] = new BitmapImage(new Uri("pack://application:,,,/img/explosion/img_explosion3.png"));
            imgExplosion[4] = new BitmapImage(new Uri("pack://application:,,,/img/explosion/img_explosion4.png"));
            imgExplosion[5] = new BitmapImage(new Uri("pack://application:,,,/img/explosion/img_explosion5.png"));

            imgMantis = new BitmapImage(new Uri("pack://application:,,,/img/boss/mantis/mantis.png"));
        }

        private void UpdateMousePosition()
        {
            // Get the mouse position once and calculate the direction to player center
            Point mousePos = Mouse.GetPosition(canvas);

            Vector2 posCentreJoueur = new Vector2(
                (float)(joueur.posJoueur.X + player.Width / 2.0f),
                (float)(joueur.posJoueur.Y + player.Height / 2.0f)
            );

            Vector2 direction = new Vector2(
                (float) (mousePos.X - posCentreJoueur.X),
                (float) (mousePos.Y - posCentreJoueur.Y)
            );

            float magnitude = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
            if (magnitude != 0) {
                direction.X /= magnitude;
                direction.Y /= magnitude;
            }

            int posGunMagnitude = 50;
            Vector2 directionSouris = new Vector2(
                posCentreJoueur.X + direction.X * posGunMagnitude,
                posCentreJoueur.Y + direction.Y * posGunMagnitude
            );

            currentAngle = (float) (Math.Atan2(direction.Y, direction.X) * 180 / Math.PI);
            

            // Update positions for both the weapon and tongue
            UpdateWeaponPosition(mousePos, posCentreJoueur, directionSouris);
            UpdateTonguePosition(mousePos, posCentreJoueur, directionSouris);
        }

        private void UpdateWeaponPosition(Point mousePos, Vector2 posCentreJoueur, Vector2 directionSouris)
        {
            // Calculate weapon position around the player
            float distanceJoueurSouris = Vector2.Distance(posCentreJoueur, new Vector2((float)mousePos.X, (float)mousePos.Y));
            posArme = directionSouris;

            // Apply transforms for the weapon
            TransformGroup transformGroup = new TransformGroup();
            ScaleTransform inverseArme = new ScaleTransform();
            RotateTransform rotationArme = new RotateTransform(currentAngle);

            // Flip the weapon image if the mouse is to the left
            inverseArme.ScaleY = (Math.Abs(currentAngle) > 90) ? -1 : 1;

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
            //Stopwatch stopwatch = new Stopwatch(); // WPF is terrible for game dev.
            //stopwatch.Start();

            if (waveCount == 10)
            {
                // bossfight
                if (!fightingBoss && AreAllEnemiesDestroyed()) startBoss();
            }
            else
            {
                if (!fightingBoss)
                {
                    // if no enemies start a wave
                    if (difficulte == "facile" || difficulte == "moyen")
                    {
                        if (ennemis.Count <= 0 && proies.Count <= 0) NouvelleVague();
                    }
                    else NouvelleVague();
                }
            }


            Ennemis.UpdateEnnemis(ennemis, joueur.hitbox, Balles, canvas , ref joueur);
            Proies.UpdateProies(canvas, proies, joueur.hitbox);
            if (this.fightingBoss && this.bossSpawned)
            {
                mantis.UpdateMantisBoss();

                if (!mantis.isAlive)
                {
                    this.fightingBoss = false;
                    this.bossSpawned = false;
                    waveCount++;
                }
            }
            AfficheScore();
            AfficheCombo();

            AffichageDeVie(joueur.nombreDeVie);
            CheckBallesSortieEcran();
            CheckCollisionProie();
            if (!joueur.estEnRoulade) joueur.ChangeJoueurDirection();
            UpdateMousePosition();
            joueur.UpdatePositionJoueur();

            //stopwatch.Stop();
            //Console.WriteLine($"{stopwatch.Elapsed}");
        }

        private async Task startBoss()
        {
            this.fightingBoss = true;
            this.bossSpawned = false;

            await Task.Delay(500);
            Explostions(100, 100, 500); 
            Explostions(300, 100, 500); 
            Explostions(500, 100, 500);
            Explostions(700, 100, 500);
            Explostions(900, 100, 500);
            Explostions(1100, 100, 500);
            ChangeBackground("img/arena/arena_broken_top.png");
            await Task.Delay(300);
            mantis = new MantisBosses(canvas, imgMantis, joueur, 300, 200, 200); //todo: change the 200x200 when the art is arting.
            await Task.Delay(400);

            // todo: restrict players movement
            
            this.bossSpawned = true;
        }

        private void ChangeBackground(string path)
        {
            var imageBrush = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri($"pack://application:,,,/{path}")),
                Stretch = Stretch.UniformToFill,
                TileMode = TileMode.FlipXY                
            }; 
            
            RenderOptions.SetBitmapScalingMode(imageBrush, BitmapScalingMode.NearestNeighbor);
            canvas.Background = imageBrush;
        }

        private void Explostions(int x, int y, int size)
        {
            Image boom = new Image();
            boom.Width = size;
            boom.Height = size;
            Canvas.SetLeft(boom, x - size / 2);
            Canvas.SetTop(boom, y - size / 2); 
            RenderOptions.SetBitmapScalingMode(boom, BitmapScalingMode.NearestNeighbor);
            canvas.Children.Add(boom);

            DispatcherTimer animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(100);
            int frame=0, maxFrames = 5;

            animationTimer.Tick += (e,s) =>
            {
                frame++;
                if (frame >= maxFrames)
                {
                    animationTimer.Stop();
                    canvas.Children.Remove(boom);
                }
                boom.Source = imgExplosion[frame];
            };
            animationTimer.Start();
        }

        private void AffichageDeVie(int nombreDeVie)
        {
            if (nombreDeVie <= 0)
            {
                Console.WriteLine("Mort");
                ImgvieJoueur.Source = imageVie0;
                pause = true;
                lab_Defaite.Visibility = Visibility.Visible;
                mort(nombreDeVie);
            }
            else
            {
                if (nombreDeVie == 5) ImgvieJoueur.Source = imageVie5;
                else if (nombreDeVie == 4) ImgvieJoueur.Source = imageVie4;
                else if (nombreDeVie == 3) ImgvieJoueur.Source = imageVie3;
                else if (nombreDeVie == 2) ImgvieJoueur.Source = imageVie2;
                else if (nombreDeVie == 1) ImgvieJoueur.Source = imageVie1;
            }
        }
        
        public void mort(int nombreDeVie)
        {
            minuterie.Stop();
            pauseVagues.Stop();
            MessageBoxResult result = MessageBox.Show("Souhaitez-vous recommencer ?", "Recommencer", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                Recommencer(5);
            }
            else
            {
                // Action pour quitter
                Application.Current.Shutdown();
            }
        }

        public void Recommencer(int nombreDeVie)
        {
            for (int i = 0; i < proies.Count; i++)
            {
                canvas.Children.Remove(proies[i].Image);
                proies.Remove(proies[i]);
            }
            Ennemis.ReccomencerEnnemis(ennemis, canvas);
            Proies.ReccomencerProies(proies, canvas);
            joueur.nombreDeVie = nombreDeVie;
            joueur.score = 0;
            nombreDeVie = 5;
            AffichageDeVie(nombreDeVie);
            AfficheScore();
            waveCount = 0;
            pauseEntreVagues = 5;
            pauseVagues.Stop();
            pauseCounter = 0;
            pause = false;
            lab_Defaite.Visibility = Visibility.Collapsed;
            minuterie.Start();
            pauseVagues.Start();
        }

        private void CheckBallesSortieEcran()
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


                    // TODO: REMOVE THIS
                    if (this.bossSpawned && this.fightingBoss) mantis.DamageMantis(5);
                }
            }
        }

        private void CheckCollisionProie()
        {
            if (expensionLangue)
            {
                if (playerTongue.Width < 300)
                {
                    // create two lines from start to end of tongue
                    RotateTransform rotation = (RotateTransform)playerTongue.RenderTransform;
                    Point tcentre = new Point(Canvas.GetLeft(playerTongue), Canvas.GetTop(playerTongue) + playerTongue.Height / 2.0f);

                    Point t11 = new Point(Canvas.GetLeft(playerTongue), Canvas.GetTop(playerTongue));
                    Point t12 = new Point(Canvas.GetLeft(playerTongue) + playerTongue.Width, Canvas.GetTop(playerTongue));

                    Point t21 = new Point(Canvas.GetLeft(playerTongue), Canvas.GetTop(playerTongue) + playerTongue.Height);
                    Point t22 = new Point(Canvas.GetLeft(playerTongue) + playerTongue.Width, Canvas.GetTop(playerTongue) + playerTongue.Height);

                    // rotate points based on the tongues angle
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

                    foreach (Proies proie in proies.ToList())
                    {
                        int x = (int)Canvas.GetLeft(proie.BoundingBox);
                        int y = (int)Canvas.GetTop(proie.BoundingBox);

                        /*
                        *   A--------B 
                        *   |        |
                        *   |        |
                        *   C--------D
                        */

                        Point intersection;
                        Line line_AB = new Line
                        {
                            X1 = x,
                            Y1 = y,
                            X2 = x + proie.BoundingBox.Width,
                            Y2 = y,
                        };

                        Line line_BD = new Line
                        {
                            X1 = x + proie.BoundingBox.Width,
                            Y1 = y,
                            X2 = x + proie.BoundingBox.Width,
                            Y2 = y + proie.BoundingBox.Height,
                        };

                        Line line_DC = new Line
                        {
                            X1 = x + proie.BoundingBox.Width,
                            Y1 = y + proie.BoundingBox.Height,
                            X2 = x,
                            Y2 = y + proie.BoundingBox.Height,
                        };

                        Line line_CA = new Line
                        {
                            X1 = x,
                            Y1 = y + proie.BoundingBox.Height,
                            X2 = x,
                            Y2 = y,
                        };

                        if (TryGetIntersection(line_frog_1, line_AB, out intersection)
                            || TryGetIntersection(line_frog_1, line_BD, out intersection)
                            || TryGetIntersection(line_frog_1, line_DC, out intersection)
                            || TryGetIntersection(line_frog_1, line_CA, out intersection)

                            || TryGetIntersection(line_frog_2, line_AB, out intersection)
                            || TryGetIntersection(line_frog_2, line_BD, out intersection)
                            || TryGetIntersection(line_frog_2, line_DC, out intersection)
                            || TryGetIntersection(line_frog_2, line_CA, out intersection))
                        {
                            expensionLangue = false;

                            if (canvas.Children.Contains(proie.Image)) canvas.Children.Remove(proie.Image);
                            if (canvas.Children.Contains(proie.BoundingBox)) canvas.Children.Remove(proie.BoundingBox);
                            if (proies.Contains(proie)) proies.Remove(proie);

                            joueur.proieManger++;
                            if (joueur.proieManger >= joueur.proiePourHeal)
                            {
                                joueur.heal();
                                joueur.proieManger = 0;
                            }
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

        private void SonGun()
        {
            // Charger le fichier audio depuis les ressources
            Uri audioUri = new Uri("/son/coupdefeu.wav", UriKind.RelativeOrAbsolute);
            Stream audioStream = Application.GetResourceStream(audioUri).Stream;
            // Créer un objet SoundPlayer pour lire le son
            SoundPlayer musique = new SoundPlayer(audioStream);
            musique.Play();
        }

        private void SonLangue()
        {
            // Charger le fichier audio depuis les ressources
            Uri audioUri = new Uri("/son/langue.wav", UriKind.RelativeOrAbsolute);
            Stream audioStream = Application.GetResourceStream(audioUri).Stream;
            // Créer un objet SoundPlayer pour lire le son
            SoundPlayer musique = new SoundPlayer(audioStream);
            musique.Play();
        }

        private void ShootGun()
        {
            //SonGun();
            int scaleX = (Math.Abs(currentAngle) > 90) ? -1 : 1;
            Balle balle = new Balle(posArme.X, posArme.Y, currentAngle, vitesseBalle, 10, canvas, imageBalle, scaleX);
            Balles.Add(balle);
        }
        
        private void ShootTung()
        {
            //SonLangue();
            if (tirLangue) return;
            else tirLangue = true;
            expensionLangue = true;
        }
        
        public void AfficheScore()
        {
            labelScore.Content = $"Score : {joueur.score} ";
        }

        public void AfficheCombo()
        {
            labelCombo.Content = $"x{joueur.scoreMultiplier} ";
        }

        private void keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D && !pause)
            {
                joueur.deplacerDroite = true;
                joueur.deplacerGauche = false;
                joueur.directionJoueur = Joueur.Directions.right;
            }
            if ((e.Key == Key.Q || e.Key == Key.A) && !pause)
            {
                joueur.deplacerGauche = true;
                joueur.deplacerDroite = false;
                joueur.directionJoueur = Joueur.Directions.left;
            }
            if (e.Key == Key.S && !pause)
            {
                joueur.deplacerBas = true;
                joueur.deplacerHaut = false;
                joueur.directionJoueur = Joueur.Directions.down;
            }
            if ((e.Key == Key.Z || e.Key == Key.W) && !pause)
            {
                joueur.deplacerHaut = true;
                joueur.deplacerBas = false;
                joueur.directionJoueur = Joueur.Directions.up;
            }
            if ((e.Key == Key.LeftCtrl || e.Key == Key.LeftShift) && !pause)
            {
                joueur.estEnRoulade = true;
            }

            if (e.Key == Key.K && !pause)
            {
                ennemis.Add(new Ennemis(TypeEnnemis.Spider, joueur.posJoueur.X + 60, joueur.posJoueur.Y + 60, 100, 100, 8, canvas));
            }

            if (e.Key == Key.Escape || e.Key == Key.Space )
            {
                // dont let the player pause during a bossfight
                if (fightingBoss) return;

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
                joueur.deplacerDroite = false;
            }
            if (e.Key == Key.Q || e.Key == Key.A)
            {
                joueur.deplacerGauche = false;
            }
            if (e.Key == Key.S)
            {
                joueur.deplacerBas = false;
            }
            if (e.Key == Key.Z || e.Key == Key.W)
            {
                joueur.deplacerHaut = false;
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