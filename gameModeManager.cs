using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class gameModeManager : NetworkBehaviour
{
    [SyncVar]
    public int turn;
    public int battleCountdown;
    public float transitionDuration = 5f;
    public float defaultFightDuration = 20f;
    [SyncVar]
    public float fightDuration = 20f;
    [SyncVar]
    public string player1fightunitname;
    [SyncVar]
    public string player2fightunitname;
    public float currentTime;
    public Text timerUI;
    public Button turnEndButton;
    [SerializeField] public GameObject playerManager;
    [SerializeField] public GameObject enemyManager;
    [HideInInspector] public enum Mode { strategy, thirdperson, transitionToThirdPerson, transitionToStrategy}
    public int modeNum;
    [SyncVar]
    public Mode currentMode = Mode.strategy;
    [HideInInspector] public enum Player { none, player1, player2 }

    public Image youWinImage;
    public Image youLoseImage;
    
    [SyncVar]
    public bool player1win = false;
    [SyncVar]
    public bool player2win = false;

    float delayCount = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        turn = 0;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimerUI();
        UpdateEndTurnButton();
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2"))
        {
            if (!isServer) return;
            GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerNetworkObject");
            if (players.Length > 1)
            {
                delayCount += 1 * Time.deltaTime;
                if (delayCount > 10)

                    foreach (GameObject player in players)
                    {
                        if (player.GetComponent<playerNetworkObjectScript>().playerName == Player.player1 && player.GetComponent<playerNetworkObjectScript>().playerNameSet && player.transform.childCount == 0)
                        {
                            player2win = true;
                        }
                        else if (player.GetComponent<playerNetworkObjectScript>().playerName == Player.player2 && player.GetComponent<playerNetworkObjectScript>().playerNameSet && player.transform.childCount == 0)
                        {
                            player1win = true;
                        }
                    }
            }
        }
    }
    

    public void EndTurn()
    {
        if (turn == 0)
        {
            turn = 1;
        } 
        else
        {
            turn = 0;
            playerManager.GetComponent<PlayerManager>().AddCurrency();
        }
        ResetTurnvariables();
    }

    public void ResetTurnvariables()
    {
        enemyManager.GetComponent<EnemyControllerAI>().ResetMe();
        playerManager.GetComponent<PlayerManager>().ResetMe();
    }

    public void ChangeMode(Mode newMode)
    {
        //currentMode = newMode;
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2"))
        {
            Debug.Log("attempting COMMAND to change mode");
            CmdChangeMode(newMode);
        }
        else
        {
            currentMode = newMode;
        }
        Debug.Log(this.transform.name + ": Transitioning to mode " + currentMode);
        if (currentMode == Mode.strategy)
        {
            modeNum = 0;
        }
        else if (currentMode == Mode.thirdperson)
        {
            currentTime = fightDuration;
            modeNum = 1;
            if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Level2"))
            {
                foreach (Transform playerUnit in playerManager.transform)
                {
                    if (!playerUnit.GetComponent<moveUnitLocal>().selected)
                    {
                        playerUnit.gameObject.SetActive(false);
                    }
                }
                foreach (Transform enemyUnit in enemyManager.transform)
                {
                    if (!enemyUnit.GetComponent<enemySoldierAI>().selected)
                    {
                        enemyUnit.gameObject.SetActive(false);
                    }
                }
            }
            //StartCoroutine(TransitionStrategyAfterTime(fightDuration));
        }
        else if (currentMode == Mode.transitionToThirdPerson)
        {
            //StartCoroutine(ThirdPersonAfterTime(transitionDuration));
        }
        else if (currentMode == Mode.transitionToStrategy)
        {
            //StartCoroutine(ExecuteAfterTime(transitionDuration));
            if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Level2"))
            {
                foreach (Transform playerUnit in playerManager.transform)
                {
                    playerUnit.gameObject.SetActive(true);
                    playerUnit.GetComponent<moveUnitLocal>().selected = false;
                }
                foreach (Transform enemyUnit in enemyManager.transform)
                {
                    enemyUnit.gameObject.SetActive(true);
                    enemyUnit.GetComponent<enemySoldierAI>().selected = false;
                }
            }
        }

    }

    public void UpdateTimerUI()
    {
        // Update timer UI
        if (currentMode == Mode.thirdperson)
        {
            if (currentTime > 0)
            {
                currentTime -= 1 * Time.deltaTime;
                timerUI.text = "Fight Time: " + currentTime.ToString("F2");
            }
            else
            {
                ChangeMode(Mode.transitionToStrategy);
            }
        }
        else
        {
            timerUI.text = "";
        }
    }

    public void UpdateEndTurnButton()
    {
        if (turn == 0)
        {
            if (currentMode != Mode.strategy)
            {
                turnEndButton.gameObject.SetActive(false);
            }
            else
            {
                turnEndButton.gameObject.SetActive(true);
            }
        }
    }

    public void GameWon()
    {
        youWinImage.gameObject.SetActive(true);
    }

    public void GameLost()
    {
        youLoseImage.gameObject.SetActive(true);
    }


    [Command]
    public void CmdChangeMode(Mode newMode)
    {
        Debug.Log("COMMAND Changingmode");
        RpcChangeMode(newMode);
    }

    [ClientRpc]
    public void RpcChangeMode(Mode newMode)
    {
        Debug.Log("RPC Changingmode");
        currentMode = newMode;
    }

    [Command]
    public void CmdSetFightUnits(string player1unitname, string player2unitname)
    {
        RpcSetFightUnits(player1unitname, player2unitname);
    }

    [ClientRpc]
    public void RpcSetFightUnits(string player1unitname, string player2unitname)
    {
        player1fightunitname = player1unitname;
        player2fightunitname = player2unitname;
    }

    //[Command]
    //public void CmdAssignNetworkAuthority(NetworkIdentity cubeId, NetworkIdentity clientId)
    //{
    //    //If -> cube has a owner && owner isn't the actual owner
    //    if (cubeId.clientAuthorityOwner != null && cubeId.clientAuthorityOwner != clientId.connectionToClient)
    //    {
    //        // Remove authority
    //        cubeId.RemoveClientAuthority(cubeId.clientAuthorityOwner);
    //    }

    //    //If -> cube has no owner
    //    if (cubeId.clientAuthorityOwner == null)
    //    {
    //        // Add client as owner
    //        cubeId.AssignClientAuthority(clientId.connectionToClient);
    //    }
    //}

    //public void RequestAuthority(NetworkIdentity networkIdentity)
    //{
    //    CmdAssignNetworkAuthority(GetComponent<NetworkIdentity>(), networkIdentity);

    //}


    // NOT CURRENTLY IN USE



    IEnumerator TransitionStrategyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        // Code to execute after the delay
        ChangeMode(Mode.transitionToStrategy);
        //Debug.Log("CHANGE MODE TO TRANSITION STRATEGY");
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        // Code to execute after the delay
        ChangeMode(Mode.strategy);
        //Debug.Log("CHANGE MODE TO STRATEGY");
    }

    IEnumerator ThirdPersonAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        // Code to execute after the delay
        ChangeMode(Mode.thirdperson);
        //Debug.Log("CHANGE MODE TO THIRD PERSON");
    }
}
