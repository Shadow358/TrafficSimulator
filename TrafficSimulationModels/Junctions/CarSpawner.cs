using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrafficSimulationModels.Cars;

namespace TrafficSimulationModels.Junctions
{
    [Serializable]
    public class CarSpawner
    {
        //---------------------- FIELDS ----------------------------
        // Junction direction object that the car spawner is attached to.
        private JunctionDirection junctionDirection;

        // Cars that enqueued for spawning
        private List<DrivingPath> queue;

        // Average time between two cars spawning. An average spawn time of 0 means no cars will spawn at all.
        private int averageSpawnTime;

        // Amount of time untill the next car will spawn
        private int? nextCarSpawn;

        // Random object used for generating next car spawn times
        private Random random;

        // Boolean that indicates whether the car spawner is active or not
        private bool activated;

        //---------------------- GETTERS AND SETTERS ---------------
        public void SetCarsPerMinute(int cars)
        {
            if (cars == 0)
            {
                this.averageSpawnTime = 0;
                return;
            }

            this.averageSpawnTime = Convert.ToInt32(60000 / cars);
        }

        public int GetCarsPerMinute()
        {
            if (this.averageSpawnTime != 0)
                return (60000 / this.averageSpawnTime);
            return 0;
        }

        public int GetQueueLength()
        {
            if (queue == null)
            {
                return 0;
            }

            return queue.Count;
        }

        //---------------------- CONSTRUCTORS ----------------------
        public CarSpawner(JunctionDirection junctionDirection, Random random)
        {
            this.random = random;
            this.junctionDirection = junctionDirection;
            SetCarsPerMinute(30);
        }

        //---------------------- METHODS ---------------------------
        /// <summary>
        /// Deactivate the car spawner.
        /// </summary>
        public void Deactivate()
        {
            this.activated = false;
        }

        /// <summary>
        /// Activate the car spawner.
        /// </summary>
        public void Activate()
        {
            this.activated = true;
        }

        /// <summary>
        /// Check whether the car spawner is active or not.
        /// </summary>
        /// <returns>True if active, otherwise false.</returns>
        public bool IsActivated()
        {
            return this.activated;
        }

        /// <summary>
        /// Start the car spawner
        /// </summary>
        public void Start()
        {
            Reset();
            nextCarSpawn = generateSpawnTime();
        }

        /// <summary>
        /// Update the car spawner.
        /// </summary>
        /// <param name="deltaTime">Amount of milliseconds past since last update call.</param>
        public void Update(int deltaTime)
        {
            if (!activated || averageSpawnTime == 0)
            {
                return;
            }

            nextCarSpawn -= deltaTime;

            while (nextCarSpawn <= 0)
            {
                nextCarSpawn += generateSpawnTime();
                queue.Add(junctionDirection.GetOutputPath());
            }

            if (queue.Count() > 0)
            {
                if (queue[0].SpawnCar())
                {
                    queue.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Reset the car spawner.
        /// </summary>
        public void Reset()
        {
            queue = new List<DrivingPath>();
            nextCarSpawn = null;
        }

        /// <summary>
        /// Generate spawn time according to average spawn time plus a random deviation.
        /// </summary>
        /// <returns>Random spawn time expressed in milliseconds</returns>
        private int generateSpawnTime()
        {
            double deviation = 0.5 + ((double)random.Next(0, 100)) / 100;
            double spawnTimeDouble = deviation * averageSpawnTime;
            int spawnTime = Convert.ToInt32(spawnTimeDouble);
            return spawnTime;
        }
    }
}
