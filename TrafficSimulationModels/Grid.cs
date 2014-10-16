using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrafficSimulationModels.Junctions;

namespace TrafficSimulationModels
{
   [Serializable]
    public class Grid
    {
        //---------------------- FIELDS ----------------------------
        // Shared random object used for traffic generation
        private Random random = new Random();

        // The amounts of junction slots horizontally
        private int junctionSlotsX = 4;

        // The amounts of junction slots vertically
        private int junctionSlotsY = 4;

        // Multidimensional array containing the junctions or null values.
        // First index refers to the column, second index refers to the row.
        private Junction[,] junctionSlots;

        //---------------------- CONSTRUCTORS ----------------------
        public Grid()
        {
            junctionSlots = new Junction[junctionSlotsX, junctionSlotsY];

            // Set all junction slots to null
            for (int x = 0; x < junctionSlotsX; x++)
            {
                for (int y = 0; y < junctionSlotsY; y++)
                {
                    junctionSlots[x, y] = null;
                }
            }
        }

        //---------------------- METHODS ---------------------------
       /// <summary>
       /// Add a junction to the grid.
       /// </summary>
       /// <param name="slotPoint">Grid slot location to add the junction</param>
       /// <param name="junctionType">Type of the junction</param>
       /// <returns></returns>
        public bool AddJunction(Point slotPoint, JunctionType junctionType)
        {
            if (!IsFree(slotPoint))
            {
                return false;
            }
            
            Junction[] neighbors = new Junction[4];

            // North
            neighbors[0] = GetJunction(new Point(slotPoint.X, slotPoint.Y - 1));

            // East
            neighbors[1] = GetJunction(new Point(slotPoint.X + 1, slotPoint.Y));
  
            // South
            neighbors[2] = GetJunction(new Point(slotPoint.X, slotPoint.Y + 1));
            
            // West
            neighbors[3] = GetJunction(new Point(slotPoint.X - 1, slotPoint.Y));
            
            Junction junctionToAdd = new Junction(junctionType, neighbors, random);

            for (int a = 0; a < 4; a++)
            {
                if (neighbors[a] != null)
                {
                    neighbors[a].SetNeighbor(getOppositeDirection((Directions)a), junctionToAdd);
                }

            }
            // Add junction to the grid
            this.junctionSlots[slotPoint.X, slotPoint.Y] = junctionToAdd;

            return true;
        }

       /// <summary>
       /// Delete a junction from the grid.
       /// </summary>
       /// <param name="slotPoint">Grid slot location to delete the junction</param>
       /// <returns>True if a junction was deleted, otherwise false.</returns>
        public bool DeleteJunction(Point slotPoint)
        {
            if (IsFree(slotPoint))
            {
                return false;
            }

            // Remove Neighbours
            Junction junction;
            //north
            junction = GetJunction( new Point(slotPoint.X, slotPoint.Y -1));
            if (junction != null)
            {
                    junction.SetNeighbor(Directions.South, null);
            }
            //east
            junction = GetJunction(new Point(slotPoint.X + 1, slotPoint.Y ));
            if (junction != null)
            {
                    junction.SetNeighbor(Directions.West, null);
            }
            //south
            junction = GetJunction(new Point(slotPoint.X, slotPoint.Y + 1));
            if (junction != null)
            {
                    junction.SetNeighbor(Directions.North, null);
            }
            //west
            junction = GetJunction(new Point(slotPoint.X - 1, slotPoint.Y));
            if (junction != null)
            {
                    junction.SetNeighbor(Directions.East, null);
            }

            // Delete the junction
            this.junctionSlots[slotPoint.X, slotPoint.Y] = null;
            return true;
        }

       /// <summary>
       /// Check if a specific slot point contains a junction or not.
       /// </summary>
       /// <param name="slotPoint">Grid slot location to check.</param>
       /// <returns>True if the grid slot location is free, otherwise false.</returns>
        public bool IsFree(Point slotPoint)
        {
            return GetJunction(slotPoint) == null;
        }

       /// <summary>
       /// Get the junction at a specific slot point.
       /// </summary>
       /// <param name="slotPoint">Grid slot location to get the junction from.</param>
       /// <returns>Junction object if the slotpoint contains a junction, otherwise null.</returns>
        public Junction GetJunction(Point slotPoint)
        {
            if(slotPoint.X < 0 || slotPoint.X >= junctionSlotsX || slotPoint.Y < 0 || slotPoint.Y >=junctionSlotsY)
            {
                return null;
            }
            Junction junction = junctionSlots[slotPoint.X, slotPoint.Y];
            return junction;
        }

       /// <summary>
       /// Start the grid
       /// </summary>
        public void Start()
        {
            foreach (Junction junction in junctionSlots)
            {
                if (junction != null)
                {
                    junction.Start();
                }
            }
        }

       /// <summary>
       /// Update the grid
       /// </summary>
       /// <param name="deltaTime">Passed time in milliseconds since last update call</param>
        public void Update(int deltaTime)
        {
            foreach (Junction junction in junctionSlots)
            {
                if (junction != null)
                {
                    junction.Update(deltaTime);
                }
            }
        }
        
        /// <summary>
        /// Reset the grid.
        /// </summary>
        public void Reset()
        {
            foreach (Junction junction in junctionSlots)
            {
                if (junction != null)
                {
                    junction.Reset();
                }
            }
        }
        
        /// <summary>
        /// Get the opposite direction of a specfic direction.
        /// </summary>
        /// <param name="direction">Direction to get the opposite direction of.</param>
        /// <returns>Opposite direction</returns>
        private Directions getOppositeDirection (Directions direction)
        {
            int d = (int)direction;
            int o = (d + 2) % 4;

            return (Directions)o;
        }

        /// <summary>
        /// Get the amount of junctions that are on the grid.
        /// </summary>
        /// <returns>Amount of junctions</returns>
        public int GetAmountOfJunctions()
        {
            int amountOfJunctions = 0;

            // Set all junction slots to null
            for (int x = 0; x < junctionSlotsX; x++)
            {
                for (int y = 0; y < junctionSlotsY; y++)
                {
                    if (junctionSlots[x, y] != null)
                    {
                        amountOfJunctions++;
                    }
                }
            }

            return amountOfJunctions;
        }

        /// <summary>
        /// Get the amount of junction slots in the X-axis
        /// </summary>
        /// <returns></returns>
        public int GetJunctionSlotsX()
        {
            return junctionSlotsX;
        }
        
        /// <summary>
        /// Get the amount of junction slots in the Y-axis
        /// </summary>
        /// <returns></returns>
        public int GetJunctionSlotsY()
        {
            return junctionSlotsY;
        }
    }
}
