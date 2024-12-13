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

        public bool doitFlip { get; set; }
        public bool estEnRoulade { get; set; }
        public double tempsRoulade { get; set; }
        public double dureeRoulade { get; set; }

        public float correctionVitesseDiagonal { get; set; }
        public float vitesseDeplacement { get; set; }
        public float friction { get; set; }

        public Image player { get; set; }
        public Grid grid { get; set; }

        public BitmapImage front { get; set; }
        public BitmapImage side { get; set; }
        public BitmapImage back { get; set; }

        public BitmapImage frontHit { get; set; }
        public BitmapImage sideHit { get; set; }
        public BitmapImage backHit { get; set; }
        
        private DispatcherTimer blinkTimer = new DispatcherTimer();
        private int blinkFrame=0;

        public enum Directions
        {
            left,
            right,
            up,
            down,
            diagUpLeft,
            diagUpRight,
            diagDownLeft,
            diagDownRight
        }

        public Directions directionJoueur { get; set; }

        public Joueur(Image player, Grid grid, BitmapImage front, BitmapImage side, BitmapImage back, BitmapImage frontHit, BitmapImage sideHit, BitmapImage backHit)
        {
            this.player = player;
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
            this.directionJoueur = Directions.right;

            this.front = front;
            this.side = side;
            this.back = back;
            this.frontHit = frontHit;
            this.sideHit = sideHit;
            this.backHit = backHit;

            this.joueurTransformGroup = new TransformGroup();
            this.joueurRoulade = new RotateTransform();
            this.joueurFlip = new ScaleTransform();
            this.posJoueur = new Vector2(50.0f, 50.0f);
            this.vitesseJoueur = new Vector2();
        }

        public void UpdatePositionJoueur()
        {
            Vector2 nouvelleVitesseJoueur = vitesseJoueur;
            Vector2 nouvellePositionJoueur = posJoueur;

            if (estEnRoulade)
            {
                // animation de la roulade 
                tempsRoulade += 16.6666667;
                joueurRoulade.Angle = ((tempsRoulade / dureeRoulade) * 2 * Math.PI) * 180 / Math.PI * (doitFlip ? 1 : -1);
                nouvelleVitesseJoueur.X = 0;
                nouvelleVitesseJoueur.Y = 0;

                switch (directionJoueur)
                {
                    case Directions.down:
                        nouvelleVitesseJoueur.Y = 15.0f;
                        break;
                    case Directions.up:
                        nouvelleVitesseJoueur.Y = -15.0f;
                        break;
                    case Directions.right:
                        nouvelleVitesseJoueur.X = 15.0f;
                        break;
                    case Directions.left:
                        nouvelleVitesseJoueur.X = -15.0f;
                        break;
                    case Directions.diagDownLeft:
                        nouvelleVitesseJoueur.X = -15.0f * correctionVitesseDiagonal;
                        nouvelleVitesseJoueur.Y = 15.0f * correctionVitesseDiagonal;
                        break;
                    case Directions.diagDownRight:
                        nouvelleVitesseJoueur.X = 15.0f * correctionVitesseDiagonal;
                        nouvelleVitesseJoueur.Y = 15.0f * correctionVitesseDiagonal;
                        break;
                    case Directions.diagUpLeft:
                        nouvelleVitesseJoueur.X = -15.0f * correctionVitesseDiagonal;
                        nouvelleVitesseJoueur.Y = -15.0f * correctionVitesseDiagonal;
                        break;
                    case Directions.diagUpRight:
                        nouvelleVitesseJoueur.X = 15.0f * correctionVitesseDiagonal;
                        nouvelleVitesseJoueur.Y = -15.0f * correctionVitesseDiagonal;
                        break;
                }

                if (tempsRoulade > dureeRoulade)
                {
                    estEnRoulade = false;
                    tempsRoulade = 0;
                }
            }
            else
            {
                joueurRoulade.Angle = 0;
                if (deplacerGauche && Canvas.GetLeft(player) > 0) nouvelleVitesseJoueur.X = -vitesseDeplacement; // bouger vers la gauche
                else if (deplacerDroite && Canvas.GetLeft(player) < grid.ActualWidth - player.ActualWidth*2) nouvelleVitesseJoueur.X = vitesseDeplacement; // bouger vers la droite
                else
                {
                    nouvelleVitesseJoueur.X *= friction; // réduire la vitesse du joueur en fonction de la friction
                    if (Math.Abs(nouvelleVitesseJoueur.X) < 0.1f) nouvelleVitesseJoueur.X = 0; // si la vitesse (positive) est inférieure à 0.1, arrêter le mouvement
                }

                if (deplacerHaut && Canvas.GetTop(player) > 0) nouvelleVitesseJoueur.Y = -vitesseDeplacement; // bouger vers le haut
                else if (deplacerBas && Canvas.GetTop(player) < grid.ActualHeight - player.ActualHeight*2) nouvelleVitesseJoueur.Y = vitesseDeplacement; // bouger vers le bas 
                else
                {
                    nouvelleVitesseJoueur.Y *= friction;
                    if (Math.Abs(nouvelleVitesseJoueur.Y) < 0.1f) nouvelleVitesseJoueur.Y = 0;
                }

                // Corrigé la vitesse du joueur si il bouge en diagonale (car sqrt(2) = 1.4 et pas 1)
                if (directionJoueur == Directions.diagUpLeft || directionJoueur == Directions.diagUpRight ||
                    directionJoueur == Directions.diagDownLeft || directionJoueur == Directions.diagDownRight)
                {
                    nouvelleVitesseJoueur.X *= correctionVitesseDiagonal;
                    nouvelleVitesseJoueur.Y *= correctionVitesseDiagonal;
                }
            }

            nouvellePositionJoueur.X += nouvelleVitesseJoueur.X;
            nouvellePositionJoueur.Y += nouvelleVitesseJoueur.Y;

            vitesseJoueur = nouvelleVitesseJoueur;
            posJoueur = nouvellePositionJoueur;

            joueurTransformGroup.Children.Clear();
            player.RenderTransformOrigin = new Point(0.5, 0.5);
            joueurTransformGroup.Children.Add(joueurRoulade);
            joueurTransformGroup.Children.Add(joueurFlip);
            player.RenderTransform = joueurTransformGroup;

            Canvas.SetLeft(player, posJoueur.X);
            Canvas.SetTop(player, posJoueur.Y);
        }

        public void ChangeJoueurDirection()
        {
            if (estEnRoulade) return;
            // Corrige la direction du joueur
            if (deplacerBas && deplacerDroite)       directionJoueur = Directions.diagDownRight;
            else if (deplacerBas && deplacerGauche)  directionJoueur = Directions.diagDownLeft;
            else if (deplacerHaut && deplacerDroite) directionJoueur = Directions.diagUpRight;
            else if (deplacerHaut && deplacerGauche) directionJoueur = Directions.diagUpLeft;
            else if (deplacerDroite)                 directionJoueur = Directions.right;
            else if (deplacerGauche)                 directionJoueur = Directions.left;
            else if (deplacerBas)                    directionJoueur = Directions.down;
            else if (deplacerHaut)                   directionJoueur = Directions.up;

            // Inverse l'image du joueur si nécessaire
            doitFlip = (directionJoueur == Directions.left || directionJoueur == Directions.diagUpLeft || directionJoueur == Directions.diagDownLeft);
            joueurFlip.ScaleX = doitFlip ? 1 : -1;

            // Change l'image du joueur dépendament de sa direction
            if (blinkFrame % 2 == 0)
            {
                if (directionJoueur == Directions.left || directionJoueur == Directions.right)                                                       player.Source = side;
                if (directionJoueur == Directions.up   || directionJoueur == Directions.diagUpLeft   || directionJoueur == Directions.diagUpRight)   player.Source = back;
                if (directionJoueur == Directions.down || directionJoueur == Directions.diagDownLeft || directionJoueur == Directions.diagDownRight) player.Source = front;
            }
            else
            {
                if (directionJoueur == Directions.left || directionJoueur == Directions.right)                                                       player.Source = sideHit;
                if (directionJoueur == Directions.up   || directionJoueur == Directions.diagUpLeft   || directionJoueur == Directions.diagUpRight)   player.Source = backHit;
                if (directionJoueur == Directions.down || directionJoueur == Directions.diagDownLeft || directionJoueur == Directions.diagDownRight) player.Source = frontHit;
            }
        }

        public void hit(int degats)
        {
            nombreDeVie-=degats;
            blinkTimer = new DispatcherTimer();
            blinkTimer.Interval = TimeSpan.FromMilliseconds(50);
            blinkTimer.Tick += BlinkPlayerEffect;
            blinkTimer.Start();
        }

        private void BlinkPlayerEffect(object? sender, EventArgs e) {
            Console.WriteLine(blinkFrame.ToString());
            blinkFrame++;

            if (blinkFrame > 10)
            {
                blinkTimer.Stop();
                blinkFrame = 0;
            }
        }
    }
}
