using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Pickups found on the default platform and not in a biome have a large chance to despawn, to encourage vistng biomes.
public class ChanceToDespawn : MonoBehaviour {


	// Use this for initialization
	void Start () {

        //If they're a child of the level nodes then this is required
        transform.parent = null;
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);


        //First determine the pickup type. Gravity and Smoke do not have biomes.
        MaterialPickup mp = gameObject.GetComponent<MaterialPickup>();

        if (mp.materialNo == 5 || mp.materialNo == 3)
        {
            //If a gravity or smoke material, chance of not despawning is 1/3.
            if (Random.Range(0, 3) != 0) Destroy(gameObject);
        }
        else
        {

            //Other pickups have 1/5 chance to NOT despawn
            if (Random.Range(0, 5) != 0) Destroy(gameObject);
        }

    }


    // Update is called once per frame
    void Update () {
        
    }

}
