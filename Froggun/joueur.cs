//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static Froggun.MainWindow;
//using static System.Runtime.InteropServices.JavaScript.JSType;
//using System.Windows.Controls;
//using System.Numerics;
//using System.Windows.Media;

//namespace Froggun
//{
//    internal class Joueur
//    {

//        public static TransformGroup joueurTransformGroup = new TransformGroup();
//        public static RotateTransform joueurRoulade = new RotateTransform();
//        public static ScaleTransform joueurFlip = new ScaleTransform();
//        public static Vector2 posJoueur = new Vector2(50.0f, 50.0f);
//        public static Vector2 vitesseJoueur = new Vector2();

//        public int nombreDeVie { get; set; }
//        public bool estInvinsible { get; set; }
//        public bool deplacerGauche { get; set; }
//        public bool deplacerDroite { get; set; }
//        public bool deplacerHaut { get; set; }
//        public bool deplacerBas { get; set; }
//        public bool doitFlip { get; set; }
//        public bool estEnRoulade { get; set; }
//        public double tempsRoulade { get; set; }
//        public double dureeRoulade { get; set; }
//        public float correctionVitesseDiagonal { get; set; }
//        public float vitesseDeplacement { get; set; }
//        public float friction { get; set; }
//        public Image player { get; set; }
//        public Grid grid { get; set; }


//        public enum Directions
//        {
//            left,
//            right,
//            up,
//            down,
//            diagUpLeft,
//            diagUpRight,
//            diagDownLeft,
//            diagDownRight
//        }

//        public Directions directionJoueur { get; set; }

//        public Joueur(Image player, Grid grid)
//        {
//            this.player = player;
//            this.grid = grid;

//            this.nombreDeVie = 5;
//            this.estInvinsible = false;
            
//            this.deplacerDroite = false;
//            this.deplacerHaut = false;
//            this.deplacerBas = false;
//            this.deplacerHaut = false;
//            this.doitFlip = false;
//            this.estEnRoulade = false;
//            this.tempsRoulade = 0;
//            this.dureeRoulade = 300;

//            this.correctionVitesseDiagonal = (float)(1.0f / Math.Sqrt(2.0f));
//            this.vitesseDeplacement = 8.0f;
//            this.friction = 0.4f;
//            this.directionJoueur = Directions.right;
//    }
        
//        private void UpdatePositionJoueur()
//        {
//            if (estEnRoulade)
//            {
//                // animation de la roulade 
//                tempsRoulade += 16.6666667;
//                joueurRoulade.Angle = ((tempsRoulade / dureeRoulade) * 2 * Math.PI) * 180 / Math.PI * (doitFlip ? 1 : -1);
//                vitesseJoueur.X = 0;
//                vitesseJoueur.Y = 0;

//                switch (directionJoueur)
//                {
//                    case Directions.down:
//                        vitesseJoueur.Y = 15.0f;
//                        break;
//                    case Directions.up:
//                        vitesseJoueur.Y = -15.0f;
//                        break;
//                    case Directions.right:
//                        vitesseJoueur.X = 15.0f;
//                        break;
//                    case Directions.left:
//                        vitesseJoueur.X = -15.0f;
//                        break;
//                    case Directions.diagDownLeft:
//                        vitesseJoueur.X = -15.0f * correctionVitesseDiagonal;
//                        vitesseJoueur.Y = 15.0f * correctionVitesseDiagonal;
//                        break;
//                    case Directions.diagDownRight:
//                        vitesseJoueur.X = 15.0f * correctionVitesseDiagonal;
//                        vitesseJoueur.Y = 15.0f * correctionVitesseDiagonal;
//                        break;
//                    case Directions.diagUpLeft:
//                        vitesseJoueur.X = -15.0f * correctionVitesseDiagonal;
//                        vitesseJoueur.Y = -15.0f * correctionVitesseDiagonal;
//                        break;
//                    case Directions.diagUpRight:
//                        vitesseJoueur.X = 15.0f * correctionVitesseDiagonal;
//                        vitesseJoueur.Y = -15.0f * correctionVitesseDiagonal;
//                        break;
//                }

//                if (tempsRoulade > dureeRoulade)
//                {
//                    estEnRoulade = false;
//                    tempsRoulade = 0;
//                }
//            }
//            else
//            {
//                joueurRoulade.Angle = 0;

//                if (deplacerGauche && Canvas.GetLeft(player) > 0) vitesseJoueur.X = -vitesseDeplacement; // bouger vers la gauche
//                else if (deplacerDroite && Canvas.GetLeft(player) < grid.ActualWidth - player.ActualWidth) vitesseJoueur.X = vitesseDeplacement; // bouger vers la droite
//                else
//                {
//                    vitesseJoueur.X *= friction; // réduire la vitesse du joueur en fonction de la friction
//                    if (Math.Abs(vitesseJoueur.X) < 0.1f) vitesseJoueur.X = 0; // si la vitesse (positive) est inférieure à 0.1, arrêter le mouvement
//                }

//                if (deplacerHaut && Canvas.GetTop(player) > 0) vitesseJoueur.Y = -vitesseDeplacement; // bouger vers le haut
//                else if (deplacerBas && Canvas.GetTop(player) < grid.ActualHeight - player.ActualHeight) vitesseJoueur.Y = vitesseDeplacement; // bouger vers le bas 
//                else
//                {
//                    vitesseJoueur.Y *= friction;
//                    if (Math.Abs(vitesseJoueur.Y) < 0.1f) vitesseJoueur.Y = 0;
//                }

//                // Corrigé la vitesse du joueur si il bouge en diagonale (car sqrt(2) = 1.4 et pas 1)
//                if (directionJoueur == Directions.diagUpLeft || directionJoueur == Directions.diagUpRight ||
//                    directionJoueur == Directions.diagDownLeft || directionJoueur == Directions.diagDownRight)
//                {
//                    vitesseJoueur.X *= correctionVitesseDiagonal;
//                    vitesseJoueur.Y *= correctionVitesseDiagonal;
//                }
//            }

//            posJoueur.X += vitesseJoueur.X;
//            posJoueur.Y += vitesseJoueur.Y;

//            joueurTransformGroup.Children.Clear();
//            player.RenderTransformOrigin = new Point(0.5, 0.5);
//            joueurTransformGroup.Children.Add(joueurRoulade);
//            joueurTransformGroup.Children.Add(joueurFlip);
//            player.RenderTransform = joueurTransformGroup;

//            Canvas.SetLeft(player, posJoueur.X);
//            Canvas.SetTop(player, posJoueur.Y);
//        }

//        private void ChangeJoueurDirection()
//        {
//            if (estEnRoulade) return;
//            // Corrige la direction du joueur
//            if (deplacerBas && deplacerDroite) directionJoueur = Directions.diagDownRight;
//            else if (deplacerBas && deplacerGauche) directionJoueur = Directions.diagDownLeft;
//            else if (deplacerHaut && deplacerDroite) directionJoueur = Directions.diagUpRight;
//            else if (deplacerHaut && deplacerGauche) directionJoueur = Directions.diagUpLeft;
//            else if (deplacerDroite) directionJoueur = Directions.right;
//            else if (deplacerGauche) directionJoueur = Directions.left;
//            else if (deplacerBas) directionJoueur = Directions.down;
//            else if (deplacerHaut) directionJoueur = Directions.up;

//            // Inverse l'image du joueur si nécessaire
//            doitFlip = (directionJoueur == Directions.left || directionJoueur == Directions.diagUpLeft || directionJoueur == Directions.diagDownLeft);
//            joueurFlip.ScaleX = doitFlip ? 1 : -1;

//            // Change l'image du joueur dépendament de sa direction
//            if (directionJoueur == Directions.left || directionJoueur == Directions.right)                                                       player.Source = imgFrogSide;
//            if (directionJoueur == Directions.up   || directionJoueur == Directions.diagUpLeft   || directionJoueur == Directions.diagUpRight)   player.Source = imgFrogBack;
//            if (directionJoueur == Directions.down || directionJoueur == Directions.diagDownLeft || directionJoueur == Directions.diagDownRight) player.Source = imgFrogFront;
//        }

//    }
//}
