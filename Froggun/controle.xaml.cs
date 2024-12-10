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
    /// Logique d'interaction pour controle.xaml
    /// </summary>
    public partial class controle : Window
    {
        public string Resultat { get; private set; }
        public controle()
        {
            InitializeComponent();
        }


        private void BoutonFond_Click(object sender, RoutedEventArgs e)
        {

        }


        private void ButtonAide_Click(object sender, RoutedEventArgs e)
        {
            Resultat = "aide";  // Assigner la chaîne que vous souhaitez retourner
            this.DialogResult = true;
        }

        private void boutonChoixTouche_Click(object sender, RoutedEventArgs e)
        {
            Resultat = "choixTouche";  // Assigner la chaîne que vous souhaitez retourner
            this.DialogResult = true;
        }

        private void boutonQuitter_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
