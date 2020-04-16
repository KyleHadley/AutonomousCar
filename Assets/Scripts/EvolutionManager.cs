using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{

    // Create an instance of the EvolutionManager
    public static EvolutionManager Singleton = null;

    [SerializeField] int CarCount = 20; // Number of cars per generation
    [SerializeField] GameObject CarPrefab; // The prefab of the car created for each instance
    [SerializeField] Text GenerationNumberText; // Keep track of current generation for GUI
    [SerializeField] Text BestFitnessText; // Keep track of current best fitness
    [SerializeField] Text LapCompleteText; // Tracks the first lap to be solved by the network
    [SerializeField] Text TimeElapsedText; // Keeps track of how much time has been elapsed
    [SerializeField] GameObject cam; // Camera
    HandleNetworkData handleData;
    NeuralNetwork aNeuralnet;

    int GenerationCount = 0; // Track the current number of generations
    bool firstLapComplete; // Tracks the first lap complete
    float elapsedTime = 0.0f;

    List<Car> Cars = new List<Car>(); // List of cars currently available

    NeuralNetwork BestNeuralNetwork = null;//The current best neural network available
    int bestFitness = -1;// The fitness of the best neural network

    // Use this for initialization
    void Start()
    {
        if (Singleton == null)// If no other instances exist then create one
        {
            Singleton = this;
        }
        else
        {
            gameObject.SetActive(false);// Another instance is in place so deactivate this one
        }

        
        firstLapComplete = false;
        BestNeuralNetwork = new NeuralNetwork(Car.NextNetwork);// Set the best neural network to become a new network

        StartGeneration();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        TimeElapsedText.text = "Time Elapsed: " + elapsedTime;// Keep track of current time
    }

    private void FixedUpdate()
    {
        Car BestCar = transform.GetChild(0).GetComponent<Car>(); // The best car in the bunch is the first one

        for (int i = 1; i < transform.childCount; i++) // Loop over all the cars
        {
            Car CurrentCar = transform.GetChild(i).GetComponent<Car>(); // Get the component of the current car

            if (CurrentCar.Fitness > BestCar.Fitness) // If the current car is better than the best car
            {
                BestCar = CurrentCar; // Then, the best car is the current car
            }
            //GameObject cam = GameObject.FindWithTag("MainCamera");
            //CameraMovement camtarg = cam.GetComponent<CameraMovement>().SetTarget(BestCar);
            //CameraMovement = 
                cam.GetComponent<CameraMovement>().SetTarget(BestCar);
            
        }
    }

    void StartGeneration()
    {
        GenerationCount++;// Increment generation count
        GenerationNumberText.text = "Generation: " + GenerationCount; // Update current generation text
        BestFitnessText.text = "Current Best Fitness: " + bestFitness; // Update current best fitness

        for (int i = 0; i < CarCount; i++)
        {
            if (i == 0)
            {
                Car.NextNetwork = BestNeuralNetwork; // Make sure one car uses the best network
            }
            else
            {
                Car.NextNetwork = new NeuralNetwork(BestNeuralNetwork); // Clone the best neural network and set it to be the next car

                Car.NextNetwork.Mutate(); // Mutate it
            }

            // Instantiate a new car and add it to the list
            Cars.Add(Instantiate(CarPrefab, transform.position, Quaternion.identity, transform).GetComponent<Car>());
        }
        /*tk
        aNeuralnet = BestNeuralNetwork;
        handleData = GetComponent<HandleNetworkData>();
        handleData.SaveNetwork(aNeuralnet);*/
    }

    // Called by cars when they die (crash into a wall)
    public void CarDead(Car DeadCar, int Fitness)
    {
        Cars.Remove(DeadCar);// Remove car from list
        Destroy(DeadCar.gameObject); // Destroy dead car
        // could also freeze it and make it transparent/plain colour?

        if (Fitness > bestFitness)// If better than the current best car
        {
            BestNeuralNetwork = DeadCar.DrivingNN; // make sure it becomes best car
            bestFitness = Fitness; // then set best fitness
        }

        // If no more cars exist, create a new generation
        if (Cars.Count <= 0)
        {
            StartGeneration();
        }
    }

    public void LapComplete()
    {
        if (!firstLapComplete)
        {
            LapCompleteText.text = "First Lap Success: " + GenerationCount;
            firstLapComplete = true;
        }
    }
}
