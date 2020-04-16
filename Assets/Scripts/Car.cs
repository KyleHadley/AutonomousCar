using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{

    // Variables
    [SerializeField] bool UserInput = false;// Defines where the user can control the car or the neural network should
    [SerializeField] LayerMask SensorMask;// Choose which layer the walls will be so the car knows what to collide with
    [SerializeField] float FitnessUnchangedDie = 5;// Number of seconds to wait to check if the fitness hasn't increased

    [SerializeField] Sprite bestCarSprite;

    // Public neural network that refers to the next neural network to be set in the next instantiated car
    public static NeuralNetwork NextNetwork = new NeuralNetwork(new uint[] { 6, 4, 3, 2 }, null);

    public string UniqueID { get; private set; }// The unique ID of the car

    public int Fitness { get; private set; }// Current fitness/score of the car - Number of checkpoints it has hit.

    public float Velocity { get; private set; }// Current velocity of the car 

    public Quaternion Rotation { get; private set; }// Current rotation of the car

    public NeuralNetwork DrivingNN { get; private set; }// The neural network of current car

    // Constants
    private const float turnSpeed = 100f;
    private const float accelerationVelocity = 5f;
    private const float frictionVelocity = 2f;
    private const float maxVelocity = 15f;

    Rigidbody rb;// Rigidbody control of the game object
    LineRenderer lr;// Renders lines of the car for visual effect

    // Use this for initialization
    void Awake()
    {
        UniqueID = System.Guid.NewGuid().ToString();// Assign a new unique ID for the current car

        // Set the current nextwork to become the next network
        DrivingNN = NextNetwork;
        NextNetwork = new NeuralNetwork(NextNetwork.Topology, null);// Make sure next network is reassigned to avoid having another car use the same network

        // Assign components
        //rb = GetComponent<Rigidbody>();
        lr = GetComponent<LineRenderer>();

        StartCoroutine(IsNotImproving()); // Start checking if the score stayed the same for a while

        lr.numPositions = 17; // Make sure the line is long enough
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (UserInput)// If user input = true
        {
            Drive(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"));// Move the car according to the input
        }
        else// If using the neural network
        {
            float Vertical;
            float Horizontal;

            FeedSensorInputs(out Vertical, out Horizontal);

            Drive(Vertical, Horizontal); //Moves the car!!
        }
    }

    // Checks every x seconds if the car has made no improvements and forces it to evolve
    IEnumerator IsNotImproving()
    {
        while (true)
        {
            int OldFitness = Fitness;// Save initial fitness
            yield return new WaitForSeconds(FitnessUnchangedDie);// Wait some time
            if (OldFitness == Fitness) // Check if fitness hasn't changed
            {
                WallHit(); // Kill current car
            }
        }
    }

    // Function that handles car movement
    public void Drive(float v, float h)
    {
        //rb.velocity = transform.right * v * 4;
        //rb.angularVelocity = transform.up * h * 3;

        Vector3 direction = new Vector3(0, 1, 0);
        transform.rotation = Rotation;
        direction = Rotation * direction;

        //this.transform.position += direction * Velocity * Time.deltaTime;
        this.transform.position += direction * Velocity * Time.deltaTime;

        // If not accellerating, apply some friction
        //if(v <= 0)
        //{
            ApplyFriction();
        //}
    }

    private void ApplyFriction()
    {
        if(Velocity > 0)
        {
            Velocity -= frictionVelocity * Time.deltaTime;
            if(Velocity < 0)
            {
                Velocity = 0;
            }
        }
        else if(Velocity < 0)
        {
            Velocity += frictionVelocity * Time.deltaTime;
            if(Velocity > 0)
            {
                Velocity = 0;
            }
        }
    }

    // Cast a ray and also make is visible with line renderer
    double CastRay(Vector2 RayDirection, Vector2 LineDirection, int LinePositionIndex)
    {
        float maxLength = 10f; // Maximum length of ray

        RaycastHit2D hit;
        if (Physics2D.Raycast(transform.position, RayDirection, maxLength, SensorMask))// Cast a ray and gather ray information
        {
            hit = Physics2D.Raycast(transform.position, RayDirection, maxLength, SensorMask);// Get distance of the hit in the line
            float hitDist = hit.distance;
            //Vector3 test = new Vector3(0, 1, 0);
            //Debug.DrawLine(transform.position + test, hit.point);
            Debug.DrawLine(transform.position, hit.point);

            // Line rendering *NOT FIXED*
            //lr.SetPosition(0, transform.position);
            //lr.SetPosition(LinePositionIndex, hitDist * LineDirection);// Set position of the line

            return hitDist;// Return distance
        }
        else
        {
            //lr.SetPosition(LinePositionIndex, LineDirection * maxLength);// Set distance of the hit in line to be the maximum distance

            return maxLength;// Return maximum length
        }
    }

    // Cast all the rays and propate all this data
    void FeedSensorInputs(out float Vertical, out float Horizontal)
    {
        double[] NeuralInput = new double[NextNetwork.Topology[0]];

        /*
         *Alter to just do front, right left AND forward-right + forward-left
         */

        // Cast forward, right and left
        NeuralInput[0] = CastRay(transform.up, transform.up, 1);// / 4;
        //NeuralInput[1] = CastRay(-transform.forward, -transform.forward, 3);// / 4;
        NeuralInput[1] = CastRay(transform.right, transform.right, 5);// / 4;
        NeuralInput[2] = CastRay(-transform.right, -transform.right, 7);// / 4;

        // Cast forward-right and forward-left
        float SqrtHalf = Mathf.Sqrt(0.5f);
        NeuralInput[3] = CastRay(transform.right * SqrtHalf + transform.up * SqrtHalf,
            Vector2.right * SqrtHalf + Vector2.up * SqrtHalf, 9) / 4;
        NeuralInput[4] = CastRay(-transform.right * SqrtHalf + transform.up * SqrtHalf,
            Vector2.right * SqrtHalf + -Vector2.up * SqrtHalf, 13) / 4;

        // Feed through the network
        double[] NeuralOutput = DrivingNN.FeedForward(NeuralInput);

        // Get vertical value
        if (NeuralOutput[0] <= 0.25f)
        {
            Vertical = 0;
        }
        else if (NeuralOutput[0] >= 0.75f)
        {
            Vertical = accelerationVelocity;
        }
        else
        {
            Vertical = 0;
        }

        // Get horizontal value
        if (NeuralOutput[1] <= 0.25f)
        {
            Horizontal = -1;
        }
        else if (NeuralOutput[1] >= 0.75f)
        {
            Horizontal = 1;
        }
        else
        {
            Horizontal = 0;
        }

        // If the output is standing still then move car forward
        if (Vertical == 0 && Horizontal == 0)
        {
            Vertical = 1;
        }

        bool canAccelerate = false;
        if(Vertical < 0)
        {
            canAccelerate = Velocity > Vertical * maxVelocity;
        }
        else if(Vertical > 0)
        {
            canAccelerate = Velocity < Vertical * maxVelocity;
        }

        // set velocity
        if (canAccelerate)
        {
            Velocity += (float)Vertical * accelerationVelocity * Time.deltaTime;

            // Cap velocity max speed
            if(Velocity > maxVelocity)
            {
                Velocity = maxVelocity;
            }
            else if(Velocity < -maxVelocity)
            {
                Velocity = -maxVelocity;
            }
        }


        // Set rotation
        Rotation = transform.rotation;
        Rotation *= Quaternion.AngleAxis((float)-Horizontal * turnSpeed * Time.deltaTime, new Vector3(0, 0, 1));
    }

    // Called when the car hits any checkpoint
    public void CheckpointCaptured()
    {
        //Debug.Log("Checkpoint hit! fitness: " + Fitness);
        Fitness++; // Increase the fitness score
    }

    // Called when a car hits a wall
    public void WallHit()
    {
        EvolutionManager.Singleton.CarDead(this, Fitness);// Notify the evolution manager that car is dead

        gameObject.SetActive(false);// Deactivate the car
    }

    public void ChangeColour()
    {
        //if(bestCar)
       // {
            Debug.Log("hellooo");
            gameObject.GetComponent<SpriteRenderer>().sprite = bestCarSprite;
      //  }

    }
}
