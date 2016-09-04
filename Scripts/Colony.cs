using UnityEngine;
using System.Collections;


public class Colony : MonoBehaviour {

    public float minScale = .7f;
    public float maxScale = 1.0f;
    public float stepMin = 0.1f;
    public float stepMax = 0.2f;
 

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        float scale = transform.localScale.x;
        scale += Random.Range(stepMin, stepMax);
        scale = Mathf.Clamp(scale, minScale, maxScale);
        transform.localScale = new Vector3(scale, scale, scale);
	}
}
