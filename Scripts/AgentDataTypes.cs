using UnityEngine;
using System.Collections;

public struct AgentData
{
    public const int size = (sizeof(float) * 5) + (sizeof(int)*2);
    public Vector2 pos;
    public Vector2 vel;    
    public float life;
    public int targetIndex;
    public int playerID;
}



