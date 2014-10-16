using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficSimulationModels.Cars
{
    [Serializable]
    public class PathPoint
    {
        //---------------------- FIELDS ----------------------------
        // Point that the path point represents
        private Point point;

        //---------------------- CONSTRUCTORS ----------------------
        public PathPoint(Point point)
        {
            this.point = point;
        }

        //---------------------- METHODS ---------------------------
        /// <summary>
        /// Calculate the amount of distance towards another point.
        /// </summary>
        /// <param name="point">Point to calculate the distance to.</param>
        /// <returns>Distance</returns>
        public double DistanceTo(PathPoint point)
        {
            int a = this.point.X - point.GetPoint().X;
            int b = this.point.Y - point.GetPoint().Y;
            double distance = Math.Sqrt(a * a + b * b);
            return distance;
        }

        /// <summary>
        /// Calculate an intermediate point between this and another point with a given distance.
        /// </summary>
        /// <param name="direction">Direction point</param>
        /// <param name="distance">Distance from this point</param>
        /// <returns>Distance</returns>
        public PathPoint CalculatePoint(PathPoint direction, double distance)
        {
            Point thisPoint = point;
            Point directionPoint = direction.GetPoint();

            double x;
            double y;

            double yDiff = (directionPoint.Y - thisPoint.Y);
            double xDiff = (directionPoint.X - thisPoint.X);

            // Avoid devide by 0 error
            if (xDiff == 0)
            {
                x = 0;
                if (thisPoint.Y < directionPoint.Y)
                {
                    y = -distance;
                }
                else
                {
                    y = distance;
                }
            }
            else if (yDiff == 0)
            {
                y = 0;
                if (thisPoint.X < directionPoint.X)
                {
                    x = -distance;
                }
                else
                {
                    x = distance;
                }
            } 
            else
            {
                if (yDiff > 0)
                {
                    distance = -distance;
                }

                double division = xDiff / yDiff;
                double angle = Math.Atan(division);
                x = Math.Sin(angle) * distance;
                y = Math.Cos(angle) * distance;
            }

            PathPoint result = new PathPoint(new Point(Convert.ToInt32(thisPoint.X - x), Convert.ToInt32(thisPoint.Y - y)));
            return result;
        }

        /// <summary>
        /// Get represented point.
        /// </summary>
        /// <returns>Point</returns>
        public Point GetPoint()
        {
            return point;
        }
        
        /// <summary>
        /// Get a scaled point.
        /// </summary>
        /// <param name="scale">Scale factor</param>
        /// <returns>Point</returns>
        public Point GetPoint(double scale)
        {
            return new Point(Convert.ToInt32(point.X * scale), Convert.ToInt32(point.Y * scale));
        }
    }
}
