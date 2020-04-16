using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

    //[SerializeField] string LayerHitName = "Agent_Car"; // Name of the layer set on each car
    [SerializeField] bool FinalCheckpoint = false;// Set true for the last checkpoint of the map

    List<string> AllGuids = new List<string>(); // List of IDs for the cars
    //EvolutionManager Manager;
    [SerializeField] GameObject Manager;

    private void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        //Manager = Manager.GetComponent<EvolutionManager>();
        

        sr.color = new Color(1f, 1f, 1f, 0.5f);// Make sprite 50% transparent
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("testt" + other.gameObject.name + other.gameObject.tag.ToString());
        if(other.gameObject.tag == "Agent_Car")
        {
            //Car CarComponent = other.transform.parent.GetComponent<Car>(); // Get component of the car
            Car CarComponent = other.GetComponent<Car>();
            string carID = CarComponent.UniqueID; // Get the unique ID of the car
            
            // Double check and ensure the car count is increased and increased only once
            if(!AllGuids.Contains(carID))
            {
                AllGuids.Add(carID);
                CarComponent.CheckpointCaptured(); // Increase the car's fitness
            }

            if(FinalCheckpoint)
            {
                CarComponent.CheckpointCaptured();
                Debug.Log(carID);
                //Manager.LapComplete();
                EvolutionManager.Singleton.LapComplete();
                // Add function to call from manager and change some text
            }
        }
    }

}
