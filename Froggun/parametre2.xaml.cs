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
    /// Logique d'interaction pour parametre2.xaml
    /// </summary>
    public partial class parametre2 : Window
    {
        public string Resultat { get; private set; }
        public parametre2()
        {
            InitializeComponent();
        }

        private void boutonAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;

        }

        private void boutonJouer_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

        }

        private void boutonParametre(object sender, RoutedEventArgs e)
        {
            Resultat = "parametre";  // Assigner la chaîne que vous souhaitez retourner
            this.DialogResult = true;
        }
    }
}
