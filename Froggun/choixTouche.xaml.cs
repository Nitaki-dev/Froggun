﻿using System;
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
    /// Logique d'interaction pour choixTouche.xaml
    /// </summary>
    public partial class choixTouche : Window
    {
        public choixTouche()
        {
            InitializeComponent();
        }

        private void boutonQuitter_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}