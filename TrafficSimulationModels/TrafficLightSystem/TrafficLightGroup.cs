using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficSimulationModels.TrafficLightSystem
{
    [Serializable]
    public class TrafficLightGroup
    {
        //---------------------- FIELDS ----------------------------
        // Total green time of the traffic light group expressed in milliseconds
        private int totalTime;

        // Traffic lights within the traffic light group
        private List<TrafficLight> trafficLights;

        // Amount of traffic lights that turned red again
        private int trafficLightsDone;
        
        // Event that gets fires when all the traffic lights in the group turned red again
        public delegate void DoneHandler();
        public event DoneHandler Done;

        // Name of the traffic light group
        private string name;

        //---------------------- CONSTRUCTORS ----------------------
        public TrafficLightGroup(List<TrafficLight> trafficLights, string name)
        {
            this.trafficLights = trafficLights;
            this.name = name;
            trafficLightsDone = 0;
        }

        //---------------------- METHODS ---------------------------
        
        /// <summary>
        /// Sets the green time for each traffic light of the group.
        /// </summary>
        public void Start()
        {
            reset(false);
            foreach (TrafficLight trafficLight in trafficLights)
            {
                trafficLight.StateChanged += OnTrafficLightStateChanged;
                trafficLight.SetGreen(totalTime);
            }
        }

        /// <summary>
        /// Adds a traffic light to the group.
        /// </summary>
        /// <param name="trafficLight">The traffic light to be added.</param>
        public void AddTrafficLight(TrafficLight trafficLight)
        {
            trafficLights.Add(trafficLight);
        }

        /// <summary>
        /// Counts down the time of each traffic light and updates its state.
        /// </summary>
        /// <param name="deltaTime">Time reduced from the green time of the traffic lights.</param>
        public void Update(int deltaTime)
        {
            foreach (TrafficLight trafficLight in trafficLights)
            {
                trafficLight.Update(deltaTime);
            }
        }


        /// <summary>
        /// Resets the group's traffic lights green time to null and state to red.
        /// </summary>
        public void Reset()
        {
            reset(true);
        }

        /// <summary>
        /// Checks all the traffic lights of the group and if all of them are done it reserts them.
        /// </summary>
        /// <param name="trafficLight">The traffic light that changed its state.</param>
        /// <param name="trafficLightState">The current state of the traffic lights.</param>
        public void OnTrafficLightStateChanged(TrafficLight trafficLight, TrafficLightState trafficLightState)
        {
            if (trafficLightState == TrafficLightState.red)
            {
                trafficLight.StateChanged -= OnTrafficLightStateChanged;
                trafficLightsDone++;

                if (trafficLightsDone == trafficLights.Count())
                {
                    Reset();
                    if (Done != null)
                    {
                        Done();
                    }
                }
            }
        }

        /// <summary>
        /// Resets the <param name="trafficLightsDone">TrafficLightsDone</param> and all the traffic lights of this group.
        /// </summary>
        /// <param name="recursive">If true resets all the traffic lights of the group.</param>
        private void reset(bool recursive)
        {
            trafficLightsDone = 0;

            if (recursive)
            {
                foreach (TrafficLight trafficLight in trafficLights)
                {
                    trafficLight.Reset();
                }
            }
        }

        /// <summary>
        /// Set the total time of the traffic light group.
        /// </summary>
        /// <param name="time">Total time expressed in milliseconds</param>
        public void SetTotalTime(int time)
        {
            this.totalTime = time;
        }

        /// <summary>
        /// Get the traffic lights in the group
        /// </summary>
        /// <returns>Traffic lights</returns>
        public List<TrafficLight> GetTrafficLights()
        {
            return trafficLights;
        }

        /// <summary>
        /// Get the name of the group
        /// </summary>
        /// <returns>Name</returns>
        public string GetName()
        {
            return this.name;

        }

        /// <summary>
        /// Get the total time of the traffic light group
        /// </summary>
        /// <returns>Total time</returns>
        public int GetTotalTime()
        {
            return this.totalTime;
        }
    }
}
