using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ParticleInterface : MonoBehaviour
{
   /*
    The following script is built on the script for the tutorial from lab 5. It uses it as a base and heavily modifies it to work with the newer plugins.
*/

    public float m_MaxDistance;
    public GameObject particleObject;
    public GameObject playerReference;

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec
    {
        public float x, y, z;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Particle
    {
        public Vec Pos;
        public Vec Vel;
        public Vec Accl;
        public Vec Col;
    }
    //entry points for our dll
    [DllImport("ParticleSystem")]
    public static extern void initEmitter(int type);
    [DllImport("ParticleSystem")]
    public static extern void CleanUpSystem();
    [DllImport("ParticleSystem")]
    public static extern void initParticles(Particle[] p, int Size, float range, Vec vel, Vec accl);
    [DllImport("ParticleSystem")]
    public static extern void updateEmitter(Vec position);
    [DllImport("ParticleSystem")]
    public static extern void updateParameter(float value, int type);
    [DllImport("ParticleSystem")]
    public static extern void updateParticles(Particle[] p, float dt);
    [DllImport("ParticleSystem")]
    public static extern void setVelocityLimit(float newLimit);
    [DllImport("ParticleSystem")]
    public static extern void finish();
    [DllImport("ParticleSystem")]
    public static extern int getState();

    // Use this for initialization
    Particle[] g_Arr = new Particle[50];
    GameObject[] ParticleArr = new GameObject[50];


    ParticleSystem.Particle[] emittedParticles;

    public Vector3 pVelocity;
    public Vector3 pAcceleration;

    private Vec pos;
    private Vec accl;

    public float maxVelocity;

    public int type;

    public bool modifyEmitters;
    public bool modifyParticles;

    Vec newVelocity;
    Vec newAcceleration;
    

    void SetPosition()
    {

        for (int i = 0; i < 50; i++)
        {

            ParticleSystem p = ParticleArr[i].GetComponent<ParticleSystem>();


            if (!p.isPlaying) continue;

            ParticleSystem.ColorOverLifetimeModule lifetimeColor = p.colorOverLifetime;
            lifetimeColor.enabled = true;
            lifetimeColor.color = new Color(g_Arr[i].Col.x, g_Arr[i].Col.y, g_Arr[i].Col.z);
            ParticleSystem.TrailModule trail = p.trails;
            if (getState() > 1 && type == 1) trail.enabled = false;

            //Modify emitters here --------------------------------------------------------------------------------
            if (modifyEmitters)
            { 

            ParticleArr[i].transform.position = new Vector3(g_Arr[i].Pos.x, g_Arr[i].Pos.y, g_Arr[i].Pos.z);
            
 //           ParticleSystem.MainModule m = p.main;
//            m.startColor = new Color(g_Arr[i].Col.x, g_Arr[i].Col.y, g_Arr[i].Col.z);


            }

            if (modifyParticles)
            {

                //Modify individual particles here ---------------------------------------------------------------------
             p.GetParticles(emittedParticles);

            if (type == 0) for (int j = 0; j < emittedParticles.Length; j++) emittedParticles[j].position += new Vector3(g_Arr[j].Vel.x, g_Arr[j].Vel.y, g_Arr[j].Vel.z);
            else for (int j = 0; j < emittedParticles.Length; j++) emittedParticles[j].position = new Vector3(g_Arr[j].Pos.x, g_Arr[j].Pos.y, g_Arr[j].Pos.z);

            p.SetParticles(emittedParticles, emittedParticles.Length);
            

            }

            //velText.text = g_Arr[i].Vel.x.ToString() + " " + g_Arr[i].Vel.y.ToString() + " " + g_Arr[i].Vel.z.ToString();
        }
    }
    void Start()
    {

        emittedParticles = new ParticleSystem.Particle[50];

        //Acceleration and velocity to set later on
        newVelocity.x = pVelocity.x;
        newVelocity.y = pVelocity.y;
        newVelocity.z = pVelocity.z;


        newAcceleration.x = pAcceleration.x;
        newAcceleration.y = pAcceleration.y;
        newAcceleration.z = pAcceleration.z;

        //Instantiate other particle emitters. Also place them randomly around the player.

        Transform p = playerReference.transform;
        for (int i = 0; i < 50; i++)
        {
            ParticleArr[i] = Instantiate<GameObject>(particleObject);
            ParticleArr[i].transform.position = new Vector3 (Random.Range(p.position.x-30.0f, p.position.x + 30.0f),
                p.position.y - 60.0f,
                Random.Range(p.position.z-30.0f, p.position.z + 30.0f) * i);
            //else if (type == 2) ParticleArr[i].transform.position = new Vector3(Random.Range(-20.0f, 20.0f), transform.position.y, Random.Range(-20.0f, 20.0f));
            //else ParticleArr[i].transform.position = new Vector3(Random.Range(-20.0f, 20.0f), Random.Range(-20.0f, 20.0f), Random.Range(-20.0f, 20.0f));
        }
        initEmitter(type);


        //Initialize dll stuff here--------------------------------------------------------------

        //Time based parameter (These functions are made to be for nonspecific classes)
        updateParameter(2.0f, 0);

        //Original velocity parmeter
        updateParameter(1.0f, 1);

        //Initialize stuff from the particle structure
        updateEmitter(pos);
        initParticles(g_Arr, 50, m_MaxDistance, newVelocity, newAcceleration);
        SetPosition();
        setVelocityLimit(maxVelocity);
        
    }

    // Update is called once per frame
    void Update()
    {

        pos.x = transform.position.x;
        pos.y = transform.position.y;
        pos.z = transform.position.z;

        updateEmitter(pos);
        updateParticles(g_Arr, Time.deltaTime);
        
        SetPosition();

        if (Input.GetKeyDown(KeyCode.Escape) || getState() == -1)
        {
            Destroy(gameObject);
        }

        
    }

    void OnDestroy()
    {

        //Debug.Log("On destroy called");
        finish();

        for(int i = 50 -1; i >= 0; i--)
        {
            Destroy(ParticleArr[i]);
        }
    }
}
