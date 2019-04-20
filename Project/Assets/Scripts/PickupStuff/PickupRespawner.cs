using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A pickup respawner, mostly for the skirmish so there's an infinite amount of materials to experiment with.
public class PickupRespawner : MonoBehaviour {

    public GameObject pickupToRespawn;

    private GameObject childPickup;
    private int respawnTimer;

	// Use this for initialization
	void Start () {
        childPickup = Instantiate(pickupToRespawn, gameObject.transform.position, gameObject.transform.rotation);
    }
	
	// Update is called once per frame
	void Update () {
		
        //If child pickup is gone then eventually respawn one
        if (!childPickup && respawnTimer == 0)
        {
            respawnTimer = 600;

        }

        if (respawnTimer > 0)
        {
            respawnTimer--;

            //At last second on timer make another pickup
            if (respawnTimer == 1) childPickup = Instantiate(pickupToRespawn, gameObject.transform.position, gameObject.transform.rotation);

        }
	}
}
