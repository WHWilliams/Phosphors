using UnityEngine;
using System.Collections;


[RequireComponent(typeof(TextMesh))]
public class Stats : MonoBehaviour {

    private TextMesh textMesh;
    private AgentWorld world;
    public float screenX = 0.1f;
    public float screenY = 0.1f;
    public float scaleRatio = 1.0f / 60.0f;


    // Use this for initialization
    void Start () {
        world = FindObjectOfType<AgentWorld>();
        textMesh = GetComponent<TextMesh>();  
    }

    string getUpgradesProgress()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();

        foreach(Candidate c in System.Enum.GetValues(typeof(Candidate)))
        {
            
            string percentageString = (world.upgradeProgressRatio(c)*100.0f).ToString();
            if(percentageString.Length > 5)
            {
                percentageString = percentageString.Remove(4);
            }
            percentageString += "%";

            builder.AppendLine(c.ToString() + ":" + percentageString);
        }

        return builder.ToString();
    }

    void Update()
    {        
        screenX = Mathf.Clamp01(screenX);
        screenY = Mathf.Clamp01(screenY);

        float camSize = Camera.main.orthographicSize;
        textMesh.transform.localScale = new Vector3(scaleRatio*camSize,scaleRatio*camSize,1.0f);       

        int x = (int)(screenX*Screen.width);
        int y = (int)(Screen.height - screenY*Screen.height);

        Vector3 textWorldPos = new Vector3(x, y, 0.0f);
        textWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(x, y,0.0f));
        this.transform.position = textWorldPos;

        textMesh.color = world.triColor;

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.AppendLine("TOTAL:" + world.getAllTimeScore().ToString());
        builder.AppendLine("AVAIL:" + world.getScore().ToString());
        builder.Append(getUpgradesProgress());
        


        textMesh.text = builder.ToString();
    }
}
