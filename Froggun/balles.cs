using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Froggun
{
    public class Balle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        public double Speed { get; set; }
        public double Degats { get; set; }
        public Image BalleImage { get; set; }
        public bool hasHit { get; set; }
        public bool ScaleX { get; set; }
        public Balle(double x, double y, double angle, double speed, double Degats, Canvas canvas, BitmapImage BalleImageSource, int ScaleX)
        {
            X = x;
            Y = y;
            Angle = angle;
            Speed = speed;
            hasHit = false;
            BalleImage = new Image
            {
                Source = BalleImageSource,
                Width = 10,
                Height = 10
            };

            Canvas.SetLeft(BalleImage, X);
            Canvas.SetTop(BalleImage, Y);

            RotateTransform rotationBalle = new RotateTransform(angle);
            ScaleTransform scaleBalle = new ScaleTransform();
            TransformGroup groupBalle = new TransformGroup();
            
            scaleBalle.ScaleY = ScaleX;
            groupBalle.Children.Add(scaleBalle);
            groupBalle.Children.Add(rotationBalle);

            BalleImage.RenderTransform = groupBalle;

            canvas.Children.Add(BalleImage);
        }

        public void UpdatePositionBalles()
        {
            X += Math.Cos(Angle * Math.PI / 180.0) * Speed;
            Y += Math.Sin(Angle * Math.PI / 180.0) * Speed;
            
            Canvas.SetLeft(BalleImage, X);
            Canvas.SetTop(BalleImage, Y);
        }
    }
}
