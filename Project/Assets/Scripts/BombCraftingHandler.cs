using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BombCraftingHandler : MonoBehaviour {

  //Every special bomb can go here
    public GameObject[] bombs;
    public Sprite[] matTextures;
    public Sprite[] bombSprites;
    public Sprite[] descriptionSprites;
    public Sprite emptySprite;

    public Color[] panelColours;

    public GameObject materialTextReference;


    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public GameObject getSpecificBomb(bool[] usingMaterial)
    {
        int bombToUse = 0;
        string ingredientString = "";

        for (int i = 0; i < usingMaterial.Length; i++)
        {
            if (usingMaterial[i]) ingredientString += "+";
            else ingredientString += "-";
            
        }
        if (ingredientString == "-++-") bombToUse = 1;
        

        return bombs[bombToUse];
    }
}
