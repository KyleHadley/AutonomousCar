using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable All

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
    [SerializeField] Text ActiveCarText; // Keeps track of how much time has been elapsed
    [SerializeField] GameObject cam; // Camera
    HandleNetworkData handleData;
    NeuralNetwork aNeuralnet;

    int GenerationCount = 0; // Track the current number of generations
    bool firstLapComplete; // Tracks the first lap complete
    float elapsedTime = 0.0f;

    //List<Car> currentEvolutionCars = new List<Car>(); // List of cars currently available
    List<Car> listOfCars = new List<Car>(); // List of cars currently still active
    //List<Car> inactiveCars = new List<Car>(); // List of cars currently still active
    public Car activeFirstCar { private set; get; }

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
        ActiveCarText.text = "Active Cars: " + GetActiveCars().Count + " | Inactive Cars: " + GetInactiveCars().Count;

        // If no more cars are active, create a new generation
        if (listOfCars.Count(x => x.IsActive) <= 0)
        {
            BeginNextGeneration();
        }
        else if (listOfCars.Count == 1 && listOfCars.FirstOrDefault(x => x.IsBestNetwork))
        {
            Debug.Log("Last car remaining is the previous best generation, skipping to begin next new generation.");
            Wait(1, BeginNextGeneration);
        }
    }

    private void BeginNextGeneration()
    {
        RemoveAllCars();
        StartGeneration();
    }

    private void FixedUpdate()
    {
        //Car firstCar = transform.GetChild(0).GetComponent<Car>(); // The best car in the bunch is the first one
        Car firstCar = listOfCars.FirstOrDefault(x => x.IsActive);

        if (firstCar != null)
        {
            //var test = cam.GetComponent<CameraMovement>();
            if (activeFirstCar == null && firstCar != null)
            {
                activeFirstCar = firstCar;
            }

            //for (int i = 1; i < transform.childCount; i++) // Loop over all the cars
            foreach (var currentCar in listOfCars)
            {
                if (currentCar.HasReachedFinalCheckpoint)
                {
                    RemoveFromActiveList(currentCar);
                }
                else if (!currentCar.HasReachedFinalCheckpoint && currentCar.Fitness > firstCar.Fitness
                ) // If the current car is better than the best car
                {
                    //BestCar = CurrentCar; // Then, the best car is the current car
                    activeFirstCar = currentCar;
                }
            }

            var camera = cam.GetComponent<CameraMovement>();

            if (camera.CurrentTarget != activeFirstCar)
            {
                camera.SetTarget(activeFirstCar);
            }

        }
    }

    /// <summary>
    /// Waits x seconds before executing the action
    /// </summary>
    /// <param name="seconds"> Number of seconds to wait </param>
    /// <param name="action"> The method we want to execute </param>
    public void Wait(float seconds, Action action)
    {
        StartCoroutine(_wait(seconds, action));
    }

    IEnumerator _wait(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        callback();
    }

    void StartGeneration()
    {
        GenerationCount++;// Increment generation count
        GenerationNumberText.text = "Generation: " + GenerationCount; // Update current generation text
        BestFitnessText.text = "Current Best Fitness: " + bestFitness; // Update current best fitness

        for (int i = 0; i < CarCount; i++)
        {
            // Todo, if last car alive is the best car, speed up ending current generation
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
            //currentEvolutionCars.Add(Instantiate(CarPrefab, transform.position, Quaternion.identity, transform).GetComponent<Car>());
            listOfCars.Add(Instantiate(CarPrefab, transform.position, Quaternion.identity, transform).GetComponent<Car>());
            listOfCars[i].SetActive(true);

            if (i == 0)
            {
                listOfCars[i].UpdateBestCarSprite();
            }
        }
        /*tk
        aNeuralnet = BestNeuralNetwork;
        handleData = GetComponent<HandleNetworkData>();
        handleData.SaveNetwork(aNeuralnet);*/
    }

    //private void RemoveAllInactiveCars()
    private void RemoveAllCars()
    {
        foreach (var car in listOfCars)//.Where(x => !x.IsActive))
        {
            
            // Delete any cars still existing
            if (car.gameObject != null && gameObject.activeInHierarchy)
            {
                //var co = GameObject.Find()
                Destroy(car.gameObject);
            }
            //listOfCars.Remove(car);
        }

        // Clear list of cars for next generation
        listOfCars.Clear();
    }

    // Called by cars when they die (crash into a wall)
    public void CarDead(Car DeadCar, int Fitness)
    {
        //RemoveFromActiveList(DeadCar);
        listOfCars.Remove(DeadCar);
        Destroy(DeadCar.gameObject); // Destroy dead car
        UpdateFirstTargetForCamera(DeadCar);
        // todo: toggle freezing it and make it transparent/plain colour?

        if (Fitness > bestFitness)// If better than the current best car
        {
            BestNeuralNetwork = DeadCar.DrivingNN; // make sure it becomes best car
            bestFitness = Fitness; // then set best fitness
        }
    }

    private void RemoveFromActiveList(Car car)
    {
        car.SetActive(false);
        UpdateFirstTargetForCamera(car);
    }

    private void UpdateFirstTargetForCamera(Car car)
    {
        var camera = cam.GetComponent<CameraMovement>();

        if (camera.CurrentTarget == car.gameObject)
        {
            activeFirstCar = GetActiveBestCar();

            if (activeFirstCar != null)
            {
                camera.SetTarget(activeFirstCar);
            }
        }
    }

    private Car GetActiveBestCar()
    {
        var listOfCars = this.listOfCars.OrderByDescending(x => x.Fitness).ToList();
        Car nextBestCar = listOfCars.FirstOrDefault(x => x.IsActive && !x.HasReachedFinalCheckpoint);
        return nextBestCar;
    }

    private List<Car> GetInactiveCars()
    {
        return listOfCars.Where(x => !x.IsActive).ToList();
    }

    private List<Car> GetActiveCars()
    {
        return listOfCars.Where(x => x.IsActive).ToList();
    }

    public void LapComplete(Car car)
    {
        RemoveFromActiveList(car);
        if (!firstLapComplete)
        {
            LapCompleteText.text = "First Lap Success: " + GenerationCount;
            firstLapComplete = true;
        }
    }
}
