using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Froggun
{
    public class Balle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double angle { get; set; }
        public double vitesse { get; set; }
        public double degats { get; set; }
        public Image balleImage { get; set; }
        public bool aToucher { get; set; }
        public bool inverseX { get; set; }
        public Balle(double x, double y, double angle, double speed, double Degats, Canvas canvas, BitmapImage BalleImageSource, int ScaleX)
        {
            X = x;
            Y = y;
            this.angle = angle;
            vitesse = speed;
            aToucher = false;
            balleImage = new Image
            {
                Source = BalleImageSource,
                Width = 10,
                Height = 10
            };

            //position initial 
            Canvas.SetLeft(balleImage, X);
            Canvas.SetTop(balleImage, Y);

            // transformations (rotation & inverse)
            RotateTransform rotationBalle = new RotateTransform(angle);
            ScaleTransform scaleBalle = new ScaleTransform();
            TransformGroup groupBalle = new TransformGroup();
            
            scaleBalle.ScaleY = ScaleX;
            groupBalle.Children.Add(scaleBalle);
            groupBalle.Children.Add(rotationBalle);
            balleImage.RenderTransform = groupBalle;

            canvas.Children.Add(balleImage);
        }

        public void UpdatePositionBalles()
        {
            // mmmm trigonométrie (calcule de la nouvelle position basé sur l'angle de la balle)
            X += Math.Cos(angle * Math.PI / 180.0) * vitesse;
            Y += Math.Sin(angle * Math.PI / 180.0) * vitesse;
            
            Canvas.SetLeft(balleImage, X);
            Canvas.SetTop(balleImage, Y);
        }
    }
}
