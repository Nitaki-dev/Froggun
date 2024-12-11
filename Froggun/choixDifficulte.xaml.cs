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
    /// Logique d'interaction pour choixDifficulte.xaml
    /// </summary>
    public partial class choixDifficulte : Window
    {
        public string Resultat { get; private set; }
        public choixDifficulte()
        {
            InitializeComponent();
        }

        private void boutonFacile_Click(object sender, RoutedEventArgs e)
        {
            Resultat = "facile";  // Assigner la chaîne que vous souhaitez retourner
            this.DialogResult = true;
        }

        private void boutonMoyen_Click(object sender, RoutedEventArgs e)
        {
            Resultat = "moyen";  // Assigner la chaîne que vous souhaitez retourner
            this.DialogResult = true;
        }

        private void boutonDifficile_Click(object sender, RoutedEventArgs e)
        {
            Resultat = "difficile";  // Assigner la chaîne que vous souhaitez retourner
            this.DialogResult = true;
        }

        private void boutonExtreme_Click(object sender, RoutedEventArgs e)
        {
            Resultat = "extreme";  // Assigner la chaîne que vous souhaitez retourner
            this.DialogResult = true;

        }
    }
}
