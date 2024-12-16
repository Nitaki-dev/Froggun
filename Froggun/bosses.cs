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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Froggun
{
    internal class MantisBosses
    {
        public int MaxHP { get; set; }
        public int HP { get; set; }
        public int CurrentPhase { get; set; }
        public BitmapImage ImageBoss { get; set; }
        private Image Image { get; set; }
        private Rect Hitbox { get; set; }
        private Canvas canvas { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool isAlive { get; set; }
        public bool isAttacking { get; set; }
        public Joueur Joueur { get; set; }
        public Rectangle BossbarEmpty { get; set; }
        public Rectangle Bossbar { get; set; }
        public Dictionary<Guid, Rect> DamageAreas { get; set; }
        private Random Random { get; set; }
        private bool isMoving { get; set; }
        private bool isPoking { get; set; }
        private bool isRaining { get; set; }
        private bool isSwinging { get; set; }

        public MantisBosses(Canvas canvas, BitmapImage ImageBoss, Joueur joueur, int MaxHP, int width, int height)
        {
            this.canvas = canvas;
            this.ImageBoss = ImageBoss;
            this.Joueur = joueur;
            this.MaxHP = MaxHP;
            this.HP = MaxHP;
            this.Width = width;
            this.Height = height;
            this.isAlive = true;
            this.DamageAreas = new Dictionary<Guid, Rect>();
            this.Random = new Random();
            this.isMoving = false;

            this.isPoking = false;
            this.isRaining = false;
            this.isSwinging = false;

            this.Image = new Image { Width = width, Height = height, Source = ImageBoss };
            Canvas.SetLeft(this.Image, canvas.ActualWidth/2-this.Width/2);
            Canvas.SetTop(this.Image, 0); 

            RenderOptions.SetBitmapScalingMode(this.Image, BitmapScalingMode.NearestNeighbor);
            canvas.Children.Add(this.Image);

            this.Hitbox = new Rect { X = Canvas.GetLeft(this.Image), Y = Canvas.GetTop(this.Image), Width = width, Height = height };
            InitBossbar();
        }

        public void DamageMantis(int damage)
        {
            HP -= damage;
            
            // update the bossbar
            double newWidth = ((int)canvas.ActualWidth - 100 * 2) * ((double)HP / MaxHP);
            Bossbar.Width = newWidth;
        }
        
        public void DefeatMantis()
        {
            BitmapImage imgPokeAttack = new BitmapImage(new Uri("pack://application:,,,/img/boss/mantis/poke_attack.png"));
            BitmapImage sourceRain = new BitmapImage(new Uri("pack://application:,,,/img/boss/mantis/acide_droplet.png"));
            BitmapImage imgSwingAttack = new BitmapImage(new Uri("pack://application:,,,/img/boss/mantis/poke_attack.png"));

            // create list of all the boss attacks images 
            List<Image> toRemove = canvas.Children.OfType<Image>()
                .Where(img => img.Source is BitmapImage bitmap &&
                             (bitmap.UriSource == imgPokeAttack.UriSource ||
                              bitmap.UriSource == sourceRain.UriSource    ||
                              bitmap.UriSource == imgSwingAttack.UriSource))
                .ToList();

            // remove them, as well as the areas where the player will get damaged
            foreach (Image img in toRemove) canvas.Children.Remove(img);
            foreach (KeyValuePair<Guid, Rect> r in DamageAreas) DamageAreas.Remove(r.Key);
            
            canvas.Children.Remove(this.Image);
            canvas.Children.Remove(BossbarEmpty);
            canvas.Children.Remove(Bossbar);
            Joueur.score += 40000;
            this.isAlive = false;
        }

        public void UpdateMantisBoss(List<Balle> balles, Joueur joueur)
        {
            if (HP <= 0) DefeatMantis();
            Rect newHitbox = new Rect { X = Canvas.GetLeft(this.Image), Y = Canvas.GetTop(this.Image), Width = this.Width, Height = this.Height };
            this.Hitbox = newHitbox;

            // if the boss is not attacking, check if bullet hits it
            for (int j = 0; j < balles.Count; j++)
            {
                Balle balle = balles[j];
                if (balle.hasHit) continue;

                Rect rImgBalle = new Rect(balle.X, balle.Y, 25, 25);

                if (this.Hitbox.IntersectsWith(rImgBalle))
                {
                    // damage the boss
                    DamageMantis(joueur.degats);
                    balles.RemoveAt(j);
                    balle.hasHit = true;
                    canvas.Children.Remove(balle.BalleImage);

                    break;
                }
            }

            // 3 phases
            if (between(HP, MaxHP, (int)(MaxHP / 3) * 2)) //100/3 = 33.3*2 = 66.6
            {
                if (!this.isAttacking)
                {
                    // pick one of two attacks
                    int nextAttack = Random.Next(2); // 0 or 1
                    Console.WriteLine("Next attack: " + nextAttack);
                    switch (nextAttack)
                    {
                        case 0:
                            PokeAttack();
                            break;
                        case 1:
                            AcidrainAttack();
                            break;
                    }
                }
            }
            else
            if (between(HP, (int)(MaxHP / 3) * 2, (int)(MaxHP / 3)))
            {
                if (!this.isAttacking)
                {
                    // pick one of three attacks
                    int nextAttack = Random.Next(3);
                    Console.WriteLine("Next attack: " + nextAttack);
                    switch (nextAttack)
                    {
                        case 0:
                            PokeAttack();
                            break;
                        case 1:
                            AcidrainAttack();
                            break;
                        case 2:
                            SwingAttack();
                            break;
                    }
                }
            }
            else
            {
                // doesnt matter if an other attack is active, still try to attack again.
                int nextAttack = Random.Next(3);
                Console.WriteLine("Next attack: " + nextAttack);
                switch (nextAttack)
                {
                    case 0:
                        if (!isPoking) PokeAttack();
                        break;
                    case 1:
                        if (!isRaining) AcidrainAttack();
                        break;
                    case 2:
                        if (!isSwinging) SwingAttack();
                        break;
                }
            }

            foreach (KeyValuePair<Guid, Rect> r in DamageAreas) {
                //Rectangle re = DebugRect(r.Value); // debug areas where player will get damadged
                //canvas.Children.Add(re);
                if (Joueur.hitbox.IntersectsWith(r.Value))
                {
                    if (!Joueur.estInvinsible) Joueur.hit(1);
                }
            }
        }

        private bool between(int a, int b, int c)
        {
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
            int marginX = 100;
            int marginY = 50;
            int width = (int)canvas.ActualWidth - marginX * 2;
            int height = 30;

            BossbarEmpty = new Rectangle { Width = width, Height = height, Fill = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            Canvas.SetTop(BossbarEmpty, (int)canvas.ActualHeight - marginY - height);
            Canvas.SetLeft(BossbarEmpty, marginX);

            Bossbar = new Rectangle { Width = width, Height = height, Fill = Brushes.DarkRed, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            Canvas.SetTop(Bossbar, (int)canvas.ActualHeight - marginY - height);
            Canvas.SetLeft(Bossbar, marginX);

            canvas.Children.Add(BossbarEmpty);
            canvas.Children.Add(Bossbar);
        }

        private async void PokeAttack()
        {
            isPoking = true;
            for (int i = 0; i < 5; i++) // change based on difficulty
            {
                if (!isAlive) return;
                Poke();
                await Task.Delay(900);
            }
            await Task.Delay(1000);
            isAttacking = false;
            isPoking = false;
        }

        private async void Poke()
        {
            this.isAttacking = true;
            BitmapImage imgPokeAttack = new BitmapImage(new Uri("pack://application:,,,/img/boss/mantis/poke_attack.png"));
            Image imgPoke = new Image { Source = imgPokeAttack, Height = 200, Width = 38, Stretch = Stretch.Fill };
            Canvas.SetLeft(imgPoke, Joueur.posJoueur.X);
            Canvas.SetTop(imgPoke, 0); // starting position
            RenderOptions.SetBitmapScalingMode(imgPoke, BitmapScalingMode.NearestNeighbor);
            canvas.Children.Add(imgPoke);

            Rect dmg = new Rect
            {
                X = Canvas.GetLeft(imgPoke),
                Y = Canvas.GetTop(imgPoke),
                Width = imgPoke.Width,
                Height =  imgPoke.Height
            };

            Guid id = AddDmgArea(dmg);

            double targetY = canvas.ActualHeight;
            double currentY = 0;
            int steps = 30;
            double stepDelay = 300.0 / steps; // time per step in ms
            double stepSize = targetY / steps;

            // animate image and hitbox to move from top to bottom of the screen
            for (int i = 0; i < steps; i++)
            {
                if (!isAlive) return;
                currentY += stepSize;
                Canvas.SetTop(imgPoke, currentY);
                DamageAreas[id] = new Rect { X=dmg.X, Y=currentY, Width=dmg.Width, Height=dmg.Height };

                await Task.Delay((int)stepDelay);
            }

            canvas.Children.Remove(imgPoke);
            DamageAreas.Remove(id);
        }

        private async void AcidrainAttack()
        {
            isRaining = true;
            this.isAttacking = true;
            BitmapImage sourceRain = new BitmapImage(new Uri("pack://application:,,,/img/boss/mantis/acide_droplet.png"));

            // pick random side (left middle right)
            Random r = new Random();
            int side = r.Next(0, 3);

            // define max bounds where the droplets can spawn
            int min = (int)(canvas.ActualWidth / 3) * side;
            int max = (int)((canvas.ActualWidth / 3) * side + (canvas.ActualWidth / 3));

            // move the mantis to the zone picked
            for (int i = 0; i < 100; i++)
            {
                if (!isAlive) return;
                double f = easeInOutQuint((double)i / 100, Canvas.GetLeft(Image), (min + max) / 2 - Image.Width / 2);
                Canvas.SetLeft(Image, f);

                await Task.Delay(10);
            }

            int spawnDuration = 3000; // spawn raindrops for 3000ms aka 3s
            int spawnInterval = 20;   // wait 20ms between each new raindrops

            Rect dmg = new Rect
            {
                X = min,
                Y = 0,
                Width = max-min,
                Height = canvas.ActualHeight
            };

            Guid id = AddDmgArea(dmg);

            for (int i = 0; i < spawnDuration / spawnInterval; i++)
            {
                if (!isAlive) return;
                Raindrop(sourceRain, r.Next(min, max), 180);
                await Task.Delay(spawnInterval);
            }

            DamageAreas.Remove(id);
            await Task.Delay(1000);
            isAttacking = false;
            isRaining = false;
        }

        private async void Raindrop(BitmapImage source, int x, int y)
        {
            this.isAttacking = true;
            Image raindrop = new Image { Source = source, Height = 16, Width = 16, Stretch = Stretch.Fill };
            RenderOptions.SetBitmapScalingMode(raindrop, BitmapScalingMode.NearestNeighbor);
            Canvas.SetLeft(raindrop, x);
            Canvas.SetTop(raindrop, y);
            canvas.Children.Add(raindrop);

            double targetY = canvas.ActualHeight - raindrop.Height;
            double currentY = y;
            
            int steps = 30;
            double stepDelay = 100.0 / steps;
            double stepSize = targetY / steps;

            for (int i = 0; i < steps; i++)
            {
                if (!isAlive) return;
                currentY += stepSize;
                Canvas.SetTop(raindrop, currentY);
                await Task.Delay((int)stepDelay);
            }

            canvas.Children.Remove(raindrop);
        }
    
        private async void SwingAttack()
        {
            isSwinging = true;
            this.isAttacking = true;

            BitmapImage imgSwingAttack = new BitmapImage(new Uri("pack://application:,,,/img/boss/mantis/poke_attack.png"));
            Image imgSwing = new Image { Source = imgSwingAttack, Height = canvas.ActualHeight-150, Width = 74, Stretch = Stretch.Fill };
            Canvas.SetLeft(imgSwing, -imgSwing.Width);
            Canvas.SetTop(imgSwing, 150);
            RenderOptions.SetBitmapScalingMode(imgSwing, BitmapScalingMode.NearestNeighbor);
            canvas.Children.Add(imgSwing);

            double targetX = canvas.Width+imgSwing.Width;
            double currentX = -imgSwing.Width;
            int steps = 50;
            double stepDelay = 500.0 / steps;
            double stepSize = canvas.ActualWidth / steps;

            // define damage zone for the swing
            Rect dmg = new Rect
            {
                X = currentX,
                Y = 150,
                Width = imgSwing.Width,
                Height = canvas.ActualHeight
            };

            Guid id = AddDmgArea(dmg);

            // animate the swing
            for (int i = 0; i < steps; i++)
            {
                if (!isAlive) return;
                currentX += stepSize;
                Canvas.SetLeft(imgSwing, currentX);

                DamageAreas[id] = new Rect { X = currentX, Y = dmg.Y, Width = dmg.Width, Height = dmg.Height };

                await Task.Delay((int) stepDelay);
            }

            await Task.Delay(500);
            
            DamageAreas.Remove(id);
            canvas.Children.Remove(imgSwing);

            this.isAttacking = false;
            isSwinging = false;
        }

        public Guid AddDmgArea(Rect area)
        {
            Guid uniqueId = Guid.NewGuid();
            DamageAreas[uniqueId] = area;
            return uniqueId;
        }
        
        private Rectangle DebugRect(Rect rect)
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
