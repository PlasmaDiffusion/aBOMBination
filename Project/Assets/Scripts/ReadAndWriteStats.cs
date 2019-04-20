using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using UnityEngine;


public class ReadAndWriteStats : MonoBehaviour {

    string readString;

    int currentStatIndex;

    //Stats to log
    private int[] integerStats;

    
    public int gamesPlayed;
    public int bombsThrown;
    public int bombsCrafted;


    public int fireMaterialsUsed;
    public int iceMaterialsUsed;
    public int smokeMaterialsUsed;
    public int explosionMaterialsUsed;
    public int blackholeMaterialsUsed;
    public int scatterMaterialsUsed;


    [DllImport("Logger And Reader")]
    public static extern IntPtr readDialogue(string fileName);


    [DllImport("Logger")]
    public static extern void Log(string CharName, string ItemName, string Value);




    // Use this for initialization
    void Start () {

        //Debug.Log("About to read");

         readString = Marshal.PtrToStringAnsi(readDialogue("Logger/stats.txt"));


        //Debug.Log("Just read: " + readString);

        integerStats = new int[9];
        currentStatIndex = 0;

        for (int i = 0; i < 9; i++) integerStats[i] = 0;



        extractStats();
    }
	
	// Update is called once per frame
	void Update () {
	
        


	}

    void extractStats()
    {
        if (readString == null) return;

        string currentStatString = "";
        

       //Debug.Log("length" + readString.Length.ToString());

        //Add each stat from the string until a \n
        for (int i = 0; i < readString.Length; i++)
        {
            if (readString[i] != '\n')
                currentStatString += readString[i];
            else
            {
                int parse = 0;

                //Debug.Log("About to parse: " + currentStatString.ToString());

                if (!Int32.TryParse(currentStatString, out parse)) continue;


                integerStats[currentStatIndex] = parse;
                //Debug.Log("Parsed int: " + integerStats[currentStatIndex].ToString());
                currentStatString = "";

                
                

                currentStatIndex++;

                if (currentStatIndex > 8) break;
            }
        }

        //Once everything is extracted put the data into the actual variables
        gamesPlayed = integerStats[0];
        bombsThrown = integerStats[1];
        bombsCrafted = integerStats[2];

        fireMaterialsUsed = integerStats[3];
        iceMaterialsUsed = integerStats[4];
        smokeMaterialsUsed = integerStats[5];
        explosionMaterialsUsed = integerStats[6];
        blackholeMaterialsUsed = integerStats[7];
        scatterMaterialsUsed = integerStats[8];


    }

    public void writeStats()
    {
        //First get variables into array
        integerStats[0] = gamesPlayed;
        integerStats[1] = bombsThrown;
        integerStats[2] = bombsCrafted;

        integerStats[3] = fireMaterialsUsed;
        integerStats[4] = iceMaterialsUsed;
        integerStats[5] = smokeMaterialsUsed;
        integerStats[6] = explosionMaterialsUsed;
        integerStats[7] = blackholeMaterialsUsed;
        integerStats[8] = scatterMaterialsUsed;

        //Then get a string ready to write to the file
        string output = "";

        for (int i = 0; i < integerStats.Length; i++)
        {
            output += integerStats[i].ToString();

            //Add an end of line (unless it's the last one)
            if (i!= integerStats.Length -1) output += '\n';
        }

        //Write variables to the file
        Log("Logger", "stats", output);

    }

}
