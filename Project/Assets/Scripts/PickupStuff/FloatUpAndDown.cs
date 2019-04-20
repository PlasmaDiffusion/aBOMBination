using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatUpAndDown : MonoBehaviour {

    float verticalSpeed;
    float verticalPos;

	// Use this for initialization
	void Start () {
        verticalSpeed = 0.2f;
        verticalPos = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position +=new Vector3(0.0f, verticalSpeed * Time.deltaTime, 0.0f);
        verticalPos += verticalSpeed * Time.deltaTime;
        if (verticalPos > 1.0f) verticalSpeed = verticalSpeed * -1.0f;
        else if (verticalPos < 0.0f) verticalSpeed = verticalSpeed * -1.0f;
    }
}
