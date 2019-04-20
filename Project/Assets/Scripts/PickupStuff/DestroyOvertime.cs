using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOvertime : MonoBehaviour {

    float time;

	// Use this for initialization
	void Start () {
        time = 3.0f;
	}
    public void setText(int pickupId)
    {

        string message = "";

        switch (pickupId)
        {
            case 1: message = "Freeze Material";
                break;
            case 2:
                message = "Fire Material";
                break;
            case 3:
                message = "Smoke Material";
                break;
            case 4:
                message = "Radius Material";
                break;
            case 5:
                message = "Blackhole Material";
                break;
            case 6:
                message = "Scatter Material";
                break;
        }
        gameObject.GetComponent<TextMesh>().text = message;
    }
	
	// Update is called once per frame
	void Update () {
        time -= Time.deltaTime;

        if (time < 0.0f) Destroy(gameObject);
	}
}
