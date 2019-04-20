using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeGenerator : MonoBehaviour
{
    public int life = 0;
    public GameManager TimeManager;
    public AudioClip platformDrop;

    private bool dead = false;


    // Use this for initialization
    void Start()
    {
        TimeManager = GameObject.FindObjectOfType<GameManager>();

        //Die an extra 10 seconds earlier unless in the center
        if (life > 5) life += 10;
    }
    // Update is called once per frame
    void Update()
    {
        //Warning feedback
        if (TimeManager.time <= life + 5 && !dead)
        {
            gameObject.GetComponent<Renderer>().material.SetColor("_Color", gameObject.GetComponent<ParticleControl>().TargetColor);
        }
        //Destroy platform
        if (TimeManager.time <= life && !dead)
        {
        //Remove pickups on the platform too
        GameObject pickupDestroyer = Instantiate(GameObject.Find("PickupDestroyer"), transform);
            pickupDestroyer.transform.parent = null;
            pickupDestroyer.transform.localScale = new Vector3(20.0f, 20.0f, 20.0f);
            pickupDestroyer.AddComponent<Rigidbody>();

        AudioSource.PlayClipAtPoint(platformDrop, transform.position);

            //Mesh collider doesn't like rigid bodies... so gone goes the collision
            Destroy(gameObject.GetComponent<MeshCollider>());

            gameObject.AddComponent<Rigidbody>();
            gameObject.GetComponent<Rigidbody>().useGravity = true;
            dead = true;
        }
    }
}
