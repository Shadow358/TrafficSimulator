using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrafficSimulationModels.Cars;
using TrafficSimulationModels.TrafficLightSystem;

namespace TrafficSimulationModels.Junctions
{
    public enum Lanes {
        Left = 0,
        Middle = 1,
        Right = 2,
        Crossing = 3
    }
    [Serializable]
    public class JunctionDirection
    {
        //---------------------- FIELDS ----------------------------
        // Distribution rates for each lane
        private int[] distributionRates;

        // Output paths of the junction direction
        private DrivingPath[] outputPaths;

        // Intermediate path before the junction itself
        private DrivingPath inputPath;

        // Car spawner attached to this direction
        private CarSpawner carSpawner;

        // Direction of the junction direction
        private Directions direction;

        // Crossing attached to this direction
        private Crossing crossing;

        // Shared random object used by the crossing and car spawner
        private Random random;

        //---------------------- CONSTRUCTORS ----------------------
        public JunctionDirection(Directions direction, DrivingPath[] outputPaths, DrivingPath inputPath, Random random)
        {
            this.outputPaths = outputPaths;
            this.inputPath = inputPath;
            this.inputPath.Connect(GetOutputPath);
            this.direction = direction;
            this.distributionRates = new int[3] {25, 50, 25 };
            this.carSpawner = new CarSpawner(this, random);
            this.random = random;
        }

        //---------------------- METHODS ---------------------------
        /// <summary>
        /// Get a random driving path according to distribution rates.
        /// </summary>
        /// <returns>Driving path</returns>
        public DrivingPath GetOutputPath()
        {
            int seed = random.Next(1,101);

            if (seed <= distributionRates[(int)Lanes.Left])
            {
                return outputPaths[(int)Lanes.Left];
            }
            else if (seed <= distributionRates[(int)Lanes.Left] + distributionRates[(int)Lanes.Middle])
            {
                return outputPaths[(int)Lanes.Middle];
            }
            else
            {
                return outputPaths[(int)Lanes.Right];
            }
        }

        /// <summary>
        /// Add a crossing to this direction
        /// </summary>
        public void AddCrossing()
        {
            this.crossing = new Crossing(random);
        }

        /// <summary>
        /// Remove the crossing of this direction
        /// </summary>
        public void RemoveCrossing()
        {
            this.crossing = null;
        }

        /// <summary>
        /// Start the junction direction
        /// </summary>
        public void Start()
        {
            if (inputPath != null)
            {
                inputPath.Start();
            }

            foreach (DrivingPath drivingPath in outputPaths)
            {
                drivingPath.Start();
            }

            if (crossing != null)
            {
                crossing.Start();
            }

            carSpawner.Start();
        }

        /// <summary>
        /// Update the junction direction
        /// </summary>
        /// <param name="deltaTime">Milliseconds past since last update call</param>
        public void Update(int deltaTime)
        {
            if (inputPath != null)
            {
                inputPath.Update(deltaTime);
            }
            foreach (DrivingPath drivingPath in outputPaths)
            {
                drivingPath.Update(deltaTime);
            }

            if (crossing != null)
            {
                crossing.Update(deltaTime);
            }

            carSpawner.Update(deltaTime);
        }

        /// <summary>
        /// Reset the junction direction
        /// </summary>
        public void Reset()
        {
            if (inputPath != null)
            {
                inputPath.Reset();
            }

            foreach (DrivingPath drivingPath in outputPaths)
            {
                drivingPath.Reset();
            }

            if (crossing != null)
            {
                crossing.Reset();
            }

            carSpawner.Reset();
        }

        /// <summary>
        /// Convert the junction direction to an edge direction
        /// </summary>
        public void ConvertToEdgeJunctionDirection() 
        {
            carSpawner.Activate();
        }

        /// <summary>
        /// Convert the junction direction to a non edge direction
        /// </summary>
        public void ConvertToNonEdgeJunctionDirection() 
        {
            carSpawner.Deactivate();
        }

        /// <summary>
        /// Check whether the junction direction is an edge direction or not
        /// </summary>
        /// <returns>True if junction direction is an edge direction, otherwise false.</returns>
        public bool IsEdgeDirection()
        {
            return carSpawner.IsActivated();
        }

        /// <summary>
        /// Get the car spawner attached to the junction direction.
        /// </summary>
        /// <returns>Car spawner</returns>
        public CarSpawner GetCarSpawner()
        {
            return carSpawner;
        }

        /// <summary>
        /// Get the input path
        /// </summary>
        /// <returns>Input path</returns>
        public DrivingPath GetInputPath()
        {
            return inputPath;
        }

        /// <summary>
        /// Get the crossing attached to this junction direction
        /// </summary>
        /// <returns>Crossing</returns>
        public Crossing GetCrossing()
        {
            return crossing;
        }

        /// <summary>
        /// Set the distribution rate of a particular lane
        /// </summary>
        /// <param name="lane">Lane to set the percentage</param>
        /// <param name="percentage">Percentage</param>
        /// <returns>Whether the percentage could be set or not</returns>
        public bool SetDistribution(Lanes lane, int percentage)
        {
            if (percentage < 0 || percentage > 100)
            {
                return false;
            }

            if (lane == Lanes.Middle)
            {
                throw new Exception("Can not change middle lane percentage");
            }

            int percentageExceeded = percentage - distributionRates[(int)lane];



            distributionRates[(int)lane] = percentage;


            distributionRates[1] -= percentageExceeded;

            percentageExceeded = 0;
            if (distributionRates[1] < 0)
            {
                percentageExceeded = -distributionRates[1];
                distributionRates[1] = 0;
            }

            Lanes oppositeDirection = Lanes.Right;
            if (lane == Lanes.Right)
            {
                oppositeDirection = Lanes.Left;
            }

            distributionRates[(int)oppositeDirection] -= percentageExceeded;

            // Verify sum property
            if (distributionRates.Sum() != 100)
            {
                throw new Exception("Sum of distribution rates is not 100." + distributionRates.Sum());
            }

            return true;
        }

        /// <summary>
        /// Get the distribution rate of a particular lane
        /// </summary>
        /// <param name="lane">Lane to get the percentage</param>
        /// <returns>Distribution rate</returns>
        public int GetDistribution(Lanes lane)
        {
            return distributionRates[(int)lane];
        }

        /// <summary>
        /// Get the traffic light of a particular lane
        /// </summary>
        /// <param name="lane">Lane to get the traffic light from</param>
        /// <returns>Traffic light</returns>
        public TrafficLight GetTrafficLight(Lanes lane)
        {
            return outputPaths[(int)lane].GetTrafficLight();
        }

        /// <summary>
        /// Get the output path
        /// </summary>
        /// <param name="lane">Lane of the output path</param>
        /// <returns>Driving path</returns>
        public DrivingPath GetOutputPath(Lanes lane)
        {
            return outputPaths[(int)lane];
        }
    }
}
