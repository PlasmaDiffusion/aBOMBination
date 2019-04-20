using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {


    Text healthText;
    RectTransform rect;

    // Use this for initialization
    void Start () {

        healthText = transform.GetChild(0).GetChild(0).GetComponent<Text>();

       rect = gameObject.GetComponent<RectTransform>();


    }
	
	// Update is called once per frame
	void Update () {

       //Hopefully parsing every frame doesn't kill the game :P
        float width = float.Parse(healthText.text) * 3.3f;

        rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);

      //Framerate debugging
      // healthText.text = (1.0f / Time.deltaTime).ToString();
	}

}
