using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class AgentWorld : MonoBehaviour {

    private PhosphorManager phosphorManager;
    private Chat chat;
    private Vector4[] targetsArray;

    public NumberGhost numberGhostPrefab;

    public ComputeShader computeShader;
    public int maxAgents = 512;
    public int spawnerBlockCount = 1;
    public float unitScale = 1.0f;
    public float maxAccel = 0.5f;
    public float phosphorRadius = 1.0f;
    public Color triColor;    
    int BLOCK_SIZE = 64;

    public int haul = 10;
    private int allTimeScore = 0;


    private int agentCount;
    public int AgentCount { get { return agentCount; } }

    private int Score
    {
        get { return score; }
        set
        {
            if(value > score) allTimeScore += value - score; ;

            score = value;
        }
    }

    public int getAllTimeScore()
    {
        return allTimeScore;
    }

    ComputeBuffer agentBuffer0;
    ComputeBuffer agentBuffer1;
    ComputeBuffer freeBuffer;
    ComputeBuffer spawnBuffer;
    ComputeBuffer agentInstanceBuffer;
    ComputeBuffer targetsBuffer;
    ComputeBuffer returnBuffer;
    int[] returnIds;
    

    ComputeBuffer agentCountArgBuffer;

    private List<AgentData> spawningAgents;
    private Dictionary<Candidate, int> upgradeCosts;

    public Shader triShader;
    Material triMaterial;

    int KernelMain;
    int KInit;
    int KSpawn;
    int KInstantiate;
    int KPhosphorFeed;
    int KReturn;

    int score;

    public float speedIncrement = .05f;
    public int burstIncrement = 1;
    public int haulIncrement = 10;

    public int speedCost = 100;
    public int burstCost = 100;
    public int haulCost = 100;

    private int speedBal = 0;
    private int burstBal = 0;
    private int haulBal = 0;

    public int speedCostIncrement = 100;
    public int burstCostIncrement = 100;
    public int haulCostIncrement = 100;

    public float upgradeProgressRatio(Candidate nominee)
    {
        switch (nominee)
        {
            case Candidate.speed:
                return (float)speedBal / speedCost;
            case Candidate.burst:
                return (float)burstBal / burstCost;
            case Candidate.haul:
                return (float)haulBal / haulCost;
        }
        return 0.0f;
    }

    public bool payTowardsupgrade(Candidate nominee, int sum)
    {
        if (sum > Score) return false;

        
        switch(nominee)
        {
            case Candidate.speed:
                if (sum > speedCost - speedBal) sum = speedCost - speedBal;
                speedBal += sum;
                if(speedBal>=speedCost)
                {
                    speedBal -= speedCost;
                    speedCost += speedCostIncrement;
                    maxAccel += speedIncrement;
                }
                break;
            case Candidate.burst:
                if (sum > burstCost - burstBal) sum = burstCost - burstBal;
                burstBal += sum;
                if(burstBal >= burstCost)
                {
                    burstBal -= burstCost;
                    burstCost += burstCostIncrement;
                    FindObjectOfType<AgentSpawner>().spawnCount += burstIncrement;
                }
                break;
            case Candidate.haul:
                if (sum > haulCost - haulBal) sum = haulCost - haulBal;
                haulBal += sum;
                if(haulBal>=haulCost)
                {
                    haulBal -= haulCost;
                    haulCost += haulCostIncrement;
                    haul += haulIncrement;
                }
                break;
        }

        // TODO apply upgrade
        Score -= sum;
        return true;
    }

    public int getScore()
    {
        return Score;        
    }

    void OnValidate()
    {
        int border = BLOCK_SIZE;
        while(maxAgents > border)
        {
            border *= 2;
        }
        maxAgents = border;
    }


	void Start ()
    {
        
        // Grab phosphor manager
        phosphorManager = FindObjectOfType<PhosphorManager>();
        // Grab chat
        chat = FindObjectOfType<Chat>();

        // Create Kernels and Material
        KernelMain = computeShader.FindKernel("KMain");
        KInit = computeShader.FindKernel("KInit");
        KSpawn = computeShader.FindKernel("KSpawn");
        KInstantiate = computeShader.FindKernel("KInstantiate");
        KPhosphorFeed = computeShader.FindKernel("KPhosphorFeed");
        KReturn = computeShader.FindKernel("KReturn");

        triMaterial = new Material(triShader);

        ///
        // Create Buffers 
        // Agent Buffers
        agentBuffer0 = new ComputeBuffer(maxAgents, AgentData.size);
        agentBuffer1 = new ComputeBuffer(maxAgents, AgentData.size);
        
        // Spawn Buffer 
        spawnBuffer = new ComputeBuffer(BLOCK_SIZE*spawnerBlockCount, AgentData.size);
        spawningAgents = new List<AgentData>(BLOCK_SIZE * spawnerBlockCount);        

        // Free Buffer
        freeBuffer = new ComputeBuffer(maxAgents, sizeof(int), ComputeBufferType.Append);
        freeBuffer.ClearAppendBuffer();

        // Triangle Instance Buffer
        agentInstanceBuffer = new ComputeBuffer(maxAgents, sizeof(int), ComputeBufferType.Append);
        agentInstanceBuffer.ClearAppendBuffer();

        // Args Buffer
        agentCountArgBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.DrawIndirect);
        int[] dArgs = new int[] { 0, 1, 0, 0 };
        agentCountArgBuffer.SetData(dArgs);

        // Targets Buffer
        targetsBuffer = new ComputeBuffer(phosphorManager.phosphorsToSpawn + 1, sizeof(float) * 4);
        targetsArray = new Vector4[phosphorManager.phosphorsToSpawn + 1];
        // Set home target end of array to zero
        targetsArray[phosphorManager.phosphorsToSpawn] = Vector4.zero;

        // Return Buffer... awful way to do this?
        returnBuffer = new ComputeBuffer(maxAgents, sizeof(int), ComputeBufferType.Append);
        returnIds = new int[maxAgents];
        returnBuffer.ClearAppendBuffer();


        // Run init shader on initial buffer
        computeShader.SetBuffer(KInit, "appendFreeBuffer", freeBuffer);
        computeShader.SetBuffer(KInit, "agentBufferOut", agentBuffer0);
        computeShader.Dispatch(KInit, maxAgents / BLOCK_SIZE, 1, 1);
        

        // Init constant uniforms 
        computeShader.SetInt("maxAgents", maxAgents);
        computeShader.SetInt("maxPhosphors", phosphorManager.phosphorsToSpawn);
        
    }

    

    bool isEvenInBuffer = true;
    ComputeBuffer agentBufferIn;
    ComputeBuffer agentBufferOut;
    void Update()
    {
        // Flip Buffers
        agentBufferIn = isEvenInBuffer ? agentBuffer0 : agentBuffer1;
        agentBufferOut = isEvenInBuffer ? agentBuffer1 : agentBuffer0;
        isEvenInBuffer = !isEvenInBuffer;

        // Update Targets
        for (int i = 0; i < phosphorManager.phosphorsToSpawn; i++)
        {
            targetsArray[i] = phosphorManager.getTarget(i);
        }

        targetsBuffer.SetData(targetsArray);




        ///
        // Dispatch Compute Buffers        
        computeShader.SetFloat("phosphorRadius", phosphorRadius);
        // Set Compute Shader Buffers and uniforms for Main
        computeShader.SetFloat("maxAccel", maxAccel);
        computeShader.SetBuffer(KernelMain, "targetsBuffer", targetsBuffer);
        computeShader.SetBuffer(KernelMain, "agentBufferIn", agentBufferIn);
        computeShader.SetBuffer(KernelMain, "agentBufferOut", agentBufferOut);
        computeShader.SetBuffer(KernelMain, "appendFreeBuffer", freeBuffer);
        // Dispatch Main Kernel
        computeShader.Dispatch(KernelMain, maxAgents / BLOCK_SIZE, 1, 1);

        // Set Buffers for feed kernel
        computeShader.SetBuffer(KPhosphorFeed, "agentBufferIn", agentBufferIn);
        computeShader.SetBuffer(KPhosphorFeed, "agentBufferOut", agentBufferOut);
        // Run phosphor feed kernel for each phosphor
        for (int i = 0; i < phosphorManager.phosphorsToSpawn; i++)
        {
            Vector2 pPos = phosphorManager.getTarget(i);
            computeShader.SetFloat("phosphorX", pPos.x);
            computeShader.SetFloat("phosphorY", pPos.y);
            computeShader.Dispatch(KPhosphorFeed, maxAgents / BLOCK_SIZE, 1, 1);
        }



        // Set Compute Shader Buffers and uniforms for Spawn 
        spawnBuffer.SetData(spawningAgents.ToArray());
        computeShader.SetInt("agentsToSpawn", spawningAgents.Count);
        computeShader.SetBuffer(KSpawn, "spawnBuffer", spawnBuffer);
        computeShader.SetBuffer(KSpawn, "consumeFreeBuffer", freeBuffer);
        computeShader.SetBuffer(KSpawn, "agentBufferOut", agentBufferOut);

        // Dispatch Spawn Kernel
        computeShader.Dispatch(KSpawn, spawnerBlockCount, 1, 1);
        spawningAgents.Clear();

        // Set Compute Shader Buffers and uniforms for Return
        computeShader.SetBuffer(KReturn, "returnBuffer", returnBuffer);
        computeShader.SetBuffer(KReturn, "appendFreeBuffer", freeBuffer);
        computeShader.SetBuffer(KReturn, "agentBufferIn", agentBufferIn);
        computeShader.SetBuffer(KReturn, "agentBufferOut", agentBufferOut);

        // Dispatch Return Kernel
        computeShader.Dispatch(KReturn, maxAgents / BLOCK_SIZE, 1, 1);

        // Update score from return Buffer
        int[] returnCountArgs = new int[] { 0, 1, 0, 0 };
        ComputeBuffer.CopyCount(returnBuffer, agentCountArgBuffer, 0);
        agentCountArgBuffer.GetData(returnCountArgs);
        int returnedCount = returnCountArgs[0];
        returnBuffer.GetData(returnIds);
        bool haveAnyReturned = false;
        int scoreThisFrame = 0;
        for (int i = 0; i < returnedCount; i++)
        {
            haveAnyReturned = true;
            Score += haul;
            scoreThisFrame += haul;
            chat.addToPlayerScore(returnIds[i], haul);            
        }
        if (haveAnyReturned)
        {
            NumberGhost ghost = GameObject.Instantiate<NumberGhost>(numberGhostPrefab);
            ghost.scoreText = "+" + scoreThisFrame.ToString();
            chat.updateLeaderBoard();
        }

        // Update agent count
        int[] agentCountArgs = new int[] { 0, 1, 0, 0 };
        ComputeBuffer.CopyCount(freeBuffer, agentCountArgBuffer, 0);
        agentCountArgBuffer.GetData(agentCountArgs);
        agentCount = maxAgents - agentCountArgs[0];
    }

    void OnRenderObject()
    {    
         ///
        // Render Frame        
        // Get arg buffer for triangles   
        // Instantiate active particles for rendering
        computeShader.SetBuffer(KInstantiate, "triAppendBuffer", agentInstanceBuffer);
        computeShader.SetBuffer(KInstantiate, "agentBufferIn", agentBufferIn);
        computeShader.Dispatch(KInstantiate, maxAgents / BLOCK_SIZE, 1, 1);
        ComputeBuffer.CopyCount(agentInstanceBuffer, agentCountArgBuffer, 0);
        // Set uniforms
        triMaterial.SetPass(0);
        triMaterial.SetColor("col", triColor);
        triMaterial.SetFloat("unitScale", unitScale);
        // Set Renderer Buffers
        triMaterial.SetBuffer("agentBuffer", agentBufferIn);
        triMaterial.SetBuffer("triBuffer", agentInstanceBuffer);

        // Draw Verts
        Graphics.DrawProceduralIndirect(MeshTopology.Points, agentCountArgBuffer, 0);

        // Clear Instance Buffer
        agentInstanceBuffer.ClearAppendBuffer();

        // Clear returned agents
        returnBuffer.ClearAppendBuffer();


    }



    void OnDisable()
    {
        // Release Buffers
        agentBuffer0.Release();
        agentBuffer1.Release();
        spawnBuffer.Release();
        freeBuffer.Release();
        agentInstanceBuffer.Release();
        targetsBuffer.Release();
        returnBuffer.Release();

        agentCountArgBuffer.Release();
    }


    
    public bool Spawn(AgentData spawningAgent)
    {
        if (spawningAgents.Count >= BLOCK_SIZE*spawnerBlockCount || spawningAgents.Count >= (maxAgents-BLOCK_SIZE*spawnerBlockCount*4) - agentCount) return false;

        if (spawningAgent.life < 0.1f) spawningAgent.life = 0.1f;
        spawningAgents.Add(spawningAgent);
        return true;
    }

}

public static class MyExtensions
{
    public static void ClearAppendBuffer(this ComputeBuffer appendBuffer)
    {
        // This resets the append buffer buffer to 0
        RenderTexture dummy1 = RenderTexture.GetTemporary(8, 8, 24, RenderTextureFormat.ARGB32);
        RenderTexture dummy2 = RenderTexture.GetTemporary(8, 8, 24, RenderTextureFormat.ARGB32);
        RenderTexture active = RenderTexture.active;

        Graphics.SetRandomWriteTarget(1, appendBuffer);
        Graphics.Blit(dummy1, dummy2);
        Graphics.ClearRandomWriteTargets();

        RenderTexture.active = active;

        dummy1.Release();
        dummy2.Release();
    }
}