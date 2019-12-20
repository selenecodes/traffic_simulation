using UnityEngine;


/// <summary>
/// Class <c>Car</c> the agent that is controlling our "car" (3d sphere).
/// </summary>
public class Car : MonoBehaviour
{
    [System.NonSerialized]
    public int lane;

    private Rigidbody rb;
    private Road road;

    private double initialSpeed;
    private bool finishedCourse = false;

    void Start()
    {
        this.rb = GetComponent<Rigidbody>();
        
        this.road = GameObject.FindGameObjectWithTag("road").GetComponent<Road>();
        this.initialSpeed = CalculateInitialSpeed(this.road.maxSpeed);

        Vector3 initialVelocity = new Vector3((float) -this.initialSpeed, 0, 0);
        this.rb.velocity = initialVelocity;
        
        InvokeRepeating("logCarSpeed", 1,1);
    }

    /// <summary>Log the car speed by adding it to an array on <c>Road</c></summary>

    void logCarSpeed() {
        if (!this.finishedCourse) {
            this.road.carSpeeds.Add(this.rb.velocity.magnitude);
        }
    }

    void FixedUpdate()
    {
        if (!finishedCourse && this.rb.position.x <= (this.road.transform.position.x - (this.road.transform.localScale.x * 5))) {
            this.finishedCourse = true;
            this.road.carsFinishedTrack++;

            Vector3 initialVelocity = new Vector3((float)-this.initialSpeed, 0, 0);
            this.rb.velocity = initialVelocity;

            if (this.road.carsFinishedTrack == this.road.carCount) {
                this.road.saveData();
            }
        }

        int carCount = GameObject.FindGameObjectsWithTag("car").Length;

        if (carCount > 1 && !finishedCourse) {
            float closestCarDistance = getClosestCarDistance();

            // !Note distance -1 means it's unset
            if (closestCarDistance != -1) {
                float brakingForce = CalculateVelocity(closestCarDistance);
                Vector3 velo = this.rb.velocity;

                this.rb.velocity = new Vector3(velo.x + brakingForce, 0, velo.z);
            } else {
                Vector3 initialVelocity = new Vector3((float)-this.initialSpeed, 0, 0);
                this.rb.velocity = initialVelocity;
            }
        }
    }

    /// <summary> This function gets the car in front of our current car (if there is a car in front of it at all)</summary>
    float getClosestCarDistance() {
        GameObject[] cars = GameObject.FindGameObjectsWithTag("car");

        float minDistance = -1F; 
        foreach (GameObject car in cars)
        {
            Car otherCarObject = car.GetComponent<Car>();
            Vector3 otherCar = car.transform.position;
            Vector3 thisCar = this.rb.transform.position;

            float angle = Vector3.Angle(thisCar, otherCar);
            
            if (thisCar.x > otherCar.x && this.lane == otherCarObject.lane) {
                float distance = System.Math.Abs(otherCar.x - thisCar.x);
                
                if (minDistance == -1F){
                    minDistance = distance;
                } else if (distance < minDistance) {
                    minDistance = distance;
                }
            }
        }
        
        return minDistance;
    }

    /// <summary> This function calculates the starting (initial) speed for the car
    ///    (<paramref name="max"/>).</summary>
    /// <param><c>max</c>This is the max speed a car can have on this road</param>

    double CalculateInitialSpeed(int max)
    {
        int std = this.road.std;
        int averageSpeed = max - std;
        int minSpeed = max - (std * 2);

        System.Random rand = new System.Random();

        double u1 = 1.0 - rand.NextDouble();
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) *
                        System.Math.Sin(2.0 * System.Math.PI * u2);
        double randNormal = averageSpeed + std * randStdNormal;

        if (randNormal > max) {
            return max;
        } else if (randNormal < minSpeed) {
            return minSpeed;
        } else {
            return randNormal; 
        }
    }

    /// <summary> This function calculates the new velocity for a car 
    ///    (<paramref name="distance"/>).</summary>
    /// <param><c>distance</c> is the new distance from This car to the car in front of it</param>

    private float CalculateVelocity(float distance)
    {
        int brakeDistance = 15;

        if (distance < brakeDistance) {
            Debug.DrawLine(this.rb.position, new Vector3(this.rb.position.x,10,this.rb.position.z), Color.red);
            return this.rb.velocity.sqrMagnitude / 250 *.8F;
        } else if (distance > 40 &&  this.rb.velocity.x > -this.road.maxSpeed) {
            return -5F;
        }

        return 0F;
    }
}
