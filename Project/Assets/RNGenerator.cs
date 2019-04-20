using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RNGenerator : MonoBehaviour {
    GameManager platformManager;
    GameObject platformSpawn;
	// Use this for initialization
	void Start () {


        platformManager = GameObject.FindObjectOfType<GameManager>();


        int biomeIndex = 0;
        biomeIndex = Random.Range(0, platformManager.TerminalNodes.Count);


        platformSpawn = platformManager.TerminalNodes[biomeIndex];
        platformSpawn.GetComponent<NodeGenerator>().life = gameObject.GetComponent<NodeGenerator>().life;

        //Forest gets moved down a little to not require jumping
        if (biomeIndex == 2)
            transform.position += new Vector3(0.0f, -0.8f, 0.0f);


        Instantiate(platformSpawn, gameObject.transform.position, gameObject.transform.rotation);
        Object.Destroy(gameObject);
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
