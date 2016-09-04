using UnityEngine;
using System.Collections;

public class LeaderBoardText : MonoBehaviour {

    private TextMesh textMesh;
    private AgentWorld world;
    public float screenX = 0.1f;
    public float screenY = 0.1f;
    public float scaleRatio = 1.0f / 60.0f;

    // Use this for initialization
    void Start () {
        textMesh = GetComponent<TextMesh>();
        world = FindObjectOfType<AgentWorld>();
	}

    public void setLeaderBoardText(string s)
    {
        textMesh.text = "LeaderBoard:\n"+s;
        textMesh.text = textMesh.text.Replace("<", "");
        textMesh.text = textMesh.text.Replace(">", "");
    }

    // Update is called once per frame
    void Update () {
        screenX = Mathf.Clamp01(screenX);
        screenY = Mathf.Clamp01(screenY);

        float camSize = Camera.main.orthographicSize;
        textMesh.transform.localScale = new Vector3(scaleRatio * camSize, scaleRatio * camSize, 1.0f);

        int x = (int)(screenX * Screen.width);
        int y = (int)(Screen.height - screenY * Screen.height);

        Vector3 textWorldPos = new Vector3(x, y, 0.0f);
        textWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(x, y, 0.0f));
        this.transform.position = textWorldPos;

        textMesh.color = world.triColor;
    }
}
