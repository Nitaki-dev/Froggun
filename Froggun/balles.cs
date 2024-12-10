using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Froggun
{
    public class Balle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; } // Radian
        public double Speed { get; set; }
        public Image BalleImage { get; set; }
        
        public Balle(double x, double y, double angle, double speed, Canvas canvas, BitmapImage BalleImageSource)
        {
            X = x;
            Y = y;
            Angle = angle;
            Speed = speed;

            BalleImage = new Image
            {
                Source = BalleImageSource,
                Width = 25,
                Height = 25
            };

            Canvas.SetLeft(BalleImage, X);
            Canvas.SetTop(BalleImage, Y);

            RotateTransform rotationBalle = new RotateTransform(angle * 180 / Math.PI);
            BalleImage.RenderTransform = rotationBalle;

            canvas.Children.Add(BalleImage);
        }

        public void UpdatePositionBalles()
        {
            X += Math.Cos(Angle) * Speed;
            Y += Math.Sin(Angle) * Speed;
            
            Canvas.SetLeft(BalleImage, X);
            Canvas.SetTop(BalleImage, Y);
        }
    }
}
