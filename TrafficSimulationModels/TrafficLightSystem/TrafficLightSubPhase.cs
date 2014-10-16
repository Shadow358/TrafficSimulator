using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficSimulationModels.TrafficLightSystem
{
    [Serializable]
    public class TrafficLightSubPhase
    {
        //---------------------- FIELDS ----------------------------
        // Total time that the traffic light subphase will take
        private int totalTime;

        // Traffic light group that is currently active
        private int? currentTrafficLightGroup;

        // List of traffic light groups that will turn green one by one
        private List<TrafficLightGroup> trafficLightGroups;

        // Distribution percentages of the total time over each group
        private List<int> trafficLightGroupPercentages;

        // Name of the subphase
        private string name;

        // Event that gets fired whenever the traffic light subphase is done
        public delegate void DoneHandler();
        public event DoneHandler Done;

        //---------------------- CONSTRUCTORS ----------------------
        public TrafficLightSubPhase(List<TrafficLightGroup> trafficLightGroups, int totalTime, string name)
        {
            this.name = name;
            SetTrafficLightGroups(trafficLightGroups);
        }

        //---------------------- METHODS ---------------------------
        
        /// <summary>
        /// Starts the timer for the groups of this subphase.
        /// </summary>
        public void Start()
        {
            reset(false);

            if(trafficLightGroups.Count == 0) {
                throw new Exception("Traffic light subphase contains no traffic light groups");
            }

            trafficLightGroups[0].Done += trafficLightGroupDone;
            trafficLightGroups[0].Start();
        }

        /// <summary>
        /// Sets and unsets the events thrown for each traffic light group.
        /// </summary>
        public void trafficLightGroupDone()
        {
            if (currentTrafficLightGroup == null)
            {
                throw new Exception("Current group can not be null if traffic light group is still active.");
            }

            trafficLightGroups[(int)currentTrafficLightGroup].Done -= trafficLightGroupDone;
            currentTrafficLightGroup++;

            if (currentTrafficLightGroup >= trafficLightGroups.Count)
            {
                reset(false);
                if (Done != null)
                {
                    Done();
                }
                return;
            }

            trafficLightGroups[(int)currentTrafficLightGroup].Done += trafficLightGroupDone;
            trafficLightGroups[(int)currentTrafficLightGroup].Start();
        }

        /// <summary>
        /// Counts down the time of each traffic light group.
        /// </summary>
        /// <param name="deltaTime">Time reduced from the green time of the traffic lights.</param>
        public void Update(int deltaTime)
        {
            foreach (TrafficLightGroup trafficLightGroup in trafficLightGroups)
            {
                trafficLightGroup.Update(deltaTime);
            }
        }

        /// <summary>
        /// Resets this subphase.
        /// </summary>
        public void Reset()
        {
            reset(true);
        }

        /// <summary>
        /// Resets completely or not each group of this subphase.
        /// </summary>
        /// <param name="recursive">If yes resets the group completely.</param>
        private void reset(bool recursive)
        {
            currentTrafficLightGroup = 0;
            foreach (TrafficLightGroup trafficLightGroup in trafficLightGroups)
            {
                trafficLightGroup.Done -= trafficLightGroupDone;
                if (recursive)
                {
                    trafficLightGroup.Reset();
                }
            }
        }

        /// <summary>
        /// Sets the green time of traffic light groups according to percentages list.
        /// </summary>
        private void setSubPhaseGreenTimes()
        {
            int currentPhase = 0;
            foreach (int percentage in trafficLightGroupPercentages)
            {
                int time;
                if (percentage == 0)
                {
                    time = 0;
                }
                else
                {
                    time = Convert.ToInt32((double)(percentage / (double)100) * totalTime);
                }
                trafficLightGroups[currentPhase].SetTotalTime(time);
                currentPhase++;
            }
        }

        /// <summary>
        /// Sets the total time of this subphase and the corresponding groups.
        /// </summary>
        /// <param name="time">The total time of the subphase.</param>
        public void SetTotalTime(int time)
        {
            this.totalTime = time;
            setSubPhaseGreenTimes();
        }

        public string GetName()
        {
            return name;
        }

        /// <summary>
        /// Adds a percentage to percentages list.
        /// </summary>
        /// <param name="percentage">The percentage integer.</param>
        public void AddPercentage(int percentage)
        {
            this.trafficLightGroupPercentages.Add(percentage);
        }

        /// <summary>
        /// Adds a percentage to percentages list in the specified index.
        /// </summary>
        /// <param name="index">The index of the percentage to be added.</param>
        /// <param name="percentage">The percentage integer.</param>
        public void AddPercentage(int index, int percentage)
        {
            this.trafficLightGroupPercentages.Insert(index, percentage);
        }


        /// <summary>
        /// Removes a percentage from percentages list.
        /// </summary>
        /// <param name="index">The index of the removed percentage.</param>
        public void RemovePercentage(int index)
        {
            this.trafficLightGroupPercentages.RemoveAt(index);
        }

        /// <summary>
        /// Sets the times of the groups of this subphase according to percentages list.
        /// </summary>
        /// <returns>True: all percentages assigned. False: trafficLightGroups list and percentages list have different count.</returns>
        public bool SetGroupsTime()
        {
            if (this.trafficLightGroups.Count != this.trafficLightGroupPercentages.Count)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < trafficLightGroupPercentages.Count; i++)
                {
                    trafficLightGroups[i].SetTotalTime(Convert.ToInt32(trafficLightGroupPercentages[i] * totalTime / 100));
                }
                return true;
            }
        }

        /// <summary>
        /// Gets the traffic light groups of the subphase.
        /// </summary>
        /// <returns>The list of traffic light groups.</returns>
        public List<TrafficLightGroup> GetTrafficLightGroups()
        {
            return trafficLightGroups;
        }

        /// <summary>
        /// Sets all the subphase time in the first group of traffic lights.
        /// </summary>
        public void SetTrafficLightGroupPercentages()
        {
            trafficLightGroupPercentages = new List<int>();
            for (int i = 0; i < trafficLightGroups.Count(); i++)
            {
                if (i == 0)
                {
                    trafficLightGroupPercentages.Add(100);
                }
                else
                {
                    trafficLightGroupPercentages.Add(0);
                }
            }
        }

        /// <summary>
        /// Sets a percentage to percentages list in the specified index.
        /// </summary>
        /// <param name="index">The index of the percentage to be added</param>
        /// <param name="percentage">The percentage integer.</param>
        /// <returns>True: OK, False: assignment failed.</returns>
        public bool SetTrafficLightGroupPercentage(int index, int percentage)
        {
            if (trafficLightGroupPercentages == null)
            {
                SetTrafficLightGroupPercentages();
            }

            // Check valid index, last index is not valid as it does not have a next phase to compensate percentages with.
            if (index < 0 || index >= trafficLightGroups.Count() - 1)
            {
                return false;
            }

            if (percentage < 0 || percentage > 100)
            {
                return false;
            }

            int gap = trafficLightGroupPercentages[index] - percentage;
            if (trafficLightGroupPercentages[index + 1] + gap < 0 || trafficLightGroupPercentages[index + 1] + gap > 100)
            {
                return false;
            }
            trafficLightGroupPercentages[index] = percentage;
            trafficLightGroupPercentages[index + 1] += gap;
            setSubPhaseGreenTimes();
            return true;
        }

        /// <summary>
        /// Gets the traffic light groups percentages of the subphase.
        /// </summary>
        /// <returns>The list of traffic light groups percentages.</returns>
        public List<int> GetTrafficLightGroupPercentages()
        {
            return this.trafficLightGroupPercentages;
        }

        /// <summary>
        /// Set traffic light groups
        /// </summary>
        /// <param name="trafficLightGroups">List of traffic light groups</param>
        public void SetTrafficLightGroups(List<TrafficLightGroup> trafficLightGroups)
        {
            this.trafficLightGroups = trafficLightGroups;
            SetTrafficLightGroupPercentages();
        }

        /// <summary>
        /// Adds a traffic light group to this subphase.
        /// </summary>
        /// <param name="trafficLightGroup">The traffic light group to be added.</param>
        public void AddTrafficLightGroup(TrafficLightGroup trafficLightGroup)
        {
            trafficLightGroupPercentages.Add(0);
            trafficLightGroups.Add(trafficLightGroup);
        }

        /// <summary>
        /// Removes a traffic light group from the list.
        /// </summary>
        /// <param name="index">The index of the group to be removed.</param>
        public void RemoveTrafficLightGroup(int index)
        {
            if (trafficLightGroups.Count() > 2)
            {
                throw new NotImplementedException("Removing traffic light group from sub phase with more than two traffic light groups is not (yet) supported.");
            }

            if (trafficLightGroups.Count == 1)
            {
                trafficLightGroups = new List<TrafficLightGroup>();
                trafficLightGroupPercentages = new List<int>();
                return;
            }

            trafficLightGroups.RemoveAt(index);
            trafficLightGroupPercentages.RemoveAt(index);
            trafficLightGroupPercentages[0] = 100;
        }
    }
}
