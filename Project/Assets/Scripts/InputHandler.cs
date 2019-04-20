using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class InputHandler : MonoBehaviour {
    [DllImport("InputPlugin")]
    public static extern void createControllers(int amount);
    [DllImport("InputPlugin")]
    public static extern void updateControllers();
    [DllImport("InputPlugin")]
    public static extern bool getButton(int player, int button);
    [DllImport("InputPlugin")]
    public static extern bool getDpad(int player, int button);
    [DllImport("InputPlugin")]
    public static extern float getThumbstick(int player, int axis);
    [DllImport("InputPlugin")]
    public static extern float getTrigger(int player, int trigger);
    [DllImport("InputPlugin")]
    public static extern void removeControllers();


    //A: 0, B: 1, Y: 2, X: 3, RB: 4, LB: 5, Start: 6, Back: 7, RS: 8, LS: 9

    public int controllerCount = 4;

    public struct ControllerInput
    {
        public bool inputA;
        public bool inputB;
        public bool inputX;
        public bool inputY;

        public bool inputStart;
        public bool inputBack;

        public bool inputLeftStick;
        public bool inputRightStick;

        public bool inputDpadRight;
        public bool inputDpadLeft;
        public bool inputDpadUp;
        public bool inputDpadDown;

        public bool inputRB;
        public bool inputLB;

        public float rightTrigger;
        public float leftTrigger;

        public float leftStickX;
        public float leftStickY;

        public float rightStickX;
        public float rightStickY;
    }

    public ControllerInput[] controller;



    // Use this for initialization
    void Start ()
    {
        controller = new ControllerInput[controllerCount];

        defaultInputs();

        createControllers(controllerCount);

        Debug.Log("created controllers");
    }

    private void defaultInputs()
    {
        for (int i = 0; i < controllerCount; i++)
        {


            //Default values
            controller[i].inputA = false;
            controller[i].inputB = false;
            controller[i].inputX = false;
            controller[i].inputY = false;

            controller[i].inputStart = false;
            controller[i].inputBack = false;

            controller[i].inputLeftStick = false;
            controller[i].inputRightStick = false;

            controller[i].inputDpadRight = false;
            controller[i].inputDpadLeft = false;
            controller[i].inputDpadUp = false;
            controller[i].inputDpadDown = false;
            controller[i].inputRB = false;
            controller[i].inputLB = false;

            controller[i].rightTrigger = 0.0f;
            controller[i].leftTrigger = 0.0f;

            controller[i].leftStickX = 0.0f;
            controller[i].leftStickY = 0.0f;

            controller[i].rightStickX = 0.0f;
            controller[i].rightStickY = 0.0f;
        }
    }


    // Update is called once per frame
    void Update () {

        updateControllers();

        
        //Get all new inputs for every controller
        for (int i = 0; i < controllerCount; i++)
        {

            try
            {
            //A: 0, B: 1, Y: 2, X: 3, RB: 4, LB: 5, Start: 6, Back: 7, RS: 8, LS: 9
            controller[i].inputA = getButton(i, 0);
            controller[i].inputB = getButton(i, 1);
            controller[i].inputY = getButton(i, 2);
            controller[i].inputX = getButton(i, 3);

            controller[i].inputRB = getButton(i, 4);
            controller[i].inputLB = getButton(i, 5);

            controller[i].inputStart = getButton(i, 6);
            controller[i].inputBack = getButton(i, 7);

            controller[i].inputRightStick = getButton(i, 8);
            controller[i].inputLeftStick = getButton(i, 9);

            controller[i].leftStickX = getThumbstick(i, 0);
            controller[i].leftStickY = getThumbstick(i, 1);

            controller[i].rightStickX = getThumbstick(i, 2);
            controller[i].rightStickY = getThumbstick(i, 3);

            controller[i].leftTrigger = getTrigger(i, 0);
            controller[i].rightTrigger = getTrigger(i, 1);

            controller[i].inputDpadUp = getDpad(i, 0);
            controller[i].inputDpadDown = getDpad(i, 1);
            controller[i].inputDpadRight = getDpad(i, 2);
            controller[i].inputDpadLeft = getDpad(i, 3);
            }
            catch (System.NullReferenceException ex)
            {
                Debug.LogError(ex);
                defaultInputs();
                continue;
            }


        }




        crapUnityInputUpdate(0, "", true, false, false, false);
        crapUnityInputUpdate(1, "P2", true, false, false, false);
        crapUnityInputUpdate(2, "P3", true, false, false, false);
        crapUnityInputUpdate(3, "P4", true, false, false, false);


        /*Examples of detecting input, taken from my older project
        for (int i = 0; i < controllerCount; i++)
        {


            if (controller[i].inputA) drawCircle(344, 85, i);
            if (controller[i].inputY) drawCircle(344, 36, i);
            if (controller[i].inputX) drawCircle(320, 60, i);
            if (controller[i].inputB) drawCircle(369, 62, i);

            if (controller[i].inputLeftStick) drawCircle(159 + (controller[i].leftStickX * 5.0f), 60 + (controller[i].leftStickY * -5.0f), i);
            if (controller[i].inputRightStick) drawCircle(300 + (controller[i].rightStickX * 5.0f), 115 + (controller[i].rightStickY * -5.0f), i);

            if (controller[i].inputDpadRight) drawCircle(226, 119, i);
            if (controller[i].inputDpadLeft) drawCircle(180, 119, i);
            if (controller[i].inputDpadDown) drawCircle(204, 139, i);
            if (controller[i].inputDpadUp) drawCircle(204, 97, i);


            if (controller[i].inputBack) drawCircle(229, 60, i);
            if (controller[i].inputStart) drawCircle(279, 60, i);

            if (controller[i].rightTrigger != 0.0f) drawCircle(350, 0, i);
            if (controller[i].leftTrigger != 0.0f) drawCircle(154, 0, i);

            if (controller[i].inputRB) drawCircle(345, 10, i);
            if (controller[i].inputLB) drawCircle(159, 10, i);

        }*/
    }

    //Overwrite plugin input with Unity's regular input. Boolean arguments can be used if going for only specific buttons/axis
    void crapUnityInputUpdate(int i, string playerInputString, bool LS = true, bool RS = true, bool buttons = true, bool Triggers = true)
    {
        if (buttons)
        {


        controller[i].inputA = (Input.GetAxis("D-PadVertical" + playerInputString) == 1);
        controller[i].inputB = (Input.GetAxis("D-PadVertical" + playerInputString) == -1);
        controller[i].inputY = (Input.GetAxis("D-PadHorizontal" + playerInputString) == 1);
        controller[i].inputX = (Input.GetAxis("D-PadHorizontal" + playerInputString) == -1);

        controller[i].inputRB = Input.GetButtonUp("CycleRight" + playerInputString);
        controller[i].inputLB = Input.GetButtonUp("CycleLeft" + playerInputString);

        //controller[i].inputStart = getButton(i, 6);
        //controller[i].inputBack = getButton(i, 7);
        }



        //controller[i].inputRightStick = Input.GetButtonUp("CycleRight" + playerInputString);
        //controller[i].inputLeftStick = Input.GetButtonUp("CycleLeft" + playerInputString);


        if (LS)
        {
        controller[i].leftStickX = Input.GetAxis("Horizontal" + playerInputString);
        controller[i].leftStickY = Input.GetAxis("Vertical" + playerInputString);
        }

        if (RS)
        {
        controller[i].rightStickX = Input.GetAxis("RightStickH" + playerInputString);
        controller[i].rightStickY = Input.GetAxis("RightStickV" + playerInputString);
        }

        if (Triggers)
        {
        controller[i].leftTrigger = Input.GetAxisRaw("Jump" + playerInputString);
        controller[i].rightTrigger = Input.GetAxis("Throw" + playerInputString);
        }

        //controller[i].inputDpadUp = getDpad(i, 0);
        //controller[i].inputDpadDown = getDpad(i, 1);
        //controller[i].inputDpadRight = getDpad(i, 2);
        //controller[i].inputDpadLeft = getDpad(i, 3);
    }

    void OnDestroy()
    {
        removeControllers();
    }
}
