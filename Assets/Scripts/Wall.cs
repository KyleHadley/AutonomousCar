using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    [SerializeField] string LayerHitName = "Agent_Car";// The name of the layer which the car may collide with

    private void OnCollisionEnter2D(Collision2D collision)// Once anything hits the wall
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer(LayerHitName))// Make sure it collides with the car (by setting its layer)
        {
            collision.transform.GetComponent<Car>().WallHit();// Tell the car it has impacted with the wall
        }
    }
}
