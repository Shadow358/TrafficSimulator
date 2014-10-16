using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TrafficSimulationModels
{
    public class Simulation
    {
        //---------------------- FIELDS ----------------------------
        // The grid that will be simulates
        private Grid grid;

        // The timer that will generate ticks
        private Timer timer;

        // Stop watch used to keep track of actual time passed between each tick event.
        private Stopwatch stopwatch;

        // Speed of the simulation, with 1 being realtime
        private double speed = 1;

        private bool isStarted;

        // Default frame rate
        private int defaultFps = 40;

        // Event that gets fired whenever the grid has been updated. The application must then redraw everything.
        public delegate void UpdateHandler(int deltaTime);
        public UpdateHandler Updated;

        //---------------------- CONSTRUCTORS ----------------------
        /// <summary>
        /// Create a simulation based on an existing grid.
        /// </summary>
        /// <param name="grid">Grid that the simulation will run </param>
        public Simulation(Grid grid)
        {
            this.grid = grid;
            timer = new Timer();
            stopwatch = new Stopwatch();
            isStarted = false;

            // Set timer at default frame rate
            SetFPS(defaultFps);
            timer.Tick += onTick;
            
        }

        //---------------------- METHODS ---------------------------
        /// <summary>
        /// Start the simulation.
        /// </summary>
        public void Start()
        {
            isStarted = true;
            Reset();
            grid.Start();
            stopwatch.Start();
            timer.Start();
        }

        /// <summary>
        /// Continue the simulation when paused.
        /// </summary>
        public void Continue()
        {
            stopwatch.Start();
            timer.Start();
        }

        /// <summary>
        /// Pause the simulation
        /// </summary>
        public void Pause()
        {
            timer.Stop();
            stopwatch.Stop();
        }

        /// <summary>
        /// Stop the simulation
        /// </summary>
        public void Stop()
        {
            isStarted = false;
            Reset();
            timer.Stop();
            grid.Reset();
        }

        /// <summary>
        /// Reset the simulation
        /// </summary>
        private void Reset()
        {
            stopwatch.Stop();
        }

        /// <summary>
        /// Event handler for when the timer ticks.
        /// </summary>
        private void onTick(object sender, EventArgs e)
        {
            int elapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds;
            int deltaTime = Convert.ToInt32(elapsedMilliseconds * speed);
            stopwatch.Restart();
            grid.Update(deltaTime);
            Updated(deltaTime);
        }

        /// <summary>
        /// Set speed of the simulation
        /// </summary>
        /// <param name="speed">Speed of the simulation, with 1 being realtime.</param>
        public void SetSpeed(double speed)
        {
            if (speed > 0)
            {
                this.speed = speed;
            }
        }

        public bool IsStarted()
        {
            return isStarted;
        }

        /// <summary>
        /// Get the speed of the simulation
        /// </summary>
        /// <returns>Speed of the simulation, with 1 being realtime.</returns>
        public double GetSpeed()
        {
            return this.speed;
        }

        /// <summary>
        /// Set the amount of frames per second.
        /// </summary>
        /// <param name="frameRate">Frames per second.</param>
        public void SetFPS(int fps)
        {
            if (fps > 0)
            {
                int frameLength = 1000 / fps;
                this.timer.Interval = frameLength;
            }
        }

        /// <summary>
        /// Get the amount of frames per second
        /// </summary>
        /// <returns>Frames per second.</returns>
        public double GetFrameRate()
        {
            return 1000 / this.timer.Interval;
        }
    }
}
