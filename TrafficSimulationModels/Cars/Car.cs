using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrafficSimulationModels.TrafficLightSystem;

namespace TrafficSimulationModels.Cars
{
    public enum CarAction
    {
        WaitingForTrafficLight,
        Driving
    }
    [Serializable]
    public class Car
    {
        //---------------------- FIELDS ----------------------------
        // Current driving path that the car is driving on
        private DrivingPath drivingPath;

        // Distance on the path expressed in centimeters
        private double distance;

        // Next path that the car will take
        private DrivingPath nextPath;

        // Currently in front
        private Car carInFront;

        // Last car on the next path
        private Car carNextPath;

        // Next traffic light that the car will pass
        private TrafficLight nextTrafficLight;

        // Distance towards the next traffic light
        private double? trafficLightDistance;

        // Acceleration of the car expressed in meters per second per second
        private double acceleration = 2.30;

        // Deceleration of the car expressed in meters per second per second
        private double deceleration = 3.00;

        // Current speed expressed in meters per second
        private double speed; // m/s

        // Max speed that a car will drive expressed in meters per seconds
        private double maxSpeed = 13.89;

        // Event that will be fired whenever the car leaves the path
        public delegate void CarLeaveHandler(Car car);
        public event CarLeaveHandler LeftPath;

        // Delegate for an action that a car can perform: Gas and Brake
        private delegate void Action(int deltaTime);


        //---------------------- CONSTRUCTORS ----------------------
        public Car(DrivingPath drivingPath)
        {
            enterPath(drivingPath, 0);
        }

        //---------------------- METHODS ---------------------------
        /// <summary>
        /// Update the car.
        /// </summary>
        /// <param name="deltaTime">Amount of time passed since last update call expressed in ms</param>
        public void Update(int deltaTime)
        {
            // Check for passing a traffic light
            if (trafficLightPassed())
            {
                setNextTrafficLight();
            }

            // Perform calculated action
            Action action = calculateAction();
            if (action != null)
            {
                action(deltaTime);
            }

            // Check for path leave
            if (distance > drivingPath.GetPathLength())
            {
                DrivingPath previousDrivingPath = this.drivingPath;
                leavePath();
                if (nextPath != null)
                {
                    enterPath(nextPath, distance - previousDrivingPath.GetPathLength());
                }
            }
        }

        /// <summary>
        /// Handler for when the car in front leaves the path. 
        /// </summary>
        /// <param name="car">Car that left the path</param>
        private void carInFrontLeave(Car car)
        {
            carInFront.LeftPath -= carInFrontLeave;
            carInFront = null;
        }

        /// <summary>
        /// Handler for when a car enters the next path.
        /// </summary>
        /// <param name="car">Car that has entered the path</param>
        private void onCarNextPathEnter(Car car)
        {
            carNextPath = car;
            carNextPath.LeftPath += carNextPath_LeftPath;
        }

        /// <summary>
        /// Enter a path.
        /// </summary>
        /// <param name="drivingPath">Driving path to enter</param>
        /// <param name="distance">Initial distance of the car on the path</param>
        private void enterPath(DrivingPath drivingPath, double distance)
        {
            this.distance = distance;
            this.drivingPath = drivingPath;
            this.nextPath = drivingPath.GetNextDrivingPath();
            this.drivingPath.AddCar(this);
            carInFront = drivingPath.GetCarInFront(this);

            if (carInFront != null)
            {
                carInFront.LeftPath += carInFrontLeave;
            }

            if (nextPath != null)
            {
                carNextPath = nextPath.GetLastCar();
                if (carNextPath != null)
                {
                    carNextPath.LeftPath += carNextPath_LeftPath;
                }
                nextPath.CarEntered += onCarNextPathEnter;
            }

            setNextTrafficLight();
        }

        /// <summary>
        /// Handler for when the car on the next path leaves its path.
        /// </summary>
        /// <param name="car">Car that has left the path.</param>
        void carNextPath_LeftPath(Car car)
        {
            car.LeftPath -= carNextPath_LeftPath;

            if (carNextPath == car)
            {
                carNextPath = null;
            }
        }

        /// <summary>
        /// Leave current path.
        /// </summary>
        private void leavePath()
        {
            drivingPath.RemoveCar(this);
            this.drivingPath = null;
            this.carInFront = null;
            if (nextPath != null)
            {
                nextPath.CarEntered -= onCarNextPathEnter;
            }
            if (LeftPath != null)
            {
                LeftPath(this);
            }
        }

        /// <summary>
        /// Calculate whether the car should brake or gas, taking other cars and traffic lights in account.
        /// </summary>
        /// <returns>Action that the car should perform next</returns>
        private Action calculateAction()
        {
            // Factors:
            // - keep distance from car in front
            // - keep distance from car on next path
            // - brake for a red traffic light if possible
            // - avoid entering the unsafe area of the junction if there is not enough space

            List<double> thingsToStopFor = new List<double>();

            // Car in front
            if (carInFront != null)
            {
                thingsToStopFor.Add(carInFront.GetDistance() + carInFront.GetBrakeDistance());
            }

            // Car on the next path
            if (carNextPath != null)
            {
                double pathLength = drivingPath.GetPathLength();
                double carDistance = carNextPath.GetDistance();
                double brakeDistance = carNextPath.GetBrakeDistance();

                thingsToStopFor.Add(drivingPath.GetPathLength() + carNextPath.GetDistance() + carNextPath.GetBrakeDistance());
            }

            // Red or orange traffic light
            if (trafficLightDistance != null)
            {
                if (nextTrafficLight.GetTrafficLightState() != TrafficLightState.green && canBrakeFor((double)trafficLightDistance))
                {
                    thingsToStopFor.Add((double)trafficLightDistance);
                } 
                else if (nextPath != null)
                {
                    // Check for enough space on next path
                    int carsBeforeThis = drivingPath.GetCarsBefore(this);
                    int carsOnNextPath = nextPath.GetCars().Count();
                    int capacity = nextPath.GetCapacity();

                    if (carsBeforeThis + carsOnNextPath > capacity)
                    {
                        thingsToStopFor.Add((double)trafficLightDistance);
                    }
                }
            }

            // Return Brake if the car is about to collide with any object, else return Gas. 
            foreach (double distance in thingsToStopFor)
            {
                if (isCloseToBrakingDistance(distance))
                {
                    return Brake;
                }
            }

            return Gas;
        }

        /// <summary>
        /// Put the gas pedal down for a given amount of time.
        /// </summary>
        /// <param name="time">Time to put the gas pedal down expressed in milliseconds.</param>
        private void Gas(int time)
        {
            double timeSeconds = (double)time / 1000;
            speed += (double)acceleration * timeSeconds;
            if (speed > maxSpeed)
            {
                speed = maxSpeed;
            }
            distance += Convert.ToInt32((double)(speed/10) * time);
        }

        /// <summary>
        /// Put the brake pedal down for a given amount of time.
        /// </summary>
        /// <param name="time">Time to put the gaspedal down expressed in milliseconds</param>
        private void Brake(int time)
        {
            double timeSeconds = (double)time / 1000;
            speed -= (double)deceleration * timeSeconds;
            if (speed < 0)
            {
                speed = 0;
            }

            distance += Convert.ToInt32((double)(speed / 10) * time);
        }

        /// <summary>
        /// Calculate the distance between this and the next car.
        /// </summary>
        /// <param name="car">Car to calculate the distance to.</param>
        /// <returns>Distance expressed in centimeters.</returns>
        private double distanceTo(Car car)
        {
            return car.GetDistance() - distance;
        }

        /// <summary>
        /// Calculate the distance to a given distance.
        /// </summary>
        /// <param name="distance">Distance to calculate distance to.</param>
        /// <returns></returns>
        private double distanceTo(double distance)
        {
            return distance - this.distance;
        }

        /// <summary>
        /// Retrieve and set the next traffic light.
        /// </summary>
        private void setNextTrafficLight()
        {
            if (distance < drivingPath.GetTrafficLightDistance())
            {
                nextTrafficLight = drivingPath.GetTrafficLight();
            }
            else if(nextPath != null && nextPath.GetTrafficLight() != null)
            {
                nextTrafficLight = nextPath.GetTrafficLight();
            }
            else
            {
                nextTrafficLight = null;
            }

            trafficLightDistance = calculateDistanceToNextTrafficLight();
        }

        /// <summary>
        /// Calculate the distance to the next traffic light.
        /// </summary>
        /// <returns>Distance toward the next traffic light.</returns>
        private double? calculateDistanceToNextTrafficLight()
        {
            if (nextTrafficLight == null)
            {
                return null;
            }

            if (nextTrafficLight == drivingPath.GetTrafficLight())
            {
                // Traffic light is on this path
                return drivingPath.GetTrafficLightDistance() - distance;
            }

            // Traffic light is on this path
            return drivingPath.GetPathLength() - distance + nextPath.GetTrafficLightDistance();
        }

        /// <summary>
        /// Check if the car has passed the current traffic light.
        /// </summary>
        /// <returns>True if the car passed the currect traffic light, otherwise false.</returns>
        private bool trafficLightPassed()
        {
            if (trafficLightDistance != null && distanceTo((double)trafficLightDistance) <= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the car is close to the braking distance.
        /// </summary>
        /// <param name="distance">Distance expressed in meters</param>
        /// <returns>True if the car is close to braking speed, otherwise false.</returns>
        private bool isCloseToBrakingDistance(double distance)
        {
            double brakeDistance = GetBrakeDistance();
            return this.distance + brakeDistance + getSafetyMargin() > distance;
        }

        /// <summary>
        /// Check if the car can brake for a specific distance.
        /// </summary>
        /// <param name="distance">Distance to check for</param>
        /// <returns>True if the car can brake for the given distance, otherwise false.</returns>
        private bool canBrakeFor(double distance)
        {
            double brakeDistance = GetBrakeDistance();
            return this.distance + brakeDistance <= distance + getSafetyMargin();
        }

        /// <summary>
        /// Calculate the distance from the car untill the end of the path.
        /// </summary>
        /// <returns>Remaing path distance expressed in centimeters</returns>
        private double remainingPathLength()
        {
            return drivingPath.GetPathLength() - distance;
        }

        /// <summary>
        /// Calculate the distance that the car would still travel if it would start braking at this instance.
        /// </summary>
        /// <returns>Brake distance expressed in centimeters</returns>
        public double GetBrakeDistance()
        {
            double brakeTime = (double)(speed / deceleration);
            double distanceTravelled = brakeTime * (double)(speed / 2) * 100;
            return distanceTravelled;
        }

        /// <summary>
        /// Get the distance from the start of the path untill the cars current position.
        /// </summary>
        /// <returns>Distance expressed in centimeters.</returns>
        public double GetDistance()
        {
            return this.distance;
        }

        /// <summary>
        /// Get the safety margin of the car. The safety margin is the amount of space that the car will
        /// keep between itself and any other object.
        /// </summary>
        /// <returns>Safety margin expressed in centimeters.</returns>
        private double getSafetyMargin()
        {
            return drivingPath.GetMinMargin() + (this.speed * 80);
        }
    }
}
