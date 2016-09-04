using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
public class NumberGhost : MonoBehaviour {
    public string scoreText = "";
    float perturbationRadius = 2.0f;
    float speed = .1f;
    int time = 200;
    Vector3 vel;
    // Use this for initialization
	void Start () {
        Vector2 norm = Random.insideUnitCircle.normalized;
        Vector2 perturbation =  norm * perturbationRadius;
        vel = norm * speed;
        transform.position = transform.position + new Vector3(perturbation.x,perturbation.y,0.0f);
        GetComponent<TextMesh>().text = scoreText;
        

    }
	
	// Update is called once per frame
	void Update () {
        transform.position += vel;
        time--;
        if (time < 0) GameObject.Destroy(this.gameObject);
	}
}
