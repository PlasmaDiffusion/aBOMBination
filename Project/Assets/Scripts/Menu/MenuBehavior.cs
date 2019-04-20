using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBehavior : MonoBehaviour {

    public static int numPlayers;

    public static string ipAddress = "127.0.0.1";

    public Sprite bg1;
    public Sprite bg2;
    public Sprite bg3;

    // Use this for initialization
    void Start () {
		
	}

    public void LoadScene(int scene)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    public void ShowMultiplayerButtons()
    {

        GameObject.Find("Panel").GetComponent<Image>().sprite = bg2;

        gameObject.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(true);


        if (GameObject.Find("Quit"))
            GameObject.Find("Quit").SetActive(false);
    }

    public void ShowPlayerCountButtons()
    {



        GameObject.Find("Panel").GetComponent<Image>().sprite = bg3;

        gameObject.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(true);
        transform.GetChild(2).gameObject.SetActive(true);



        transform.parent.GetChild(1).gameObject.SetActive(false);

    }

    public void SetPlayerCount(int playerCount)
    {
        numPlayers = playerCount;

        //Make black quad visible and remove buttons so the camera actually refreshes properly and doesnt leave an ugly p4 viewport when theres only 3 players >_>
        GameObject.Find("ClearScreen").GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        transform.parent.parent.GetChild(0).gameObject.SetActive(false);
        transform.parent.parent.GetChild(1).gameObject.SetActive(false);
        //transform.parent.parent.GetChild(2).gameObject.SetActive(false);
        //transform.parent.parent.GetChild(3).gameObject.SetActive(false);
        //GameObject.Find("Quit").gameObject.SetActive(false);

        //Get ip
        if (playerCount == 1)
        {
            Text t = transform.GetChild(0).GetChild(2).GetComponent<Text>();
            ipAddress = t.text;
            Debug.Log(ipAddress);
        }

        LoadScene(2);
    }

    public void CloseScene()
    {
        Application.Quit();
    }


    void OnMouseUp()
    {
        
    }

	// Update is called once per frame
	void Update () {
        //Reset
        if (Input.GetKeyUp(KeyCode.Escape)) UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
