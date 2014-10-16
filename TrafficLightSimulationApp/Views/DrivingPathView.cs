using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrafficSimulationModels.Cars;
using TrafficSimulationModels.TrafficLightSystem;

namespace TrafficLightSimulationApp.Views
{
    public class DrivingPathView
    {
        private DrivingPath drivingPath;
        private bool showPath = false;

        public void ShowPath()
        {
            showPath = true;
        }

        public void HidePath()
        {
            showPath = false;
        }

        public DrivingPathView(DrivingPath drivingPath)
        {
            this.drivingPath = drivingPath;
        }

        public void Draw(Graphics g)
        {
            if (showPath)
            {
                drawPath(g);
            }
            drawCars(g);
            drawTrafficLight(g);
        }

        public DrivingPath GetDrivingPath()
        {
            return drivingPath;
        }

        private void drawPath(Graphics g)
        {
            if (drivingPath == null)
            {
                return;
            }

            if (drivingPath.GetPathPoints() != null)
            {
                List<PathPoint> pathPoints = drivingPath.GetPathPoints();
                Pen pen = new Pen(Color.Black);
                for (int i = 0; i + 1 < pathPoints.Count(); i++)
                {
                    g.DrawLine(pen, pathPoints[i].GetPoint(), pathPoints[i + 1].GetPoint());
                }
            }
        }

        private void drawCars(Graphics g)
        {
            //SolidBrush brush = new SolidBrush(Color.Black);
            List<PathPoint> carPoints = drivingPath.GetCarPoints();
            //List<Color> colors = drivingPath.GetColors();

            int i = 0;
            foreach (PathPoint carPoint in carPoints)
            {
                //SolidBrush brush = new SolidBrush(colors[i]);
                SolidBrush brush = new SolidBrush(Color.Black);
                Point point = carPoint.GetPoint();
                Rectangle myRectangle = new Rectangle(point.X - 30, point.Y - 30, 60, 60);
                g.FillEllipse(brush, myRectangle);
                i++;
            }
        }

        private void drawTrafficLight(Graphics g)
        {
            if (drivingPath.GetTrafficLight() == null)
            {
                return;
            }

            SolidBrush brush;

            switch (drivingPath.GetTrafficLight().GetTrafficLightState())
            {
                case TrafficLightState.green:
                    brush = new SolidBrush(Color.Green);
                    break;

                case TrafficLightState.orange:
                    brush = new SolidBrush(Color.Orange);
                    break;

                case TrafficLightState.red:
                    brush = new SolidBrush(Color.Red);
                    break;

                default:
                    throw new NotImplementedException();
            }

            Point point = drivingPath.GetTrafficLightPoint().GetPoint();
            Rectangle myRectangle = new Rectangle(point.X - 50, point.Y - 50, 100, 100);
            g.FillEllipse(brush, myRectangle);
        }
    }
}
