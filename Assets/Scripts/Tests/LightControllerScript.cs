using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightControllerScript : MonoBehaviour {
    Light thisLight;

    // Use this for initialization
    void Start () {
        thisLight = GetComponent<Light>();
    }
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyUp("b"))
        {
            if (thisLight.enabled)
                thisLight.enabled = false;
            else
                thisLight.enabled = true;
        }
	}
}
