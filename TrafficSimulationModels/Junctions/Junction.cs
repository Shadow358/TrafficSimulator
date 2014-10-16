using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TrafficSimulationModels.Cars;
using TrafficSimulationModels.TrafficLightSystem;

namespace TrafficSimulationModels.Junctions
{
    public enum JunctionType
    {
        Basic = 0,
        Crossing = 1
    }

    public enum Directions
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }

    [Serializable]
    public class Junction
    {
        //---------------------- FIELDS ----------------------------
        // Shared Random object passed to crossings and car spawners
        private Random random;

        // Junctions adjacent to this junction, null if no neighbor.
        // Indices are according to enumeration Directions
        private Junction[] neighbors;

        // Each junction always has exactly four car directions.
        // Indices are according to enumeration Directions
        private JunctionDirection[] junctionDirections;

        // Type of the junction
        private JunctionType junctionType;

        // The traffic light phases according to the junction type.
        private List<TrafficLightPhase> trafficLightPhases;

        // Index of the current traffic light phase, null if no
        // active phase.
        private int? currentPhase;

        // Real size of the junction expressed in centimeters.
        private int size = 2000;

        // Distance between each junction
        private int distanceBetweenJunctions = 12000;

        // Path of the xml file that contains the driving path info
        private string drivingPathXmlFile = "TrafficSimulationModels.Assets.DrivingPaths.xml";

        // Path of the xml file that contains the traffic light system info
        private string trafficLightSystemXmlFile = "TrafficSimulationModels.Assets.TrafficLightSystems.xml";

        //---------------------- CONSTRUCTORS ----------------------
        public Junction(JunctionType junctionType, Junction[] neighbors, Random random)
        {
            this.random = random;
            this.junctionType = junctionType;
            createJunctionDirections();

            this.neighbors = new Junction[4];
            for(int j = 0; j < 4; j++)
            {
                this.SetNeighbor((Directions)j, neighbors[j]);
            }
            SetTrafficLightSystem();
        }

        //---------------------- METHODS ---------------------------
        /// <summary>
        /// Recursively cycles through each phase. Gets called when
        /// a phase is done and initializes the next phase.
        /// </summary>
        private void onPhaseDone()
        {
            if (currentPhase == null)
            {
                throw new Exception("Current phase can not be null if a sub phase is still active.");
            }

            trafficLightPhases[(int)currentPhase].Done -= onPhaseDone;
            currentPhase++;

            if (currentPhase >= trafficLightPhases.Count)
            {
                // Last phase reached
                currentPhase = 0;
            }
            trafficLightPhases[(int)currentPhase].Done += onPhaseDone;
            trafficLightPhases[(int)currentPhase].Start();
        }

        /// <summary>
        /// Start the junction
        /// </summary>
        public void Start()
        {
            currentPhase = 0;
            trafficLightPhases[(int)currentPhase].Start();
            trafficLightPhases[(int)currentPhase].Done += onPhaseDone;

            foreach (JunctionDirection junctionDirection in junctionDirections)
            {
                junctionDirection.Start();
            }
        }

        /// <summary>
        /// Update the junction
        /// </summary>
        /// <param name="deltaTime">Milliseconds past since last update call.</param>
        public void Update(int deltaTime)
        {
            // Update active traffic light phase
            if(currentPhase == null) {
                throw new Exception("Can not call update on an inactive junction.");
            }

            trafficLightPhases[(int)currentPhase].Update(deltaTime);

            // Update junction directions
            foreach (JunctionDirection junctionDirection in junctionDirections)
            {
                junctionDirection.Update(deltaTime);
            }
        }

        /// <summary>
        /// Reset the junction
        /// </summary>
        public void Reset()
        {
            reset(true);
        }

        /// <summary>
        /// (Recursively) reset the junction
        /// </summary>
        /// <param name="recursive">True if junction should be recursively reset, otherwise false.</param>
        private void reset(bool recursive)
        {
            if (currentPhase == null)
            {
                // Junction wasn't active
                return;
            }

            trafficLightPhases[(int)currentPhase].Reset();
            trafficLightPhases[(int)currentPhase].Done -= onPhaseDone;

            if (recursive)
            {
                foreach (JunctionDirection junctionDirection in junctionDirections)
                {
                    junctionDirection.Reset();
                }
            }

            currentPhase = null;
        }

        /// <summary>
        /// Load the traffic light system using the xml file.
        /// </summary>
        /// <param name="name">Name of the traffic light system to load.</param>
        private void loadTrafficLightSystem(string name)
        {
            this.trafficLightPhases = new List<TrafficLightPhase>();

            // Load the traffic light phase xml file
            Assembly assembly;
            StreamReader textStreamReader;
            assembly = Assembly.GetExecutingAssembly();

            List<PathPoint>[] drivingPaths = new List<PathPoint>[3];
            int?[] trafficLights = new int?[3];
            Stream stream = assembly.GetManifestResourceStream(trafficLightSystemXmlFile);

            if (stream == null)
            {
                throw new Exception("Traffic light system xml file could not be found in " + drivingPathXmlFile);
            }

            textStreamReader = new StreamReader(stream);

            if(textStreamReader == null) {
                throw new Exception("Traffic light system xml file could not be found in " + drivingPathXmlFile);
            }

            XDocument doc = XDocument.Parse(textStreamReader.ReadToEnd());

            // Find traffic light system with given name
            List<XElement> trafficLightSystems = doc.Descendants("trafficLightSystem").ToList();

            XElement trafficLightSystem = null;
            foreach (XElement currentTrafficLightSystem in trafficLightSystems)
            {
                if (currentTrafficLightSystem.Attribute("name").Value == name)
                {
                    trafficLightSystem = currentTrafficLightSystem;
                    break;
                }
            }

            if (trafficLightSystem == null)
            {
                throw new Exception("Unknown traffic light system " + name);
            }

            // Parse all phases in the traffic light system
            int defaultTime = Convert.ToInt32(trafficLightSystem.Attribute("defaultPhaseTime").Value);
            List<XElement> phases = trafficLightSystem.Descendants("phase").ToList();

            foreach (XElement phase in phases)
            {
                trafficLightPhases.Add(parsePhase(phase, defaultTime));
            }
        }

        /// <summary>
        /// Parse a traffic light phase from the xml file.
        /// </summary>
        /// <param name="phase">Phase to parse</param>
        /// <param name="defaultTime">Default time to set each subphase</param>
        /// <returns>Traffic light phase that has been parsed</returns>
        private TrafficLightPhase parsePhase(XElement phase, int defaultTime)
        {
            List<XElement> subphasesXML = phase.Descendants("subphase").ToList();
            string phaseName = phase.Attribute("name").Value;
            string phaseAbbreviation = phase.Attribute("abbreviation").Value;
            
            List<TrafficLightSubPhase> subphases = new List<TrafficLightSubPhase>();

            foreach (XElement subphaseXML in subphasesXML)
            {
                List<TrafficLightGroup> trafficLightGroups = new List<TrafficLightGroup>();
                string subphaseName = subphaseXML.Attribute("name").Value;
                List<XElement> trafficLightGroupsXML = subphaseXML.Descendants("group").ToList();
                foreach (XElement trafficLightGroupXML in trafficLightGroupsXML)
                { 
                    List<XElement> trafficLightsXML = trafficLightGroupXML.Descendants("trafficLight").ToList();
                    List<TrafficLight> trafficLights = new List<TrafficLight>();
                    string groupName = trafficLightGroupXML.Attribute("name").Value;
                    foreach (XElement trafficLight in trafficLightsXML)
                    {
                        Directions direction = (Directions)Convert.ToInt32(trafficLight.Attribute("direction").Value);
                        Lanes lane = (Lanes)Convert.ToInt32(trafficLight.Attribute("lane").Value);

                        trafficLights.Add(GetTrafficLight(direction, lane));
                    }

                    trafficLightGroups.Add(new TrafficLightGroup(trafficLights, groupName));
                }

                TrafficLightSubPhase subphase = new TrafficLightSubPhase(trafficLightGroups, defaultTime, subphaseName);
                if (trafficLightGroupsXML.Count() == 2)
                {
                    subphase.SetTrafficLightGroupPercentage(0, 50);
                }
                subphases.Add(subphase);
            }

            TrafficLightPhase trafficLightPhase = new TrafficLightPhase(subphases, defaultTime, phaseName, phaseAbbreviation);

            return trafficLightPhase;
        }

        /// <summary>
        /// Set traffic light system corresponding to the junction type.
        /// </summary>
        private void SetTrafficLightSystem()
        {
            trafficLightPhases = new List<TrafficLightPhase>();

            switch (this.junctionType)
            {
                case JunctionType.Basic:
                    loadTrafficLightSystem("Basic");
                    break;

                case JunctionType.Crossing:
                    loadTrafficLightSystem("Crossing");
                    break;
            }
        }

        /// <summary>
        /// Create junction direction objects.
        /// </summary>
        private void createJunctionDirections() {
            // Load and parse the XML file
            Assembly assembly;
            StreamReader textStreamReader;
            assembly = Assembly.GetExecutingAssembly();

            List<PathPoint>[] drivingPaths = new List<PathPoint>[3];
            int?[] trafficLights = new int?[3];

            Stream stream = assembly.GetManifestResourceStream(drivingPathXmlFile);
            if (stream == null)
            {
                throw new Exception("Driving path xml file could not be found in " + drivingPathXmlFile);
            }
            textStreamReader = new StreamReader(stream);

            if(textStreamReader == null) {
                throw new Exception("Driving path xml file could not be found in " + drivingPathXmlFile);
            }

            XDocument doc = XDocument.Parse(textStreamReader.ReadToEnd());

            List<XElement> xmlLanes = doc.Descendants("lane").ToList();

            foreach (XElement xmlLane in xmlLanes)
            {
                List<XElement> xmlSegments = xmlLane.Descendants("segment").ToList();
                int id = Convert.ToInt32(xmlLane.Attribute("id").Value);
                List<PathPoint> drivingPath = new List<PathPoint>();

                foreach(XElement xmlSegment in xmlSegments) {

                    if (xmlSegment.Attribute("type") == null)
                    {
                        throw new Exception("Missing attribute 'type' in segment element.");
                    }

                    if (xmlSegment.Attribute("x") == null)
                    {
                        throw new Exception("Missing attribute 'x' in segment element.");
                    }

                    int x;
                    if (!Int32.TryParse(xmlSegment.Attribute("x").Value, out x))
                    {
                        throw new Exception("Expected 'x' to be an integer in segment.");
                    }

                    if (xmlSegment.Attribute("y") == null)
                    {
                        throw new Exception("Missing attribute 'y' in segment element.");
                    }

                    int y;
                    if (!Int32.TryParse(xmlSegment.Attribute("y").Value, out y))
                    {
                        throw new Exception("Expected 'y' to be an integer in segment.");
                    }

                    string yString = xmlSegment.Attribute("y").Value;

                    switch(xmlSegment.Attribute("type").Value) {
                        case "point":
                            drivingPath.Add(new PathPoint(new Point(x, y)));
                            break;

                        case "turn":
                            if (xmlSegment.Attribute("size") == null)
                            {
                                throw new Exception("Missing attribute 'size' in segment element with type turn.");
                            }
                            int size;
                            if (!Int32.TryParse(xmlSegment.Attribute("size").Value, out size))
                            {
                                throw new Exception("Expected 'y' to be an integer in segment.");
                            }
                            drivingPath.AddRange(CreateTurn(new PathPoint(new Point(x, y)), size, (Lanes)id));
                            break;

                        default:
                            throw new Exception("Invalid value for attribute 'type' in segment.");
                    }

                    List<XElement> xmlTrafficLight = xmlLane.Descendants("trafficLight").ToList();

                    if (xmlTrafficLight.Count() > 1)
                    {
                        throw new Exception("One driving path can only have one traffic light.");
                    }
                    else if (xmlTrafficLight.Count() == 1)
                    {
                        if (xmlTrafficLight[0].Attribute("distance") == null)
                        {
                            throw new Exception("Missing attribute 'distance' in trafficLight element.");
                        }

                        int distance;
                        if (!Int32.TryParse(xmlTrafficLight[0].Attribute("distance").Value, out distance))
                        {
                            throw new Exception("Expected 'distance' to be an integer in trafficLight.");
                        }

                        trafficLights[id] = distance;
                    }
                    else
                    {
                        trafficLights[id] = null;
                    }
                }

                drivingPaths[id] = drivingPath; 
            }

            junctionDirections = new JunctionDirection[4];

            for(int d = 0; d < 4; d++) {
                // Start at south
                int id;
                if (d <= 1)
                {
                    id = d + 2;
                } else {
                    id = d - 2;
                }

                DrivingPath inputPath = new DrivingPath(distanceBetweenJunctions);
                DrivingPath[] outputPaths = new DrivingPath[3];

                for (int l = 0; l < 3; l++)
                {
                    outputPaths[l] = new DrivingPath(drivingPaths[l]);

                    if(trafficLights[l] != null) {
                        outputPaths[l].SetTrafficLight(new TrafficLight(true), (int)trafficLights[l]);
                    }
                }

                junctionDirections[id] = new JunctionDirection((Directions)id, outputPaths, inputPath, random);

                if (this.junctionType == JunctionType.Crossing)
                    junctionDirections[id].AddCrossing();

                for(int i = 0; i < drivingPaths.Length; i++)
                {
                    drivingPaths[i] = TurnClockwise(drivingPaths[i]);
                }
            }
        }

        /// <summary>
        /// Calculates a curve for the specified lane.
        /// </summary>
        /// <param name="startPoint">Starting point of the curve</param>
        /// <param name="size">The size of the rectangle of the curve</param>
        /// <param name="lane">Lane that needs a curve</param>
        /// <returns>A list of points to draw a smooth curve</returns>
        private List<PathPoint> CreateTurn(PathPoint startPoint, int size, Lanes lane)
        {
            Point centerPoint;
            Point p = new Point();
            double startAngle;
            double endAngle = Math.PI / 2; // 90 degrees in radians;
            double deltaAngle = Math.PI / 36; // 5 degrees in radians
            List<PathPoint> result = new List<PathPoint>();

            if (lane == Lanes.Left)
            {
                // Center is left lower corner of the rectangle
                centerPoint = new Point(startPoint.GetPoint().X - size, startPoint.GetPoint().Y);
                startAngle = 0;

                for (double angle = startAngle; angle <= endAngle; angle += deltaAngle)
                {
                    p.X = (int)Math.Round(centerPoint.X + size * Math.Cos(angle));
                    p.Y = (int)Math.Round(centerPoint.Y + size * Math.Sin(angle));
                    p.Y = startPoint.GetPoint().Y - (p.Y - startPoint.GetPoint().Y);
                    result.Add(new PathPoint(p));
                }
            }
            else // The lane is Lanes.Right
            {
                // Center is right lower corner of the rectangle
                centerPoint = new Point(startPoint.GetPoint().X + size, startPoint.GetPoint().Y);
                startAngle = Math.PI; // 180 degrees

                for (double angle = startAngle; angle >= endAngle; angle -= deltaAngle)
                {
                    p.X = (int)Math.Round(centerPoint.X + size * Math.Cos(angle));
                    p.Y = (int)Math.Round(centerPoint.Y + size * Math.Sin(angle));
                    p.Y = startPoint.GetPoint().Y - (p.Y - startPoint.GetPoint().Y);
                    result.Add(new PathPoint(p));
                }
            }

            // Making sure that last point corresponds to the end of curve
            if (result[result.Count - 1].GetPoint().Y != startPoint.GetPoint().Y - size)
            {
                p.X = (int)Math.Round(centerPoint.X + size * Math.Cos(endAngle));
                p.Y = (int)Math.Round(centerPoint.Y + size * Math.Sin(endAngle));
                p.Y = startPoint.GetPoint().Y - (p.Y - startPoint.GetPoint().Y);
                result.Add(new PathPoint(p));
            }

            return result;
        }

        /// <summary>
        /// Turn a path clockwise
        /// </summary>
        /// <param name="path">Path to turn clockwise</param>
        /// <returns>Turned path</returns>
        public List<PathPoint> TurnClockwise(List<PathPoint> path)
        {
            List<PathPoint> rotatedPath = new List<PathPoint>();

            foreach (PathPoint p in path)
            {
                rotatedPath.Add(TurnClockwise(p));
            }
            return rotatedPath;
        }

        /// <summary>
        /// Turn a point clockwise
        /// </summary>
        /// <param name="point">Point to turn</param>
        /// <returns>Turned point</returns>
        public PathPoint TurnClockwise(PathPoint point)
        {
            return new PathPoint(TurnClockwise(point.GetPoint()));
        }

        /// <summary>
        /// Turn a point clockwise
        /// </summary>
        /// <param name="point">Point to turn</param>
        /// <returns>Turned point</returns>
        public Point TurnClockwise(Point point)
        {
            int XRotationPoint = size / 2;
            int YRotationPoint = size / 2;
            int theta = 90;
            int CosTheta = Convert.ToInt16(Math.Cos(theta));
            int SinTheta = Convert.ToInt16(Math.Sin(theta));
            int px = Convert.ToInt16(point.X);
            int py = Convert.ToInt16(point.Y);
            int pDashx = CosTheta * (px - XRotationPoint) - SinTheta * (py - YRotationPoint) + XRotationPoint;
            int pDashy = SinTheta * (px - XRotationPoint) + CosTheta * (py - YRotationPoint) + YRotationPoint;
            Point tmpPoint = new Point(pDashx, pDashy);

            return tmpPoint;
        }

        /// <summary>
        /// Get an adjacent junction
        /// </summary>
        /// <param name="direction">Direction</param>
        /// <returns>Adjacent junction, null if none</returns>
        public Junction GetNeighbor(Directions direction)
        {
            return neighbors[(int)direction];
        }

        /// <summary>
        /// Get the traffic light phase.
        /// </summary>
        /// <returns>Traffic light phases</returns>
        public List<TrafficLightPhase> GetTrafficLightPhases()
        {
            return this.trafficLightPhases;
        }

        /// <summary>
        /// Get adjacent junctions
        /// </summary>
        /// <returns>Neighbours</returns>
        public Junction[] GetNeighbors()
        {
            return neighbors;
        }

        /// <summary>
        /// Get a junction direction.
        /// </summary>
        /// <param name="direction">Direction to retrieve</param>
        /// <returns>Junction direction</returns>
        public JunctionDirection GetJunctionDirection(Directions direction)
        {
            return junctionDirections[(int)direction];
        }

        /// <summary>
        /// Get driving path identified by direction and lane
        /// </summary>
        /// <param name="direction">Direction</param>
        /// <param name="lane">Lane</param>
        /// <returns>Driving path</returns>
        public DrivingPath GetDrivingPath(Directions direction, Lanes lane)
        {
            return junctionDirections[(int)direction].GetOutputPath(lane);
        }

        /// <summary>
        /// Set a neighbour
        /// </summary>
        /// <param name="direction">Direction of the neighbour</param>
        /// <param name="junction">Junction that will be the neighbour</param>
        public void SetNeighbor(Directions direction, Junction junction)
        {
            neighbors[(int)direction] = junction;

            Directions oppositeDirection = (Directions)(((int)direction + 2) % 4);
            int lane = 2;

            if (junction == null)
            {
                for (int d = 1; d < 4; d++)
                {
                    int directionToDisconnect = (int)direction + d;

                    while (directionToDisconnect > 3)
                    {
                        directionToDisconnect -= 4;
                    }

                    DrivingPath drivingPathToDisconnect = this.GetDrivingPath((Directions)directionToDisconnect, (Lanes)lane);
                    drivingPathToDisconnect.Disconnect();

                    lane--;
                }

                this.junctionDirections[(int)direction].ConvertToEdgeJunctionDirection();
            }
            else
            {
                for (int d = 1; d < 4; d++)
                {
                    int directionToConnect = (int)direction + d;

                    while (directionToConnect > 3)
                    {
                        directionToConnect -= 4;
                    }

                    DrivingPath drivingPathToConnect = this.GetDrivingPath((Directions)directionToConnect, (Lanes)lane);
                    drivingPathToConnect.Connect(junction.GetJunctionDirection(oppositeDirection).GetInputPath());

                    lane--;
                }

                this.junctionDirections[(int)direction].ConvertToNonEdgeJunctionDirection();
            }
        }

        /// <summary>
        /// Remove a neighbour
        /// </summary>
        /// <param name="direction">Neighbour to remove</param>
        public void RemoveNeighbor(Directions direction)
        {
            neighbors[(int)direction] = null;
        }

        /// <summary>
        /// Get the type of the junction
        /// </summary>
        /// <returns>Junction type</returns>
        public JunctionType GetJunctionType()
        {
            return junctionType;
        }

        /// <summary>
        /// Get the traffic light identified by direction and lane
        /// </summary>
        /// <param name="direction">Direction of the traffic light</param>
        /// <param name="lane">Lane of the traffic light</param>
        /// <returns>Traffic light</returns>
        public TrafficLight GetTrafficLight(Directions direction, Lanes lane)
        {
            if (lane == Lanes.Crossing)
            {
                return GetPedestrianTrafficLight(direction);
            }

            return junctionDirections[(int)direction].GetTrafficLight(lane);
        }

        /// <summary>
        /// Pedestrian traffic light attached to the junction
        /// </summary>
        /// <param name="direction">Direction</param>
        /// <returns>Pedestrian traffic light</returns>
        public TrafficLight GetPedestrianTrafficLight(Directions direction)
        {
            return junctionDirections[(int)direction].GetCrossing().GetTrafficLight();
        }
    }
}
