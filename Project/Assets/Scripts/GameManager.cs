using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public int time;
    private int maxTime;
    public int endGameTime; //Invisible timer that ends the game and goes to the title screen

   public GameObject fireworkEmitter;

    public List<GameObject> TerminalNodes; //Types of nodes that can spawn at the end of a generated map.

    bool gameEnded;
    public bool inSkirmish;

    private GameObject winnerObject;
    
    bool countingDown;
	// Use this for initialization
	void Start () {

        maxTime = time;

        gameEnded = false;

        countingDown = false;

        //Determine if in skirmish
        Scene scene = SceneManager.GetActiveScene();

        if (scene.name == "skirmishScene") inSkirmish = true;
        else inSkirmish = false;

        setTimerText();

        StartCoroutine(Timer());

    }
	
	// Update is called once per frame
	void Update () {
        
    }

    IEnumerator Timer()
    {
        //Count down only after camera zooms in
        if (!countingDown)
        {

        GameObject camera = GameObject.Find("cameraControllerP1");

         if (camera)
          {
            countingDown = camera.GetComponent<CameraController>().controlling;
          }
         
        }
        
        //Countdown clock
        yield return new WaitForSecondsRealtime(1);
        if (time > 0 && !gameEnded && !inSkirmish && countingDown)
        {
            time--;
            setTimerText();
        }

        //Remove the now unused spawn platform
        if (time == maxTime - 10) removeSpawn();

        //After game ended it'll wait a few seconds.
        if (gameEnded)
        {
            endGameTime--;
            if (endGameTime <= 0) finishGame();
        }
        else if (time <= 0) //If the game didn't end yet then check if time ran out
        {
                //If it did it's a draw :/
                setWinText("Draw", new Color(0.8f, 0.0f, 0.8f));

            //If online, send winner to server (In this case the winner is -1, and the server interprets that as a draw)
            GameObject networkGameObject = GameObject.Find("ConnectionHandler");

            if (networkGameObject != null)
            {
                NetworkClient networkObject = networkGameObject.GetComponent<NetworkClient>();

                Debug.Log("Sending draw message");
                networkObject.sendWinMessage(-1);
            }


            gameEnded = true;
        }

        if (inSkirmish) checkIfPlayersReady();

        StartCoroutine(Timer());

    }

    //Remove the spawn platform and player's first throws in case they're yet to use it
    private void removeSpawn()
    {
        //Move the platform into the great beyond... It'll come back later...
       GameObject.Find("SpawnPlatform").gameObject.transform.position = new Vector3(10000.0f, -10000.0f, 0.0f);

        //Everyone can't throw a spawn bomb beyond this point
        for (int i = 0; i < 4; i++)
        {
            GameObject playerObject = GameObject.Find("Player" + (i + 1).ToString()); //Find each player by name. All players should be named like Player1, Player2, etc...

            //If player exists check if they're dead X_X
            if (playerObject)
            {
                playerObject.GetComponent<Player>().firstThrow = false;
            }
         }
    }

    public void checkForWinner()
    {
        int winnerIndex = 0;
        int aliveCount = 0;

        //Check if someone won the game with an array of alive flags
        bool[] alivePlayers = new bool[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject playerObject = GameObject.Find("Player" + (i + 1).ToString()); //Find each player by name. All players should be named like Player1, Player2, etc...

            //If player exists check if they're dead X_X
            if (playerObject)
            {
                Player p = playerObject.GetComponent<Player>();

                if (p.getHealth() <= 0.0f) alivePlayers[i] = false;
                else
                {
                    alivePlayers[i] = true;
                    winnerIndex = i; //If the player is alive, they may be a winner
                    winnerObject = playerObject;
                }
            }
            else alivePlayers[i] = false;

            if (alivePlayers[i]) aliveCount += 1;

        }

        //If ones alive then they have won! The player number of the winner was just recorded too.
        if (aliveCount == 1 && !gameEnded)
        {
            switch (winnerIndex)
            {
                case 0:
                    setWinText("Player 1\nWins!", new Color(0.8f, 0.5f, 0.5f));
                    break;

                case 1:
                    setWinText("Player 2\nWins!", new Color(0.5f, 0.5f, 0.8f));
                    break;

                case 2:
                    setWinText("Player 3\nWins!", new Color(0.8f, 0.8f, 0.5f));
                    break;

                case 3:
                    setWinText("Player 4\nWins!", new Color(0.5f, 0.8f, 0.5f));
                    break;


            }

            //Send winner to server
            GameObject networkGameObject = GameObject.Find("ConnectionHandler");

            if (networkGameObject != null)
            {
                NetworkClient networkObject = networkGameObject.GetComponent<NetworkClient>();
                networkObject.sendWinMessage(winnerIndex);
            }


            gameEnded = true;
        }

    }

    private void setWinText(string winner, Color color)
    {
        Text winText = GameObject.Find("WinText").GetComponent<Text>();

        //Set background text
        winText.text = winner;
        Text frontWinText = winText.transform.GetChild(0).GetComponent<Text>();

        //Set front text
        frontWinText.text = winner;
        frontWinText.color = color;


        gameEnded = true;

        time = -1;
        setTimerText();

        if (winner != "Draw")
        { 

        //Make fire works appear!
        Instantiate(fireworkEmitter).GetComponent<ParticleInterface>().playerReference = winnerObject;

        GameObject.Find("SpawnPlatform").gameObject.transform.position = new Vector3(winnerObject.transform.position.x, winnerObject.transform.position.y - 10.0f, winnerObject.transform.position.z);
            GameObject.Find("SpawnPlatform").gameObject.transform.localScale = new Vector3(20.0f, 0.0f, 20.0f);

            winnerObject.GetComponent<Player>().playVictoryAnimation();
        }
    }

    void setTimerText()
    {
        string newTimeText = time.ToString();

        //If time is -1 the game ended. Don't display the timer anymore. Also don't display it in skirmish.
        if (time == -1 || inSkirmish) newTimeText = "";

        Text timeText = GameObject.Find("TimerText").GetComponent<Text>();

        //Set background text
        timeText.text = newTimeText;
        Text frontTimeText = timeText.transform.GetChild(0).GetComponent<Text>();

        //Set front text
        frontTimeText.text = newTimeText;
    }

    //Exit to menu or whatever
    void finishGame()
    {

        ReadAndWriteStats statsManager = GetComponent<ReadAndWriteStats>();

        statsManager.gamesPlayed++;
        statsManager.writeStats();

        
        //Send end game message to server
        GameObject networkGameObject = GameObject.Find("ConnectionHandler");

        //Go back to menu if offline. If online, go back to the lobby?
        if (networkGameObject == null)
        {
            SceneManager.LoadScene("Menu");
        }
        else
        {
            //Wait for message to change scene
        }
    }

    void checkIfPlayersReady()
    {
        bool everyoneReady = true;

        for (int i = 0; i < 4; i++)
        {
            GameObject playerObject = GameObject.Find("Player" + (i + 1).ToString()); //Find each player by name. All players should be named like Player1, Player2, etc...

            //If player exists check if they're ready to end the skirmish
            if (playerObject != null)
            {
                if (!playerObject.GetComponent<Player>().ready) everyoneReady = false;
             
            }
        }


        if (everyoneReady || Input.GetKeyDown(KeyCode.Tab))
        {

            //Send start game message to server
            GameObject networkGameObject = GameObject.Find("ConnectionHandler");

            if (networkGameObject != null)
            {
                NetworkClient networkObject = networkGameObject.GetComponent<NetworkClient>();

                networkObject.sendGameStartMessage();
            }
            else //If offline then just go to whatever map instantly
            { 
                SceneManager.LoadScene(Random.Range(3, 6));
            }
        }

    }
}
