using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrafficSimulationModels.Junctions;
using TrafficSimulationModels.TrafficLightSystem;
using TrafficSimulationModels.Cars;
using TrafficLightSimulationApp.Helpers;

namespace TrafficLightSimulationApp.Views
{
    public class JunctionView
    {
        private Junction junction = null;

        public delegate void SetBackground(JunctionView junctionView, object background);
        private SetBackground setBackground;
        private bool selected = false;
        private Color selectedColor = Color.FromArgb(183, 237, 255);

        private bool hovered = false;
        private Color hoveredColor = Color.FromArgb(120, 120, 120);

        private Color defaultColor = Color.FromArgb(30, 30, 30);
        private Color occupiedColor = Color.White;

        private DrivingPathView[,] drivingPathViews;

        public JunctionView(SetBackground setBackground)
        {
            this.setBackground = setBackground;
        }

        public void SetJunction(Junction junction)
        {
            this.junction = junction;
            //DrivingPathView drivingPathView = new DrivingPathView(junction.GetD
            drivingPathViews = new DrivingPathView[4,3];
            for (int d = 0; d < 4; d++)
            {
                for (int l = 0; l < 3; l++)
                {
                    drivingPathViews[d,l] = new DrivingPathView(junction.GetDrivingPath((Directions)d, (Lanes)l));
                }
            }

            UpdateBackground();
        }

        public void DeleteJunction()
        {
            this.junction = null;
            Draw();
        }

        public Junction GetJunction()
        {
            return junction;
        }

        public void SetHovered(bool hovered)
        {
            this.hovered = hovered;
            UpdateBackground();
        }

        public void Select()
        {
            selected = true;
            UpdateBackground();
        }

        public void Deselect()
        {
            selected = false;
            UpdateBackground();
        }

        public void Draw()
        {
            UpdateBackground();
        }

        public void Draw(Graphics g)
        {
            if (junction != null)
            {
                foreach (DrivingPathView drivingPathView in drivingPathViews)
                {
                    drivingPathView.Draw(g);
                }

                int barTotalHeight = 490;

                if (junction.GetJunctionType() == JunctionType.Crossing)
                {
                    PathPoint[] trafficLightLocation = new PathPoint[4];
                    trafficLightLocation[0] = new PathPoint(new Point(599, 579));

                    for (int i = 1; i < 4; i++)
                    {
                        trafficLightLocation[i] = junction.TurnClockwise(trafficLightLocation[i - 1]);
                    }

                    for (int d = 0; d < 4; d++)
                    {
                        TrafficLight pedestrianLight = junction.GetJunctionDirection((Directions)d).GetCrossing().GetTrafficLight();
                        if (pedestrianLight == null)
                            return;

                        SolidBrush brush;
                        switch (pedestrianLight.GetTrafficLightState())
                        {
                            case TrafficLightState.green:
                                brush = new SolidBrush(Color.Green);
                                break;

                            case TrafficLightState.red:
                                brush = new SolidBrush(Color.Red);
                                break;

                            default:
                                throw new NotImplementedException();
                        }

                        
                        //trafficLightLocation[d].GetPoint(0.05);
                        Point drawingPoint = trafficLightLocation[d].GetPoint();
                        Rectangle myRectangle = new Rectangle(drawingPoint.X - 50, drawingPoint.Y - 50, 100, 100);
                        g.FillEllipse(brush, myRectangle);

                        int pedestriansWaiting = junction.GetJunctionDirection((Directions)d).GetCrossing().GetPedestriansWaiting();
                        string drawString;

                        if(pedestriansWaiting > 99) {
                            drawString = "99+";
                        } else {
                            drawString = pedestriansWaiting.ToString();
                        }
                        Font drawFont = new System.Drawing.Font("Arial", 100);
                        SolidBrush drawBrush = new SolidBrush(Color.Black);

                        int offsetX = 100;
                        int offsetY = 100;
                        
                        if(d < 2) {
                            offsetY = -offsetY;
                        }

                        if(d == 0 || d == 3) {
                            offsetX = -offsetX;
                        }
                        if (pedestriansWaiting > 0)
                        {
                            g.DrawString(drawString, drawFont, drawBrush, drawingPoint.X - 55 + offsetX, drawingPoint.Y - 55 + offsetY);
                        }
                    }
                }

                Junction[] neighbours = junction.GetNeighbors();
                Point[] locations = new Point[4] {
                    new Point(1130, 0),
                    new Point(2000 - barTotalHeight,1130),
                    new Point(770, 2000- barTotalHeight),
                    new Point(0, 770)
                };

                for (int d = 0; d < 4; d++)
                {
                    SolidBrush brushBackground = new SolidBrush(Color.Gray);
                    Rectangle rectangleBackground = new Rectangle();

                    switch ((Directions)d)
                    {
                        case Directions.North:
                            rectangleBackground = new Rectangle(locations[d], new Size(100, barTotalHeight));
                            break;

                        case Directions.East:
                            rectangleBackground = new Rectangle(locations[d], new Size(barTotalHeight, 100));
                            break;

                        case Directions.South:
                            rectangleBackground = new Rectangle(locations[d], new Size(100, barTotalHeight));
                            break;

                        case Directions.West:
                            rectangleBackground = new Rectangle(locations[d], new Size(barTotalHeight, 100));
                            break;
                    }

                    g.FillRectangle(brushBackground, rectangleBackground);

                    if (neighbours[d] != null)
                    {
                        Directions oppositeDirection = (Directions)((d + 2) % 4);
                        DrivingPath inputPath = neighbours[d].GetJunctionDirection(oppositeDirection).GetInputPath();

                        int percentage = Convert.ToInt32(Math.Round(inputPath.GetTrafficOccupation()*100));
                        int capacity = inputPath.GetCapacity();
                        double trafficOccupation = inputPath.GetTrafficOccupation();

                        string drawString = percentage + "%";
                        Font drawFont = new System.Drawing.Font("Arial", 100);
                        SolidBrush drawBrush = new SolidBrush(Color.Black);

                        int x = 0;
                        int y = 0;

                        Color color = new HSLColor((double)((1-trafficOccupation) * 74), (double)240, (double)115);
                        SolidBrush brushForeground = new SolidBrush(color);
                        int barHeight = Convert.ToInt32(Math.Round(trafficOccupation * barTotalHeight));

                        
                        //Rectangle rectangleForeground = new Rectangle(new Point(locations[i].X, locations[i].Y + (barHeight - barTotalHeight)), new Size(100, barHeight));
                        Rectangle rectangleForeground = new Rectangle();
                        switch((Directions)d)
                        {
                            case Directions.North:
                                x = locations[d].X + 120;
                                y = locations[d].Y + 180;
                                rectangleForeground = new Rectangle(new Point(locations[d].X, locations[d].Y), new Size(100, barHeight));
                                break;

                            case Directions.East:
                                x = locations[d].X + 180;
                                y = locations[d].Y + 90;
                                rectangleForeground = new Rectangle(new Point(locations[d].X + (barTotalHeight - barHeight), locations[d].Y), new Size(barHeight, 100));
                                break;

                            case Directions.South:
                                x = locations[d].X - 120;
                                y = locations[d].Y + 180;
                                rectangleForeground = new Rectangle(new Point(locations[d].X, locations[d].Y + (barTotalHeight - barHeight)), new Size(100, barHeight));
                                break;

                            case Directions.West:
                                x = locations[d].X + 120;
                                y = locations[d].Y - 140;
                                rectangleForeground = new Rectangle(new Point(locations[d].X, locations[d].Y), new Size(barHeight, 100));
                                break;
                        }

                        
                        g.FillRectangle(brushForeground, rectangleForeground);
                        g.DrawString(drawString, drawFont, drawBrush, x, y);
                    }
                }

                Point carsWaitingPoint = new Point(450,0);

                for (int d = 0; d < 4; d++)
                {
                    string drawString = "";
                    JunctionDirection junctionDirection = junction.GetJunctionDirection((Directions)d);
                    if (junctionDirection.IsEdgeDirection())
                    {
                        int carsQueued = junctionDirection.GetCarSpawner().GetQueueLength();
                        if (carsQueued > 99)
                        {
                            drawString = "99+";
                        }
                        else
                        {
                            drawString = carsQueued.ToString();
                        }
                    
                        Font drawFont = new System.Drawing.Font("Arial", 100);
                        SolidBrush drawBrush = new SolidBrush(Color.Black);

                        int offsetX = 0;
                        int offsetY = 0;
                        switch (d)
                        {
                            case 1:
                                offsetX = -220;
                                break;

                            case 2:
                                offsetY = -200;
                                offsetX = -110;
                                break;

                            case 3:
                                offsetY = -100;
                                break;
                        }

                        if (carsQueued > 0)
                        {
                            g.DrawString(drawString, drawFont, drawBrush, carsWaitingPoint.X + offsetX, carsWaitingPoint.Y + offsetY);
                        }
                    }
                    carsWaitingPoint = junction.TurnClockwise(carsWaitingPoint);
                }
            }
        }

        private void UpdateBackground()
        {
            setBackground(this, null);

            if (junction != null)
            {
                JunctionType junctionType = junction.GetJunctionType();
                switch (junctionType)
                {
                    case JunctionType.Basic:
                        setBackground(this, global::TrafficLightSimulationApp.Properties.Resources.junctionBasic);
                        break;

                    case JunctionType.Crossing:
                        setBackground(this, global::TrafficLightSimulationApp.Properties.Resources.junctionCrossing);
                        break;
                }
            }

            if (hovered)
            {
                setBackground(this, hoveredColor);
            }
            else if (selected)
            {
                setBackground(this, selectedColor);
            }
            else if (junction != null)
            {
                setBackground(this, occupiedColor);
            } 
            else
            {
                setBackground(this, defaultColor);
            }
        }
    }
}
