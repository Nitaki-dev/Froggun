using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Froggun
{
    public class Sampler
    {
        private readonly double width;
        private readonly double height;
        private readonly double distanceMin;
        private readonly Random random;

        // Poisson Disk Sampling https://bost.ocks.org/mike/algorithms/
        public Sampler(double width, double height, double minDistance, int? seed = null)
        {
            this.width = width;
            this.height = height;
            this.distanceMin = minDistance;
            this.random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public List<Point> GeneratePoints()
        {
            var points = new List<Point>();

            // où ajouter des points
            AjouterDesPointsAuBord(0, 0, width, 0, points); // haut
            AjouterDesPointsAuBord(0, height, width, height, points); // bas
            AjouterDesPointsAuBord(0, 0, 0, height, points); // gauche
            AjouterDesPointsAuBord(width, 0, width, height, points); // droite

            return points;
        }

        private void AjouterDesPointsAuBord(double x1, double y1, double x2, double y2, List<Point> points)
        {
            // distance entre les deux extrémités
            double distance = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
            int nbrPoints = (int)(distance / distanceMin);

            // divise le bord en segments égaux pour placer les points
            for (int i = 0; i <= nbrPoints; i++)
            {
                double t = (double)i / nbrPoints;
                double x = x1 + t * (x2 - x1);
                double y = y1 + t * (y2 - y1);

                // si les points sont assez loin on les ajoutes
                if (EstAssezLoin(new Point(x, y), points)) points.Add(new Point(x, y));
            }
        }

        // calcule la difference de position entre deux points
        private bool EstAssezLoin(Point candidate, List<Point> points) 
        {
            foreach (var point in points)
            {
                double dx = candidate.X - point.X;
                double dy = candidate.Y - point.Y;
                if (Math.Sqrt(dx * dx + dy * dy) < distanceMin) return false;
            }
            return true;
        }
    }
}
