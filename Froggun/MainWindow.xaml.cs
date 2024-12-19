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
using System.Windows.Interop;

namespace Froggun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int WINDOW_WIDTH = 1280, WINDOW_HEIGHT = 720;
        public static int fenetreGauche = 100;
        public static int fenetreHaut = 100;

        private bool pause = false;
        private static DispatcherTimer minuterie = new DispatcherTimer();
        private static DispatcherTimer pauseEntreVaguesTimer = new DispatcherTimer();

        private float angleActuelle;
        private static bool tirLangue, expensionLangue;
        private static readonly int expensionLangueVitesse = 60;
        private static readonly int retractionLangueVitesse = 80;
        private static Vector2 positionArme = new Vector2();
        private static int distancePisolet = 50;

        private static BitmapImage imageBalle;

        private static BitmapImage[] imagesVie = new BitmapImage[6];
        private static Rectangle bgRegenVie;
        private static Rectangle fgRegenVie;

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
        
        int pauseEntreVagues = 5; // en secondes
        int pauseCounter = 0;
        int nombreDeVagues = 0;
        private bool timerEstActif = false;

        public bool estEnCombatAvecBoss = false;
        public bool estBossApparu = false;

        private MediaPlayer musiqueDeFond;
        private MediaPlayer musiqueDeJeu;

        private Joueur joueur;
        private BossMante mante;

        public MainWindow()
        {
            InitializeComponent();
            InitMusique(true);

            RenderOptions.ProcessRenderMode = RenderMode.Default;

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
                                musiqueDeFond.Volume = Properties.Settings.Default.Volume;
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
                if (fentreDifficulte.DialogResult == false)
                {
                    Application.Current.Shutdown();
                    return;
                }
                InitMusique(false);
                InitMusiqueJeux(true);
                this.Left = fenetreGauche;
                this.Top = fenetreHaut;
            }

            lab_Pause.Visibility = Visibility.Collapsed;
            lab_Defaite.Visibility = Visibility.Collapsed;

            InitImage();

            Rect hitbox = new Rect(Canvas.GetLeft(joueurImage), Canvas.GetTop(joueurImage), joueurImage.Width, joueurImage.Height);

            joueur = new Joueur(joueurImage, hitbox, 
                /*posX*/ 640 - (int)joueurImage.Width/2, 
                /*posY*/ 360 - (int)joueurImage.Height / 2, grid, 
                /*imgs*/ imgFrogFront, imgFrogSide, imgFrogBack, imgFrogFrontHit, imgFrogSideHit, imgFrogBackHit);

            InitialiserMinuterie();

            RenderOptions.SetBitmapScalingMode(canvas.Background, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(joueurImage, BitmapScalingMode.NearestNeighbor);
            
            // corrige la taille de la fenetre 
            Measure(new Size(WINDOW_WIDTH, WINDOW_HEIGHT));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));

            ChangerFond("img/arena/arena_unaltered.png");
        }

        private void InitMusique(bool jouer)
        {
            if (jouer)
            {
                try
                {
                    musiqueDeFond = new MediaPlayer();
                    musiqueDeFond.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/son/intro.mp3"));
                    musiqueDeFond.Volume = Properties.Settings.Default.Volume;
                    musiqueDeFond.MediaEnded += RelanceMusique;
                    musiqueDeFond.Play();
                } 
                catch(Exception ex) {Console.WriteLine(ex.ToString());}
            }
            else musiqueDeFond.Stop();
        }

        private void RelanceMusique(object? sender, EventArgs e)
        {
            musiqueDeFond.Position = TimeSpan.Zero;
            musiqueDeFond.Volume = Properties.Settings.Default.Volume;
            musiqueDeFond.Play();
        }

        private void InitMusiqueJeux(bool jouer)
        {
            if (jouer)
            {
                musiqueDeJeu = new MediaPlayer();
                musiqueDeJeu.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/son/son_jeu.mp3"));
                musiqueDeJeu.MediaEnded += RelanceMusiqueJeux;
                musiqueDeJeu.Volume = Properties.Settings.Default.Volume; 
                musiqueDeJeu.Play();
            }
            else musiqueDeJeu.Stop();
        }
        
        private void RelanceMusiqueJeux(object? sender, EventArgs e)
        {
            musiqueDeJeu.Position = TimeSpan.Zero;
            musiqueDeJeu.Volume = Properties.Settings.Default.Volume;
            musiqueDeJeu.Play();
        }

        private void NouvelleVague()
        {
            if (timerEstActif || estEnCombatAvecBoss) return; // si pas de décompte de vague et pas de boss
            timerEstActif = true;                             // alors commencer le décompte

            if (difficulte == "facile" || difficulte == "moyen") pauseEntreVagues = 5;
            else pauseEntreVagues = 10;

            // lancer un timer pour le debut de chaques vagues
            pauseEntreVaguesTimer = new DispatcherTimer();
            pauseEntreVaguesTimer.Interval = TimeSpan.FromSeconds(1);
            pauseEntreVaguesTimer.Tick += Vague;
            pauseCounter = 0;
            pauseEntreVaguesTimer.Start();
        }

        private void Vague(object? sender, EventArgs e)
        {
            if (pause || estEnCombatAvecBoss)
            {
                pauseEntreVaguesTimer.Stop();
                return;
            }

            pauseCounter++;
            labelWave.Content = $"Vague {nombreDeVagues+1}";
            labelAlerte.Content = $"Prochaine vague dans {pauseEntreVagues - pauseCounter} secondes!";

            if (pauseCounter < pauseEntreVagues) return;
            labelAlerte.Content = " ";
            if (difficulte == "facile" && !TousLesEnnemisSontMort()) return;
            nombreDeVagues++;
            
            int nbrProies;
            int nbrPetitEnnemis;
            int nbrMoyenEnnemis;
            int nbrGrandEnnemis;

            if (nombreDeVagues <= 3)
            {
                nbrProies = nombreDeVagues;
                nbrPetitEnnemis = nombreDeVagues * 3;
                nbrMoyenEnnemis = 1;
                nbrGrandEnnemis = 0;
            }
            else if (nombreDeVagues <= 5)
            {
                nbrProies = nombreDeVagues;
                nbrPetitEnnemis = nombreDeVagues * 3;
                nbrMoyenEnnemis = 2;
                nbrGrandEnnemis = 0;
            }
            else if (nombreDeVagues <= 8)
            {
                nbrProies = nombreDeVagues;
                nbrPetitEnnemis = nombreDeVagues * 2;
                nbrMoyenEnnemis = nombreDeVagues / 2 + 1;
                nbrGrandEnnemis = nombreDeVagues / 8;
            }
            else if (nombreDeVagues <= 13)
            {
                nbrProies = (int)(nombreDeVagues/1.5);
                nbrPetitEnnemis = (int)(nombreDeVagues/1.3);
                nbrMoyenEnnemis = (int)(nombreDeVagues/1.3);
                nbrGrandEnnemis = 2;
            }
            else if (nombreDeVagues <= 21)
            {
                nbrProies = (int)(nombreDeVagues/2);
                nbrPetitEnnemis = nombreDeVagues * 2;
                nbrMoyenEnnemis = nombreDeVagues - 3;
                nbrGrandEnnemis = 1;
            }
            else
            {
                nbrProies = 15; //max
                nbrPetitEnnemis = 35+(nombreDeVagues-21)/2;
                nbrMoyenEnnemis = 20+(nombreDeVagues-21)/3;
                nbrGrandEnnemis = 2+(nombreDeVagues-21)/5;
                if (nombreDeVagues % 4 == 0)
                {
                    nbrPetitEnnemis = nbrPetitEnnemis - 6;
                    nbrMoyenEnnemis = nbrMoyenEnnemis + 3;
                }
                else if (nombreDeVagues % 3 == 0)
                {
                    nbrPetitEnnemis = nbrPetitEnnemis - 10;
                    nbrMoyenEnnemis = nbrMoyenEnnemis - 4;
                    nbrGrandEnnemis = nbrGrandEnnemis + 2;
                }
                else if (nombreDeVagues % 2 == 0)
                {
                    nbrPetitEnnemis = nbrPetitEnnemis + 11;
                    nbrMoyenEnnemis = nbrMoyenEnnemis + 7;
                    nbrGrandEnnemis = 0;
                }
            }

            if (difficulte == "facile")
            {
                nbrPetitEnnemis /= 2;
            }
            if (difficulte == "difficile")
            {
                nbrMoyenEnnemis *= 2;
                nbrGrandEnnemis += 2;
            }

            // poisson disk sampling (voir Sampler.cs)
            Sampler sampler = new Sampler(1260, 680, 50);
            List<Point> listeDePoints = sampler.GeneratePoints();

            // "astuce" pour mêler le contenu d'une list, grace à "user453230"
            // source: https://stackoverflow.com/questions/273313/randomize-a-listt
            Random alea = new Random();
            listeDePoints = listeDePoints.OrderBy(_ => alea.Next()).ToList();

            // creations de nouvelles listes pour chaques type d'ennemis
            List<Point> petits = listeDePoints.Take(nbrPetitEnnemis).ToList();
            List<Point> moyens = listeDePoints.Skip(nbrPetitEnnemis).Take(nbrMoyenEnnemis).ToList(); //skip les petits ennemis
            List<Point> grands = listeDePoints.Skip(nbrPetitEnnemis + nbrMoyenEnnemis).Take(nbrGrandEnnemis).ToList(); //skip les petits et moyens ennemis
            List<Point> proiesList = listeDePoints.Skip(nbrPetitEnnemis + nbrMoyenEnnemis + nbrGrandEnnemis).Take(nbrProies).ToList(); //skip tous les ennemis

            // creation des ennemis
            foreach (Point point in petits) ennemis.Add(new Ennemis(TypeEnnemis.Firefly, point.X, point.Y, 50, 50, 12, canvas));
            foreach (Point point in moyens) ennemis.Add(new Ennemis(TypeEnnemis.Spider, point.X, point.Y, 100, 100, 8, canvas));
            foreach (Point point in grands) ennemis.Add(new Ennemis(TypeEnnemis.Squit, point.X, point.Y, 150, 150, 13, canvas));
            foreach (Point point in proiesList) proies.Add(new Proies(TypeProies.Fly, point.X, point.Y, 50, 50, 3, 500, 200, canvas));
            
            pauseEntreVaguesTimer.Stop();
            timerEstActif = false;
        }

        private bool TousLesEnnemisSontMort()
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

            imagesVie[0] = new BitmapImage(new Uri("pack://application:,,,/img/vie/health0.png"));
            imagesVie[1] = new BitmapImage(new Uri("pack://application:,,,/img/vie/health1.png"));
            imagesVie[2] = new BitmapImage(new Uri("pack://application:,,,/img/vie/health2.png"));
            imagesVie[3] = new BitmapImage(new Uri("pack://application:,,,/img/vie/health3.png"));
            imagesVie[4] = new BitmapImage(new Uri("pack://application:,,,/img/vie/health4.png"));
            imagesVie[5] = new BitmapImage(new Uri("pack://application:,,,/img/vie/health5.png"));

            bgRegenVie = new Rectangle { Width = regenVie.Width-15, Height = regenVie.Height-20, Fill= (SolidColorBrush)new BrushConverter().ConvertFrom("#464646") };
            fgRegenVie = new Rectangle { Width = 0, Height = regenVie.Height-20, Fill=Brushes.DarkRed };
            Canvas.SetTop(bgRegenVie, Canvas.GetTop(regenVie) + 10);
            Canvas.SetTop(fgRegenVie, Canvas.GetTop(regenVie) + 10);
            Canvas.SetLeft(bgRegenVie, Canvas.GetLeft(regenVie));
            Canvas.SetLeft(fgRegenVie, Canvas.GetLeft(regenVie));
            canvas.Children.Add(bgRegenVie);
            canvas.Children.Add(fgRegenVie);

            imgExplosion[0] = new BitmapImage(new Uri("pack://application:,,,/img/explosion/img_explosion0.png"));
            imgExplosion[1] = new BitmapImage(new Uri("pack://application:,,,/img/explosion/img_explosion1.png"));
            imgExplosion[2] = new BitmapImage(new Uri("pack://application:,,,/img/explosion/img_explosion2.png"));
            imgExplosion[3] = new BitmapImage(new Uri("pack://application:,,,/img/explosion/img_explosion3.png"));
            imgExplosion[4] = new BitmapImage(new Uri("pack://application:,,,/img/explosion/img_explosion4.png"));
            imgExplosion[5] = new BitmapImage(new Uri("pack://application:,,,/img/explosion/img_explosion5.png"));

            imgMantis = new BitmapImage(new Uri("pack://application:,,,/img/boss/mantis/mantis.png"));
        }

        private void UpdatePositionSouris()
        {
            // position de la souris sur l'ecran
            Point positionSouris = Mouse.GetPosition(canvas);

            // position du centre de l'image du joueur
            Vector2 positionCentreJoueur = new Vector2(
                (float)(joueur.posJoueur.X + joueurImage.Width / 2.0f),
                (float)(joueur.posJoueur.Y + joueurImage.Height / 2.0f)
            );

            // difference entre les deux
            Vector2 direction = new Vector2(
                (float) (positionSouris.X - positionCentreJoueur.X),
                (float) (positionSouris.Y - positionCentreJoueur.Y)
            );

            float magnitude = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y); // pythagore
            if (magnitude != 0) {
                direction.X /= magnitude;
                direction.Y /= magnitude;
            }

            // position a une distance donnée du joueur, en direction de la souris
            Vector2 positionCible = new Vector2(
                positionCentreJoueur.X + direction.X * distancePisolet,
                positionCentreJoueur.Y + direction.Y * distancePisolet
            );

            angleActuelle = (float) (Math.Atan2(direction.Y, direction.X) * 180 / Math.PI);

            // Update positions for both the weapon and tongue
            UpdatePositionArme(positionSouris, positionCentreJoueur, positionCible);
            UpdatePositionLangue(positionSouris, positionCentreJoueur, positionCible);
        }

        private void UpdatePositionArme(Point positionSouris, Vector2 posCentreJoueur, Vector2 positionCible)
        {
            // calculer la position de l'arme autour du joueur
            float distanceJoueurSouris = Vector2.Distance(posCentreJoueur, new Vector2((float)positionSouris.X, (float)positionSouris.Y));
            positionArme = positionCible;

            // transformation de l'arme (rotation et inversement en X)
            TransformGroup transformGroup = new TransformGroup();
            ScaleTransform inverseArme = new ScaleTransform();
            RotateTransform rotationArme = new RotateTransform(angleActuelle);

            // inverser l'arme si elle est a droite du joueur
            inverseArme.ScaleY = (Math.Abs(angleActuelle) > 90) ? -1 : 1;

            transformGroup.Children.Add(inverseArme);
            transformGroup.Children.Add(rotationArme);
            gun.RenderTransform = transformGroup;

            Canvas.SetTop(gun, positionArme.Y);
            Canvas.SetLeft(gun, positionArme.X);
        }

        private void UpdatePositionLangue(Point positionSouris, Vector2 posCentreJoueur, Vector2 positionCible)
        {
            // changer la rotation de la langue a partir de l'angle calculer précedament
            RotateTransform rotationArme = new RotateTransform(angleActuelle);
            if (!tirLangue) langueJoueur.RenderTransform = rotationArme;

            Canvas.SetTop(langueJoueur, positionCible.X > 0 ? posCentreJoueur.Y : posCentreJoueur.Y + langueJoueur.Height / 2.0f);
            Canvas.SetLeft(langueJoueur, posCentreJoueur.X);
        }

        public static bool IntersectionLigneLigne(Line line1, Line line2, out Point intersection)
        {
            // version adapté de l'expliquation de Gareth Rees en C#,
            // expliquation disponible ici:https://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
            Point p = new Point(line1.X1, line1.Y1);
            Point r = new Point(line1.X2 - line1.X1, line1.Y2 - line1.Y1);
            Point q = new Point(line2.X1, line2.Y1);
            Point s = new Point(line2.X2 - line2.X1, line2.Y2 - line2.Y1);

            double rCrossS = ProduitCroise(r, s);
            Point qMinusP = new Point(q.X - p.X, q.Y - p.Y);
            double qMinusPCrossS = ProduitCroise(qMinusP, s);
            double qMinusPCrossR = ProduitCroise(qMinusP, r);

            if (rCrossS == 0) {
                if (qMinusPCrossR == 0) {
                    double t0 = (qMinusP.X * r.X + qMinusP.Y * r.Y) / (r.X * r.X + r.Y * r.Y);
                    double t1 = t0 + (s.X * r.X + s.Y * r.Y) / (r.X * r.X + r.Y * r.Y);

                    if ((t0 >= 0 && t0 <= 1) || (t1 >= 0 && t1 <= 1)) {
                        intersection = new Point(p.X + t0 * r.X, p.Y + t0 * r.Y);
                        return true;
                    }
                }
                intersection = new Point();
                return false;
            }

            if (rCrossS != 0) {
                double t = qMinusPCrossS / rCrossS;
                double u = qMinusPCrossR / rCrossS;

                if (t >= 0 && t <= 1 && u >= 0 && u <= 1) {
                    intersection = new Point(p.X + t * r.X, p.Y + t * r.Y);
                    return true;
                }
            }
            
            intersection = new Point();
            return false;
        }

        private static double ProduitCroise(Point v1, Point v2) // Produit croisé
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }

        // version adapté et modifier du code de Nils Pipenbrinck en C#
        // https://stackoverflow.com/questions/2259476/rotating-a-point-about-another-point-2d
        private Point PivotPoint(Point point, Point centre, double a)
        {
            double radians = a * (Math.PI / 180); // Convertire angle en radians
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            double x = point.X - centre.X;
            double y = point.Y - centre.Y;

            double nouveauX = x * cos - y * sin + centre.X;
            double nouveauY = x * sin + y * cos + centre.Y;

            return new Point(nouveauX, nouveauY);
        }

        private void Loop(object? sender, EventArgs e)
        {
            if (pause) return;
            Stopwatch stopwatch = new Stopwatch(); // WPF is terrible for game dev.
            stopwatch.Start();

            // bossfight 
            if ((nombreDeVagues + 1) % 9 == 0)
            {
                if (!estEnCombatAvecBoss && TousLesEnnemisSontMort()) startBoss();
            }
            else
            {
                if (!estEnCombatAvecBoss)
                {
                    // si tous les ennemis sont mort
                    if (difficulte == "facile" || difficulte == "moyen") {
                        if (ennemis.Count <= 0 && proies.Count <= 0) NouvelleVague();
                    }
                    // si la difficulter est dure ou extreme alors c'est dès que la pause entre chaque vague. même si il reste des ennemis
                    else NouvelleVague();
                }
            }

            Ennemis.UpdateEnnemis(ennemis, Balles, canvas , ref joueur);
            Proies.UpdateProies(canvas, proies, joueur.hitbox);
            
            if (this.estEnCombatAvecBoss && this.estBossApparu)
            {
                // update le boss
                mante.UpdateBossMante(Balles, joueur);

                if (!mante.estVivant)
                {
                    this.estEnCombatAvecBoss = false;
                    this.estBossApparu = false;
                    nombreDeVagues++;
                }
            }

            AfficheScore();
            AfficheMultiplicateurDeScore();

            AffichageDeVie(joueur.nombreDeVie);
            CheckBallesSortieEcran();
            CheckCollisionProie();

            if (!joueur.estEnRoulade) joueur.ChangeJoueurDirection();
            UpdatePositionSouris();
            joueur.UpdatePositionJoueur(canvas);

            stopwatch.Stop();
            Console.WriteLine($"FPS: {Math.Round(1000.0 / stopwatch.Elapsed.TotalMilliseconds)}  {stopwatch.Elapsed} aka {stopwatch.Elapsed.TotalMilliseconds}ms");
        }

        private async Task startBoss()
        {
            this.estEnCombatAvecBoss = true;
            this.estBossApparu = false;

            await Task.Delay(500);
            
            // "animation" d'explosions
            Explosion(100, 100, 500); 
            Explosion(300, 100, 500); 
            Explosion(500, 100, 500);
            Explosion(700, 100, 500);
            Explosion(900, 100, 500);
            Explosion(1100, 100, 500);

            ChangerFond("img/arena/arena_broken_top.png");
            await Task.Delay(300);
            mante = new BossMante(canvas, imgMantis, joueur, 4000, 215, 266);
            await Task.Delay(400);

            joueur.mouvementRestraintHaut = true;
            this.estBossApparu = true;
        }

        private void ChangerFond(string path)
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

        public void Explosion(int x, int y, int size)
        {
            Image boom = new Image();
            boom.Width = size;
            boom.Height = size;
            Canvas.SetLeft(boom, x - size / 2);
            Canvas.SetTop(boom, y - size / 2); 
            RenderOptions.SetBitmapScalingMode(boom, BitmapScalingMode.NearestNeighbor);
            canvas.Children.Add(boom);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            int compteurImage=0, maxImage = 5;

            timer.Tick += (e,s) => {
                compteurImage++;
                if (compteurImage >= maxImage) {
                    timer.Stop();
                    canvas.Children.Remove(boom);
                }
                boom.Source = imgExplosion[compteurImage];
            };
            timer.Start();
        }

        private void AffichageDeVie(int nombreDeVie)
        {
            if (nombreDeVie <= 0)
            {
                Console.WriteLine("Mort");
                ImgvieJoueur.Source = imagesVie[0];
                pause = true;
                lab_Defaite.Visibility = Visibility.Visible;
                mort(nombreDeVie);
            } else {
                ImgvieJoueur.Source = imagesVie[nombreDeVie];
            }
        }
        
        public void mort(int nombreDeVie)
        {
            minuterie.Stop();
            pauseEntreVaguesTimer.Stop();
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
            Ennemis.ReccomencerEnnemis(ennemis, canvas);
            Proies.ReccomencerProies(proies, canvas);

            if (estBossApparu) estBossApparu = false;
            if (estEnCombatAvecBoss)
            {
                estEnCombatAvecBoss = false;
                mante.VaincreMante();
                ChangerFond("img/arena/arena_unaltered.png");
            }

            joueur.score = 0;
            joueur.nombreDeVie = nombreDeVie;
            AffichageDeVie(nombreDeVie);
            AfficheScore();
            nombreDeVagues = 0;
            pauseEntreVagues = 5;
            pauseEntreVaguesTimer.Stop();
            pauseCounter = 0;
            pause = false;
            
            joueur.deplacerGauche = false;
            joueur.deplacerDroite = false;
            joueur.deplacerHaut = false;
            joueur.deplacerBas = false;
            joueur.keyBufferGauche = false;
            joueur.keyBufferDroite = false;
            joueur.keyBufferHaut = false;
            joueur.keyBufferBas = false;
            
            lab_Defaite.Visibility = Visibility.Collapsed;
            minuterie.Start();
            pauseEntreVaguesTimer.Start();
        }

        private void CheckBallesSortieEcran()
        {
            // pour chaques balles
            for (int i = 0; i < Balles.Count; i++)
            {
                Balle balle = Balles[i];
                balle.UpdatePositionBalles();
                // si la balle est en dehors de l'ecran
                if (balle.X < -balle.balleImage.ActualWidth || balle.Y < -balle.balleImage.ActualHeight
                    || balle.X > grid.ActualWidth || balle.Y > grid.ActualHeight)
                {
                    // supprimer la balle 
                    Balles.RemoveAt(i);
                    canvas.Children.Remove(balle.balleImage);
                }
            }
        }

        private void CheckCollisionProie()
        {
            if (expensionLangue)
            {
                if (langueJoueur.Width < 300)
                {
                    // create two lines from start to end of tongue
                    RotateTransform rotation = (RotateTransform)langueJoueur.RenderTransform;
                    Point tcentre = new Point(Canvas.GetLeft(langueJoueur), Canvas.GetTop(langueJoueur) + langueJoueur.Height / 2.0f);

                    Point t11 = new Point(Canvas.GetLeft(langueJoueur), Canvas.GetTop(langueJoueur));
                    Point t12 = new Point(Canvas.GetLeft(langueJoueur) + langueJoueur.Width, Canvas.GetTop(langueJoueur));

                    Point t21 = new Point(Canvas.GetLeft(langueJoueur), Canvas.GetTop(langueJoueur) + langueJoueur.Height);
                    Point t22 = new Point(Canvas.GetLeft(langueJoueur) + langueJoueur.Width, Canvas.GetTop(langueJoueur) + langueJoueur.Height);

                    // rotate points based on the tongues angle
                    Point point_start_1 = PivotPoint(t11, tcentre, rotation.Angle);
                    Point point_start_2 = PivotPoint(t21, tcentre, rotation.Angle);
                    Point point_end_1 = PivotPoint(t22, tcentre, rotation.Angle);
                    Point point_end_2 = PivotPoint(t12, tcentre, rotation.Angle);

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

                    foreach (Proies proie in proies.ToList())
                    {
                        int x = (int)Canvas.GetLeft(proie.Hitbox);
                        int y = (int)Canvas.GetTop(proie.Hitbox);

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
                            X2 = x + proie.Hitbox.Width,
                            Y2 = y,
                        };

                        Line line_BD = new Line
                        {
                            X1 = x + proie.Hitbox.Width,
                            Y1 = y,
                            X2 = x + proie.Hitbox.Width,
                            Y2 = y + proie.Hitbox.Height,
                        };

                        Line line_DC = new Line
                        {
                            X1 = x + proie.Hitbox.Width,
                            Y1 = y + proie.Hitbox.Height,
                            X2 = x,
                            Y2 = y + proie.Hitbox.Height,
                        };

                        Line line_CA = new Line
                        {
                            X1 = x,
                            Y1 = y + proie.Hitbox.Height,
                            X2 = x,
                            Y2 = y,
                        };

                        if (IntersectionLigneLigne(line_frog_1, line_AB, out intersection)
                            || IntersectionLigneLigne(line_frog_1, line_BD, out intersection)
                            || IntersectionLigneLigne(line_frog_1, line_DC, out intersection)
                            || IntersectionLigneLigne(line_frog_1, line_CA, out intersection)

                            || IntersectionLigneLigne(line_frog_2, line_AB, out intersection)
                            || IntersectionLigneLigne(line_frog_2, line_BD, out intersection)
                            || IntersectionLigneLigne(line_frog_2, line_DC, out intersection)
                            || IntersectionLigneLigne(line_frog_2, line_CA, out intersection))
                        {
                            expensionLangue = false;

                            if (canvas.Children.Contains(proie.Image)) canvas.Children.Remove(proie.Image);
                            if (canvas.Children.Contains(proie.Hitbox)) canvas.Children.Remove(proie.Hitbox);
                            if (proies.Contains(proie)) proies.Remove(proie);

                            if (joueur.nombreDeVie < 5) joueur.proieManger++;
                            if (joueur.proieManger >= joueur.proiePourHeal)
                            {
                                joueur.heal();
                                joueur.proieManger = 0;
                            }
                        }
                    }
                    
                    fgRegenVie.Width = (bgRegenVie.ActualWidth) * ((double)joueur.proieManger / (double)joueur.proiePourHeal);

                    foreach (Ennemis ennemie in ennemis.ToList())
                    {
                        double x = ennemie.X;
                        double y = ennemie.Y;

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
                            X2 = x + ennemie.width,
                            Y2 = y,
                        };

                        Line line_BD = new Line
                        {
                            X1 = x + ennemie.width,
                            Y1 = y,
                            X2 = x + ennemie.width,
                            Y2 = y + ennemie.height,
                        };

                        Line line_DC = new Line
                        {
                            X1 = x + ennemie.width,
                            Y1 = y + ennemie.height,
                            X2 = x,
                            Y2 = y + ennemie.height,
                        };

                        Line line_CA = new Line
                        {
                            X1 = x,
                            Y1 = y + ennemie.height,
                            X2 = x,
                            Y2 = y,
                        };

                        if (IntersectionLigneLigne(line_frog_1, line_AB, out intersection)
                            || IntersectionLigneLigne(line_frog_1, line_BD, out intersection)
                            || IntersectionLigneLigne(line_frog_1, line_DC, out intersection)
                            || IntersectionLigneLigne(line_frog_1, line_CA, out intersection)

                            || IntersectionLigneLigne(line_frog_2, line_AB, out intersection)
                            || IntersectionLigneLigne(line_frog_2, line_BD, out intersection)
                            || IntersectionLigneLigne(line_frog_2, line_DC, out intersection)
                            || IntersectionLigneLigne(line_frog_2, line_CA, out intersection))
                        {
                            expensionLangue = false;

                            joueur.nombreDeVie--;
                            joueur.score -= 500;
                        }
                    }
                    if (expensionLangue) langueJoueur.Width += expensionLangueVitesse;
                }
                else
                {
                    expensionLangue = false;
                }
            }
            else
            {
                if (langueJoueur.Width > 0)
                {
                    if (langueJoueur.Width <= retractionLangueVitesse) langueJoueur.Width = 0;
                    else langueJoueur.Width -= retractionLangueVitesse;
                }
                else tirLangue = false;
            }

        }

        private void SonArme()
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

        private void TirerArme()
        {
            SonArme();
            double vitesseBalle = 30.0;
            int scaleX = (Math.Abs(angleActuelle) > 90) ? -1 : 1;
            Balle balle = new Balle(positionArme.X, positionArme.Y, angleActuelle, vitesseBalle, 10, canvas, imageBalle, scaleX);
            Balles.Add(balle);
        }
        
        private void TirerLangue()
        {
            SonLangue();
            if (tirLangue) return;
            else tirLangue = true;
            expensionLangue = true;
        }

        public void AfficheScore()
        {
            labelScore.Content = $"Score : {joueur.score} ";
        }

        public void AfficheMultiplicateurDeScore()
        {
            labelCombo.Content = $"x{joueur.multiplicateurDeScore} ";
        }

        private void keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Touche.ToucheDroite && !pause)
            {
                joueur.keyBufferDroite = true;
                joueur.keyBufferGauche = false;
                if (!joueur.estEnRoulade)
                {
                    joueur.deplacerDroite = true;
                    joueur.deplacerGauche = false;
                    joueur.directionJoueur = Joueur.Directions.droite;
                }
            }

            if (e.Key == Touche.ToucheGauche && !pause)
            {
                joueur.keyBufferGauche = true;
                joueur.keyBufferDroite = false;
                if (!joueur.estEnRoulade)
                {
                    joueur.deplacerGauche = true;
                    joueur.deplacerDroite = false;
                    joueur.directionJoueur = Joueur.Directions.gauche;
                }
            }

            if (e.Key == Touche.ToucheBas && !pause)
            {
                joueur.keyBufferBas = true;
                joueur.keyBufferHaut = false;
                if (!joueur.estEnRoulade)
                {
                    joueur.deplacerBas = true;
                    joueur.deplacerHaut = false;
                    joueur.directionJoueur = Joueur.Directions.bas;
                }
            }

            if (e.Key == Touche.ToucheHaut && !pause)
            {
                joueur.keyBufferHaut = true;
                joueur.keyBufferBas = false;
                if (!joueur.estEnRoulade)
                {
                    joueur.deplacerHaut = true;
                    joueur.deplacerBas = false;
                    joueur.directionJoueur = Joueur.Directions.haut;
                }
            }

            if (e.Key == Touche.ToucheRoulade && !pause)
            {
                joueur.estEnRoulade = true;
            }

            if (e.Key == Touche.TouchePause)
            {
                // dont let the player pause during a bossfight
                if (estEnCombatAvecBoss) return;

                pause=!pause;
                if (pause)
                {
                    lab_Pause.Visibility = Visibility.Visible;
                }
                else
                {
                    if (!pauseEntreVaguesTimer.IsEnabled) pauseEntreVaguesTimer.Start();
                    lab_Pause.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void keyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Touche.ToucheDroite)
            {
                joueur.deplacerDroite = false;
                joueur.keyBufferDroite = false;
            }
            if (e.Key == Touche.ToucheGauche)
            {
                joueur.keyBufferGauche = false;
                joueur.deplacerGauche = false;
            }
            if (e.Key == Touche.ToucheBas)
            {
                joueur.deplacerBas = false;
                joueur.keyBufferBas = false;
            }
            if (e.Key == Touche.ToucheHaut)
            {
                joueur.deplacerHaut = false;
                joueur.keyBufferHaut = false;
            }
        }

        private void leftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (pause) return;
            TirerArme();
        }

        private void rightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (pause) return;
            TirerLangue();
        }
    }
}