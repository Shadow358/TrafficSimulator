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
using TrafficSimulationModels.Junctions;
using TrafficSimulationModels.TrafficLightSystem;

namespace TrafficSimulationModels.Cars
{
    [Serializable]
    public class DrivingPath
    {
        //---------------------- FIELDS ----------------------------
        // Traffic light that is attached to the path.
        private TrafficLight trafficLight;

        // Amount of distance that car will between the car and the traffic light when turned red expressed in centimeters.
        private double trafficLightDrawingOffsetDistance = 50;

        // Distance of the location expressed in centimeters.
        private double? trafficLightDistance;

        // Distance at which the traffic light will be drawn 
        private double? trafficLightDrawingDistance {
            get
            {
                if (trafficLightDistance == null)
                {
                    return null;
                }
                else
                {
                    return (double)trafficLightDistance + trafficLightDrawingOffsetDistance;
                }
            }
        }

        // Cars that are currently on the path
        private List<Car> cars;

        // Points of the path
        private List<PathPoint> pathPoints;

        // Distance towards each point
        private List<double> pathPointDistances;

        // Driving path that is connected to this path
        private DrivingPath nextDrivingPath;

        // Delegate for retrieving a driving path in case it is a variable path.
        public delegate DrivingPath PathRetriever();
        private PathRetriever getNextDrivingPath;

        // Event that gets fired whenever a car enters the path.
        public delegate void CarHandler(Car car);
        public event CarHandler CarEntered;

        // Minimal distance that cars will keep towards the car in front.
        private double minMargin = 70;

        // Distance that a car must have driven so that another car can spawn
        private int safeDistance = 100;

        // Temporary list that keeps track of cars that have left the path
        private List<Car> carsToRemove;

        // Temporary list that keeps track of car that have entered the path
        private List<Car> carsToAdd;

        // Boolean whether the locations of the cars have already been calculated for this update call
        private bool pointsCalculated = false;

        // Locations of the car on the path
        private List<PathPoint> carPoints;

        // Location of the traffic light
        private PathPoint trafficLightPoint;

        //---------------------- CONSTRUCTORS ----------------------
        public DrivingPath(List<PathPoint> pathPoints)
        {
            Reset();

            foreach (PathPoint pathPoint in pathPoints)
            {
                addPoint(pathPoint);
            }
        }

        public DrivingPath(int distance)
        {
            Reset();

            addPoint(new PathPoint(new Point(0, 0)));
            addPoint(new PathPoint(new Point(distance, 0)));
        }

        //---------------------- METHODS ---------------------------
        /// <summary>
        /// Set traffic light of the driving path
        /// </summary>
        /// <param name="trafficLight">Traffic light to set</param>
        /// <param name="trafficLightDistance">Distance of the traffic light</param>
        public void SetTrafficLight(TrafficLight trafficLight, int trafficLightDistance)
        {
            this.trafficLightDistance = trafficLightDistance;
            this.trafficLight = trafficLight;
        }

        /// <summary>
        /// Retrieve the minimal margin that cars will maintain.
        /// </summary>
        /// <returns>Minimal margin expressed in centimeters</returns>
        public double GetMinMargin()
        {
            return minMargin;
        }

        /// <summary>
        /// Calculate the capacity of the driving path
        /// </summary>
        /// <returns>Amount of cars that fit on the path.</returns>
        public int GetCapacity()
        {
            return Convert.ToInt32(Math.Floor((GetTotalDistance() / (minMargin + 11))));
        }

        /// <summary>
        /// Calculate the traffic occupation
        /// </summary>
        /// <returns>Traffic occupation expressed as a factor, with 0 as no traffic and 1 completely full.</returns>
        public double GetTrafficOccupation()
        {
            double factor = ((double)cars.Count / GetCapacity());

            if (factor > 1)
            {
                return 1;
            }

            return factor;
        }

        /// <summary>
        /// Retrieve the last car (with minimal distance) on the path.
        /// </summary>
        /// <returns>Last car object</returns>
        public Car GetLastCar()
        {
            if (cars.Count() > 0)
            {
                return cars.Last();
            }

            return null;
        }

        /// <summary>
        /// Calculate the amount of cars that are before the given car on the path.
        /// </summary>
        /// <param name="car">Car to calculate the cars in front of.</param>
        /// <returns>Amount of cars.</returns>
        public int GetCarsBefore(Car car)
        {
            int index = 0;

            while (index < cars.Count() && cars[index] != car)
            {
                index++;
            }

            return index;
        }

        /// <summary>
        /// Get the total distance of the path.
        /// </summary>
        /// <returns>Total distance of the path expressed in centimeters</returns>
        public double GetTotalDistance()
        {
            return pathPointDistances.Last();
        }

        /// <summary>
        /// Get the car in front of a car.
        /// </summary>
        /// <param name="car">Car to get the car in front of</param>
        /// <returns>Car in front</returns>
        public Car GetCarInFront(Car car)
        {
            bool carFound = false;
            int index = 0;

            // Find the given car
            while (!carFound && index < cars.Count())
            {
                if(cars[index] == car){
                    carFound = true;
                } else {
                    index++;
                }
            }

            // The given car is not on the driving path
            if (!carFound)
            {
                if (!carsToAdd.Contains(car))
                {
                    throw new Exception("Car is not on the driving path");
                }
                else
                {
                    if (cars.Count() == 0)
                    {
                        return null;
                    }

                    return cars.Last();
                }
            }

            // There is no car in front
            if(index + 1 >= cars.Count()) {
                return null;
            }

            // Return car in front
            return cars[index + 1];
        }

        /// <summary>
        /// Calculate the locations of the cars on the path.
        /// </summary>
        private void calculatePoints()
        {
            // Do not recalculate points if points have already been calculated this update frame
            if (pointsCalculated)
            {
                return;
            }

            this.carPoints = new List<PathPoint>();
            trafficLightPoint = null;

            List<Car> carsOnPath = cars;
            foreach (Car car in carsToRemove)
            {
                carsOnPath.Remove(car);
            }

            int currentCar = carsOnPath.Count() - 1;
            int currentPoint = 1;
            bool lastCarFound = false;
            bool lastPathPointPassed = false;
            bool trafficLightFound = false;
            if (trafficLight == null)
            {
                trafficLightFound = true;
            }
            double previousCarDistance;

            // Loop ends when either the last car is found or the path ends
            while (!lastPathPointPassed && !(lastCarFound && trafficLightFound))
            {
                if (currentCar < 0)
                {
                    lastCarFound = true;
                }

                if (currentPoint >= pathPointDistances.Count())
                {
                    lastPathPointPassed = true;
                    break;
                }

                // Check for traffic light
                if (!trafficLightFound && trafficLightDrawingDistance <= pathPointDistances[currentPoint])
                {
                    double remainingDistance = (double)trafficLightDrawingDistance - pathPointDistances[currentPoint - 1];
                    trafficLightPoint = pathPoints[currentPoint - 1].CalculatePoint(pathPoints[currentPoint], remainingDistance);
                    trafficLightFound = true;

                }

                // Check for car
                if (!lastCarFound && carsOnPath[currentCar].GetDistance() <= pathPointDistances[currentPoint])
                {
                    // Current car is in current section, get point and find next car
                    double remainingDistance = carsOnPath[currentCar].GetDistance() - pathPointDistances[currentPoint - 1];

                    // Calculate the intermediate point between the two points
                    PathPoint carPoint = pathPoints[currentPoint - 1].CalculatePoint(pathPoints[currentPoint], remainingDistance);

                    carPoints.Add(carPoint);

                    // Go to next car
                    currentCar--;

                    previousCarDistance = cars[currentCar + 1].GetDistance();
                    // Check if cars descendant ordered by distance condition hold
                }
                else
                {
                    // Current car is not in current section, increase current point
                    currentPoint++;
                }
            }

            pointsCalculated = true;
        }

        /// <summary>
        /// Get the locations of the cars.
        /// </summary>
        /// <returns>Location of the car expressed in centimeters.</returns>
        public List<PathPoint> GetCarPoints()
        {
            calculatePoints();
            return carPoints;
        }

        /// <summary>
        /// Get the location of the traffic light.
        /// </summary>
        /// <returns>Location of the traffic light expressed in centimeters.</returns>
        public PathPoint GetTrafficLightPoint()
        {
            calculatePoints();
            return trafficLightPoint;
        }

        /// <summary>
        /// Get  the next driving path.
        /// </summary>
        /// <returns>Next driving path.</returns>
        public DrivingPath GetNextDrivingPath()
        {
            // Fixed next path
            if (nextDrivingPath != null)
            {
                return nextDrivingPath;
            }

            // Variable next path
            if (getNextDrivingPath != null)
            {
                return getNextDrivingPath();
            }

            // No next path
            return null;
        }

        /// <summary>
        /// Get the traffic light distance.
        /// </summary>
        /// <returns>Traffic light distance expressed in centimeters.</returns>
        public double? GetTrafficLightDistance()
        {
            return trafficLightDistance;
        }

        /// <summary>
        /// Get the traffic light attached to the path.
        /// </summary>
        /// <returns>Traffic light</returns>
        public TrafficLight GetTrafficLight()
        {
            return trafficLight;
        }

        /// <summary>
        /// Get the length of the path.
        /// </summary>
        /// <returns>Path length expressed in centimeters.</returns>
        public double GetPathLength()
        {
            return pathPointDistances[pathPointDistances.Count() - 1];
        }

        /// <summary>
        /// Get the points of the path.
        /// </summary>
        /// <returns>Path points</returns>
        public List<PathPoint> GetPathPoints()
        {
            return pathPoints;
        }

        /// <summary>
        /// Get the cars that are currently on the path.
        /// </summary>
        /// <returns>Cars that are currently on the path.</returns>
        public List<Car> GetCars()
        {
            return cars;
        }

        /// <summary>
        /// Add a car on the driving path..
        /// </summary>
        /// <param name="car">Car to add.</param>
        public void AddCar(Car car)
        {
            if (CarEntered != null)
            {
                CarEntered(car);
            }
            this.carsToAdd.Add(car);
        }

        /// <summary>
        /// Remove a car from the driving path.
        /// </summary>
        /// <param name="car">Car to remove.</param>
        public void RemoveCar(Car car)
        {
            this.carsToRemove.Add(car);
        }

        /// <summary>
        /// Spawn a car on the driving path.
        /// </summary>
        /// <returns>True if there was enough space to spawn the car, otherwise false.</returns>
        public bool SpawnCar()
        {
            if (GetLastCar() != null && GetLastCar().GetDistance() < safeDistance)
            {
                return false;
            }
            Car newCar = new Car(this);
            return true;
        }
        
        /// <summary>
        /// Connect the driving path to another driving path
        /// </summary>
        /// <param name="drivingPath">Driving path to connect to</param>
        public void Connect(DrivingPath drivingPath)
        {
            this.getNextDrivingPath = null;
            this.nextDrivingPath = drivingPath;
        }

        /// <summary>
        /// Connect the driving path to a delegate that will return a driving path.
        /// </summary>
        /// <param name="nextDrivingPathRetriever">Delegate that will return a driving path.</param>
        public void Connect(PathRetriever nextDrivingPathRetriever)
        {
            this.nextDrivingPath = null;
            this.getNextDrivingPath = nextDrivingPathRetriever;
        }

        /// <summary>
        /// Disconnect the driving path to any other driving path.
        /// </summary>
        public void Disconnect()
        {
            this.getNextDrivingPath = null;
            this.nextDrivingPath = null;
        }

        /// <summary>
        /// Start for simulation
        /// </summary>
        public void Start()
        {
            Reset();
        }

        /// <summary>
        /// Update the driving path.
        /// </summary>
        /// <param name="deltaTime">Amount of milliseconds passed since last update call.</param>
        public void Update(int deltaTime)
        {
            foreach (Car car in cars)
            {
                car.Update(deltaTime);
            }

            foreach (Car car in carsToRemove)
            {
                cars.Remove(car);
            }

            foreach (Car car in carsToAdd)
            {
                cars.Add(car);
            }

            carsToRemove.Clear();
            carsToAdd.Clear();

            pointsCalculated = false;
        }

        /// <summary>
        /// Reset the driving path.
        /// </summary>
        public void Reset()
        {
            carPoints = new List<PathPoint>();
            cars = new List<Car>();
            carsToAdd = new List<Car>();
            carsToRemove = new List<Car>();
        }

        /// <summary>
        /// Add a point to the driving path.
        /// </summary>
        /// <param name="pathPoint">Point to add.</param>
        private void addPoint(PathPoint pathPoint)
        {
            if (pathPointDistances == null) {
                pathPointDistances = new List<double>();
            }

            if (pathPoints == null)
            {
                pathPoints = new List<PathPoint>();
            }

            pathPoints.Add(pathPoint);
            int addedPointIndex = pathPoints.Count() - 1;
            if (pathPoints.Count() > 1)
            {
                // Point added is not the first point
                double distance = pathPoints[addedPointIndex - 1].DistanceTo(pathPoints[addedPointIndex]) + pathPointDistances[addedPointIndex - 1];
                pathPointDistances.Add(distance);
            }
            else
            {
                // Point added is first point
                pathPointDistances.Add(0);
            }

            return;
        }
    }
}
