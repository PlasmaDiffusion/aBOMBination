using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombAttributes : MonoBehaviour {

    //Structure for all of a bomb's parameters.
    public struct BombData
    {
        public bool hasIngredient;

        public int count; //In case we want a stackable inventory?

        public int materialsAdded; //Caps at 4

        //Generic parameters
        public float time;
        public Vector3 explosionScaleSpeed;
        public float explosionScaleLimit;
        public float explosionLifetime;

        public float damage;

        //Special on/off parameters
        public int fire;
        public int freeze;
        public int smoke;
        public int blackhole;
        public int scatter;
        public float MaxRange;

        public int[] materialIDs;
    }


    // Use this for initialization
    void Start () {




    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
