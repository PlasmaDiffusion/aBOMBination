using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialPickup : MonoBehaviour {

    private Player player;
    public int materialNo;

    //sound effects
    public AudioClip firePickup;
    public AudioClip icePickup;
    public AudioClip smokePickup;
    public AudioClip scatterPickup;
    public AudioClip tntPickup;
    public AudioClip gravityPickup;

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    void OnTriggerEnter(Collider other)
    {
        //Destroy self if theres a duplicate on top (Weird bug that means players get 2 materials)
        if (other.gameObject.CompareTag("MaterialPickup")) Destroy(gameObject);

        //Give player item to craft with

        if (other.gameObject.CompareTag("Player"))
        {
            bool found = false;

            player = other.gameObject.GetComponent<Player>();

			//First see if the player has the material id in the inventory
			for (int i = 0; i < 4; i++)
			{

				//If they do add onto it
				if (player.materialID[i] == materialNo)
				{
					player.materialCount[i] += 1;
					player.setMaterialCountText(i);
                    found = true;

					break;
				}



			}
            //If not then find an empty slot to add it to
            if (!found)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (player.materialID[i] == 0)
                    {
                        player.materialID[i] = materialNo;
                        player.materialCount[i] = 1;
                        player.setMaterialCountText(i);


                        //And also add the image
                        setMaterialImageByType(player, i);


                        break;
                    }
                }
            }

            //pickup sound effects
            switch (materialNo)
            {
                case 1:
                    AudioSource.PlayClipAtPoint(icePickup, transform.position);
                    break;
                case 2:
                    AudioSource.PlayClipAtPoint(firePickup, transform.position);
                    break;
                case 3:
                    AudioSource.PlayClipAtPoint(smokePickup, transform.position);
                    break;
                case 4:
                    AudioSource.PlayClipAtPoint(tntPickup, transform.position);
                    break;
                case 5:
                    AudioSource.PlayClipAtPoint(gravityPickup, transform.position);
                    break;
                case 6:
                    AudioSource.PlayClipAtPoint(scatterPickup, transform.position);
                    break;
            }


            /*
            //Spawn text saying what you picked up (unused rn cause it rotates with shit)
           GameObject textThing = Instantiate(GameObject.Find("BombCraftingHandler").GetComponent<BombCraftingHandler>().materialTextReference,
            transform);
            textThing.transform.parent = other.GetComponent<Player>().cameraHandler.transform.GetChild(1);
            textThing.GetComponent<DestroyOvertime>().setText(materialNo);

            //Only owner can see text
            if (other.name == "Player1") textThing.layer = 8;
            if (other.name == "Player2") textThing.layer = 9;
            if (other.name == "Player3") textThing.layer = 10;
            if (other.name == "Player4") textThing.layer = 11;
            */
            //Now end it all and destroy yourself :(

            //Show material description on player's HUD
            player.makeDescription(materialNo);



            Destroy(gameObject);
        }

    }

    void OnTriggerStay(Collider other)
    {
        //Destroy self if theres a duplicate on top (Weird bug that means players get 2 materials)
        if (other.gameObject.CompareTag("MaterialPickup")) Destroy(gameObject);
    }

    void setMaterialImageByType(Player p, int index)
    {
        if (p.onlineOpponent) return;

        BombCraftingHandler imagesReference;

        imagesReference = GameObject.Find("BombCraftingHandler").GetComponent<BombCraftingHandler>();

        p.materialImages[index].sprite = imagesReference.matTextures[materialNo - 1];



        //Also recolour the panel here
        player.materialPanels[index].color = GameObject.Find("BombCraftingHandler").GetComponent<BombCraftingHandler>().panelColours[materialNo];
    }

    //Unused because descs are images
    string getMaterialDescription()
    {

        string desc = "???";

        switch (materialNo)
        {
            case 1:
                desc = "Freeze";
                break;
            case 2:
                desc = "Fire: Damage overtime";
                break;
            case 3:
                desc = "Smoke Bomb";
                break;
            case 4:
                desc = "TNT: Larger radius";
                break;
            case 5:
                desc = "Gravity: Pulls opponents in";
                break;
            case 6:
                desc = "Scatter into small bombs";
                break;

        }

        return desc;
    }
}
