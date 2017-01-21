using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class character : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    void moveTo (Vector3 position)
    {
        transform.position = position;             
    }

    IEnumerator MoveFromTo(Vector3 a, Vector3 b, float speed) {
         float step = (speed / (a - b).magnitude) * Time.fixedDeltaTime;
         float t = 0;
         while (t <= 1.0f) {
             t += step; // Goes from 0 to 1, incrementing by step each time
             yield return new WaitForFixedUpdate();         // Leave the routine and return here in the next frame
         }
         transform.position = b;
 }
}
