using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScript : MonoBehaviour
{
    private GameObject player;

    private DangerArea dangerZone;
    [HideInInspector]
    public bool checkPlayerAlive;

	// Use this for initialization
	void Start ()
	{
	    player = GameObject.FindGameObjectWithTag("Player");
	    dangerZone = FindObjectOfType<DangerArea>();
	    checkPlayerAlive = player.transform.GetComponent<Unit>().alive;
	}
	
	// Update is called once per frame
	void Update () {
	    if (checkPlayerAlive)
	    {
	        checkPlayerAlive = player.transform.GetComponent<Unit>().alive;
	        if (!checkPlayerAlive)
	        {
	            dangerZone.enemieIdle();
	            checkPlayerAlive = false;
	        }
	    }
	}
}
