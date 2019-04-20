using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {

    public string playerInputString = ""; //String to be added onto local input

    public int playerNumber;

    InputHandler input;

    public bool player1;
    public bool ready = false;

    //Online opponents vars
    public bool onlineOpponent = false;
    private Vector3 lastPos;

    //Required components
    public GameObject cameraHandler;
    private CameraController cameraController;
    private GameObject aimingQuad;
    public Animator animator;
    [HideInInspector]
    public CharacterController controller;


    //Colour reference for when player is frozen
    public Color color;

    //Public stuffs to tweak that go in the inspector
    public float m_Speed;   
    public float m_MaxSpeed;
    public float m_JumpSpeed;
    public float m_JumpLimit;
    public float m_RotationSpeed;
    public float m_Gravity;
    public float m_GravityLimit;

    private float forwardFaceVector;
    private float rightFaceVector;

    //Bomb stuff
    bool canThrow;
    [HideInInspector]
    public bool firstThrow = false;
    public float throwRechargeTime;
    private float throwRecharge;
    private float throwingPower;

    //Velocity movemnt stuff
    private float jumpVelocity = 0.0f;
    private float jumpOffset = 0.0f;
    private bool jumpTriggerPressed =false;
    private float velocityUp;

    private float velocityForward;
    private float velocityRight;

    [HideInInspector]
    public Vector3 explosionForce;

    //Booleans that check for when a button is released
    private bool[] dpadReleased = new bool[4]; //In case of accidental crafting from holding the button more than a frame
    private bool prevRT = false;
    private bool prevLT = false;
    private bool prevRB = false;
    private bool prevLB = false;

    private float health;
    private float invinciblityFrames;
    private bool dyingAnimation;

    private GameObject throwBar;

    private LaunchArcMesh throwArc;

    //Status effects
    private float stunned;
    private float burning;


    //Array for inventory.
    BombAttributes.BombData[] craftedBombs = new BombAttributes.BombData[5];

    //Current bomb being crafted.
    BombAttributes.BombData newerBomb;

    //Material variables for crafting
    public int[] materialCount;
    public int[] materialID;
    public bool[] activeCraftingMaterial;

    //Hud stuff -----------------------
    public GameObject hudReference;

     Text[] textForHUD;
     Text[] textForInventory;
     Image[] textDescriptions;
     Text healthText;
     Image selectedBombImage;
    [HideInInspector]
    public Image[] materialImages;
    [HideInInspector]
    public Image[] materialPanels;
    Image[] inventoryImages;
    Transform readyImage;

    private float descriptionTimeLimit = 2.5f;
    private float descriptionTime = 0.0f;
    //----------------------------------

    public GameObject bombHandlerReference;


    private GameObject bomb;
    private BombCraftingHandler bombHandler;

    int selectedBomb;


    public GameObject fireEmitterReference;
    private GameObject childFireEmitter;

    //Sound effects
    public AudioClip craftComplete;
    public AudioClip fireCraft;
    public AudioClip smokeCraft;
    public AudioClip iceCraft;
    public AudioClip scatterCraft;
    public AudioClip gravityCraft;
    public AudioClip tntCraft;
    public AudioClip bombThrow;
    public AudioClip walking;
    public AudioClip death;
    public AudioClip hit;
    public AudioClip victory;

    private ReadAndWriteStats statManager;


    // Use this for initialization
    void Start() {

        input = GameObject.Find("Input Handler").GetComponent<InputHandler>();

        //Find hud object  in scene
        findHUD();
        
        animator = transform.GetChild(0).GetComponent<Animator>();

        Renderer rend = transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>();

        color = rend.material.color;

        //Determine if player 1 for key input purposes. Reverse the values to give keyboard input to player 2.
        if (playerNumber == 0 && !onlineOpponent) player1 = true;
        else player1 = false;

        //Initialize character controller for movemnet
        controller = gameObject.GetComponent<CharacterController>();
        cameraController = cameraHandler.GetComponent<CameraController>();

        //Initialize some bomb throwing and bomb crafting related variables
        canThrow = true;
        firstThrow = false; //Unusued first throw thing
        bombHandlerReference = GameObject.FindGameObjectsWithTag("BombList")[0];
        bombHandler = bombHandlerReference.GetComponent<BombCraftingHandler>();
        selectedBomb = 0;

        statManager = GameObject.Find("GameManager").GetComponent<ReadAndWriteStats>();

        explosionForce = new Vector3(0.0f, 0.0f, 0.0f);

        //Child stuff for throw feedback
        if (!onlineOpponent)
        { 
        throwBar = cameraHandler.transform.GetChild(0).gameObject;
        aimingQuad = transform.GetChild(1).gameObject;
        throwArc = aimingQuad.GetComponent<LaunchArcMesh>();
        }

        //Inventory stuff
        for (int i = 0; i < 4; i++)
        {
            materialCount[i] = 0;
            materialID[i] = 0;

            if (!onlineOpponent)
            textForHUD[0].text = materialCount[i].ToString();

            activeCraftingMaterial[i] = false;

            dpadReleased[i] = false;
        }

        //Make sure bomb inventory structure stuff are at default values.
        for (int i = 0; i < 5; i++)
        {
            makeBombDefaults(ref craftedBombs[i]);
        }
        makeBombDefaults(ref newerBomb);
        setInventoryText();

        dyingAnimation = false;

        throwingPower = -0.1f;
        health = 100.0f;
        invinciblityFrames = 0.0f;
        //Status effect stuff
        stunned = 0.0f;
        burning = 0.0f;
    }

    // Update is called once per frame
    void Update() {

        manageStatusEffects();

        animator.SetBool("throwing", false);

        //Don't have any input if stunned
        if (stunned > 0.0f) {
            
            //Still make explosions do stuff though
            applyExplosionForce();
            controller.Move(new Vector3(0.0f, -0.1f, 0.0f));

            return;
                }


        //Also don't have any input if in the middle of dying :P
        if (dyingAnimation)
        {
            //Scale the player down

           transform.localScale = transform.localScale * 0.9f;

            //Once the player is scaled down detach the camera
            if (transform.localScale.y < 0.1f)
            {
                if (!onlineOpponent)
                cameraHandler.transform.GetChild(1).transform.parent = null;

                Destroy(gameObject);
            }

            return;
        }

        //Player dies if fallen
        if (transform.position.y < -50.0f)
        {
            damage(100.0f);
           // AudioSource.PlayClipAtPoint(death, transform.position);
            checkIfDead();
        }

        //Check if ready to end skrimish
        if ((Input.GetKeyUp(KeyCode.Return) && player1) || (input.controller[playerNumber].inputStart) || Input.GetKeyUp(KeyCode.Tab))
        {



            //Send winner to server
            GameObject networkGameObject = GameObject.Find("ConnectionHandler");

            if (networkGameObject != null)
            {
                NetworkClient networkObject = networkGameObject.GetComponent<NetworkClient>();

                networkObject.sendReadyMessage();
                
            }
            ready = true;

            if (readyImage.gameObject.activeSelf)
            {
                //Swap out images through changing transparency
                readyImage.parent.GetChild(9).GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                readyImage.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

            }
        }

        //Below this if statement is all input so if the player is an online opponent end this function.
        if (onlineOpponent)
        {
            //Just check if player is moving, if so animate them
            if (Mathf.Approximately(Vector3.Distance(lastPos, transform.position), 0)) animator.SetBool("moving", false);
            else animator.SetBool("moving", true);

            lastPos = transform.position;
            return;
        }

        //Wait for camera zoom for input
        if (!cameraController.controlling) return;

        //Movement input (Control sticks)
        //Debug.Log("Y: " + input.controller[playerNumber].leftStickY);
        velocityForward = (input.controller[playerNumber].leftStickY);
        //Debug.Log("X: " + input.controller[playerNumber].leftStickX);
        velocityRight = (input.controller[playerNumber].leftStickX);

        //Deadzone
        if (velocityForward < 0.2f && velocityForward > -0.2f) velocityForward = 0.0f;
        if (velocityRight < 0.2f && velocityRight > -0.2f) velocityRight = 0.0f;

        //Keyboard Movement input
        if (player1)
        {
            velocityForward = (Input.GetAxis("Vertical" + playerInputString));
            velocityRight = (Input.GetAxis("Horizontal" + playerInputString));
        }

        if (velocityForward != 0.0f || velocityRight != 0.0f)
        {
            forwardFaceVector = velocityForward; rightFaceVector = velocityRight;
            animator.SetBool("moving", true);
            //AudioSource.PlayClipAtPoint(walking, transform.position);
        }
        else
        {
            animator.SetBool("moving", false);
            //AudioSource.Pause();
        }

        velocityForward *= m_Speed;
        velocityRight *= m_Speed;
        

        //float oldYVel = m_Rigidbody.velocity.y;

        if (!firstThrow) //Lock movement that isn't related to turning if yet to throw spawn bomb
        {
            if (velocityUp > -m_Gravity)
            velocityUp -= (m_Gravity * Time.deltaTime);


            //Jump pressed
            if (input.controller[playerNumber].leftTrigger > 0.2 && !jumpTriggerPressed)
            {
                jumpTriggerPressed = true;
                player1 = false; //Once player 1 (keyboard user) uses the left trigger, the keyboard controls get disabled. Otherwise, the game will think the jump button is always being released.
                
            }
           

            if (((Input.GetKeyDown(KeyCode.LeftControl) && player1) || jumpTriggerPressed) && controller.isGrounded)
            {


                jumpVelocity = m_JumpSpeed;
                velocityUp = 0.0f;
                animator.SetBool("jumping", true);

            }

            //Jump release
            if (input.controller[playerNumber].leftTrigger < 0.1) jumpTriggerPressed = false;

            if ((Input.GetKeyUp(KeyCode.LeftControl) && player1) || (!jumpTriggerPressed  && !player1))
            {
                if (jumpVelocity > 0) jumpVelocity /= 2.0f;
            }

            if (jumpVelocity > 0.0f) jumpVelocity -= (1.0f);
            else
            {
                jumpVelocity = 0.0f;
                
            }

            if (velocityUp < m_JumpLimit && Time.deltaTime < 0.02f) //The delta time check prevents the jump from going berserk if the framerate tanks...
            velocityUp += (jumpVelocity * Time.deltaTime); //Jump velocity increases for every frame the button is held.
            //Debug.Log(jumpVelocity * Time.deltaTime);

            if (velocityUp < 0.0f) animator.SetBool("jumping", false);
            

            //Debug.Log(jumpVelocity);
            
            //controller.SimpleMove(new Vector3(velocityRight, 0.0f, velocityForward));
            controller.Move(cameraHandler.transform.forward * velocityForward * Time.deltaTime);
            controller.Move(cameraHandler.transform.right * velocityRight * Time.deltaTime);
            applyExplosionForce();
            controller.Move(new Vector3(0.0f,velocityUp, 0.0f));


         


        }

        Vector3 movement = (cameraHandler.transform.forward * forwardFaceVector) + (cameraHandler.transform.right * rightFaceVector);
        //movement = movement.normalized; dont normalize it'll kill the framerate :(
        if (movement != Vector3.zero)
        {
            Quaternion faceRotation = Quaternion.LookRotation(movement);
            transform.rotation = faceRotation;
        }


        //Mathf.LerpAngle(transform.rotation.ToAxisAngle, faceRotation.ToAngleAxis, 1.0f);



        //------------------------------------------------------------------------------

        //Cycle through inventory
        if ((Input.GetKeyUp(KeyCode.E) && player1) || (input.controller[playerNumber].inputRB && !prevRB))
        {
            selectedBomb++;

            if (selectedBomb > 4)
            {
                selectedBomb = 0;
            }
            //Debug.Log(selectedBomb.ToString() + " " + playerInputString);

            selectedBombImage.rectTransform.position = inventoryImages[selectedBomb].rectTransform.position;
            
        }
        //Cycle through inventory
        else if ((Input.GetKeyUp(KeyCode.Q) && player1) || (input.controller[playerNumber].inputLB && !prevLB))
        {
            selectedBomb--;

            if (selectedBomb < 0)
            {
                selectedBomb = 4;
            }
            //Debug.Log(selectedBomb);

            selectedBombImage.rectTransform.position = inventoryImages[selectedBomb].rectTransform.position;
        }

        //------------------------------------------------------------------------------

        //Prepare to throw bomb
        if ((Input.GetKey(KeyCode.Space) && player1) || input.controller[playerNumber].rightTrigger > 0.5)
        {

            if (throwingPower < 2.0f) throwingPower += (2.0f * Time.deltaTime);
            
            throwBar.transform.localScale = new Vector3(throwingPower, 0.1f, 0.1f);


            //Make the arc move with the y velocity
            throwArc.velocity = (3.5f * throwingPower) + throwingPower;
            throwArc.angle = 50;
            if (firstThrow) throwArc.velocity = 5.0f + (throwingPower * 5.0f);
            else if (velocityForward != 0.0f || velocityRight != 0.0f)
            {
                //Arc for when moving


                Vector3 newVelocity = transform.forward.normalized * (throwingPower * 1.1f);
                newVelocity += (cameraHandler.transform.forward * velocityForward * 0.75f) + (cameraHandler.transform.right * velocityRight * 0.75f);

                newVelocity.y += 4.0f * throwingPower; //Make it arc upwards a little

                throwArc.angle = 30;
                throwArc.velocity = newVelocity.magnitude;
            }

            throwArc.MakeArcMesh(throwArc.CalculateArcArray());

        

        }

        //Debug.Log(Input.GetAxis("Throw" + playerInputString).ToString());
        //Throw a bomb

        //if (((Input.GetKeyUp(KeyCode.Space) && player1) || (input.controller[playerNumber].rightTrigger < 0.5 && input.controller[playerNumber].rightTrigger > 0.0)) && canThrow && throwingPower > 0.0f)
        if (((Input.GetKeyUp(KeyCode.Space) && player1) || (input.controller[playerNumber].rightTrigger < 0.5 && prevRT)) && canThrow && throwingPower > 0.0f)
        {

            throwBar.transform.localScale = new Vector3(0.0f, 0.1f, 0.1f);
            AudioSource.PlayClipAtPoint(bombThrow, transform.position);

            //Special warp bomb for first throw
            if (firstThrow)
            {
                canThrow = false;
                throwRecharge = throwRechargeTime;
                throwBomb(true);

                throwBar.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.2f, 0.2f, 0.2f));
            }
            else
            {
                throwBomb();

                //Now prevent more from being thrown for a moment
                throwRecharge = throwRechargeTime;
                canThrow = false;

                throwBar.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.2f, 0.2f, 0.2f));
                throwArc.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.0f, 0.0f, 1.0f));
            }

            

        }

        //Check if able to throw again
        if (throwRecharge > 0)
        {
            throwRecharge -= 1 * Time.deltaTime;
            //Debug.Log(throwRecharge);

            if (throwRecharge <= 0)
            {
                canThrow = true;
                throwBar.GetComponent<Renderer>().material.SetColor("_Color", new Color(1.0f, 1.0f, 1.0f));
                throwArc.GetComponent<Renderer>().material.SetColor("_Color", new Color(1.0f, 0.156f, 0.0f, 0.541f));
            }
        }

        //Dpad/arrow keys crafting input -------------------------------------------------

        //Toggle crafting material 0
        if ((Input.GetKeyUp(KeyCode.UpArrow) && player1) || (dpadReleased[0] && input.controller[playerNumber].inputY))
        {
            addMaterial(materialID[0], 0);
            dpadReleased[0] = false;
        }

        //Toggle crafting material 1
        if ((Input.GetKeyUp(KeyCode.DownArrow) && player1) || (dpadReleased[1] && input.controller[playerNumber].inputA))
        {
            addMaterial(materialID[1], 1);
            dpadReleased[1] = false;
        }

        //Toggle crafting material 2
        if ((Input.GetKeyUp(KeyCode.RightArrow) && player1) || (dpadReleased[2] && input.controller[playerNumber].inputB))
        {
            addMaterial(materialID[2], 2);
            dpadReleased[2] = false;
        }
        //Toggle crafting material 3
        if ((Input.GetKeyUp(KeyCode.LeftArrow) && player1) || (dpadReleased[3] && input.controller[playerNumber].inputX))
        {
            addMaterial(materialID[3], 3);
            dpadReleased[3] = false;
        }


        //Exit game on escape
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            //Check if connected, if so destroy
            GameObject networkObject = GameObject.Find("ConnectionHandler");
            if (networkObject)
            {
                Debug.Log("Destroyed network object. Now exiting.");
                Destroy(networkObject);
            }

            SceneManager.LoadScene(0);
        }

        //Dpad input doesn't detect if a button was released. These if statements will emulate that. This prevents all materials of the same type from being added immediately.
        if (!input.controller[playerNumber].inputY) dpadReleased[0] = true;
        if (!input.controller[playerNumber].inputA) dpadReleased[1] = true;
        if (!input.controller[playerNumber].inputB) dpadReleased[2] = true;
        if (!input.controller[playerNumber].inputX) dpadReleased[3] = true;

        //Other input release checks
        if (input.controller[playerNumber].inputRB) prevRB = true;
        else prevRB = false;
        if (input.controller[playerNumber].inputLB) prevLB = true;
        else prevLB = false;
        if (input.controller[playerNumber].rightTrigger > 0.0) prevRT = true;
        else prevRT = false;

        //Finally, remove material descriptions overtime
        if (descriptionTime > 0.0f)
        {
            descriptionTime -= Time.deltaTime;

            if (descriptionTime<= 0.0f)
            {
                makeDescription(0);
            }
        }
    }

    //Toggling isn't a thing anymore, so this will be unused.
    void toggleCraftingMaterials(int materialNo)
    {

        //Toggle crafting item to On  (But not if the player has none of that material)
        if (!activeCraftingMaterial[materialNo] && materialCount[materialNo] > 0)
        {
            activeCraftingMaterial[materialNo] = true;

            //Colour text for active materials
            switch (materialNo)
            {
                case 0:
                    textForHUD[0].color = new Color(0.0f, 0.0f, 1.0f);
                    break;
                case 1:
                    textForHUD[1].color = new Color(1.0f, 0.0f, 0.0f);
                    break;
                case 2:
                    textForHUD[2].color = new Color(1.0f, 1.0f, 0.0f);
                    break;
                case 3:
                    textForHUD[3].color = new Color(0.0f, 1.0f, 0.0f);
                    break;
            }

        }
        else
        {
            activeCraftingMaterial[materialNo] = false;

            //Make text white for unactive materials
            textForHUD[materialNo].color = new Color(1.0f, 1.0f, 1.0f);
        }

        //Make sure number is correct on HUD
        setMaterialCountText(materialNo);
    }

    void OnCollisionEnter(Collision collision)
    {
      
        //Access all contact points from collision
        if (collision.contacts.Length > 0)
        {
            //Debug.Log("Collision");
            if (Vector3.Dot(transform.up, collision.contacts[0].normal) > 0.5f)
            {
                Debug.Log("Grounded");
            }
        }


    }

    public void setMaterialCountText(int slotNumber)
    {
        if (onlineOpponent) return;

        textForHUD[slotNumber].text = materialCount[slotNumber].ToString();

    }

    void addMaterial(int newMaterialID, int materialSlot)
    {
        //Don't do anything if the material slot is actually empty OR the bomb already has 4 materials added to it
        if (materialCount[materialSlot] <= 0 || craftedBombs[selectedBomb].materialsAdded >= 4) return;


        craftedBombs[selectedBomb].materialsAdded += 1;
        //Every material's added effect happens here!
        switch (newMaterialID)
        {
            
            default:
                break;
            case 1:

                craftedBombs[selectedBomb].freeze++;
                statManager.iceMaterialsUsed++;
                AudioSource.PlayClipAtPoint(iceCraft, transform.position);


                break;
            case 2:
                craftedBombs[selectedBomb].fire += 1;
                statManager.fireMaterialsUsed++;
                AudioSource.PlayClipAtPoint(fireCraft, transform.position);

                break;
            case 3:
                craftedBombs[selectedBomb].smoke += 1;
                statManager.smokeMaterialsUsed++;
                AudioSource.PlayClipAtPoint(smokeCraft, transform.position);

                break;
            case 4:
                craftedBombs[selectedBomb].explosionScaleLimit += 3.0f;
                statManager.explosionMaterialsUsed++;
                AudioSource.PlayClipAtPoint(tntCraft, transform.position);
                break;
            case 5:
                craftedBombs[selectedBomb].blackhole += 1;
                statManager.blackholeMaterialsUsed++;
                AudioSource.PlayClipAtPoint(gravityCraft, transform.position);

                break;
            case 6:
                craftedBombs[selectedBomb].scatter += 1;
                statManager.scatterMaterialsUsed++;
                AudioSource.PlayClipAtPoint(scatterCraft, transform.position);

                break;
        }

        //Add in id to display image later

        //To do this, find an empty slot to add the material onto
        for (int i = 0; i < 4; i++)
        {
           
            if(craftedBombs[selectedBomb].materialIDs[i] == -1)
            {

                craftedBombs[selectedBomb].materialIDs[i] = (newMaterialID - 1);

                break;
            }
        }

     
        //Set the flag for the new bomb to now be craftable
        craftedBombs[selectedBomb].hasIngredient = true;

        //Remove material
        materialCount[materialSlot]--;

        //Remove material type from HUD and inventory if all out
        if (materialCount[materialSlot] == 0)
        {
            materialID[materialSlot] = 0;

            materialImages[materialSlot].sprite = bombHandler.emptySprite;

            materialPanels[materialSlot].color = new Color(0.33f, 0.33f, 0.33f, 1.0f);
        }

        setInventoryText();

        //Update text
        setMaterialCountText(materialSlot);
        

    }

    //Make a bomb
    void throwBomb(bool first = false)
    {
        statManager.bombsThrown++;

        //First get a few transformations ready for spawning the bomb
        Transform bombTransform = transform;
        

        Vector3 forwardOffset = transform.forward;

        bombTransform.position.Set(bombTransform.position.x + forwardOffset.x, bombTransform.position.y + 5.0f, bombTransform.position.z + forwardOffset.z);
        
        
        
       
        //Now see what bomb the player is going to craft
        bomb = bombHandler.getSpecificBomb(activeCraftingMaterial);

        //The new bomb is ready to be spawned!
        GameObject newBomb = Instantiate(bomb, forwardOffset + transform.position, transform.rotation);

        Bomb newBombClass = newBomb.GetComponent<Bomb>();
        newBombClass.ThrowingPlayer = gameObject;

        if (craftedBombs[selectedBomb].hasIngredient)
        { 

        //Transfer all bomb attributes here from the player's inventory data to the new bomb
        newBombClass.attributes = craftedBombs[selectedBomb];

            newBombClass.modifyRimLight();

        //Reset bomb now that it's been used

        craftedBombs[selectedBomb].count--;

            makeBombDefaults(ref craftedBombs[selectedBomb]);

            setInventoryText();
        }
        else
        {
            //If an empty inventory slot was selected, throw in a weak regular bomb
            newBombClass.attributes = default(BombAttributes.BombData);
            makeBombDefaults(ref newBombClass.attributes);
        }

        if (first)
        {
            newBombClass.First = true;
            newBombClass.attributes.MaxRange = 5.0f;
            newBombClass.time = throwingPower * newBombClass.attributes.MaxRange * 0.5f;
        }
        Vector3 newVelocity = forwardOffset.normalized * ((throwingPower * 4.0f * newBombClass.attributes.MaxRange) + 1.1f);
        newVelocity += (cameraHandler.transform.forward * velocityForward) + (cameraHandler.transform.right * velocityRight);

        //Arc upwards quite a bit if standing still
        //if (velocityForward == 0.0f && velocityRight == 0.0f)
            newVelocity.y += 8.0f;
       // else newVelocity.y += 5.0f; //Otherwise just a little

        //Influence throw based on how long the button was held
        newVelocity.y += throwingPower * newBombClass.attributes.MaxRange;

        //throwArc.spawnTarget();
        throwArc.velocity = 0.05f;
        throwArc.MakeArcMesh(throwArc.CalculateArcArray());

        newBomb.GetComponent<Rigidbody>().velocity = newVelocity;

        Debug.Log("BOMB " + newBombClass.attributes.ToString());

        GameObject networkGameObject = GameObject.Find("ConnectionHandler");

        if (networkGameObject != null)
        {
            NetworkClient networkObject = networkGameObject.GetComponent<NetworkClient>();
            if (networkObject)
            {
                if (networkObject.connected)
                {

                    //Send a message to create a bomb on other clients
                    networkObject.sendBombMessage(newBombClass.transform.position, newVelocity, newBombClass.attributes);
                }
            }

        }




        throwingPower = -0.1f;

        animator.SetBool("throwing", true);


    }



    void makeBombDefaults(ref BombAttributes.BombData bombToReset)
    {
        //Set some stuff to 0 and some specific stuff to 
        bombToReset = default(BombAttributes.BombData);
        bombToReset.explosionScaleSpeed = new Vector3(15.0f, 15.0f, 15.0f);
        bombToReset.explosionScaleLimit = 15.0f;
        bombToReset.explosionLifetime = 3.0f;
        bombToReset.fire = 0;
        bombToReset.freeze = 0;
        bombToReset.blackhole = 0;
        bombToReset.scatter = 0;
        bombToReset.materialsAdded = 0;
        bombToReset.damage = 25.0f;
        bombToReset.MaxRange = 1.0f;

        bombToReset.materialIDs = new int[4];
        bombToReset.materialIDs[0] = -1;
        bombToReset.materialIDs[1] = -1;
        bombToReset.materialIDs[2] = -1;
        bombToReset.materialIDs[3] = -1;
    }
 
    void setInventoryText()
    {
        if (onlineOpponent) return;

        for (int i =0; i < 5; i++)
        {
           
                textForInventory[i].text = craftedBombs[i].count.ToString();


                  BombCraftingHandler imagesReference;

                imagesReference = GameObject.Find("BombCraftingHandler").GetComponent<BombCraftingHandler>();



             if (craftedBombs[i].materialsAdded > 0)
            {
               inventoryImages[i].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

               inventoryImages[i].sprite = imagesReference.bombSprites[craftedBombs[i].materialsAdded - 1];

            }
             else
            {
                inventoryImages[i].color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            }

                //Put materials the bomb has too
                for (int j = 0; j < 4; j++)
                {
                    if (craftedBombs[i].materialIDs.Length != 4) break;

                    Image materialImage = inventoryImages[i].rectTransform.GetChild(j).GetComponent<Image>();

                    if (craftedBombs[i].materialIDs[j] != -1)
                    {
                        materialImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                        materialImage.sprite = imagesReference.matTextures[craftedBombs[i].materialIDs[j]];
                    }
                    else
                    {
                        //Make invisible if nothings there
                        materialImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                        materialImage.sprite = null;
                    }
                }

            
            

        }

    }

    public void damage(float damageAmount, bool animate = false)
    {
        //Damage player if they don't have invinciblity frames. Fire ignores these frames and is occuring if animate was not set to true
        if (invinciblityFrames <= 0.0f || !animate)
            health -= damageAmount;

        //Only send health update message if the client player took damage to prevent unfair cases.
        if (!onlineOpponent)
        {
            //Update health
            healthText.text = health.ToString();

            //Send new health to server
            GameObject networkGameObject = GameObject.Find("ConnectionHandler");
            if (networkGameObject != null)
            {
                NetworkClient networkObject = networkGameObject.GetComponent<NetworkClient>();
                networkObject.sendPlayerHealth();
            }
        }

        //Fire does not trigger the animation or invinciblity frames
        if (animate)
        {
            invinciblityFrames = 0.2f;

            if(!onlineOpponent)
            AudioSource.PlayClipAtPoint(hit, transform.position);

            if (animator != null)
            animator.SetBool("damaged", true);
        }
        

        
    }

    public void checkIfDead()
    {
        //If the player did die...
        if (health <= 0)
        {
            //Turn off burning
            if (burning > 0)
            { 
            burning = 0.1f;
            }

            AudioSource.PlayClipAtPoint(death, transform.position);
            //Play a dying animation (shrinking)
            dyingAnimation = true;

            //And check if there's a winner now (unless in skirmish)
            GameManager manager = GameObject.Find("GameManager").GetComponent<GameManager>();

            if (!manager.inSkirmish)
            manager.checkForWinner();
            else //Respawn in skirmish (Or any other mode)
            {
                health = 100;

                if (!onlineOpponent)
                healthText.text = health.ToString();


                dyingAnimation = false;
                transform.position = GameObject.Find("SpawnPlatform").transform.position + new Vector3(0.0f, 10.0f, 0.0f);
            }
        }
    }

    void manageStatusEffects()
    {
        //Invinciblity frames for animation
        if (invinciblityFrames > 0.0f) invinciblityFrames -= Time.deltaTime;
        else animator.SetBool("damaged", false);

        if (burning > 0.0f)
        {
            
            damage(1.3f * Time.deltaTime);
            checkIfDead();
            burning -= 1.0f * Time.deltaTime;
            childFireEmitter.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            //Get rid of fire particles
            if (burning <= 0.0f)
            {
                Destroy(childFireEmitter);
            }
        }

        if (stunned > 0.0f)
        {
            stunned -= 1.0f * Time.deltaTime;

            //Recolour if run out
            if (stunned <= 0.0f)
            {

                

                //Renderer rend = GetComponent<Renderer>();
                Renderer rend = transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>();

                rend.material.color = color;
                /*
                rend.material.SetColor("_Color", new Color(0.309f, 0.0f, 0.0f));
                if (playerInputString == "P2")
                {
                    rend.material.SetColor("_Color", new Color(1.0f, 1.0f, 0.317f));
                    rend.material = material;
                }
                if (playerInputString == "P3")
                {
                    rend.material.SetColor("_Color", new Color(1.0f, 1.0f, 0.317f));
                }
                if (playerInputString == "P4")
                {
                    rend.material.SetColor("_Color", new Color(1.0f, 1.0f, 0.317f));
                }*/

            }
        }
    }

    //Add a status effect based on a number. Duration is based on ticks * delta time
    public void addStatusEffect(int statusID, float duration)
    {
        switch (statusID)
        {
            case 1:

                burning = duration;

                if (childFireEmitter == null)
                childFireEmitter = Instantiate(fireEmitterReference, transform);
   

                break;

            case 2:

                stunned = duration;
                //Renderer rend = GetComponent<Renderer>();
                Renderer rend = transform.GetChild(0).GetChild(1).GetComponent<Renderer>();
                rend.material.SetColor("_Color", Color.cyan);

                break;


        }
    }

    void applyExplosionForce()
    {

        if (explosionForce.x != 0.0f) explosionForce.x *= 0.9f;

        if (explosionForce.y != 0.0f) explosionForce.y *= 0.9f;

        if (explosionForce.z != 0.0f) explosionForce.z *= 0.9f;

        controller.Move(explosionForce);
    }

    void findHUD()
    {
        if (onlineOpponent) return;


        Transform wheel = hudReference.transform.Find("HUD_WheelSprite");
        Transform inventory = hudReference.transform.Find("HUD_InventorySprite");
        Transform healthBar = hudReference.transform.Find("HealthBarImage");
        Transform descriptions = hudReference.transform.Find("HUD_Descriptions");


        materialPanels = new Image[4];

        materialPanels[0] = hudReference.transform.GetChild(0).GetComponent<Image>();
        materialPanels[1] = hudReference.transform.GetChild(1).GetComponent<Image>();
        materialPanels[2] = hudReference.transform.GetChild(2).GetComponent<Image>();
        materialPanels[3] = hudReference.transform.GetChild(3).GetComponent<Image>();

        if (wheel == null || inventory == null || healthBar == null) return;

        textForHUD = new Text[4];

        //Wheel stuff
        textForHUD[0] = wheel.GetChild(0).GetComponent<Text>();
        textForHUD[1] = wheel.GetChild(1).GetComponent<Text>();
        textForHUD[2] = wheel.GetChild(2).GetComponent<Text>();
        textForHUD[3] = wheel.GetChild(3).GetComponent<Text>();

        materialImages = new Image[4];

        materialImages[0] = wheel.GetChild(4).GetComponent<Image>();
        materialImages[1] = wheel.GetChild(5).GetComponent<Image>();
        materialImages[2] = wheel.GetChild(6).GetComponent<Image>();
        materialImages[3] = wheel.GetChild(7).GetComponent<Image>();


        //Health stuff
        healthText = healthBar.GetChild(0).GetChild(0).GetComponent<Text>();

        //Dreaded Inventory stuff
        selectedBombImage = inventory.GetChild(0).GetComponent<Image>();

        textForInventory = new Text[5];

        textForInventory[0] = inventory.GetChild(1).GetComponent<Text>();
        textForInventory[1] = inventory.GetChild(2).GetComponent<Text>();
        textForInventory[2] = inventory.GetChild(3).GetComponent<Text>();
        textForInventory[3] = inventory.GetChild(4).GetComponent<Text>();
        textForInventory[4] = inventory.GetChild(5).GetComponent<Text>();

        inventoryImages = new Image[5];

        inventoryImages[0] = inventory.GetChild(6).GetComponent<Image>();
        inventoryImages[1] = inventory.GetChild(7).GetComponent<Image>();
        inventoryImages[2] = inventory.GetChild(8).GetComponent<Image>();
        inventoryImages[3] = inventory.GetChild(9).GetComponent<Image>();
        inventoryImages[4] = inventory.GetChild(10).GetComponent<Image>();

        textDescriptions = new Image[4];

        //Bomb Material description stuff
        for (int i = 0; i < 4; i++)
        {
            textDescriptions[i] = descriptions.GetChild(i).GetComponent<Image>();

            textDescriptions[i].gameObject.SetActive(false);
        }

        //Ready image (Skirmish only)
        readyImage = hudReference.transform.Find("ReadyImage");


        if (MenuBehavior.numPlayers <= 2) readyImage.GetComponent<RectTransform>().localPosition += new Vector3(0.0f, -100.0f);

        GameManager manager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (!manager.inSkirmish) readyImage.gameObject.SetActive(false);
    }
        //Getters
    public float getHealth() { return health; }
    public void setHealth(float newHealth) { health = newHealth; }
    public void playVictoryAnimation()
    {
        animator.SetBool("won", true);
        AudioSource.PlayClipAtPoint(victory, transform.position);
    }

    public void makeDescription(int matID)
    {
        if (onlineOpponent) return;


            bool success = false;

        //Hide the first visible description if time ran out
        if (matID == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                if (textDescriptions[i].gameObject.activeSelf)
                {

                    textDescriptions[i].gameObject.SetActive(false);

                    descriptionTime = descriptionTimeLimit;

                    return;

                }
            }
        }

        //If not hiding, then a new description has appeared. Set the time limit for it.


        //Find the first description panel that is invisible.
        for (int i = 0; i < 4; i++)
        {
            //If invisible (inactive), text becomes the new description variable.
            if (!textDescriptions[i].gameObject.activeSelf)
            {
                //Update text
                textDescriptions[i].sprite = bombHandler.descriptionSprites[matID];

                success = true;

                
                textDescriptions[i].gameObject.SetActive(true);
                break;
            }


        }


        //If nothing is visible, overwrite the last one
        if (!success)
        {
            

            textDescriptions[3].sprite = bombHandler.descriptionSprites[matID];
            
            textDescriptions[3].gameObject.SetActive(true);

            //Don't refresh the timer here cause the list is full.
        }
        else 
        {
            //Refresh description timer
            descriptionTime = descriptionTimeLimit;
        }

    }

    void OnDestroy()
    {
        if (onlineOpponent)
        Debug.Log("Destroyed online opponent at scene: " + SceneManager.GetActiveScene().name);
    }


}
