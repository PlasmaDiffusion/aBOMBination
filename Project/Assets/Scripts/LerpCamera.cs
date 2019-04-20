using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpCamera : MonoBehaviour {

    private Transform targetTransform;

    //private float t;
    //private bool lerpingRotation;

	// Use this for initialization
	void Start () {
        //Find target transform
        targetTransform = transform.parent.GetChild(0).transform;

        //Detach self from player
        transform.parent = null;
        //lerpingRotation = false;

        //t = 1.0f;
	}
	
	// Update is called once per frame
	void Update () {


        /*
        //If player has rotated then start lerp rotating.
        if ((transform != targetTransform) && !lerpingRotation && t >= 1.0f)
        {
            t = 0.0f;
             lerpingRotation = true;

        }

        //Perform a lerp rotation until at 1.0
        if (lerpingRotation)
        {
            t += (0.01f * Time.deltaTime);
            if (t > 1.0f)
            {
                t = 1.0f;

                lerpingRotation = false;
            }

            //transform.position = targetTransform.position;
            transform.position = Vector3.Lerp(transform.position, targetTransform.position, t);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, t);
            
        }*/
    }
    
   void LateUpdate()
    {

        transform.position = Vector3.Lerp(transform.position, targetTransform.position, 2.5f * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, 2.5f * Time.deltaTime);
    }
}
