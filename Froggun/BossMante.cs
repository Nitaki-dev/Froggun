using Accessibility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Froggun
{
    internal class BossMante
    {
        public int MaxPV { get; set; }
        public int PV { get; set; }
        public int PhaseActuelle { get; set; }
        public BitmapImage ImageBoss { get; set; }
        private Image Image { get; set; }
        private Rect Hitbox { get; set; }
        private Canvas canvas { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool estVivant { get; set; }
        public bool estEnAttack { get; set; }
        public Joueur Joueur { get; set; }
        public Rectangle BossbarVide { get; set; }
        public Rectangle Bossbar { get; set; }
        public Dictionary<Guid, Rect> zoneDegats { get; set; }
        private Random Random { get; set; }
        private bool estEnTrainDePiquer { get; set; }
        private bool estEnTrainDePleuvoir { get; set; }
        private bool estEnTrainDeBalayer { get; set; }

        public BossMante(Canvas canvas, BitmapImage ImageBoss, Joueur joueur, int MaxHP, int width, int height)
        {
            this.canvas = canvas;
            this.ImageBoss = ImageBoss;
            this.Joueur = joueur;
            this.MaxPV = MaxHP;
            this.PV = MaxHP;
            this.Width = width;
            this.Height = height;
            this.estVivant = true;
            this.zoneDegats = new Dictionary<Guid, Rect>();
            this.Random = new Random();

            this.estEnTrainDePiquer = false;
            this.estEnTrainDePleuvoir = false;
            this.estEnTrainDeBalayer = false;

            this.Image = new Image { Width = width, Height = height, Source = ImageBoss };
            Canvas.SetLeft(this.Image, canvas.ActualWidth/2-this.Width/2);
            Canvas.SetTop(this.Image, 0); 

            RenderOptions.SetBitmapScalingMode(this.Image, BitmapScalingMode.NearestNeighbor);
            canvas.Children.Add(this.Image);

            this.Hitbox = new Rect { X = Canvas.GetLeft(this.Image), Y = Canvas.GetTop(this.Image), Width = width, Height = height };
            InitBossbar();
        }

        public void DegatsMante(int degats) // fair des degat au boss
        {
            PV -= degats;
            
            // update la bar de vie du boss
            double nWidth = ((int)canvas.ActualWidth - 100 * 2) * ((double)PV / MaxPV);
            Bossbar.Width = nWidth;
        }
        
        public void VaincreMante()
        {
            BitmapImage sourcePique = new BitmapImage(new Uri("pack://application:,,,/img/boss/mantis/poke_attack.png"));
            BitmapImage sourceGoutte = new BitmapImage(new Uri("pack://application:,,,/img/boss/mantis/acide_droplet.png"));
            BitmapImage sourceBalayage = new BitmapImage(new Uri("pack://application:,,,/img/boss/mantis/poke_attack.png"));

            // liste de toutes les image d'attaque du boss
            List<Image> aSupprimer = canvas.Children.OfType<Image>()
                .Where(img => img.Source is BitmapImage bitmap &&
                             (bitmap.UriSource == sourcePique.UriSource ||
                              bitmap.UriSource == sourceGoutte.UriSource    ||
                              bitmap.UriSource == sourceBalayage.UriSource))
                .ToList();

            // supprimer les images & les zones de dégats créer par le boss
            foreach (Image img in aSupprimer) canvas.Children.Remove(img);
            foreach (KeyValuePair<Guid, Rect> r in zoneDegats) zoneDegats.Remove(r.Key);

            canvas.Children.Remove(this.Image);
            canvas.Children.Remove(BossbarVide);
            canvas.Children.Remove(Bossbar);
            Joueur.score += 40000;
            this.estVivant = false;
        }

        public async void UpdateBossMante(List<Balle> balles, Joueur joueur)
        {
            if (PV <= 0) VaincreMante();
            Rect nouvelleHitbox = new Rect { X = Canvas.GetLeft(this.Image), Y = Canvas.GetTop(this.Image), Width = this.Width, Height = this.Height };
            this.Hitbox = nouvelleHitbox;

            // verif si les balles touche le boss
            for (int j = 0; j < balles.Count; j++)
            {
                Balle balle = balles[j];
                if (balle.aToucher) continue;

                Rect rImgBalle = new Rect(balle.X, balle.Y, 25, 25);

                if (this.Hitbox.IntersectsWith(rImgBalle))
                {
                    // fair des degats au boss si oui
                    DegatsMante(joueur.degats);
                    balles.RemoveAt(j);
                    balle.aToucher = true;
                    canvas.Children.Remove(balle.balleImage);

                    break;
                }
            }

            // 3 phases
            if (entre(PV, MaxPV, (int)(MaxPV / 3) * 2)) //100/3 = 33.3*2 = 66.6
            {
                if (!this.estEnAttack)
                {
                    // choisie une attaque parmis 2
                    int prochaineAttack = Random.Next(2); // 0 or 1
                    Console.WriteLine("Next attack: " + prochaineAttack);
                    switch (prochaineAttack)
                    {
                        case 0:
                            AttaquePique();
                            break;
                        case 1:
                            AttaquePluieAcide();
                            break;
                    }
                }
            }
            else
            if (entre(PV, (int)(MaxPV / 3) * 2, (int)(MaxPV / 3)))
            {
                if (!this.estEnAttack)
                {
                    // choisie une attaque parmis 3
                    int prochaineAttack = Random.Next(3);
                    Console.WriteLine("Next attack: " + prochaineAttack);
                    switch (prochaineAttack)
                    {
                        case 0:
                            AttaquePique();
                            break;
                        case 1:
                            AttaquePluieAcide();
                            break;
                        case 2:
                            AttaqueBalayage();
                            break;
                    }
                }
            }
            else
            {
                // peu importe si une autre attaque est active, essayez quand même d'attaquer à nouveau
                int prochaineAttack = Random.Next(3);
                Console.WriteLine("Next attack: " + prochaineAttack);
                switch (prochaineAttack)
                {
                    case 0:
                        if (!estEnTrainDePiquer) AttaquePique();
                        break;
                    case 1:
                        if (!estEnTrainDePleuvoir) AttaquePluieAcide();
                        break;
                    case 2:
                        if (!estEnTrainDeBalayer) AttaqueBalayage();
                        break;
                }
                await Task.Delay(300); // petite pause entre chaque attaques sionon c'est trop dure
            }

            foreach (KeyValuePair<Guid, Rect> r in zoneDegats) {
                //Rectangle re = DebugRect(r.Value); // debug des zones de dégats
                //canvas.Children.Add(re);
                if (Joueur.hitbox.IntersectsWith(r.Value))
                {
                    if (!Joueur.estInvinsible) Joueur.hit(1);
                }
            }
        }

        private bool entre(int a, int b, int c)
        {
            // est ce que A est entre B et C
            return (a <= b && a >= c);
        }

        // https://easings.net/#easeInOutQuint
        private double easeInOutQuint(double x, double start, double end)
        {
            double easedX = x < 0.5
                ? 16 * x * x * x * x * x
                : 1 - Math.Pow(-2 * x + 2, 5) / 2;
            return start + (end - start) * easedX;
        }

        private void InitBossbar()
        {
            //initialisation des rectangle pour la var de vie du boss
            int marginX = 100;
            int marginY = 50;
            int width = (int)canvas.ActualWidth - marginX * 2;
            int height = 30;

            BossbarVide = new Rectangle { Width = width, Height = height, Fill = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            Canvas.SetTop(BossbarVide, (int)canvas.ActualHeight - marginY - height);
            Canvas.SetLeft(BossbarVide, marginX);

            Bossbar = new Rectangle { Width = width, Height = height, Fill = Brushes.DarkRed, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            Canvas.SetTop(Bossbar, (int)canvas.ActualHeight - marginY - height);
            Canvas.SetLeft(Bossbar, marginX);

            canvas.Children.Add(BossbarVide);
            canvas.Children.Add(Bossbar);
        }

        private async void AttaquePique()
        {
            estEnTrainDePiquer = true;
            // faire spawn 5 attaques qui vise le joueur
            for (int i = 0; i < 5; i++)
            {
                if (!estVivant) return;
                Pique();
                await Task.Delay(900);
            }
            await Task.Delay(1000);
            estEnAttack = false;
            estEnTrainDePiquer = false;
        }

        private async void Pique()
        {
            this.estEnAttack = true;
            
            //creation de l'image
            BitmapImage sourceImage = new BitmapImage(new Uri("pack://application:,,,/img/boss/mantis/poke_attack.png"));
            Image image = new Image { Source = sourceImage, Height = 200, Width = 38, Stretch = Stretch.Fill };
            Canvas.SetLeft(image, Joueur.posJoueur.X);
            Canvas.SetTop(image, 0); // starting position
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            canvas.Children.Add(image);

            // creation de la zone de dégat
            Rect zoneDegat = new Rect
            {
                X = Canvas.GetLeft(image),
                Y = Canvas.GetTop(image),
                Width = image.Width,
                Height =  image.Height
            };

            Guid id = AjouterZoneDegat(zoneDegat);

            double yObjectif = canvas.ActualHeight;
            double yActuelle = 0;
            int etape = 30;
            double delayEntreEtapes = 300.0 / etape; // temp par etapes en ms
            double tailleEtapes = yObjectif / etape;

            // animé la hitbox et l'image pour que elle bouge vers le bas
            for (int i = 0; i < etape; i++)
            {
                if (!estVivant) return;
                yActuelle += tailleEtapes;
                Canvas.SetTop(image, yActuelle);
                zoneDegats[id] = new Rect { X=zoneDegat.X, Y=yActuelle, Width=zoneDegat.Width, Height=zoneDegat.Height };

                await Task.Delay((int)delayEntreEtapes);
            }

            canvas.Children.Remove(image);
            zoneDegats.Remove(id);
        }

        private async void AttaquePluieAcide()
        {
            estEnTrainDePleuvoir = true;
            this.estEnAttack = true;
            BitmapImage sourceGoutteAcide = new BitmapImage(new Uri("pack://application:,,,/img/boss/mantis/acide_droplet.png"));

            // pick random side (left middle right)
            Random r = new Random();
            int cote = r.Next(0, 3);

            // define max bounds where the droplets can spawn
            int min = (int)(canvas.ActualWidth / 3) * cote;
            int max = (int)((canvas.ActualWidth / 3) * cote + (canvas.ActualWidth / 3));

            // bouger le boss a la position par rapport au coté choisi
            for (int i = 0; i < 100; i++)
            {
                if (!estVivant) return;
                double f = easeInOutQuint((double)i / 100, Canvas.GetLeft(Image), (min + max) / 2 - Image.Width / 2);
                Canvas.SetLeft(Image, f);

                await Task.Delay(10);
            }

            int dureeDeSpawn = 1500;
            int intervalDeSpawn = 20;

            // ajout de la zone de dégat
            Rect zoneDegat = new Rect
            {
                X = min,
                Y = 0,
                Width = max-min,
                Height = canvas.ActualHeight
            };

            Guid id = AjouterZoneDegat(zoneDegat);

            // ajouter les goutes de pluis 
            for (int i = 0; i < dureeDeSpawn / intervalDeSpawn; i++)
            {
                if (!estVivant) return;
                AjouterGoutte(sourceGoutteAcide, r.Next(min, max), 180);
                await Task.Delay(intervalDeSpawn);
            }

            // supprimer zones de degats quand c'est fini
            zoneDegats.Remove(id);
            await Task.Delay(300);
            estEnAttack = false;
            estEnTrainDePleuvoir = false;
        }

        private async void AjouterGoutte(BitmapImage source, int x, int y)
        {
            this.estEnAttack = true;
            
            // creation de l'image
            Image goutteAcide = new Image { Source = source, Height = 16, Width = 16, Stretch = Stretch.Fill };
            RenderOptions.SetBitmapScalingMode(goutteAcide, BitmapScalingMode.NearestNeighbor);
            Canvas.SetLeft(goutteAcide, x);
            Canvas.SetTop(goutteAcide, y);
            canvas.Children.Add(goutteAcide);

            // même logique que l'attaque pique
            double yObjectif = canvas.ActualHeight - goutteAcide.Height;
            double yActuelle = y;
            int etapes = 30;
            double delayEntreEtapes = 100.0 / etapes;
            double tailleEtapes = yObjectif / etapes;

            for (int i = 0; i < etapes; i++)
            {
                if (!estVivant) return;
                yActuelle += tailleEtapes;
                Canvas.SetTop(goutteAcide, yActuelle);
                await Task.Delay((int)delayEntreEtapes);
            }

            canvas.Children.Remove(goutteAcide);
        }
    
        private async void AttaqueBalayage()
        {
            estEnTrainDeBalayer = true;
            this.estEnAttack = true;

            // creation de l'image
            BitmapImage sourceImage = new BitmapImage(new Uri("pack://application:,,,/img/boss/mantis/poke_attack.png"));
            Image image = new Image { Source = sourceImage, Height = canvas.ActualHeight-150, Width = 74, Stretch = Stretch.Fill };
            Canvas.SetLeft(image, -image.Width);
            Canvas.SetTop(image, 150);
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            canvas.Children.Add(image);

            // même logique que l'attaque pique mais horizontal
            double xObjectif = canvas.Width+image.Width;
            double xActuelle = -image.Width;
            int etapes = 50;
            double delayEntreEtapes = 500.0 / etapes;
            double tailleEtapes = canvas.ActualWidth / etapes;

            // creation de la zone de dégat
            Rect zoneDegat = new Rect
            {
                X = xActuelle,
                Y = 150,
                Width = image.Width,
                Height = canvas.ActualHeight
            };

            Guid id = AjouterZoneDegat(zoneDegat);

            // animation de l'image
            for (int i = 0; i < etapes; i++)
            {
                if (!estVivant) return;
                xActuelle += tailleEtapes;
                Canvas.SetLeft(image, xActuelle);

                zoneDegats[id] = new Rect { X = xActuelle, Y = zoneDegat.Y, Width = zoneDegat.Width, Height = zoneDegat.Height };

                await Task.Delay((int) delayEntreEtapes);
            }

            await Task.Delay(500);
            
            zoneDegats.Remove(id);
            canvas.Children.Remove(image);

            this.estEnAttack = false;
            estEnTrainDeBalayer = false;
        }

        public Guid AjouterZoneDegat(Rect area)
        {
            // utilisation de GUID affin de garder une trace de chaques zones de dégats même lorsque on en ajouter et supprimes des zones entre temps
            Guid idUnique = Guid.NewGuid();
            zoneDegats[idUnique] = area;
            return idUnique;
        }
        
        private Rectangle DebugRect(Rect rect) // fonction de debug
        {
            return new Rectangle
            {
                Width = rect.Width,
                Height = rect.Height,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                RenderTransform = new TranslateTransform(rect.X, rect.Y)
            };
        }
    }
}
