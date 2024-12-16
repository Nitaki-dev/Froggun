using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;

namespace Froggun
{
    public partial class choixTouche : Window
    {
        // Configuration des touches
        public Key ToucheHaut { get; set; }
        public Key ToucheBas { get; set; }
        public Key ToucheGauche { get; set; }
        public Key ToucheDroite { get; set; }
        public Key ToucheRoulade { get; set; }
        public Key TouchePause { get; set; }

        private MediaPlayer musique;  // Lecteur pour la musique de fond
        private Button selectedButton;
        private MediaPlayer bruitClic; // Lecteur pour les bruits de clics
        private Dictionary<Key, string> usedKeys; // Dictionnaire pour suivre les touches utilisées

        public choixTouche()
        {
            InitializeComponent();

            usedKeys = new Dictionary<Key, string>();

            // Initialisation de la musique
            musique = new MediaPlayer();
            musique.Open(new Uri("music/Main intro.mp3", UriKind.Relative));
            musique.MediaEnded += (s, e) =>
            {
                musique.Position = TimeSpan.Zero;
                musique.Play();
            };
            musique.Play();

            // Initialisation du volume
            if (Properties.Settings.Default.Volume >= 0)
            {
                musique.Volume = Properties.Settings.Default.Volume;
                sliderSon.Value = musique.Volume * 10;
            }
            else
            {
                musique.Volume = 0.5;
                sliderSon.Value = 5;
            }

            // Configuration initiale des touches
            ToucheHaut = Key.Z;
            ToucheBas = Key.S;
            ToucheDroite = Key.Q;
            ToucheGauche = Key.D;

            ToucheRoulade = Key.LeftCtrl;
            TouchePause = Key.Space;
        }

        private void boutonQuitter_Click(object sender, RoutedEventArgs e)
        {
            // Sauvegarder la valeur du volume dans les paramètres avant de fermer la fenêtre
            Properties.Settings.Default.Volume = musique.Volume;  // Sauvegarde le volume actuel
            Properties.Settings.Default.Save();  // Enregistre les paramètres

            // Fermer la fenêtre
            this.DialogResult = false;
            this.Close();
        }

        private void MajVolume(double volume)
        {
            // Mettre à jour le volume de la musique
            musique.Volume = volume / 10;  // La valeur du volume est entre 0 et 1
            Console.WriteLine($"Volume musique mis à jour : {musique.Volume}");
        }

        private void SliderSon_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Mettre à jour le volume de la musique en temps réel en fonction de la valeur du slider
            MajVolume(sliderSon.Value);
        }

        private void BtnChoixHaut_Click(object sender, RoutedEventArgs e)
        {
            selectedButton = BtnChoixHaut;
            MessageBox.Show("Choisissez votre touche pour 'Haut'");
            this.KeyDown += Window_KeyDown;
        }

        private void BtnChoixBas_Click(object sender, RoutedEventArgs e)
        {
            selectedButton = BtnChoixBas;
            MessageBox.Show("Choisissez votre touche pour 'Bas'");
            this.KeyDown += Window_KeyDown;
        }

        private void BtnChoixHautDroite_Click(object sender, RoutedEventArgs e)
        {
            selectedButton = BtnChoixHautDroite;
            MessageBox.Show("Choisissez votre touche pour 'Haut Droite'");
            this.KeyDown += Window_KeyDown;
        }

        private void BtnChoixHautGauche_Click(object sender, RoutedEventArgs e)
        {
            selectedButton = BtnChoixHautGauche;
            MessageBox.Show("Choisissez votre touche pour 'Haut Gauche'");
            this.KeyDown += Window_KeyDown;
        }

        private void BtnRoulade_Click(object sender, RoutedEventArgs e)
        {
            selectedButton = BtnRoulade;
            MessageBox.Show("Choisissez votre touche pour 'Roulade'");
            this.KeyDown += Window_KeyDown;
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            selectedButton = BtnPause;
            MessageBox.Show("Choisissez votre touche pour 'Pause'");
            this.KeyDown += Window_KeyDown;
        }

        // Gestionnaire d'événements pour la pression d'une touche
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (selectedButton != null)
            {
                // Vérifier si la touche est déjà utilisée
                if (usedKeys.ContainsKey(e.Key))
                {
                    MessageBox.Show("Cette touche est déjà utilisée pour : " + usedKeys[e.Key] + ". Veuillez choisir une autre touche.");
                    return; // Si la touche est déjà utilisée, sortir de la fonction sans l'attribuer.
                }

                // Gérer les touches spéciales (Espace et Entrée)
                if (e.Key == Key.Space)
                {
                    e.Handled = true;  // Empêcher la propagation de la touche pour éviter les conflits
                    MessageBox.Show("Vous avez choisi la touche : Espace");
                }
                else if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    MessageBox.Show("Vous avez choisi la touche : Entrée");
                }
                else
                {
                    // Ajouter la touche au dictionnaire
                    string action = selectedButton.Content.ToString();
                    usedKeys.Add(e.Key, action);

                    // Mettre à jour l'objet Touche en fonction du bouton sélectionné
                    if (selectedButton == BtnChoixHaut)
                    {
                        ToucheHaut = e.Key;
                    }
                    else if (selectedButton == BtnChoixBas)
                    {
                        ToucheBas = e.Key;
                    }
                    else if (selectedButton == BtnChoixHautDroite)
                    {
                        ToucheDroite = e.Key;
                    }
                    else if (selectedButton == BtnChoixHautGauche)
                    {
                        ToucheGauche = e.Key;
                    }
                    else if (selectedButton == BtnRoulade)
                    {
                        ToucheRoulade = e.Key;
                    }
                    else if (selectedButton == BtnPause)
                    {
                        TouchePause = e.Key;
                    }

                    // Mettre à jour l'étiquette correspondante en fonction du bouton
                    if (selectedButton == BtnChoixHaut)
                    {
                        labelHaut.Content = "Haut : " + e.Key.ToString();
                    }
                    else if (selectedButton == BtnChoixBas)
                    {
                        labelbas.Content = "Bas : " + e.Key.ToString();
                    }
                    else if (selectedButton == BtnChoixHautDroite)
                    {
                        labeldroite.Content = "Droite : " + e.Key.ToString();
                    }
                    else if (selectedButton == BtnChoixHautGauche)
                    {
                        labelgauchr.Content = "Gauche : " + e.Key.ToString();
                    }
                    else if (selectedButton == BtnRoulade)
                    {
                        labelRoulade.Content = "Roulade : " + e.Key.ToString();
                    }
                    else if (selectedButton == BtnPause)
                    {
                        labelpause.Content = "Pause : " + e.Key.ToString();
                    }

                    // Détacher l'événement après avoir capturé la touche
                    this.KeyDown -= Window_KeyDown;
                }
            }
        }
    }
}
