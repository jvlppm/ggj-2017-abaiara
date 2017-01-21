using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class skill : MonoBehaviour {
    int actionPoint;
    public Text error;
    // Use this for initialization
    void Start()
    {
        actionPoint = 2;
    }
	
	// Update is called once per frame
	void Update () {
		if (actionPoint < 0)
        {
            error.gameObject.SetActive(true);
        }
      
	}
    private void OnMouseDown()
    {
        
    }
}
