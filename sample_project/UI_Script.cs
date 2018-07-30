using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Script : MonoBehaviour
{
    [SerializeField]
    Text gameOverText, healthText, rollCountDown;

    private GameScript gameScript;

    private GameObject player;
	// Use this for initialization
	void Start ()
	{
	    gameScript = FindObjectOfType<GameScript>();
	    gameOverText.text = "";
	    healthText.text = "";
	    rollCountDown.text = "";
        player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
	    if (!gameScript.checkPlayerAlive)
	    {
	        gameOverText.text = "GAME OVER";
	    }
	    healthText.text = player.GetComponent<Unit>().health.ToString();
	    rollCountDown.text = "Roll: " + player.GetComponent<PlayerController>().rollCheck.ToString();
	}
}
