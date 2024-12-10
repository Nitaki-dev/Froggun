using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Logique d'interaction pour parametre.xaml
    /// </summary>
    public partial class parametre : Window

    {
        public string Resultat { get; private set; }

        public parametre()
        {
            InitializeComponent();

        }

        private void boutonAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            
        }

        private void boutnJouer_Click(object sender, RoutedEventArgs e)
        {         
                this.DialogResult = true;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Resultat = "parametre";  // Assigner la chaîne que vous souhaitez retourner
            this.DialogResult = true;
        }
    }
}
