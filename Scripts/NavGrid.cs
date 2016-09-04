using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NavGrid : MonoBehaviour {

    // product must be divisible by 8
    public int XEnd = 8;
    public int YEnd = 7;
    
    public float scale = 2.0f;
    public GameObject floorMarker;
    public GameObject wall;
    public GameObject targetMarker;
    
    private int[] costs;
    private GameObject[] tiles; 

    private int targetX;
    private int targetY;

    private AgentWorld agentWorld;
    private Camera Cam;
    bool hasWorldChanged = false;

    private int[] distances;
    public Vector2[] flowField;
    private int[] neighborsX;
    private int[] neighborsY;



    List<int> toVisitX;
    List<int> toVisitY;



    void passDataToWorld()
    {
        //agentWorld.target = new Vector2(targetX, targetY) * scale;
        //agentWorld.setFlowField(flowField);
    }

    // Use this for initialization
	void Start () {
        //agentWorld = FindObjectOfType<AgentWorld>();
        flowField = new Vector2[XEnd * YEnd];
        distances = new int[XEnd * YEnd];
        neighborsX = new int[8];
        neighborsY = new int[8];
        toVisitX = new List<int>(XEnd * YEnd);
        toVisitY = new List<int>(XEnd * YEnd);

        Cam = FindObjectOfType<Camera>();

        costs = new int[XEnd * YEnd];
        tiles = new GameObject[XEnd * YEnd];
        for (int i = 0;i< XEnd;i++)
        {
            for(int j = 0;j < YEnd;j++)
            {
                if (Random.Range(0.0f, 20.0f) > 123.0f)
                {
                    costs[i * YEnd + j] = int.MaxValue;
                    GameObject wallInstance = (GameObject)GameObject.Instantiate(wall, new Vector3(i, j, 0.0f) * scale, Quaternion.identity);
                    wallInstance.transform.localScale = new Vector3(scale, scale);
                    tiles[i * YEnd + j] = wallInstance;
                }
                else
                {
                    costs[i * YEnd + j] = 1;
                    tiles[i* YEnd + j] = (GameObject)GameObject.Instantiate(floorMarker, new Vector3(i, j, 0.0f) * scale, Quaternion.identity);
                }
            }
        }
        // init target
        targetX = 0;
        targetY = 0;
        costs[0] = 0;
        GameObject.Destroy(tiles[0]);
        tiles[0] = (GameObject)GameObject.Instantiate(targetMarker, Vector3.zero, Quaternion.identity);

        // initial pathing
        hasWorldChanged = true;

	}


    int getFlowNeighbors(int x, int y)
    {
        int count = 0;
        bool right = x + 1 < XEnd;
        bool left = x - 1 >= 0;
        bool above = y + 1 < YEnd;
        bool below = y - 1 >= 0;
        

        if (right)
        {
            neighborsX[count] = x + 1;
            neighborsY[count] = y;
            count++;
        }
        if (left)
        {
            neighborsX[count] = x - 1;
            neighborsY[count] = y;
            count++;
        }
        if (above)
        {
            neighborsX[count] = x;
            neighborsY[count] = y + 1;
            count++;
        }
        if (below)
        {
            neighborsX[count] = x;
            neighborsY[count] = y - 1;
            count++;
        }
        if(above && right)
        {
            neighborsX[count] = x + 1;
            neighborsY[count] = y + 1;
            count++;
        }
        if(above && left)
        {
            neighborsX[count] = x - 1;
            neighborsY[count] = y + 1;
            count++;
        }
        if (below && right)
        {
            neighborsX[count] = x + 1;
            neighborsY[count] = y - 1;
            count++;
        }
        if (below && left)
        {
            neighborsX[count] = x - 1;
            neighborsY[count] = y - 1;
            count++;
        }
        return count;
    }

    int getNeighbors(int x, int y)
    {
        int count = 0;        
        if(x+1<XEnd)
        {
            neighborsX[count] = x+1;
            neighborsY[count] = y;
            count++;
        }
        if(x-1>=0)
        {
            neighborsX[count] = x - 1;
            neighborsY[count] = y;
            count++;
        }
        if(y+1<YEnd)
        {
            neighborsX[count] = x;
            neighborsY[count] = y+1;
            count++;
        }
        if(y-1>=0)
        {
            neighborsX[count] = x;
            neighborsY[count] = y-1;
            count++;
        }

        return count;
    }

    void generateDijkstraGrid()
    {
        // set initial distances
        for(int i=0;i<XEnd* YEnd;i++)
        {
            distances[i] = int.MaxValue;
        }
        distances[targetX * YEnd + targetY] = 0;
        toVisitX.Clear();
        toVisitY.Clear();
        toVisitX.Add(targetX);
        toVisitY.Add(targetY);

        for (int i = 0; i < toVisitX.Count; i++)
        {
            int vIndex = toVisitX[i] * YEnd + toVisitY[i];
            int nCount = getNeighbors(toVisitX[i], toVisitY[i]);
            for (int j = 0; j < nCount; j++)
            {
                int nIndex = neighborsX[j] * YEnd + neighborsY[j];
                int nCost = costs[nIndex];
                int nDistance = distances[nIndex];
                if(nCost < int.MaxValue && nDistance == int.MaxValue)
                {
                    toVisitX.Add(neighborsX[j]);
                    toVisitY.Add(neighborsY[j]);
                    distances[nIndex] = distances[vIndex] + 1;
                }
            }
        }
    }


    void generateFlowField()
    {
        
        for (int i = 0;i< XEnd * YEnd;i++)
        {
            flowField[i] = Vector2.zero;
        }

        for (int i = 0; i < XEnd; i++)
        {
            for(int j = 0;j< YEnd;j++)
            {
                if (costs[i * YEnd + j] == int.MaxValue) continue;
                if (i == targetX && j == targetY) continue;
                int nCount = getFlowNeighbors(i, j);
                int minX = -1;
                int minY = -1;
                int minDist = int.MaxValue;
                for(int k = 0;k<nCount;k++)
                {
                    int nX = neighborsX[k];
                    int nY = neighborsY[k];
                    int dist = distances[nX * YEnd + nY];
                    if(dist < minDist)
                    {
                        minX = nX;
                        minY = nY;
                        minDist = dist;
                    }
                }
                if (minX > -1)
                {
                    flowField[i * YEnd + j].x = minX - i;
                    flowField[i * YEnd + j].y = minY - j;
                    flowField[i * YEnd + j].Normalize();
                }
                
            }
        }
    }


	// Update is called once per frame
	void Update () {
        mouseChangeTarget();
        mouseFlipWall();
        if (hasWorldChanged)
        {
            // repath
            generateDijkstraGrid();
            generateFlowField();
            passDataToWorld();

            hasWorldChanged = false;
        }
	}



    private Vector2 GetNavVectorFromMouse()
    {
        Vector2 coords = Input.mousePosition;
        coords = Cam.ScreenToWorldPoint(coords);
        coords /= scale;
        coords += Vector2.one * 0.5f;
        return coords;
    }

    bool changeTarget(int X, int Y)
    {        

        if (X < 0 || X >= XEnd || Y < 0 || Y >= YEnd) return false;
        
        // remove old target tile and replace with floor
        putFloorAt(targetX, targetY);
        // remove tile at new target pos and add target marker
        targetX = X;
        targetY = Y;
        GameObject.Destroy(tiles[targetX * YEnd + targetY]);
        costs[targetX * YEnd + targetY] = 0;        
        tiles[targetX * YEnd + targetY] = (GameObject)GameObject.Instantiate(targetMarker,new Vector3(targetX, targetY, 0.0f) * scale, Quaternion.identity);

        Debug.Log(string.Format("{0},{1}", targetX, targetY));
        hasWorldChanged = true;
        return true;
    }

    bool mouseFlipWall()
    {
        if (!Input.GetMouseButtonDown(0)) return false;
        Vector2 coords = GetNavVectorFromMouse();

        return flipWall(Mathf.FloorToInt(coords.x), Mathf.FloorToInt(coords.y));
    }

    bool mouseChangeTarget()
    {
        if (!Input.GetMouseButtonDown(1)) return false;
        Vector2 coords = GetNavVectorFromMouse();

        return changeTarget(Mathf.FloorToInt(coords.x), Mathf.FloorToInt(coords.y));
    }

    bool flipWall(int X, int Y)
    {
        if (X < 0 || X >= XEnd || Y < 0 || Y >= YEnd) return false;

        if (X == targetX && Y == targetY) return false;
        if(costs[X*YEnd+Y] == int.MaxValue)
        {
            putFloorAt(X, Y);
        }
        else
        {
            putWallAt(X, Y);
        }

        Debug.Log(string.Format("{0},{1}", X, Y));
        
        return true;
    }

    private void putFloorAt(int X, int Y)
    {
        // remove current tile
        GameObject.Destroy(tiles[X * YEnd + Y]);
        // add empty floor
        costs[X * YEnd + Y] = 1;
        tiles[X * YEnd + Y] = (GameObject)GameObject.Instantiate(floorMarker, new Vector3(X, Y, 0.0f) * scale, Quaternion.identity);

        hasWorldChanged = true;
    }

    private void putWallAt(int X, int Y)
    {
        // remove current tile
        GameObject.Destroy(tiles[X * YEnd + Y]);
        // add wall
        costs[X * YEnd + Y] = int.MaxValue;
        GameObject wallInstance = (GameObject)GameObject.Instantiate(wall, new Vector3(X, Y, 0.0f) * scale, Quaternion.identity);
        wallInstance.transform.localScale = new Vector3(scale, scale);
        tiles[X * YEnd + Y] = wallInstance;

        hasWorldChanged = true;
    }
}
