using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour
{
    private PlayerController player;
	// Use this for initialization
	void Start ()
	{
	    player = FindObjectOfType<PlayerController>();
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 newCamPos = new Vector2(player.transform.position.x, player.transform.position.y);
        transform.position = new Vector3(Mathf.Clamp(newCamPos.x, 2, 36), Mathf.Clamp(newCamPos.y,1,6), transform.position.z);
	}
}
