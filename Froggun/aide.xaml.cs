using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Froggun
{
    /// <summary>
    /// Logique d'interaction pour aide.xaml
    /// </summary>
    public partial class aide : Window
    {
        public aide()
        {
            InitializeComponent();
            labelGauche.Content = "     - "+Touche.ToucheGauche + " pour déplacer la grenouille vers la gauche.";
            labelDroite.Content = "     - " + Touche.ToucheDroite + " pour déplacer la grenouille vers la droite.";
            labelBas.Content = "     - " + Touche.ToucheBas + " pour déplacer la grenouille vers le bas.";
            labelHaut.Content = "     - " + Touche.ToucheHaut + " pour déplacer la grenouille vers le haut.";
            labelRoulade.Content = "     - " + Touche.ToucheRoulade + " pour rouler / esquiver et aller plus vite.";
            labelPause.Content = "     - " + Touche.TouchePause + " pour mettre le jeu en pause.";
        }

        

        private void boutonQuitter_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
