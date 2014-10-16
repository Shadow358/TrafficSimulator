using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficSimulationModels.TrafficLightSystem
{
    [Serializable]
    public class TrafficLightPhase
    {
        //---------------------- FIELDS ----------------------------
        // Total time of the traffic light phase
        private int totalTime;

        // Subphases within the traffic light phase
        private List<TrafficLightSubPhase> subPhases;

        // Amount of subphases that are done
        private int doneSubPhases;

        // Name of the traffic light phase
        private string name;

        // Abbreviation of the name of the traffic light phase
        private string nameAbbreviation;

        //  Event handler that gets fired whenever the traffic light phase is done
        public delegate void DoneHandler();
        public event DoneHandler Done;

        //---------------------- CONSTRUCTORS ----------------------
        public TrafficLightPhase(List<TrafficLightSubPhase> subPhases, int totalTime, string name, string nameAbbreviation)
        {
            this.subPhases = subPhases;
            this.name = name;
            this.nameAbbreviation = nameAbbreviation;
            SetTotalTime(totalTime);
        }

        //---------------------- METHODS ---------------------------
        
        /// <summary>
        /// Sets the event for subphase completition and starts them.
        /// </summary>
        public void Start()
        {
            reset(false);
            foreach (TrafficLightSubPhase subPhase in subPhases)
            {                
                subPhase.Done += onSubPhaseDone;
                subPhase.Start();
            }
        }

        /// <summary>
        /// Counts down the time of each subphase.
        /// </summary>
        /// <param name="deltaTime">Time reduced from the green time of the traffic lights.</param>
        public void Update(int deltaTime)
        {
            foreach (TrafficLightSubPhase subPhase in subPhases)
            {
                subPhase.Update(deltaTime);
            }
        }

        /// <summary>
        /// Restes this phase.
        /// </summary>
        public void Reset()
        {
            reset(true);
        }

        /// <summary>
        /// Resets completely or not each subphase of this phase.
        /// </summary>
        /// <param name="recursive">If yes resets the group completely.</param>
        private void reset(bool recursive)
        {
            this.doneSubPhases = 0;

            foreach (TrafficLightSubPhase subPhase in subPhases)
            {
                subPhase.Done -= onSubPhaseDone;

                if (recursive)
                {
                    subPhase.Reset();
                }
            }
        }

        /// <summary>
        /// Sets event thrown for each traffic light subphase.
        /// </summary>
        private void onSubPhaseDone()
        {
            doneSubPhases++;
            if (doneSubPhases == subPhases.Count())
            {
                reset(false);
                Done();
            }
        }

        /// <summary>
        /// Sets the time for this phase and its subphases.
        /// </summary>
        /// <param name="totalTime">The time of the phase.</param>
        public void SetTotalTime(int totalTime)
        {
            totalTime = totalTime * 1000;
            this.totalTime = totalTime;
            foreach (TrafficLightSubPhase sph in subPhases)
            {
                sph.SetTotalTime(totalTime);
            }
        }

        /// <summary>
        /// Get total time of the phase
        /// </summary>
        /// <returns></returns>
        public int GetTotalTime()
        {
            return totalTime / 1000;
        }

        /// <summary>
        /// Get the name of the phase
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return name;
        }

        /// <summary>
        /// Get the abbreviation
        /// </summary>
        /// <returns></returns>
        public string GetNameAbbreviation()
        {
            return nameAbbreviation;
        }

        /// <summary>
        /// Gets the subphases of this phase.
        /// </summary>
        /// <returns>The list of subphases.</returns>
        public List<TrafficLightSubPhase> GetTrafficLightSubPhases()
        {
            return this.subPhases;
        }
    }
}
