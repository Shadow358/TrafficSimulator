using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficSimulationModels.TrafficLightSystem
{
    public enum TrafficLightState
    {
        red,
        orange,
        green
    }
    [Serializable]
    public class TrafficLight
    {
        //---------------------- FIELDS ----------------------------
        // Amount of milliseconds that the traffic light will remain green
        private int? remainingGreenTime;

        // Boolean indicating whether the traffic light has an orange state or not
        private bool hasOrange;

        /// <summary>
        /// total duration of orange time 
        /// </summary>
        private int orangeTime = 2000;

        // The state of this traffic light (red, orange, green)
        private TrafficLightState trafficLightState;

        // Event that gets fired whenever the traffic light changes its state
        public delegate void StateChangedHandler(TrafficLight trafficLight, TrafficLightState trafficLightState);
        public event StateChangedHandler StateChanged;

        //---------------------- CONSTRUCTORS ----------------------
        public TrafficLight(bool hasOrange)
        {
            this.hasOrange = hasOrange;
        }

        //---------------------- METHODS ---------------------------

        /// <summary>
        /// Sets the green time of the traffic light and changes the state to "green"
        /// </summary>
        /// <param name="greenTime">Total green time for this traffic light</param>
        public void SetGreen(int greenTime)
        {
            remainingGreenTime = greenTime;
            changeTrafficLightState(TrafficLightState.green);
        }

        // Currently not used, but could be used to safely stop current traffic light triggered from outside. For example
        // a pressure plate.
        public void Stop()
        {
            if (remainingGreenTime != null && remainingGreenTime > orangeTime)
            {
                remainingGreenTime = orangeTime;
            }
        }

        /// <summary>
        /// Updates the traffic light state if needed
        /// </summary>
        /// <param name="deltaTime">The time fraction</param>
        public void Update(int deltaTime)
        {
            if (remainingGreenTime == null)
            {
                return;
            }

            remainingGreenTime -= deltaTime;

            if (remainingGreenTime < 0 && trafficLightState != TrafficLightState.red)
            {
                Reset();
            }
            if (hasOrange)
            {
                if (trafficLightState == TrafficLightState.green)
                {
                    if (orangeTime > remainingGreenTime)
                    {
                        changeTrafficLightState(TrafficLightState.orange);
                    }
                }
                if (trafficLightState == TrafficLightState.orange)
                {
                    if (remainingGreenTime < 0)
                    {
                        changeTrafficLightState(TrafficLightState.red);
                    }
                }
                if (trafficLightState == TrafficLightState.red)
                {
                    if (remainingGreenTime > 0)
                    {
                        changeTrafficLightState(TrafficLightState.green);
                    }
                }

            }
            else if (!hasOrange)
            {
                if (trafficLightState == TrafficLightState.green)
                {
                    if (remainingGreenTime < 0)
                    {
                        changeTrafficLightState(TrafficLightState.red);
                    }
                }
                if (trafficLightState == TrafficLightState.red)
                {
                    if (remainingGreenTime > 0)
                    {
                        changeTrafficLightState(TrafficLightState.green);
                    }
                }
            }
        }

        /// <summary>
        /// Resets the traffic light's green time to null and state to red.
        /// </summary>
        public void Reset()
        {
            remainingGreenTime = null;
            changeTrafficLightState(TrafficLightState.red);
        }

        /// <summary>
        /// Changes the traffic light state to the one passed in the method.
        /// </summary>
        /// <param name="trafficLightState">The state that the traffic light will change to.</param>
        private void changeTrafficLightState(TrafficLightState trafficLightState)
        {
            this.trafficLightState = trafficLightState;
            if (StateChanged != null)
            {
                StateChanged(this, trafficLightState);
            }
        }

        /// <summary>
        /// Returns the state of the traffic light
        /// </summary>
        /// <returns>TrafficLightState</returns>
        public TrafficLightState GetTrafficLightState()
        {
            return trafficLightState;
        }
    }
}
