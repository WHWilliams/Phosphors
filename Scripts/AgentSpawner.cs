using UnityEngine;
using System.Collections;

public class AgentSpawner : MonoBehaviour {
        


    public float spawnLife = 1.0f;
    public int spawnCount = 1;    
    public float spawnRadius = 1.0f;    
    public float targetRadius = 1.0f;

    public bool hackInMaxSpawnCountOnKeyPress = false;

    private PhosphorManager phosphorManager;
    private AgentWorld agentManager;

	// Use this for initialization
	void Start () {
        agentManager = FindObjectOfType<AgentWorld>();
        phosphorManager = FindObjectOfType<PhosphorManager>();
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown("a") && hackInMaxSpawnCountOnKeyPress) spawnCount = 1111111;
    }

    public void Spawn(int id)
    {
        if (agentManager == null) return;

        for (int i = 0; i < spawnCount; i++)
            if (!agentManager.Spawn(new AgentData
            {
                pos = Random.insideUnitCircle.normalized * spawnRadius + (Vector2)transform.position
                ,
                life = spawnLife
                ,
                vel = Vector2.zero
                ,
                targetIndex = phosphorManager.getRandomPhosphorIndex()
                ,
                playerID = -id
            })) break;
        
    }

   
}
