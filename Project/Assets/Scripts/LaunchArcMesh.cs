using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LaunchArcMesh : MonoBehaviour
{
    //From this tuorial: https://www.youtube.com/watch?v=TXHK1nPUOBE&t=143s
    Mesh mesh;
    public float meshWidth;
    
    public float velocity;
    public float angle;
    public int resolution;

    float g; //gravity force
    float radianAngle;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        g = Mathf.Abs(Physics2D.gravity.y);

    }

    void OnValidate()
    {
        //Check that lr is not null
        if (mesh != null && Application.isPlaying)
        {

        }
    }

    // Use this for initialization
    void Start()
    {
        MakeArcMesh(CalculateArcArray());
    }

   public void MakeArcMesh(Vector3[] arcVerts)
    {
        mesh.Clear();
        Vector3[] vertices = new Vector3[(resolution + 1) * 2];
        int[] triangles = new int[resolution * 6 * 2];

        for (int i = 0; i <= resolution; i++)
        {
            //Set vertices (x would be width, y is height, z is distance)
            vertices[i * 2] = new Vector3(meshWidth * 0.5f, arcVerts[i].y, arcVerts[i].x);
            vertices[i * 2 + 1] = new Vector3(meshWidth * -0.5f, arcVerts[i].y, arcVerts[i].x);

            //Set triangles
            if (i != resolution)
            {
                triangles[i * 12] = i * 2;
                triangles[i * 12 + 1] = triangles[i * 12 + 4] = i * 2 + 1;
                triangles[i * 12 + 2] = triangles[i * 12 + 3] = (i + 1) * 2;
                triangles[i * 12 + 5] = (i + 1) * 2 + 1;

                triangles[i * 12 + 6] = i * 2;
                triangles[i * 12 + 7] = triangles[i * 12 + 10] = (i + 1) * 2;
                triangles[i * 12 + 8] = triangles[i * 12 + 9] = i * 2 + 1;
                triangles[i * 12 + 11] = (i + 1) * 2 + 1; 
            }

        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
             
    }

    public Vector3[] CalculateArcArray()
    {
        Vector3[] arcArray = new Vector3[resolution + 1];
        radianAngle = Mathf.Deg2Rad * angle;
        float maxDistance = (velocity * velocity * Mathf.Sin(2 * radianAngle)) / g;

        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / (float)resolution;

            Vector3 newPointPos = CalculateArcPoint(t, maxDistance);

            arcArray[i] = newPointPos;

            //Have an aiming quad as well at the end of the arc
            /*if (i == resolution)
            {
                Transform childQuad = transform.GetChild(0);

                childQuad.localPosition = new Vector3(0.0f, -1.2f, newPointPos.x);

                //Don't show the quad if super close in range
                float quadScale = maxDistance / 5.0f;
                childQuad.localScale = new Vector3(quadScale, quadScale, quadScale);
            }*/
        }

        return arcArray;
    }

    Vector3 CalculateArcPoint(float t, float maxDistance)
    {
        float x = t * maxDistance;
        float y = x * Mathf.Tan(radianAngle) - ((g * x * x) / (2 * velocity * velocity * Mathf.Cos(radianAngle) * Mathf.Cos(radianAngle)));

        return new Vector3(x, y);
    }

   
    public void spawnTarget()
    {
        Transform target = Instantiate(transform.GetChild(0), transform);

        target.position = target.TransformPoint(target.localPosition);

        target.parent = null;
        
    }
}
