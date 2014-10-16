using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TrafficSimulationModels.Cars;
using TrafficSimulationModels.TrafficLightSystem;

namespace TrafficSimulationModels.Junctions
{
    [Serializable]
    public class Crossing
    {
        //---------------------- FIELDS ----------------------------
        // Remaining time untill the next pedestrians will spawn
        private int? nextPedestrianSpawn;

        // Remaining offset time
        private int? remainingOffsetTime;

        // The average spawn time between two spawns
        private int averageSpawnTime;

        // Amount of pedestrians that are waiting to cross
        private int pedestriansWaiting;

        // Traffic light associated with the crossing
        private TrafficLight trafficLight;

        // Minimum amount of green time in order to start decreasing the amount of pedestrians
        private int pedestrianDecreaseOffset = 7000;

        // Amount of pedestrians per minute that will traverse after the offset time has been reached
        private int pedestrianDecreaseRate = 200;

        // Time until next pedestrian will spawn
        private int nextPedestrianDespawn;

        // Average amount of pedestrians that will spawn per minute
        private int pedestriansPerMinute;

        // Random object used to generate random spawn times
        private Random random;
        
        // Boolean that indicates whether the crossing is activated or not
        private bool activated;

        //---------------------- CONSTRUCTORS ----------------------
        public Crossing(Random random)
        {
            this.trafficLight = new TrafficLight(false);
            this.trafficLight.StateChanged += trafficLightStateChanged;
            this.random = random;
            SetPedestriansPerMinute(6);
            pedestriansWaiting = 0;
            this.activated = true;
        }

        //---------------------- METHODS ---------------------------
        /// <summary>
        /// Get the amount of pedestrians that are waiting to cross.
        /// </summary>
        /// <returns>Amount of pedestrians</returns>
        public int GetPedestriansWaiting()
        {
            return pedestriansWaiting;
        }

        /// <summary>
        /// Get the traffic light of the crossing.
        /// </summary>
        /// <returns>Traffic light</returns>
        public TrafficLight GetTrafficLight()
        {
            return trafficLight;
        }

        /// <summary>
        /// Set the average amount of pedestrians that will spawn per minute.
        /// </summary>
        /// <param name="pedestrians">Amount of pedestrians</param>
        public void SetPedestriansPerMinute(int pedestrians)
        {
            this.pedestriansPerMinute = pedestrians;
            if (pedestrians != 0)
                this.averageSpawnTime = Convert.ToInt32(60000 / pedestrians);
            else
            {
                this.averageSpawnTime = 0;
                this.activated = false;
            }
        }

        /// <summary>
        /// Get the amount of pedestrians that will spawn per minute
        /// </summary>
        /// <returns></returns>
        public int GetPedestriansPerMinute()
        {
            return pedestriansPerMinute;
        }

        /// <summary>
        /// Start the crossing
        /// </summary>
        public void Start()
        {
            this.nextPedestrianSpawn = generatePedestrianSpawnTime();
            this.nextPedestrianDespawn = pedestrianDecreaseRate;
        }

        /// <summary>
        /// Update the crossing
        /// </summary>
        /// <param name="deltaTime">Milliseconds that have passed since last update call</param>
        public void Update(int deltaTime)
        {
            if (!activated)
                return;
            // Spawning pedestrians
            nextPedestrianSpawn -= deltaTime;

            if (nextPedestrianSpawn <= 0)
            {
                nextPedestrianSpawn += generatePedestrianSpawnTime();
                pedestriansWaiting++;
            }

            // Crossing pedestrians
            if (trafficLight.GetTrafficLightState() == TrafficLightState.green)
            {
                // Wait a few seconds for the first pedestrian to cross
                if (remainingOffsetTime != null)
                {
                    remainingOffsetTime -= deltaTime;
                    if (remainingOffsetTime <= 0)
                    {
                        nextPedestrianDespawn = pedestrianDecreaseRate;
                        remainingOffsetTime = null;
                    }
                }
                // Start decreasing pedestrians
                else
                {
                    nextPedestrianDespawn -= deltaTime;

                    if (nextPedestrianDespawn <= 0)
                    {
                        while (nextPedestrianDespawn < 0 && pedestriansWaiting > 0)
                        {
                            nextPedestrianDespawn += pedestrianDecreaseRate;
                            pedestriansWaiting--;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reset the crossing
        /// </summary>
        public void Reset()
        {
            pedestriansWaiting = 0;
        }

        /// <summary>
        /// Handler for when the traffic light changes its state.
        /// </summary>
        /// <param name="trafficLight">Traffic light</param>
        /// <param name="trafficLightState">Traffic light state</param>
        void trafficLightStateChanged(TrafficLight trafficLight, TrafficLightState trafficLightState)
        {
            if (trafficLightState == TrafficLightState.green)
            {
                remainingOffsetTime = pedestrianDecreaseOffset;
            }
        }

        /// <summary>
        /// Generate pedestrian spawn time.
        /// </summary>
        /// <returns>Spawn time expressed in milliseconds.</returns>
        private int generatePedestrianSpawnTime()
        {
            return Convert.ToInt32(Math.Round((0.5+((double)random.Next(0, 100)) / 100) * averageSpawnTime));
        }
    }
}
