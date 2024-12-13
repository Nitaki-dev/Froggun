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
    internal class Player
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Rect BoundingBox { get; set; }
        public Image Image { get; set; }
        public Point Position { get; set; }

        private int Health { get; set; }
        public double maxHealth { get; set; }

        public static Vector2 posJoueur = new Vector2(50.0f, 50.0f);

        private static BitmapImage imgFrogFront;
        private static BitmapImage imgFrogBack;
        private static BitmapImage imgFrogSide;
        public Player(double x, double y, double width, double height,Rect BoundingBox = new Rect())
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            
        }
        public static void UpdatePlayer()
        {

        }
        public static void SetPlayerImage(Directions direction)
        {
            //if (direction == Directions.left || direction == Directions.diagUpLeft || direction == Directions.diagDownLeft)
            //    Image = imgFrogSide; // Изображение для левого направления
          //  if (direction == Directions.up || direction == Directions.diagUpLeft || direction == Directions.diagUpRight)
          //      Source = imgFrogBack; // Изображение для верхнего направления
          //  if (direction == Directions.down || direction == Directions.diagDownLeft || direction == Directions.diagDownRight)
          //      Source = imgFrogFront; // Изображение для нижнего направления
        }

    }
}
