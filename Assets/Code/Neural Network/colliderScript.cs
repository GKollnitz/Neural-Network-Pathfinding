using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colliderScript : MonoBehaviour {
    bool up = false, down = true;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(gameObject.transform.position.y > -3)
        {
            up = false; down = true;
        }
        else if (gameObject.transform.position.y < -24)
        {
            up = true; down = false;
        }
        if(up)
            gameObject.transform.position = gameObject.transform.position + Vector3.up * Time.deltaTime * 2;
        else if(down)
            gameObject.transform.position = gameObject.transform.position + Vector3.down * Time.deltaTime * 2;

    }
}
