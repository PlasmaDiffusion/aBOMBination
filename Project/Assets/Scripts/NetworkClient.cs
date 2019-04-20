using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class NetworkClient : MonoBehaviour
{

    //DLL functions
    /*[DllImport("NetworkingPlugin10")]
    public static extern IntPtr tcpReceive();
    [DllImport("NetworkingPlugin10")]
    public static extern IntPtr udpReceive();
    [DllImport("NetworkingPlugin10")]
    public static extern bool init(string ip);
    [DllImport("NetworkingPlugin10")]
    public static extern bool sendMessage(string newMsg);
    [DllImport("NetworkingPlugin10")]
    public static extern bool getRunning();
    [DllImport("NetworkingPlugin10")]
    public static extern void close();
    [DllImport("NetworkingPlugin10")]
    public static extern IntPtr getUdp();
    [DllImport("NetworkingPlugin10")]
    public static extern IntPtr getTcp();*/

    private const string DLL_NAME = "UnityNetworkingDLL4";

    [DllImport(DLL_NAME)]
    public static extern void initialize(int port, string serverAddress);

    [DllImport(DLL_NAME)]
    public static extern void sendMsg(string msg);

    [DllImport(DLL_NAME)]
    public static extern bool hasReceived();

    [DllImport(DLL_NAME)]
    private static extern System.IntPtr getLastReceived();

    [DllImport(DLL_NAME)]
    public static extern bool hasError();

    [DllImport(DLL_NAME)]
    private static extern System.IntPtr getError();

    [DllImport(DLL_NAME)]
    public static extern void cleanUp();

    public static string getLastReceivedMessage()
    {
        return Marshal.PtrToStringAnsi(getLastReceived());
    }

    public static string getErrorMessage()
    {
        return Marshal.PtrToStringAnsi(getError());
    }


    //Flag for if the program is still connected
    public bool connected;
    private bool closed = false;

    //Client variables
    string name;
    public int clientPlayerNumber;
    Player clientPlayer;
    List<Player> serverPlayers;
    List<int> playerWins;

    Vector3[] spawnPositions;
    private Vector3 lastPosition;

    //Update times for when to send transform
    float nextUpdateTime;
    float currentUpdateTime;

    float winMessageTime;

    static bool hasChangedScene = false;
    bool duplicate = false;

    //Player prefab ref
    public GameObject playerPrefab;

    //Don't destroy when loading a new scene so this thing persists.
    void Awake()
    {
        if (SceneManager.GetActiveScene().name != "")
            DontDestroyOnLoad(transform.gameObject);
    }


    // Use this for initialization
    void Start()
    {
        
        lastPosition = new Vector3(0.0f, 0.0f, 0.0f);
        nextUpdateTime = 0.03f;
        currentUpdateTime = 0.0f;

        winMessageTime = 0.0f;

        //Make sure there's no duplicates of these objects when re-entering skirmish scene.
        if (hasChangedScene)
        {
            duplicate = true;
            Destroy(gameObject);
            return;
        }
        spawnPositions = new Vector3[4];

        serverPlayers = new List<Player>();
        playerWins = new List<int>();

        findClientPlayer();
        getSpawnPoints();


        if (MenuBehavior.numPlayers <= 1)
        {


            initialize(8888, MenuBehavior.ipAddress);
            
            //if (init(MenuBehavior.ipAddress))
            if (!hasError())
            {
                Debug.Log("Connected to server");
                connected = true;
                //Recieve server information i.e. players
            }
            else
            {
                Debug.Log("Connection failed. :(");
                connected = false;

            }

        }
        else
        {
            //Remove leaderboard if it's there
            GameObject leaderboard = GameObject.Find("LeaderBoard");

            if (leaderboard)
            {
                leaderboard.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (!connected)
        {

            Destroy(gameObject);
            return;

        }

        if (hasReceived())
        {

            //Recieve messages
            //string newChatMsg = getLastReceivedMessage();
            string newGameMsg = getLastReceivedMessage();

        //Debug.Log("RECV: " + newGameMsg);

        //if (newChatMsg != "" && newChatMsg != null) updateChat(newChatMsg);

     


        if (newGameMsg != "" && newGameMsg != null) parseGameUpdateMessage(newGameMsg);
        }

        //Send transform update here every few frames
        currentUpdateTime += Time.deltaTime;

        //Debug.Log(currentUpdateTime);

        if (currentUpdateTime >= nextUpdateTime && clientPlayer != null && (!Mathf.Approximately(Vector3.Distance(lastPosition, clientPlayer.transform.position), 0)))
        {
            send(getPlayerTransform());
            currentUpdateTime = 0.0f;
        }

        //Give time for the win message before changing back to lobby.
        if (winMessageTime > 0.0f)
        {
            winMessageTime -= Time.deltaTime;

            if (winMessageTime <= 0.0f)
            {
                winMessageTime = 0.0f;

                changeScene(2);

                updateLeaderboard();

            }
        }
            //Check if still connected
            //connected = getRunning();

        if (clientPlayer != null)
            lastPosition = clientPlayer.transform.position;

        if (!connected) Debug.Log("Connection ended");
    }

    void send(string msg)
    {
        sendMsg(msg);
        if (!hasError())
        {
            //Message was sent
            //Debug.Log("sent to server: " + msg);
        }
        else
        {
            //Cry
            Debug.Log("send failed: " + msg);
        }
    }

    void updateChat(string newChatMessage)
    {

    }

    void parseGameUpdateMessage(string newUpdateMessage)
    {
        //Debug.Log("updt:" + newUpdateMessage);

        int player = 0;

        string currentVar = "";
        Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);
        Quaternion rot = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

        int currentVarType = 0;

        string bombMsg = "";

        //Parse string, separating things by commas and spaces.
        //Strings to parse would be in this format: 0(9.09, 3.01, 2.02)(1.0, 1.0, 1.0, 1.0)
        for (int i = 1; i < newUpdateMessage.Length; i++)
        {

            //Set new variable to whatever if the current character is a space or start of a bracket 
            if (newUpdateMessage[i] == '(' || newUpdateMessage[i] == ' ')
            {

                //Read in player number
                switch (currentVarType)
                {
                    case 0: //Integer
                        player = int.Parse(currentVar);
                        currentVarType++;
                        break;

                    case 1: //Vector           
                        if (!float.TryParse(currentVar, out pos.x))
                        {
                            Debug.LogError("Failed to turn into float: " + currentVar);
                            //int.TryParse(currentVar, out pos.x);
                        }
                        currentVarType++;
                        break;

                    case 2:
                        pos.y = float.Parse(currentVar);
                        currentVarType++;
                        break;

                    case 3:
                        pos.z = float.Parse(currentVar);
                        currentVarType++;
                        break;

                    case 4: //Quaternion
                        rot.x = float.Parse(currentVar);
                        currentVarType++;
                        break;

                    case 5:
                        rot.y = float.Parse(currentVar);
                        currentVarType++;
                        break;

                    case 6:
                        rot.z = float.Parse(currentVar);
                        currentVarType++;
                        break;

                    case 7:
                        rot.w = float.Parse(currentVar);
                        currentVarType++;
                        break;
                    case 8: //Substring
                        bombMsg = newUpdateMessage.Substring(i - 3);
                        currentVarType++;
                        break;
                }

                //Reset variable string to parse

                currentVar = "";
            }
            else if (newUpdateMessage[i] == ')' || newUpdateMessage[i] == ',') //If it's any of these skip the character
            {
                //Do nothing
            }
            else //Else append onto current variable string
            {
                currentVar += newUpdateMessage[i];
            }



        }

        //The first character of every message signifies a different event
        switch (newUpdateMessage[0])
        {
            case 'c': //Chat

                break;

            case 't': //Update other players' transforms/velocties. (clientPlayerNumber) (Position) (Rotation)
                updatePlayerTransform(player, pos, rot);
                break;

            case 'b': //A player spawned a bomb; create it

                //Create same bomb player created
                parseBombMessage(player, bombMsg, pos, new Vector3(rot.x, rot.y, rot.z)); //Rot is velocity in this case...

                break;

            case 'd': //A player took damage; update their health and check if they're dead

                serverPlayers[player].setHealth(pos.x);
                serverPlayers[player].damage(0.0f, true); //<-- Don't deal damage but call this for animation and checking if dead
                break;

            case 'm': //A player picked up a bomb material. Destroy it at whatever location
                GameObject pd = GameObject.Find("PickupDestroyer");

                //Don't do this in the skirmish so materials keep respawning
                if (pd && SceneManager.GetActiveScene().buildIndex != 2)
                {
                    Instantiate(pd, pos, new Quaternion(1.0f, 1.0f, 1.0f, 1.0f));
                }

                break;

            case 'w': //A player is victorious. End the w and go back to the lobby.

                if (player < playerWins.Count && player > -1)
                {


                playerWins[player] = (int)pos.x;

                Debug.Log(newUpdateMessage);
                Debug.Log("P" + (player+1) + "Should have this many points: " + playerWins[player]);
                }

                winMessageTime = 4.0f;

                break;

            case 'j': //A player joined the lobby. Add another player.

                Debug.Log("A player joined. Currently " + player + " players in game.");
                Debug.Log("You are player " + clientPlayerNumber);

                serverPlayers.Add(new Player());
                playerWins.Add(0);
                spawnPlayer(player - 1);

                updateLeaderboard();

                break;

            case 'y': //You joined the game. Get own player index number and also total number of players.

                Debug.Log("You joined. " + player + " players in game.");

                clientPlayerNumber = player - 1;

                //Rename client player who at first will be player 1
                GameObject.Find("Player1").GetComponent<Player>().name = "Player" + player.ToString();


                //Spawn in the other already existing players
                for (int i = 0; i < player; i++)
                {
                    if (i == clientPlayerNumber)
                    {
                        serverPlayers.Add(clientPlayer);
                        playerWins.Add(0);
                        spawnPlayer(i);
                    }
                    else
                    {
                        serverPlayers.Add(new Player());
                        playerWins.Add(0);
                        spawnPlayer(i);
                    }
                }

                updateLeaderboard();

                break;

            case 'r': //A player is ready

                if (player < serverPlayers.Count)
                serverPlayers[player].ready = true;

                updateLeaderboard();

                break;

            case 's': //The game is going to start. Exit the lobby and go to the randomly chosen map number in this string.

                //Scene id in this case is the player number
                //changeScene(player);
                Debug.Log("Using seed: " + player);
                UnityEngine.Random.InitState(player);
                changeScene(3);
                

                break;

            case 'x': //A player quit the game. Shame on them. Remove whatever player that was.

                Debug.Log("Player " + player.ToString() + " disconnected.");

                //Player is the player index of whoever just left
                if (player < serverPlayers.Count && player != clientPlayerNumber)
                {
                Destroy(serverPlayers[player].gameObject);
                serverPlayers.RemoveAt(player);
                playerWins.RemoveAt(player);
                }



                //Decrease client player no if greater than client that disconnected.
                if (clientPlayerNumber > player)
                    clientPlayerNumber--;


                updateLeaderboard(true);

                break;




        }
    }

    //Call this whenever the scene changes. Player 1 will always be the client player the moment the scene starts. Afterwards they will be renamed
    void findClientPlayer()
    {
        clientPlayer = GameObject.Find("Player1").GetComponent<Player>();

        if (clientPlayer == null) Debug.LogError("Client player wasn't found.");

    }

    string getPlayerTransform()
    {
        //String output = player, position, rotation: 0(9.09, 3.01, 2.02)(1.0, 1.0, 1.0, 1.0)

        //Debug.Log(clientPlayerNumber.ToString() + clientPlayer.transform.position.ToString() + clientPlayer.transform.rotation.ToString());
        return "t" + clientPlayerNumber.ToString() + clientPlayer.transform.position.ToString() + clientPlayer.transform.rotation.ToString();
    }

    void updatePlayerTransform(int playerNo, Vector3 position, Quaternion rotation)
    {
        //Make sure not out of bounds
        if (playerNo > serverPlayers.Count - 1)
        {
            Debug.Log("Error: playerNo " + playerNo + " out of bounds");
            return;
        }

        if (serverPlayers[playerNo] != null)
        {
            serverPlayers[playerNo].transform.position = position;
            serverPlayers[playerNo].transform.rotation = rotation;
        }
        else
        {
            Debug.Log("Error: playerNo " + playerNo + " doesn't exist. Size of list is: " + serverPlayers.Count.ToString());
        }

    }

    public void sendPlayerHealth()
    {
        send("d" + clientPlayerNumber.ToString() + "(" + clientPlayer.getHealth().ToString() + " ");
    }

    //Called when a player joins or when a new scene starts
    void spawnPlayer(int newPlayerNumber)
    {

        //Check if the client player
        if (newPlayerNumber == clientPlayerNumber)
        {

            //Give proper player name/number
            clientPlayer.name = "Player" + (newPlayerNumber + 1).ToString();

            //Place player at proper spawn point
            clientPlayer.transform.position = spawnPositions[newPlayerNumber];

            //Server players gets updated
            serverPlayers[newPlayerNumber] = clientPlayer;

            Debug.Log("Client player is currently: " + serverPlayers[newPlayerNumber].name);

            return;
        }


        //Instansiate a new player from the prefab
        GameObject newPlayer = Instantiate(playerPrefab);

        //Place player at proper spawn point
        newPlayer.transform.position = spawnPositions[newPlayerNumber];

        //Give proper player name/number
        newPlayer.name = "Player" + (newPlayerNumber + 1).ToString();



        //Server players gets updated
        serverPlayers[newPlayerNumber] = newPlayer.GetComponent<Player>();

        Debug.Log("Created Online Opponent: " + serverPlayers[newPlayerNumber].name);

    }

    //Destroy the pickup the player collects at the given position
    public void sendPickUpMaterialMessage(int playerNum, Vector3 pos)
    {
        if (playerNum == clientPlayerNumber)
        {
            string msg = "m0(" + pos.ToString() + ")";
            send(msg);
        }
    }

    //Send message about spawning a bomb
    public void sendBombMessage(Vector3 pos, Vector3 vel, BombAttributes.BombData data)
    {
        //First get position and data
        string msg = "b0"+ pos.ToString() + new Quaternion(vel.x, vel.y, vel.z, 1.0f).ToString();

        msg += "(" + data.explosionScaleSpeed.x.ToString();
        msg += ", " + data.explosionLifetime.ToString();
        msg += ", " + data.explosionScaleLimit.ToString();
        msg += ", " + data.damage.ToString();
        msg += ", " + data.fire.ToString();
        msg += ", " + data.freeze.ToString();
        msg += ", " + data.smoke.ToString();
        msg += ", " + data.blackhole.ToString();
        msg += ", " + data.scatter.ToString();
        msg += ", " + data.MaxRange.ToString();
        msg += ", ";

        //Debug.Log("Sending bomb: " + msg);
        send(msg);
    }

    //Read in a bomb message and spawn it with whatever attributes
    void parseBombMessage(int playerNo, string msg, Vector3 pos, Vector3 vel)
    {

        Debug.Log("Bomb msg: " + msg);

        BombCraftingHandler bombCraftingHandler = GameObject.FindGameObjectsWithTag("BombList")[0].GetComponent<BombCraftingHandler>();
        GameObject bomb = bombCraftingHandler.bombs[0];

        float[] newAttribues = new float[10];

        string currentVarMsg = "";
        int currentVarIndex = 0;


        //First find the player that threw it to animate a throw
        /*if (playerNo < serverPlayers.Count)
        {
        if (serverPlayers[playerNo] != null && serverPlayers[playerNo].animator != null)
        serverPlayers[playerNo].animator.SetBool("throwing", true);
        }*/

        //Parse attribute message for a variety of bomb variables
        for (int i = 1; i < msg.Length; i++)
        {
            //New var on space
            if (msg[i] == ' ')
            {
                if (currentVarIndex < 10)
                    newAttribues[currentVarIndex] = float.Parse(currentVarMsg);

                //Reset current variable string and increase variable index
                currentVarMsg = "";
                currentVarIndex++;
            }
            else if (msg[i] == ',') //Do nothing on commas
            {

            }
            else
            {
                currentVarMsg += msg[i];
            }
        }


        //The new bomb is ready to be spawned.
        GameObject newBomb = Instantiate(bomb, pos, transform.rotation);

        newBomb.GetComponent<Rigidbody>().velocity = vel;

        Bomb newBombClass = newBomb.GetComponent<Bomb>();

        newBombClass.attributes = default(BombAttributes.BombData);

        //All attributes look at the array just used in the previous for loop.
        newBombClass.attributes.explosionScaleSpeed = new Vector3(15.0f, 15.0f, 15.0f); // Always the same :P
        newBombClass.attributes.explosionLifetime = newAttribues[1];
        newBombClass.attributes.explosionScaleLimit = newAttribues[2];
        newBombClass.attributes.damage = newAttribues[3];
        newBombClass.attributes.fire = (int)newAttribues[4];
        newBombClass.attributes.freeze = (int)newAttribues[5];
        newBombClass.attributes.smoke = (int)newAttribues[6];
        newBombClass.attributes.blackhole = (int)newAttribues[7];
        newBombClass.attributes.scatter = (int)newAttribues[8];
        newBombClass.attributes.MaxRange = newAttribues[9];



    }

    public void sendWinMessage(int winnerIndex)
    {
        //Is it a draw?
        if (winnerIndex == -1) send("w" + winnerIndex.ToString() + "(");

        //Only the winning player will send this message to the server. The losers will recieve.
        if (winnerIndex != clientPlayerNumber) return;

        playerWins[winnerIndex]++;

        Debug.Log("Wins: " + playerWins);

        send("w" + winnerIndex.ToString() + "(" + playerWins[winnerIndex] + "(");

    }

    public void sendReadyMessage()
    {
        //Send a message saying the player is ready and pick a random map index to potentially be chosen.
        send("r" + clientPlayerNumber.ToString() + "(");

        serverPlayers[clientPlayerNumber].ready = true;

        updateLeaderboard();
    }

    public void sendGameStartMessage()
    {
        //Send a message saying the player is ready and send a random number for seeding,so randomness is synced.

        int seed = UnityEngine.Random.Range(1, 199);

        send("s" + seed.ToString() + "(");
    }

     //Change the scene. This script should stay put and not unload after the scene change.
    void changeScene(int newSceneNumber)
    {
        hasChangedScene = true;

        SceneManager.LoadScene(newSceneNumber);


    }

    //SplitScreen script will call this function, because it ensures the other scene has loaded.
    public void postSceneChange()
    {
        //Make sure this function isn't called in the very first scene the object appears in
        if (!hasChangedScene || duplicate)
        {
            return;
        }

        Debug.Log("New scene loaded. Spawning players at spawn points");


        getSpawnPoints();
        findClientPlayer();

        //Spawn correct amount of player objects, make sure client is controlling the correct one
        for (int i = 0; i < serverPlayers.Count; i++)
        {
            spawnPlayer(i);
            Debug.Log("Spawned player " + i.ToString());
        }

        updateLeaderboard();

    }

    void getSpawnPoints()
    {

        spawnPositions[0] = GameObject.Find("Spawn1").transform.position;
        spawnPositions[1] = GameObject.Find("Spawn2").transform.position;
        spawnPositions[2] = GameObject.Find("Spawn3").transform.position;
        spawnPositions[3] = GameObject.Find("Spawn4").transform.position;

    }

    void updateLeaderboard(bool clear = false)
    {
        if (SceneManager.GetActiveScene().buildIndex != 2) return;

        Transform leaderboard = GameObject.Find("Canvas").transform.GetChild(2);
        
        //Make sure leaderboard exists. If not in the skirmish map, it won't.
        if (leaderboard)
        {
            //Clear it if set as an argument (For when players leave the game)
            if (clear)
            {
                for (int i = 0; i < 4; i++)
                {
                    Text nameText = leaderboard.GetChild(i).GetComponent<Text>();
                    nameText.text = "";

                    Text winText = nameText.gameObject.transform.GetChild(0).GetComponent<Text>();
                    winText.text = "";

                    Text readyText = nameText.gameObject.transform.GetChild(1).GetComponent<Text>();
                    readyText.text = "";
                }
            }

            for (int i = 0; i < serverPlayers.Count; i++)
            {
                if (serverPlayers[i] != null)
                {
                   
                    Text nameText = leaderboard.GetChild(i).GetComponent<Text>();
                    nameText.text = serverPlayers[i].name.ToString();
                
                    if (i == clientPlayerNumber) nameText.text += " (You)";

                    Text winText = nameText.gameObject.transform.GetChild(0).GetComponent<Text>();
                    winText.text = playerWins[i].ToString();

                    Text readyText = nameText.gameObject.transform.GetChild(1).GetComponent<Text>();
                    if (serverPlayers[i].ready) readyText.text = "Ready!";
                    else readyText.text = "";


                }
                else
                {
                        Debug.Log("Leaderboard doesn't exist yet?");
                        break;
                }

                
            }
        }

    }

    //Close socket when destroying a network client object. If a duplicate script, then this instance did not open a socket and doesn't need to be closed.
    void OnDestroy()
    {
        if (!duplicate && MenuBehavior.numPlayers <= 1)
        {
            send("x" + clientPlayerNumber.ToString());


            Debug.Log("Closing socket...");

            cleanUp();

            if (hasError())
            {
                print(getErrorMessage());
            }

            
            closed = true;
        }
    }


}