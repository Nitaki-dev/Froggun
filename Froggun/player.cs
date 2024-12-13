using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using System.Numerics;
using System.Windows.Media;
using static Froggun.MainWindow;
using System.Windows.Shapes;
using System.Windows.Automation;


namespace Froggun
{
    internal class player
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Rect BoundingBox { get; set; }
        public Image Image { get; set; }
        private int Health { get; set; }
        public double maxHealth { get; set; }

        private static BitmapImage imgFrogFront;
        private static BitmapImage imgFrogBack;
        private static BitmapImage imgFrogSide;

        public static void UpdatePlayer()
        {

        }

    }
}
