using UnityEngine;
using System.Collections;


public class Phosphor : MonoBehaviour {

    public Shader cloudShader;
    public float scale;
    public float cloudRotationSpeed;
    public float pulsationSpeed;
    public float radius = 1.0f;
    public float rotation = 0.0f;
    public float rotVel = 0.01f;
    public int pointCount;
    public Vector2 Vel
    {
        get { return vel; }
    }
    private Vector2 vel;
    private Material cloudMaterial;
    private AgentWorld world;
	
    // Use this for initialization
	void Start () {
        cloudMaterial = new Material(cloudShader);
        world = FindObjectOfType<AgentWorld>();
        cloudMaterial.SetFloat("timeOffset", Random.value);
	}



	void OnRenderObject ()
    {
        rotation += rotVel;
        if (rotation > 360.0f) rotation -= 360.0f;
        Vector2 pos = new Vector2(radius, 0.0f).Rotate(rotation);
        vel = pos - new Vector2(transform.position.x, transform.position.y);
        transform.position = pos;

        if (pointCount < 3) pointCount = 3;
        if (pointCount > 16) pointCount = 16;
        cloudMaterial.SetPass(0);
        

        cloudMaterial.SetFloat("x", transform.position.x);
        cloudMaterial.SetFloat("y", transform.position.y);
        cloudMaterial.SetColor("col", world.triColor);
        cloudMaterial.SetFloat("scale", scale);
        cloudMaterial.SetFloat("increment", 2 * Mathf.PI / pointCount);
        cloudMaterial.SetFloat("rotationSpeed", cloudRotationSpeed);
        cloudMaterial.SetFloat("pulsationSpeed", pulsationSpeed);
        cloudMaterial.SetInt("count", pointCount);

        Graphics.DrawProcedural(MeshTopology.Points, 1);
    }
}
