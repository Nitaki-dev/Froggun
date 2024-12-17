﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using System.Numerics;
using System.Windows.Media;
using static Froggun.MainWindow;
using System.Windows.Shapes;
using System.Windows.Automation;
using System.Media;

namespace Froggun
{
    public enum TypeEnnemis
    {
        Firefly,
        Spider,
        Squit
    }

    internal class Ennemis
    {
        public TypeEnnemis type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double width { get; set; }
        public double height { get; set; }
        public double vitesse { get; set; }
        public double multiplicateurVitesse { get; set; }
        public bool estRalenti { get; set; }
        private string chemainImage { get; set; }
        public int[] indexAnimation { get; set; }
        public Rect hitbox { get; set; }
        public Image image { get; set; }
        private int indexAnimationActuelle { get; set; }
        private DispatcherTimer timerAnimation { get; set; }
        private int vie { get; set; }
        public double maxVie{ get; set; }
        public bool estVivant { get; private set; }
        public bool estEntreEnCollision { get; set; }
        public Rectangle BarDeVieVide { get; set; }
        public Rectangle BarDeVie { get; set; }
        private static int BarDeVieWidth = 100; // todo change that maybe

        public Ennemis(TypeEnnemis type, double x, double y, double width, double height, double vitesse, Canvas canvas, double multiplicateurDeVitesse = 1.0, Rect Hitbox = new Rect())
        {
            X = x;
            Y = y;
            this.width = width;
            this.height = height;
            this.vitesse = vitesse;

            BarDeVieVide = new Rectangle
            {
                Fill = Brushes.Gray,
                Width = BarDeVieWidth,
                Height = 10,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            Canvas.SetLeft(BarDeVieVide, X);
            Canvas.SetTop(BarDeVieVide, Y - 15);

            canvas.Children.Add(BarDeVieVide);

            BarDeVie = new Rectangle
            {
                Fill = Brushes.Green,
                Width = BarDeVieWidth,
                Height = 10,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            Canvas.SetLeft(BarDeVie, X);
            Canvas.SetTop(BarDeVie, Y - 15);
            canvas.Children.Add(BarDeVie);

            estRalenti = false;
            estVivant = true;
            switch (type)
            {
                case TypeEnnemis.Firefly:
                    chemainImage = "img/ennemis/Firefly";
                    indexAnimation = new int[] { 1, 2 };

                    if      (difficulte == "facile")    vie = 25;
                    else if (difficulte == "moyen")     vie = 50;
                    else if (difficulte == "difficile") vie = 100;
                    else if (difficulte == "extreme")   vie = 150;

                    maxVie = vie;
                    break;
                case TypeEnnemis.Spider:
                    chemainImage = "img/ennemis/LL";
                    indexAnimation = new int[] { 1, 2, 3, 1, 4, 5 };

                    if      (difficulte == "facile")    vie = 150;
                    else if (difficulte == "moyen")     vie = 200;
                    else if (difficulte == "difficile") vie = 250;
                    else if (difficulte == "extreme")   vie = 300;

                    maxVie = vie;
                    break;
                case TypeEnnemis.Squit:
                    chemainImage = "img/ennemis/Squit";
                    indexAnimation = new int[] { 1, 2, 3, 4 };

                    if      (difficulte == "facile")    vie = 300;
                    else if (difficulte == "moyen")     vie = 400;
                    else if (difficulte == "difficile") vie = 450;
                    else if (difficulte == "extreme")   vie = 550;

                    maxVie = vie;
                    break;
            }
          
            indexAnimationActuelle = 0;
            image = new Image { Width = width, Height = height };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);

            Canvas.SetLeft(image, X);
            Canvas.SetTop(image, Y);
            canvas.Children.Add(image);

            Hitbox = new Rect(X + 5, Y + 5, this.width-10, this.height - 10);

            timerAnimation = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timerAnimation.Tick += AnimationTimer_Tick;
            timerAnimation.Start();
        }
        
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            indexAnimationActuelle++;
            if (indexAnimationActuelle >= indexAnimation.Length) indexAnimationActuelle = 0; 
            
            int frame = indexAnimation[indexAnimationActuelle];
            BitmapImage newImageSource = ObtenirSourceImagePourFrame(frame);
            image.Source = newImageSource;
        }

        private BitmapImage ObtenirSourceImagePourFrame(int frame)
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri($"pack://application:,,/{chemainImage}/{frame}.png"));
            return bitmapImage;
        }

        public static void UpdateEnnemis(List<Ennemis> ennemis, List<Balle> balles, Canvas canvas, ref Joueur joueur)
        {
            // Cache bounding box and other frequently used values
            for (int i = 0; i < ennemis.Count; i++)
            {
                Ennemis ennemi = ennemis[i];
                if (!ennemi.estVivant) continue;

                ennemi.estEntreEnCollision = false;

                // Handle collision with balls
                for (int j = 0; j < balles.Count; j++)
                {
                    Balle balle = balles[j];
                    if (balle.aToucher) continue;

                    Rect hitboxBalle = new Rect(balle.X, balle.Y, 25, 25);

                    if (ennemi.hitbox.IntersectsWith(hitboxBalle) && !ennemi.estEntreEnCollision)
                    {
                        ennemi.vie -= 50;
                        if (ennemi.vie <= 0) ennemi.Meurt(ennemis, ennemi, canvas, ref joueur);
                        if (ennemi.vie > 0) ennemi.BarDeVie.Width = BarDeVieWidth * ((double)ennemi.vie / ennemi.maxVie);

                        balle.aToucher = true;
                        canvas.Children.Remove(balle.balleImage);
                        balles.RemoveAt(j);
                        j--; // Adjust the index after removal
                        break;
                    }
                }

                // Skip if enemy is dead
                if (ennemi.vie <= 0) continue;

                // Adjust movement speed based on status (isSlowed)
                if      (difficulte == "facile")    ennemi.multiplicateurVitesse = ennemi.estRalenti ? 0.5 : 0.25;
                else if (difficulte == "moyen")     ennemi.multiplicateurVitesse = ennemi.estRalenti ? 0.6 : 0.3;
                else if (difficulte == "difficile") ennemi.multiplicateurVitesse = ennemi.estRalenti ? 0.7 : 0.35;
                else if (difficulte == "extreme")   ennemi.multiplicateurVitesse = ennemi.estRalenti ? 0.8 : 0.4;
                
                ennemi.hitbox = new Rect(ennemi.X, ennemi.Y, ennemi.width, ennemi.height);  // Update the bounding box (optimized for reuse)
                
                // Calculate direction towards the player (only do this if necessary)
                Vector2 direction = new Vector2((float)(joueur.hitbox.X - ennemi.X), (float)(joueur.hitbox.Y - ennemi.Y));
                direction = Vector2.Normalize(direction);

                double nouveauX = ennemi.X + direction.X * ennemi.vitesse * ennemi.multiplicateurVitesse;
                double nouveauY = ennemi.Y + direction.Y * ennemi.vitesse * ennemi.multiplicateurVitesse;

                // Check if enemy can move (avoid unnecessary checks)
                bool peutBouger = true;
                for (int j = 0; j < ennemis.Count; j++)
                {
                    if (i == j) continue; // Skip self
                    if (ennemis[i].hitbox.IntersectsWith(ennemis[j].hitbox))
                    {
                        peutBouger = false;
                        break; // No need to check further
                    }
                }

                if (peutBouger)
                {
                    ennemi.X = nouveauX;
                    ennemi.Y = nouveauY;
                }

                // Check if the enemy collides with the player
                if (joueur.hitbox.IntersectsWith(ennemi.hitbox) && !ennemi.estRalenti)
                {
                    ennemi.Ralentir(3); // Slow down effect
                    joueur.hit(1);
                }

                // Efficiently update canvas positions
                Canvas.SetLeft(ennemi.image, ennemi.X);
                Canvas.SetTop(ennemi.image, ennemi.Y);
                Canvas.SetLeft(ennemi.BarDeVieVide, ennemi.X);
                Canvas.SetTop(ennemi.BarDeVieVide, ennemi.Y - 15);
                Canvas.SetLeft(ennemi.BarDeVie, ennemi.X);
                Canvas.SetTop(ennemi.BarDeVie, ennemi.Y - 15);
            }

            // Collision handling between enemies
            for (int i = 0; i < ennemis.Count - 1; i++)
            {
                Ennemis ennemiA = ennemis[i];
                for (int j = i + 1; j < ennemis.Count; j++)
                {
                    Ennemis ennemiB = ennemis[j];

                    if (ennemiA.hitbox.IntersectsWith(ennemiB.hitbox))
                    {
                        // Resolve collision by pushing enemies apart
                        Vector2 direction = new Vector2((float)(joueur.hitbox.X - ennemiA.X), (float)(joueur.hitbox.Y - ennemiA.Y));
                        direction = Vector2.Normalize(direction);

                        float RepoussementDeCollision = 3.0f;
                        ennemiA.X += direction.X * RepoussementDeCollision;
                        ennemiA.Y += direction.Y * RepoussementDeCollision;

                        ennemiB.X -= direction.X * RepoussementDeCollision;
                        ennemiB.Y -= direction.Y * RepoussementDeCollision;
                    }
                }
            }
        }
        
        private void SonMortEnnemie()
        {
            // Charger le fichier audio depuis les ressources
            Uri audioUri = new Uri("/son/mortEnnemie.wav", UriKind.RelativeOrAbsolute);
            Stream audioStream = Application.GetResourceStream(audioUri).Stream;
            // Créer un objet SoundPlayer pour lire le son
            SoundPlayer musique = new SoundPlayer(audioStream);
            musique.Play();
        }

        public void Meurt(List<Ennemis> ennemis, Ennemis e, Canvas canvas, ref Joueur joueur)
        {
            estVivant = false;
            image.Visibility = Visibility.Hidden;
            canvas.Children.Remove(image);
            canvas.Children.Remove(BarDeVieVide);
            canvas.Children.Remove(BarDeVie);
            ennemis.Remove(e);
            joueur.score += Math.Round(100 * joueur.scoreMultiplier);
            AjouterUnKillALaSerie(ref joueur, e);
        }

        public static void ReccomencerEnnemis(List<Ennemis> ennemis, Canvas canvas)
        {
            for (int i = ennemis.Count-1; i >=0; i--)
            {
                ennemis[i].estVivant = false;
                ennemis[i].image.Visibility = Visibility.Hidden;
                canvas.Children.Remove(ennemis[i].image);
                canvas.Children.Remove(ennemis[i].BarDeVieVide);
                canvas.Children.Remove(ennemis[i].BarDeVie);
                ennemis.RemoveAt(i);
                Console.WriteLine(ennemis.Count);
            }
        }

        public static void AjouterUnKillALaSerie(ref Joueur joueur, Ennemis e)
        {
            joueur.killStreak++;
            joueur.killStreakTimer = 5; // you got 5 seconds to get a new kill before it resets 
            joueur.scoreMultiplier += (e.type == TypeEnnemis.Firefly) ? 0.1 : ((e.type == TypeEnnemis.Spider) ? 0.5 : 1.0);
            if (joueur.scoreMultiplier<=1) joueur.scoreMultiplier += 1;
            joueur.scoreMultiplier = Math.Round(joueur.scoreMultiplier, 1);
        }

        public void Ralentir(int durationInSeconds)
        {
            estRalenti = true;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(durationInSeconds);
            timer.Tick += (s,e) => {
                estRalenti = false;
                timer.Stop();
            };

            timer.Start();
        }
    }
}
