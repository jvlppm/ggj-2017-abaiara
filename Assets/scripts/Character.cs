using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    public Tile tile;

    public float hp;
    public float ap;
    public float speed = 1;
    public int mp;
    public Sprite avatar;
    public string displayName;
    public Skill[] skills;
    
    

	// Use this for initialization
	void Start () {
        if (tile) {
            tile.character = this;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    void moveTo (Vector3 position)
    {
        transform.position = position;
    }

    public IEnumerator MoveTo(Vector3 b) {
        var a = transform.position;
         float step = (speed / (a - b).magnitude) * Time.fixedDeltaTime;
         float t = 0;
         while (t <= 1.0f) {
             t += step; // Goes from 0 to 1, incrementing by step each time
             transform.position = Vector3.Lerp(a, b, t);
             yield return new WaitForFixedUpdate();         // Leave the routine and return here in the next frame
         }
         transform.position = b;
    }

}
