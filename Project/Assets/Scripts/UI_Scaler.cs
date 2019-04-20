using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Scaler : MonoBehaviour
{
    CanvasScaler canvas;
    SplitScreen splitScreen;

    public float overTwoPlayerScale = 0.66f;

    public float targetScale = 0.75f;
    float t = 0.0f;

    //Make the HUD scale down more for 3-4 players.
    void Start()
    {

        splitScreen = GameObject.Find("SplitScreenHandler").GetComponent<SplitScreen>();

        canvas = gameObject.GetComponent<CanvasScaler>();


    }

    //Fancy appearing animation!
    private void Update()
    {
        
        canvas.scaleFactor = Mathf.Lerp(0.0f, targetScale, t);

        t += Time.deltaTime;

        if (t >= 1)
        {
            canvas.scaleFactor = targetScale;

            //Self destruct when done so we stop updating
            Destroy(GetComponent<UI_Scaler>());
        }

    }

    private void LateUpdate()
    {

        if (splitScreen.numLocalPlayers > 2)
            targetScale = overTwoPlayerScale;
    }
}
