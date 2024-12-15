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
        private readonly double minDistance;
        private readonly Random random;

        // https://bost.ocks.org/mike/algorithms/
        public Sampler(double width, double height, double minDistance, int? seed = null)
        {
            this.width = width;
            this.height = height;
            this.minDistance = minDistance;
            this.random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public List<Point> GeneratePoints()
        {
            var points = new List<Point>();

            // sample points along the top edge
            AddEdgePoints(0, 0, width, 0, points);
            // sample points along the bottom edge
            AddEdgePoints(0, height, width, height, points);
            // sample points along the left edge
            AddEdgePoints(0, 0, 0, height, points);
            // sample points along the right edge
            AddEdgePoints(width, 0, width, height, points);

            return points;
        }

        private void AddEdgePoints(double x1, double y1, double x2, double y2, List<Point> points)
        {
            // distance entre les deux extrémités
            double distance = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
            int numPoints = (int)(distance / minDistance);

            // divise le bord en segments égaux pour placer les points
            for (int i = 0; i <= numPoints; i++)
            {
                double t = (double)i / numPoints;
                double x = x1 + t * (x2 - x1);
                double y = y1 + t * (y2 - y1);

                if (estAssezLoin(new Point(x, y), points)) points.Add(new Point(x, y));
            }
        }

        private bool estAssezLoin(Point candidate, List<Point> points)
        {
            foreach (var point in points)
            {
                if (Distance(candidate, point) < minDistance) return false;
            }
            return true;
        }

        private double Distance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
