using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitScreen : MonoBehaviour {

    public Camera cam1;
    public Camera cam2;
    public Camera cam3;
    public Camera cam4;

    public int numLocalPlayers = 1;
    public bool horizontal = false;

	// Use this for initialization
	void Start () {

        numLocalPlayers = MenuBehavior.numPlayers;
	
        //1 screen for 1 player
        if (numLocalPlayers <= 1)
        {
            //Destroy P2
            destroyLocalPlayer("Player2");
            Destroy(GameObject.Find("cameraControllerP2"));
            destroyLocalPlayer("Player3");
            Destroy(GameObject.Find("cameraControllerP3"));
            destroyLocalPlayer("Player4");
            Destroy(GameObject.Find("cameraControllerP4"));

            //Resize scren
            cam1.rect = new Rect(0, 0, 1, 1);

            //Remove P2 Hud
            Destroy(GameObject.Find("HUDPrefab2").gameObject);

            //Remove P3 Hud
            Destroy(GameObject.Find("HUDPrefab3").gameObject);

            //Remove P4 Hud
            Destroy(GameObject.Find("HUDPrefab4").gameObject);
        }
        else if (numLocalPlayers ==2)
        {
            //Destroy P
            Destroy(GameObject.Find("Player3"));
            Destroy(GameObject.Find("cameraControllerP3"));
            Destroy(GameObject.Find("Player4"));
            Destroy(GameObject.Find("cameraControllerP4"));

            //Resize scren
            cam1.rect = new Rect(0, 0, 0.5f, 1);
            cam2.rect = new Rect(0.5f, 0, 0.5f, 1);

            //Remove P3 Hud
            Destroy(GameObject.Find("HUDPrefab3").gameObject);

            //Remove P4 Hud
            Destroy(GameObject.Find("HUDPrefab4").gameObject);
        }
        else if (numLocalPlayers == 3)
        {
            Destroy(GameObject.Find("Player4"));
            Destroy(GameObject.Find("cameraControllerP4"));

            //Remove P4 Hud
            Destroy(GameObject.Find("HUDPrefab4").gameObject);
        }

        //Shrink hud and zoom out camera a little if 3 or more players
        if (numLocalPlayers >= 3)
        {
            makeHUD4PlayerReady(GameObject.Find("HUDPrefab"));
            makeHUD4PlayerReady(GameObject.Find("HUDPrefab2"));
            makeHUD4PlayerReady(GameObject.Find("HUDPrefab3"));

            if (numLocalPlayers > 3)
                makeHUD4PlayerReady(GameObject.Find("HUDPrefab4"));


           /* zoomOut(GameObject.Find("CameraP1"));
            zoomOut(GameObject.Find("CameraP2"));
            zoomOut(GameObject.Find("CameraP3"));

            if (numLocalPlayers > 3)
                zoomOut(GameObject.Find("CameraP4"));*/
        }

        //ChangeSplitScreen();

        //Make network client spawn online opponents here if online
        GameObject networker = GameObject.Find("ConnectionHandler");
        
        if (networker)
        {
            networker.GetComponent<NetworkClient>().postSceneChange();
        }
    }

    //Changes scale factor and anchoring for 3 or 4 player splitscreen
    void makeHUD4PlayerReady(GameObject hud)
    {
        Transform descs = hud.transform.GetChild(7);
        

        if (hud.name == "HUDPrefab2")
        {


        for (int i = 0; i < 4; i++)
            {
            descs.GetChild(i).GetComponent<RectTransform>().anchorMin = new Vector2(1, 0.5f);
            descs.GetChild(i).GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.5f);
                descs.GetChild(i).GetComponent<RectTransform>().localPosition += new Vector3(0, 715.0f);
            }


        }
        else if (hud.name == "HUDPrefab")
        {

            for (int i = 0; i < 4; i++)
            {
                descs.GetChild(i).GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
                descs.GetChild(i).GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);
                descs.GetChild(i).GetComponent<RectTransform>().localPosition += new Vector3(0, 715.0f);

            }
        }



        hud.GetComponent<Canvas>().scaleFactor = 0.75f;
    }
	
    void zoomOut(GameObject cam)
    {
        cam.transform.position += new Vector3(0.0f, 5.0f, -5.0f);
    }

	// Update is called once per frame
	void Update () {
		
	}

    //Unused horizontal option
    void ChangeSplitScreen()
    {
        if (horizontal)
        {
            cam1.rect = new Rect(0, 0, 1, 0.5f);
            cam2.rect = new Rect(0, 0.5f, 1, 0.5f);
        }
        else //vertical
        {
            cam1.rect = new Rect(0, 0, 0.5f, 1);
            cam2.rect = new Rect(0.5f, 0, 0.5f, 1);
        }
    }

    //Check if player is local and not online. If so, destroy them.
    void destroyLocalPlayer(string playerName)
    {
        GameObject p = GameObject.Find(playerName);

        //Does the player exist?
        if (p != null)
        {

            //Is it a local player?
            if (!p.GetComponent<Player>().onlineOpponent)
            {


                GameObject n = GameObject.Find("ConnectionHandler");

                //Is the game online right now? If yes n will exist
                if (n != null)
                {
                    //Make sure the local player doesn't happen to be the client player
                    if (!p.GetComponent<Player>().player1)
                    {
                        Destroy(p);
                        Debug.Log("Destroyed local player cause the game is online: " + playerName);
                    }
                }
                else //If offline then destroy whatever
                {
                    Destroy(p);
                }

            }
            
        }

    }
}
