using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Froggun.MainWindow;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Controls;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Shapes;

namespace Froggun
{
    internal class Joueur
    {
        public TransformGroup joueurTransformGroup { get; set; }
        public RotateTransform joueurRoulade { get; set; }
        public ScaleTransform joueurFlip { get; set; }
        public Vector2 posJoueur { get; set; }
        public Vector2 vitesseJoueur { get; set; }

        public int nombreDeVie { get; set; }
        public bool estInvinsible { get; set; }

        public bool deplacerGauche { get; set; }
        public bool deplacerDroite { get; set; }
        public bool deplacerHaut { get; set; }
        public bool deplacerBas { get; set; }

        public bool keyBufferGauche { get; set; }
        public bool keyBufferDroite { get; set; }
        public bool keyBufferHaut { get; set; }
        public bool keyBufferBas { get; set; }

        public bool doitFlip { get; set; }
        public bool estEnRoulade { get; set; }
        public double tempsRoulade { get; set; }
        public double dureeRoulade { get; set; }

        public float correctionVitesseDiagonal { get; set; }
        public float vitesseDeplacement { get; set; }
        public float friction { get; set; }
        public int proiePourHeal { get; set; }
        public int proieManger { get; set; }

        public Image joueurImage { get; set; }
        public Rect hitbox { get; set; }
        public Grid grid { get; set; }

        public BitmapImage front { get; set; }
        public BitmapImage side { get; set; }
        public BitmapImage back { get; set; }

        public BitmapImage frontHit { get; set; }
        public BitmapImage sideHit { get; set; }
        public BitmapImage backHit { get; set; }
        public int degats { get; set; }
        
        private DispatcherTimer blinkTimer = new DispatcherTimer();
        
        private int blinkFrame=0;

        public enum Directions
        {
            gauche,
            droite,
            haut,
            bas,
            diagHautGauche,
            diagHautDroite,
            diagBasGauche,
            diagBasDroite
        }

        public Directions directionJoueur { get; set; }

        public double score {  get; set; }
        public int killStreak { get; set; }
        public int timerKillstreak { get; set; }
        public double multiplicateurDeScore { get; set; }
        public DispatcherTimer StreakTimerUpdate { get; set; }

        public Joueur(Image player, Rect hitbox, int posX, int posY, Grid grid, BitmapImage front, BitmapImage side, BitmapImage back, BitmapImage frontHit, BitmapImage sideHit, BitmapImage backHit)
        {
            RenderOptions.SetBitmapScalingMode(player, BitmapScalingMode.NearestNeighbor);
            this.joueurImage = player;
            this.hitbox = hitbox;
            this.grid = grid;

            this.nombreDeVie = 5;
            this.estInvinsible = false;

            this.deplacerDroite = false;
            this.deplacerHaut = false;
            this.deplacerBas = false;
            this.deplacerHaut = false;
            this.doitFlip = false;
            this.estEnRoulade = false;
            this.tempsRoulade = 0;
            this.dureeRoulade = 300;

            this.correctionVitesseDiagonal = (float)(1.0f / Math.Sqrt(2.0f));
            this.vitesseDeplacement = 8.0f;
            this.friction = 0.4f;
            this.directionJoueur = Directions.droite;

            this.proiePourHeal = 3;
            this.proieManger = 0;

            this.front = front;
            this.side = side;
            this.back = back;
            this.frontHit = frontHit;
            this.sideHit = sideHit;
            this.backHit = backHit;

            this.joueurTransformGroup = new TransformGroup();
            this.joueurRoulade = new RotateTransform();
            this.joueurFlip = new ScaleTransform();
            this.posJoueur = new Vector2((float)posX, (float)posY);
            this.vitesseJoueur = new Vector2();

            this.score = 0;
            this.killStreak = 0;
            this.timerKillstreak = 0;
            this.multiplicateurDeScore = 0;

            this.StreakTimerUpdate = new DispatcherTimer();
            this.StreakTimerUpdate.Interval = TimeSpan.FromSeconds(1);
            this.StreakTimerUpdate.Tick += (s, e) =>
            {
                if (this.timerKillstreak > 0) this.timerKillstreak--;
                if (this.timerKillstreak <= 0)
                {
                    this.killStreak = 0;
                    this.multiplicateurDeScore = 0;
                }
            };
            this.StreakTimerUpdate.Start();

            // combien de degats fait le joueur
            this.degats = 50;

            blinkTimer = new DispatcherTimer(); 
            blinkTimer.Interval = TimeSpan.FromMilliseconds(100);
            blinkTimer.Tick += BlinkPlayerEffect;
            blinkTimer.Start();
        }

        public void UpdatePositionJoueur(Canvas c)
        {
            // ~12 frames d'invinsibilité 
            if (tempsRoulade > 50 && tempsRoulade < 250 || blinkFrame>0) estInvinsible = true;
            else estInvinsible = false;

            Vector2 nouvelleVitesseJoueur = vitesseJoueur;
            Vector2 nouvellePositionJoueur = posJoueur;
            Rect newHitbox = hitbox;

            if (estEnRoulade)
            {
                // animation de la roulade 
                tempsRoulade += 16.6666667;
                joueurRoulade.Angle = -((tempsRoulade / dureeRoulade) * 2 * Math.PI) * 180 / Math.PI;

                nouvelleVitesseJoueur.X = 0;
                nouvelleVitesseJoueur.Y = 0;

                switch (directionJoueur)
                {
                    case Directions.bas:
                        nouvelleVitesseJoueur.Y = 15.0f;
                        break;
                    case Directions.haut:
                        nouvelleVitesseJoueur.Y = -15.0f;
                        break;
                    case Directions.droite:
                        nouvelleVitesseJoueur.X = 15.0f;
                        break;
                    case Directions.gauche:
                        nouvelleVitesseJoueur.X = -15.0f;
                        break;
                    case Directions.diagBasGauche:
                        nouvelleVitesseJoueur.X = -15.0f * correctionVitesseDiagonal;
                        nouvelleVitesseJoueur.Y =  15.0f * correctionVitesseDiagonal;
                        break;
                    case Directions.diagBasDroite:
                        nouvelleVitesseJoueur.X = 15.0f * correctionVitesseDiagonal;
                        nouvelleVitesseJoueur.Y = 15.0f * correctionVitesseDiagonal;
                        break;
                    case Directions.diagHautGauche:
                        nouvelleVitesseJoueur.X = -15.0f * correctionVitesseDiagonal;
                        nouvelleVitesseJoueur.Y = -15.0f * correctionVitesseDiagonal;
                        break;
                    case Directions.diagHautDroite:
                        nouvelleVitesseJoueur.X =  15.0f * correctionVitesseDiagonal;
                        nouvelleVitesseJoueur.Y = -15.0f * correctionVitesseDiagonal;
                        break;
                }

                if (tempsRoulade > dureeRoulade)
                {
                    estEnRoulade = false;
                    tempsRoulade = 0;
                    if (this.keyBufferHaut && this.keyBufferDroite)
                    {
                        this.deplacerHaut = true;
                        this.deplacerDroite = true;
                        this.deplacerBas = false;
                        this.deplacerGauche = false;
                        this.directionJoueur = Joueur.Directions.diagHautDroite;
                    }
                    else if (this.keyBufferHaut && this.keyBufferGauche)
                    {
                        this.deplacerHaut = true;
                        this.deplacerGauche = true;
                        this.deplacerBas = false;
                        this.deplacerDroite = false;
                        this.directionJoueur = Joueur.Directions.diagHautGauche;
                    }
                    else if (this.keyBufferBas && this.keyBufferDroite)
                    {
                        this.deplacerBas = true;
                        this.deplacerDroite = true;
                        this.deplacerHaut = false;
                        this.deplacerGauche = false;
                        this.directionJoueur = Joueur.Directions.diagBasDroite;
                    }
                    else if (this.keyBufferBas && this.keyBufferGauche)
                    {
                        this.deplacerBas = true;
                        this.deplacerGauche = true;
                        this.deplacerHaut = false;
                        this.deplacerDroite = false;
                        this.directionJoueur = Joueur.Directions.diagBasGauche;
                    }
                    else if (this.keyBufferHaut)
                    {
                        this.deplacerHaut = true;
                        this.deplacerBas = false;
                        this.deplacerGauche = false;
                        this.deplacerDroite = false;
                        this.directionJoueur = Joueur.Directions.haut;
                    }
                    else if (this.keyBufferBas)
                    {
                        this.deplacerBas = true;
                        this.deplacerHaut = false;
                        this.deplacerGauche = false;
                        this.deplacerDroite = false;
                        this.directionJoueur = Joueur.Directions.bas;
                    }
                    else if (this.keyBufferDroite)
                    {
                        this.deplacerDroite = true;
                        this.deplacerGauche = false;
                        this.deplacerHaut = false;
                        this.deplacerBas = false;
                        this.directionJoueur = Joueur.Directions.droite;
                    }
                    else if (this.keyBufferGauche)
                    {
                        this.deplacerGauche = true;
                        this.deplacerDroite = false;
                        this.deplacerHaut = false;
                        this.deplacerBas = false;
                        this.directionJoueur = Joueur.Directions.gauche;
                    }
                }
            }
            else
            {
                joueurRoulade.Angle = 0;
                if      (deplacerGauche && Canvas.GetLeft(joueurImage) > 0)                         nouvelleVitesseJoueur.X = -vitesseDeplacement; // bouger vers la gauche
                else if (deplacerDroite && Canvas.GetLeft(joueurImage) < grid.Width - joueurImage.Width) nouvelleVitesseJoueur.X = vitesseDeplacement;  // bouger vers la droite
                else
                {
                    nouvelleVitesseJoueur.X *= friction; // réduire la vitesse du joueur en fonction de la friction
                    if (Math.Abs(nouvelleVitesseJoueur.X) < 0.1f) nouvelleVitesseJoueur.X = 0; // si la vitesse (positive) est inférieure à 0.1, arrêter le mouvement
                }

                if      (deplacerHaut && Canvas.GetTop(joueurImage) > 0)                          nouvelleVitesseJoueur.Y = -vitesseDeplacement; // bouger vers le haut
                else if (deplacerBas && Canvas.GetTop(joueurImage) < grid.Height - joueurImage.Height) nouvelleVitesseJoueur.Y = vitesseDeplacement;  // bouger vers le bas 
                else
                {
                    nouvelleVitesseJoueur.Y *= friction;
                    if (Math.Abs(nouvelleVitesseJoueur.Y) < 0.1f) nouvelleVitesseJoueur.Y = 0;
                }

                // Corrige la vitesse du joueur si il bouge en diagonale (car sqrt(2) = 1.4 et pas 1)
                if (directionJoueur == Directions.diagHautGauche || directionJoueur == Directions.diagHautDroite ||
                    directionJoueur == Directions.diagBasGauche || directionJoueur == Directions.diagBasDroite)
                {
                    nouvelleVitesseJoueur.X *= correctionVitesseDiagonal;
                    nouvelleVitesseJoueur.Y *= correctionVitesseDiagonal;
                }
            }

            nouvellePositionJoueur.X += nouvelleVitesseJoueur.X;
            nouvellePositionJoueur.Y += nouvelleVitesseJoueur.Y;
            newHitbox = new Rect { X=posJoueur.X, Y=posJoueur.Y, Width=joueurImage.Width, Height=joueurImage.Height };
            
            Vector2 nouveauxCentreJoueur = new Vector2((float) (nouvellePositionJoueur.X + joueurImage.Width/2), (float)(nouvellePositionJoueur.Y + joueurImage.Height / 2));
            Ellipse e = new Ellipse { Width = 1280, Height = 720 };
            
            Canvas.SetTop(e, 0);
            Canvas.SetLeft(e, 0);
            if (!EllipseContains(e, nouveauxCentreJoueur))
            {
                //apply oppositve velocity to slow down player
                nouvellePositionJoueur.X -= nouvelleVitesseJoueur.X;
                nouvellePositionJoueur.Y -= nouvelleVitesseJoueur.Y;
            }

            vitesseJoueur = nouvelleVitesseJoueur;
            posJoueur = nouvellePositionJoueur;
            hitbox = newHitbox;

            joueurTransformGroup.Children.Clear();
            joueurImage.RenderTransformOrigin = new Point(0.5, 0.5);
            joueurTransformGroup.Children.Add(joueurRoulade);
            joueurTransformGroup.Children.Add(joueurFlip);
            joueurImage.RenderTransform = joueurTransformGroup;

            Canvas.SetLeft(joueurImage, posJoueur.X);
            Canvas.SetTop(joueurImage, posJoueur.Y);
        }

        public void ChangeJoueurDirection()
        {
            if (estEnRoulade) return;

            // Corrige la direction du joueur
            if (deplacerBas && deplacerDroite)       directionJoueur = Directions.diagBasDroite;
            else if (deplacerBas && deplacerGauche)  directionJoueur = Directions.diagBasGauche;
            else if (deplacerHaut && deplacerDroite) directionJoueur = Directions.diagHautDroite;
            else if (deplacerHaut && deplacerGauche) directionJoueur = Directions.diagHautGauche;
            else if (deplacerDroite)                 directionJoueur = Directions.droite;
            else if (deplacerGauche)                 directionJoueur = Directions.gauche;
            else if (deplacerBas)                    directionJoueur = Directions.bas;
            else if (deplacerHaut)                   directionJoueur = Directions.haut;

            // Inverse l'image du joueur si nécessaire
            doitFlip = (directionJoueur == Directions.gauche || directionJoueur == Directions.diagHautGauche || directionJoueur == Directions.diagBasGauche);
            joueurFlip.ScaleX = doitFlip ? 1 : -1;

            // Change l'image du joueur dépendament de sa direction
            if (blinkFrame % 2 == 0)
            {
                if (directionJoueur == Directions.gauche || directionJoueur == Directions.droite)                                                       joueurImage.Source = side;
                if (directionJoueur == Directions.haut   || directionJoueur == Directions.diagHautGauche   || directionJoueur == Directions.diagHautDroite)   joueurImage.Source = back;
                if (directionJoueur == Directions.bas || directionJoueur == Directions.diagBasGauche || directionJoueur == Directions.diagBasDroite) joueurImage.Source = front;
            }
            else
            {
                if (directionJoueur == Directions.gauche || directionJoueur == Directions.droite)                                                       joueurImage.Source = sideHit;
                if (directionJoueur == Directions.haut   || directionJoueur == Directions.diagHautGauche   || directionJoueur == Directions.diagHautDroite)   joueurImage.Source = backHit;
                if (directionJoueur == Directions.bas || directionJoueur == Directions.diagBasGauche || directionJoueur == Directions.diagBasDroite) joueurImage.Source = frontHit;
            }
        }

        public void heal()
        {
            if (nombreDeVie+1<=5) nombreDeVie++;
        }

        public void hit(int degats)
        {
            if (estInvinsible) return;
         
            nombreDeVie-=degats;
            blinkFrame += 7;
        }

        private void BlinkPlayerEffect(object? sender, EventArgs e) {
            if (blinkFrame-1>=0) blinkFrame--;
        }

        // https://stackoverflow.com/questions/13285007/how-to-determine-if-a-point-is-within-an-ellipse
        public bool EllipseContains(Ellipse Ellipse, Vector2 point)
        {
            Point center = new Point(
                  Canvas.GetLeft(Ellipse) + (Ellipse.Width / 2),
                  Canvas.GetTop(Ellipse) + (Ellipse.Height / 2));

            double _xRadius = Ellipse.Width / 2;
            double _yRadius = Ellipse.Height / 2;

            if (_xRadius <= 0.0 || _yRadius <= 0.0) return false;
            
            // X^2/a^2 + Y^2/b^2 <= 1
            Point normalized = new Point(point.X - center.X, point.Y - center.Y);
            return ((double)(normalized.X * normalized.X) / (_xRadius * _xRadius)) + ((double)(normalized.Y * normalized.Y) / (_yRadius * _yRadius)) <= 1.0;
        }
    }
}
