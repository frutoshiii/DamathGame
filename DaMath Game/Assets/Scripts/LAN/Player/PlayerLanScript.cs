using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerLanScript : NetworkBehaviour
{
    public static PlayerLanScript Instance = null;
    public string position = "";
    public GameObject trans;
    public List<GameObject> myList = new List<GameObject>();
    private bool isPlayer1Color;
    private bool isPlayer1Turn;
    private object selectedPiece;

    void Start()
    {
        if (Instance == null) 
        {
            Instance = this;
        }

        if (isLocalPlayer) 
        {
            if (isServer && isClient) //For Host
            {
                try
                {
                    GameObject.Find("InGameManager").GetComponent<LanGameEngine>().localPlayer = this.gameObject;
                    GameObject.Find("InGameManager").GetComponent<LanGameEngine>().localString = "Player1";
                    GameObject.Find("InGameManager").GetComponent<LanGameEngine>().localColor = "Red";
                    position = "HOST";
                }
                catch (Exception e) 
                {
                    Debug.LogError(e.Message);
                }
            }
            if (!isServer && isClient) //For Client
            {
                try
                {
                    GameObject.Find("InGameManager").GetComponent<LanGameEngine>().localPlayer = this.gameObject;
                    GameObject.Find("InGameManager").GetComponent<LanGameEngine>().localString = "Player2";
                    GameObject.Find("InGameManager").GetComponent<LanGameEngine>().localColor = "Blue";
                    position = "CLIENT";
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
    }


}
