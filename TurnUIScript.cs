using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TurnUIScript : NetworkBehaviour
{
    private gameModeManager modeManager;
    private Text buttonText;
    public string endTurntext = "End Turn";
    public string otherPlayerTurnText = "Enemy Turn";
    
    private bool if_enemey_played = false;
    private int gameTurns = 1;
    private Text currencyText;
    private Text turnCounterText;
    
    // Start is called before the first frame update
    void Start()
    {
        modeManager = GameObject.FindWithTag("GameManager").GetComponent<gameModeManager>();
        buttonText = transform.Find("Text").GetComponent<Text>();
        //turnCounterText = transform.Find("TurnCounter").GetComponent<Text>();
        //turnCounterText.text = "Turn: " + gameTurns;
    }

    // Update is called once per frame
    void Update()
    {
        if (modeManager == null) { modeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>(); return; }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2")) return;
        if (modeManager.turn == 0)
        {   
            if(if_enemey_played){
                gameTurns += 1;
                if_enemey_played = false;
            }

            if (modeManager.currentMode == gameModeManager.Mode.strategy)
            {
                if (this.GetComponent<Button>().interactable == false)
                {
                    this.GetComponent<Button>().interactable = true;
                    if (!buttonText.text.Equals(endTurntext)) buttonText.text = endTurntext;
                }
            }
        } 
        else if (modeManager.turn == 1)
        {
            if (!buttonText.text.Equals(otherPlayerTurnText)) buttonText.text = otherPlayerTurnText;
            if_enemey_played = true;
        }
    }

    public void ButtonEndTurn()
    {
        if (modeManager.turn == 0)
        {
            modeManager.EndTurn();
            this.GetComponent<Button>().interactable = false;
        }
    }
    
    public void ButtonEndTurnNetworked()
    {
        if (hasAuthority)
        {
            CmdEndTurn();
        } 
        else
        {
            Debug.Log(this.transform.name + ": Has no authority");
        }
    }

    [Command]
    public void CmdEndTurn()
    {
        RpcEndTurn();
    }

    [ClientRpc]
    public void RpcEndTurn()
    {
        if (modeManager.turn == 0)
        {
            modeManager.turn = 1;
        }
        else
        {
            modeManager.turn = 0;
        }
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("PlayerNetworkObject"))
        {
            player.GetComponent<playerNetworkObjectScript>().ResetMe();
        }
    }
}
