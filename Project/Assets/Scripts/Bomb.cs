using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

    public float time;
    public GameObject newExplosion;
    private bool active;
    private float distanceToGround;

    //Variables that change based on what materials were used
    public float explosionLifetime;
    public Vector3 explosionScaleSpeed;
    public float explosionScaleLimit;

    public bool First = false;
    public GameObject ThrowingPlayer;
    public GameManager TimeManager;

    public BombAttributes.BombData attributes;

    private Rigidbody rb;

    // Use this for initialization
    void Start () {
        /*
        Debug.Log("start");

        time = 1.0f;
        active = true;

        explosionScaleSpeed = new Vector3(4.0f, 4.0f, 4.0f);
        explosionScaleLimit = 10.0f;
        explosionLifetime = 3.0f;*/

        Collider collider = gameObject.GetComponent<Collider>();

        distanceToGround = collider.bounds.extents.y;

        rb = GetComponent<Rigidbody>();

      
    }

    //Modify bombs rim lights based on crafting materials being used.
    public void modifyRimLight()
    {
        Color rimColour = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        Renderer rend = GetComponent<Renderer>();

        if (attributes.scatter > 0)
        { 
            rimColour += new Color(0.37f, 1.0f, 0.0f, 0.0f);
        }
        if (attributes.fire > 0)
        {
            rimColour += new Color(0.8f, 0.0f, 0.0f, 0.0f);
        }
        if (attributes.freeze > 0)
        {
            rimColour += new Color(0.0f, 0.0f, 0.65f, 0.0f);
        }
        if (attributes.smoke > 0)
        {
            rimColour += new Color(0.3f, 0.3f, 0.3f, 0.0f);
        }
        if (attributes.blackhole > 0)
        {
            rimColour += new Color(0.77f, 0.0f, 0.88f, 0.0f);
        }
        if(attributes.explosionScaleLimit > 15.0f)
        {
            rimColour += new Color(0.8f, 0.84f, 0.0f, 0.0f);
        }


        rend.material.SetColor("_RimColor", rimColour);
    }

    // Update is called once per frame
    void Update () {

        rb.AddForce(Physics.gravity * rb.mass);
    

        //if (isGrounded()) time = 0.0f;

        if (time > 0)
        {
            time -= 0.0f * Time.deltaTime;
        }
        else //Explosion creation code happens here!
        {

            //No natural explosion if scatter bombs
            if (attributes.scatter > 0)
            {
                //Scatter bombs
                for (int i = 0; i < attributes.scatter + 1; i++)
            {
                GameObject miniBomb = Instantiate(gameObject, transform.position, transform.rotation);

                //Offset newer bombs a little (randomly)
                 miniBomb.transform.position += new Vector3(Random.Range(-5.0f, 5.0f), 0.0f, Random.Range(-5.0f, 5.0f));
                
                //Make the mini bomes actually mini
                miniBomb.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                   

                //Make newer bomb components equal to this bomb
                Bomb miniBombClass = miniBomb.GetComponent<Bomb>();
                miniBombClass = miniBomb.GetComponent<Bomb>();

                miniBombClass.attributes = attributes;

                miniBombClass.time = 2.0f;
                miniBombClass.attributes.explosionScaleLimit /= 1.5f;

                //Make scatter deal slightly less damage
                miniBombClass.attributes.damage /= 1.2f;

                //Prevent them from inifnitely creating more
                miniBombClass.attributes.scatter = 0;

              
            }


                Destroy(gameObject);
                return;
            }


            //Explode! Create an explosion object and destroy self
            GameObject bombExplosion = Instantiate(newExplosion, transform.position, transform.rotation);
            

            //Modify that explosion based on explosion variables
            ExplosionScale newExplosionClass = bombExplosion.GetComponent<ExplosionScale>();

            newExplosionClass.explosionAttributes = attributes;

            if (First)
            {
                ThrowingPlayer.transform.position = gameObject.transform.position;
                newExplosionClass.explosionAttributes.damage = 0.0f;
                Renderer rend = bombExplosion.GetComponent<Renderer>();
                rend.material.SetColor("_Color", new Color(1.0f, 1.0f, 1.0f, 0.5f));
                rend.material.SetColor("_EmissionColor", new Color(0.1f, 0.0f, 0.1f));
                newExplosionClass.firstBomb = true;

                Player p = ThrowingPlayer.GetComponent<Player>();
                p.firstThrow = false;
            }
            




                Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        time = 0.0f;
    }

    void OnTriggerEnter(Collider other)
    {
        time = 0.0f;
    }

    bool isGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distanceToGround + 0.1f);
    }

}
