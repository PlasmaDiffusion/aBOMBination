using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ParticleControl : MonoBehaviour {

    [DllImport("ParticleDll")]
    public static extern Vec ColorPicker(int countColor, Vec currentColor, Vec[] targetColors, float duration, float runtime, float delta);

    public Color TargetColor;
    public Vec result = new Vec();

    public List<Color> FadedColors;
    List<Vec> vecs;
    public float duration = 10.0f;
    public float speed = 2.25f;
    public GameManager TimeManager;

    public static Color V2C(Vec v)
    {
        return new Color(v.x, v.y, v.z);
    }

    public static Vec C2V(Color v)
    {
        Vec result = new Vec();
        result.x = v.r;
        result.y = v.g;
        result.z = v.b;
        return result;
    }

    [System.Serializable]
    public struct Vec
    {
        public float x, y, z;
    }
    
    void Start () {
        TimeManager = GameObject.FindObjectOfType<GameManager>();
        vecs = new List<Vec>();
        foreach(Color c in FadedColors)
        {
            vecs.Add(ParticleControl.C2V(c));
        }
    }

    void Update()
    {
        result = ColorPicker(vecs.Count, result, vecs.ToArray(), duration, Time.realtimeSinceStartup, Time.deltaTime* speed);

        TargetColor = new Color(result.x, result.y, result.z);


    }
}
