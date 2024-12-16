using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Froggun
{
    /// <summary>
    /// Logique d'interaction pour choixTouche.xaml
    /// </summary>
    public partial class choixTouche : Window
    {
        private MediaPlayer musique;  // Lecteur pour la musique de fond
        private MediaPlayer bruitClic; // Lecteur pour les bruits de clics

        public choixTouche()
        {
            InitializeComponent();

            // Initialisation du lecteur musique
            musique = new MediaPlayer();
            musique.Open(new Uri("music/Main intro.mp3", UriKind.Relative));

            // Lecture en boucle
            musique.MediaEnded += (s, e) =>
            {
                musique.Position = TimeSpan.Zero;
                musique.Play();
            };

            // Lecture immédiate de la musique
            musique.Play();

            // Restaurer le volume à partir des paramètres d'application si disponible
            if (Properties.Settings.Default.Volume >= 0)  // Vérifie si le volume est défini dans les paramètres
            {
                musique.Volume = Properties.Settings.Default.Volume; // Récupère le volume enregistré
                sliderSon.Value = musique.Volume * 10;  // Mettre le slider en accord avec le volume actuel
            }
            else
            {
                // Si aucun volume n'est enregistré, définir un volume par défaut
                musique.Volume = 0.5;  // Valeur par défaut (50%)
                sliderSon.Value = 5;  // Le slider représente une valeur entre 0 et 10
            }
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
    }
}
 