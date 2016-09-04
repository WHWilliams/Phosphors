using UnityEngine;
using System.Collections;

public class PhosphorManager : MonoBehaviour
{
    public float spawnRadius = 4.0f;
    public float spawnRadiusRange = 0.5f;

    public int phosphorsToSpawn = 45;

    public float rotVelRange = 360.0f/1200.0f;
    
    private Phosphor[] phosphors;
    private Vector4[] targets;

    private float xExtends;
    private float yExtends;    

    public float XExtends { get { return xExtends; } }
    public float YExtends { get { return yExtends; } }

    public GameObject phoshporPrefab;

    Camera cam;

    public bool testToggle = true;
    


    public int getRandomPhosphorIndex()
    {
        return Random.Range(0, phosphorsToSpawn);
    }

    
   

    

    private void newPhosphor(int i)
    {

        
            
        GameObject phosphor = (GameObject)GameObject.Instantiate(phoshporPrefab, Vector3.zero, Quaternion.identity);
        Phosphor pComponent = phosphor.GetComponent<Phosphor>();
        pComponent.rotVel = Random.Range(-rotVelRange, rotVelRange);            
        pComponent.radius = spawnRadius + Random.Range(-spawnRadiusRange, spawnRadiusRange);
        pComponent.rotation = Random.Range(0.0f, 360.0f);
        phosphors[i] = pComponent;

    }

    public Vector4 getTarget(int i)
    {
        if (targets == null) return Vector4.zero;
        return targets[i];
    }
    
    // Use this for initialization
    void Start()
    {
        cam = Camera.main;
        phosphors = new Phosphor[phosphorsToSpawn];
        targets = new Vector4[phosphorsToSpawn];

        for (int i = 0;i<phosphorsToSpawn;i++)
        {
            newPhosphor(i);
        }

    }



    // Update is called once per frame
    void Update()
    {
        updateExtends();


        
        for (int i = 0; i < phosphorsToSpawn; i++)
        {
            if (phosphors[i] == null) continue;            
            targets[i] = phosphors[i].transform.position;
            targets[i].z = phosphors[i].Vel.x;
            targets[i].w = phosphors[i].Vel.y;
        }


    }

    private void updateExtends()
    {
        float worldCamExtend = cam.ScreenToWorldPoint(new Vector3(cam.orthographicSize, 0.0f)).x;
        xExtends = worldCamExtend * (cam.pixelWidth / cam.pixelHeight);
        yExtends = worldCamExtend;
    }

    private bool shouldPhosphorDie(int i)
    {
        return false;

    }

    void OnDrawGizmos()
    {

    }
    
}


public static class Vector2Extension
{
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        float tx = v.x;
        float ty = v.y;

        return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
    }
}