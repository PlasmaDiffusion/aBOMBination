using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionScale : MonoBehaviour
{

    public bool expanding; // Is the explosion scaling itself up?
 
    public BombAttributes.BombData explosionAttributes;

    public GameObject[] emitterObjectReferences;

    GameObject[] particleEmitters;

    public bool firstBomb = false;

    private bool pullTowards = false;


    public AudioClip explosion1;
    public AudioClip explosion2;
    public AudioClip explosion3;
    public AudioClip explosion4;
    public AudioClip fireExplosion;
    public AudioClip iceExplosion;
    int randExplodeSound;


    // Use this for initialization
    void Start()
    {
        Debug.Log("Explosion started");
        //scaleSpeed = new Vector3(4.0f, 4.0f, 4.0f);
        //scaleLengthLimit = 10.0f;
        expanding = true;
        //extraLifetime = 3.0f;


        particleEmitters = new GameObject[explosionAttributes.smoke];

        //Smoke particles
        for (int i = 0; i < explosionAttributes.smoke; i++)
        {
            particleEmitters[i] = Instantiate(emitterObjectReferences[0], transform.position, new Quaternion(0.0f, 0.0f, 0.0f, 0.0f));
        }

        //Recolour explosion if bigger radius
        if (explosionAttributes.explosionScaleLimit > 15.0f)
        {
            Renderer rend = GetComponent<Renderer>();
            rend.material.SetColor("_Color", new Color(0.6f, 0.0f, 0.0f, 0.5f));
        }

        if (explosionAttributes.blackhole > 0)
        {
            pullTowards = true;

            //Special purply colour
            Renderer rend = GetComponent<Renderer>();
            rend.material.SetColor("_Color", new Color(0.0f, 0.0f, 1.0f, 0.5f));

            //Blackhole will deal less damage but also last around longer. And of course it gets an extra pull radius. (Good for combining with other things like fire!)
            explosionAttributes.damage = explosionAttributes.damage / 1.2f;


            //A blackhole even gets an additional larger invisible explosion that won't damage, but will pull (if statement makes sure its not infinitely spawning more)
            if (explosionAttributes.damage != 0.0f)
            {
                explosionAttributes.explosionLifetime += (1.0f * explosionAttributes.blackhole);

                GameObject suctionObject = Instantiate(gameObject);
                ExplosionScale suction = suctionObject.GetComponent<ExplosionScale>();

                suction.explosionAttributes = explosionAttributes;

                suction.explosionAttributes.damage = 0.0f;
                suction.explosionAttributes.fire = 0;
                suction.explosionAttributes.freeze = 0;
                suction.explosionAttributes.scatter = 0;
                suction.explosionAttributes.smoke = 0;

                //Pull radius is bigger the more blackhole materials are added
                suction.explosionAttributes.explosionScaleLimit = explosionAttributes.explosionScaleLimit * (1.5f + (0.25f * suction.explosionAttributes.blackhole));

                suctionObject.transform.localScale = transform.localScale * 1.75f;

                suctionObject.GetComponent<MeshRenderer>().enabled = false;

            }
        }
        /*
        //play bomb sounds depending on bomb type
        if (explosionAttributes.fire > 0)
        {
            AudioSource.PlayClipAtPoint(fireExplosion, transform.position);
        }
        else if (explosionAttributes.freeze > 0)
        {
            AudioSource.PlayClipAtPoint(iceExplosion, transform.position);
        }
        else
        {
            randExplodeSound = Random.RandomRange(0, 3);
            if (randExplodeSound == 0)
            {
                AudioSource.PlayClipAtPoint(explosion1, transform.position);
            }
            else if (randExplodeSound == 1)
            {
                AudioSource.PlayClipAtPoint(explosion2, transform.position);
            }
            else if (randExplodeSound == 2)
            {
                AudioSource.PlayClipAtPoint(explosion3, transform.position);
            }
            else
            {
                AudioSource.PlayClipAtPoint(explosion4, transform.position);
            }
        }*/
    }

    // Update is called once per frame
    void Update()
    {

        if (expanding)
        {
            //Increase explosion scale
            Vector3 currentScale = transform.localScale;
            currentScale += explosionAttributes.explosionScaleSpeed * Time.deltaTime;
            transform.localScale = currentScale;

            //...Until it's limit is reached, then destroy it
            if (currentScale.magnitude > explosionAttributes.explosionScaleLimit)
            {
                expanding = false;

            }

        }
        else
        {
            //Already scaled up; now wait a brief moment and destroy the explosion (and any extra particle emitters)
            explosionAttributes.explosionLifetime -= 1.0f * Time.deltaTime;
            if (explosionAttributes.explosionLifetime < 0.0f)
                {
                for (int i = explosionAttributes.smoke - 1; i >= 0; i--) Destroy(particleEmitters[i]);

                Destroy(gameObject);
                }

            }
        
    }
    

    void OnTriggerEnter(Collider other)
    {
        //Do nothing as a first bomb
        if (firstBomb) return;

        //Knockback whatever is in the explosion

        Rigidbody otherRigidBody = other.gameObject.GetComponent<Rigidbody>();
        Player player = other.gameObject.GetComponent<Player>();

        

        if (otherRigidBody && !pullTowards) //Unused if we don't go back to rigid bodies
        {

            //Blast player in opposite direction of them relative to the explosion. (But not if a blackhole bomb)



            otherRigidBody.velocity = other.transform.up * 10.0f;

            Vector3 blastImpact = Vector3.Normalize(other.transform.position - transform.position) * 10.0f;

               


                otherRigidBody.velocity += blastImpact;

            
            

        }
        else if (player && !pullTowards) //For Character Controller
        {
            Vector3 blastImpact = Vector3.Normalize(other.transform.position - transform.position) * 1.0f;
            
            player.explosionForce = blastImpact;


            damagePlayer(player);
        }
        else if (player) //If blackhole then still damage once
        {
            if (explosionAttributes.damage != 0.0f)
            damagePlayer(player);
        }

        


    }
    void OnTriggerStay(Collider other)
    {
       
        //Blackhole
        if (pullTowards)
        {
            Rigidbody otherRigidBody = other.gameObject.GetComponent<Rigidbody>();

            Player player = other.gameObject.GetComponent<Player>();


            if (otherRigidBody)
            {

                Vector3 pullForce = Vector3.Normalize(other.transform.position - transform.position) * 0.5f;

                otherRigidBody.velocity -= pullForce;

                
            }
        else if (player)
            {
                Vector3 pullForce = Vector3.Normalize(other.transform.position - transform.position) * (0.1f * explosionAttributes.blackhole);
                player.explosionForce = -pullForce;


            }
        }


    }

    void damagePlayer(Player player)
    {

        //Deal some damage
        player.damage(explosionAttributes.damage, true);


        //Do extra affects here

        if (explosionAttributes.fire > 0)
        {
            player.addStatusEffect(1, 10.0f * explosionAttributes.fire);
        }
        if (explosionAttributes.freeze > 0)
        {
            player.addStatusEffect(2, 5.0f * explosionAttributes.freeze);
        }

        //And finally check if the player died.
        player.checkIfDead();

    }



}
