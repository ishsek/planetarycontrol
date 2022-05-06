using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class playerNetworkObjectScript : NetworkBehaviour
{
    public int soldiersToSpawn = 1;
    public int shieldbotsToSpawn = 1;
    public int incrementSpawnPoints = 0;

    [SyncVar(hook = "SetName")]
    public string myName;
    public gameModeManager.Player playerName;
    [SyncVar(hook = "SetPlayerNameSet")]
    public bool playerNameSet = false;
    [SyncVar(hook = "SetInitialSync")]
    public bool initialSync = false;

    public GameObject soldierUnit;
    public GameObject shieldbotUnit;
    public GameObject networkBullet;

    public List<GameObject> myUnits;
    bool checkUnits = true;
    public List<GameObject> mySpawnPoints;
    private bool checkSpawnPoints = true;

    public GameObject soldier;
    public GameObject myCamParent;
    public GameObject myCam;
    public GameObject myPointerParent;
    public GameObject myGun;
    public bool unitnamesset = false;
    public int namingCounter = 0;
    private gameModeManager modeManager;

    public List<GameObject> player1objs;
    public List<GameObject> player2objs;

    public override void OnStartClient()
    {
        this.transform.name = myName;
        if (myName != playerName.ToString())
        {
            if (myName == gameModeManager.Player.player1.ToString())
            {
                playerName = gameModeManager.Player.player1;
            }
            else if (myName == gameModeManager.Player.player2.ToString())
            {
                playerName = gameModeManager.Player.player2;
            }
        }
    }

    public override void OnStartLocalPlayer()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer == false)
        {
            // This object belongs to another player, exit function
            return;
        }

        //CmdSpawnSoldier();
        //CmdSpawnUnit(soldierUnit);
    }


    // Update is called once per frame
    void Update()
    {
        //if (GameObject.FindGameObjectsWithTag("PlayerNetworkObject").Length == 1) return;
        //if(GameObject.FindGameObjectsWithTag("PlayerNetworkObject").Length > 1 && SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Level2"))
        //{
        //    SceneManager.SetActiveScene(SceneManager.GetSceneByName("Level2"));
        //    return;
        //}

        // Move this player network object to the currently active scene
        //if (initialSync && transform.parent != GameObject.FindGameObjectWithTag("SceneChangeUtilityParent").transform)
        //{
        //    transform.SetParent(GameObject.FindGameObjectWithTag("SceneChangeUtilityParent").transform);
        //}

        // If the name is set from the server then make sure the transforms name is synced
        if (playerNameSet && myName != null && transform.name != myName) transform.name = myName;

        // If already synced initially (units have spawned) then add spawned units to my units list
        if (initialSync == true && SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2") && checkUnits)
        {
            foreach (Transform child in transform)
            {
                myUnits.Add(child.gameObject);
            }
            checkUnits = false;
        }

        //HideMyUnitsNetwork();

        // If im not the local player that owns this object dont run past here
        if (isLocalPlayer == false)
        {
            return;
        }

        if (playerNameSet && playerName == gameModeManager.Player.player1)
        {
            if (modeManager.player1win)
            {
                modeManager.youWinImage.gameObject.SetActive(true);
            }
            else if (modeManager.player2win)
            {
                modeManager.youLoseImage.gameObject.SetActive(true);
            }
        }
        else if (playerNameSet && playerName == gameModeManager.Player.player2)
        {
            if (modeManager.player1win)
            {
                modeManager.youLoseImage.gameObject.SetActive(true);
            }
            else if (modeManager.player2win)
            {
                modeManager.youWinImage.gameObject.SetActive(true);
            }
        }

        if (playerNameSet && initialSync)
        {
            if (modeManager.currentMode == gameModeManager.Mode.thirdperson)
            {
                if (playerName == gameModeManager.Player.player1)
                    CmdHideUnits(modeManager.player1fightunitname);
                else if (playerName == gameModeManager.Player.player2)
                    CmdHideUnits(modeManager.player2fightunitname);
            }
            else if (modeManager.currentMode == gameModeManager.Mode.transitionToStrategy)
            {
                CmdShowUnits();
            }
        }

        HandleUI();

        modeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>();
        if (modeManager.currentMode == gameModeManager.Mode.transitionToThirdPerson)
        {
            if (playerName == gameModeManager.Player.player1)
            {
                GameObject temp = transform.Find(modeManager.player1fightunitname).gameObject;
                temp.GetComponent<moveUnit>().selected = true;
                Camera.main.GetComponent<isometricCamera>().selectedPlayer = temp;
                Camera.main.GetComponent<ThirdPersonCamera>().selectedPlayer = temp;
            }
            else if (playerName == gameModeManager.Player.player2)
            {
                GameObject temp = transform.Find(modeManager.player2fightunitname).gameObject;
                temp.GetComponent<moveUnit>().selected = true;
                Camera.main.GetComponent<isometricCamera>().selectedPlayer = temp;
                Camera.main.GetComponent<ThirdPersonCamera>().selectedPlayer = temp;
            }
        }

        if (playerNameSet)
        {
            if (GameObject.FindGameObjectsWithTag("PlayerNetworkObject").Length > 1 && unitnamesset == false)
            {
                namingCounter++;
                if (namingCounter > 180)
                {
                    CmdSetUnitNames("test");
                    GameObject.Find("Crosshairs").SetActive(false);
                    unitnamesset = true;
                    if (playerName == gameModeManager.Player.player1)
                    {
                        foreach(Transform child in transform)
                        {
                            player1objs.Add(child.gameObject);
                        }
                    }
                    else if (playerName == gameModeManager.Player.player2)
                    {
                        foreach (Transform child in transform)
                        {
                            player2objs.Add(child.gameObject);
                        }
                    }
                }
                
            }
            Debug.Log(connectionToClient + ": Current turn: " + modeManager.turn);
            if (playerName == gameModeManager.Player.player1 && GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>().turn == 0)
            {
                RequestAuthority();
                myCam = Camera.main.gameObject;
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<moveUnit>().selected)
                    {
                        myCam.GetComponent<isometricCamera>().selectedPlayer = child.gameObject;
                    }
                }

            } else if (playerName == gameModeManager.Player.player2 && GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>().turn == 1)
            {
                RequestAuthority();
                myCam = Camera.main.gameObject;
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<moveUnit>().selected)
                    {
                        myCam.GetComponent<isometricCamera>().selectedPlayer = child.gameObject;
                    }
                }
            }
        }

        // If I havent synced yet initially then start the sync
        if (initialSync == false)
        {
            // Check how many players there are. if there's more than 1 then spawn each players units
            Debug.Log("I get to this point");
            Debug.Log("Current active scene: " + SceneManager.GetActiveScene());
            GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerNetworkObject");
            Debug.Log("Players: " + players.Length);
            if (players.Length > 0)
            {
                Debug.Log(this.name + ": Two players in the scene, spawning my units");
                

                //CmdSpawnUnit("shieldbot", Vector3.zero, Quaternion.identity);
                //SetInitialSync(true);
                if (playerNameSet != true)
                {
                    GameObject[] currentplayers = GameObject.FindGameObjectsWithTag("PlayerNetworkObject");
                    bool tester = false;
                    foreach (GameObject player in currentplayers)
                    {
                        if (player.GetComponent<playerNetworkObjectScript>().playerName == gameModeManager.Player.player1 && player != this.gameObject)
                        {
                            playerNameSet = true;
                            playerName = gameModeManager.Player.player2;
                            SetName(playerName.ToString());
                            SetPlayerNameSet(playerNameSet);
                            this.transform.name = playerName.ToString();
                            tester = true;
                            return;
                        }
                    }
                    if (tester == false)
                    {
                        playerNameSet = true;
                        playerName = gameModeManager.Player.player1;
                        SetName(playerName.ToString());
                        SetPlayerNameSet(playerNameSet);
                        this.transform.name = playerName.ToString();
                        return;
                    }
                    
                }

                if (playerNameSet)
                {
                    // Grab the current players spawn points based on which player it is
                    if (playerName == gameModeManager.Player.player1 && checkSpawnPoints)
                    {
                        GameObject parent = GameObject.FindGameObjectWithTag("Player1SpawnLocations");
                        Debug.LogWarning(parent.name + "");
                        foreach (Transform spawnPoint in parent.transform)
                        {
                            mySpawnPoints.Add(spawnPoint.gameObject);
                        }
                        checkSpawnPoints = false;
                    }
                    else if (playerName == gameModeManager.Player.player2 && checkSpawnPoints)
                    {
                        GameObject parent = GameObject.FindGameObjectWithTag("Player2SpawnLocations");
                        Debug.LogWarning(parent.name + "");
                        foreach (Transform spawnPoint in parent.transform)
                        {
                            mySpawnPoints.Add(spawnPoint.gameObject);
                        }
                        checkSpawnPoints = false;
                    }
                }
                if (checkSpawnPoints == false)
                {
                    // Spawn this players units
                    for (int i = 0; i < soldiersToSpawn; i++)
                    {
                        Vector3 pos = mySpawnPoints[incrementSpawnPoints].transform.position;
                        Quaternion rot = mySpawnPoints[incrementSpawnPoints].transform.rotation;
                        CmdSpawnUnit("soldier", pos, rot);
                        incrementSpawnPoints++;
                    }
                    for (int i = 0; i < shieldbotsToSpawn; i++)
                    {
                        CmdSpawnUnit("shieldbot", mySpawnPoints[incrementSpawnPoints].transform.position, mySpawnPoints[incrementSpawnPoints].transform.rotation);
                        incrementSpawnPoints++;
                    }
                }

                SetInitialSync(true);
            } else if (players.Length == 1)
            {
                if (playerNameSet != true)
                {
                    playerNameSet = true;
                    playerName = gameModeManager.Player.player1;
                    SetName(playerName.ToString());
                    SetPlayerNameSet(playerNameSet);
                    this.transform.name = playerName.ToString();
                }
            }
        } else
        {
            //transform.SetParent(GameObject.FindGameObjectWithTag("SceneChangeUtilityParent").transform);
            //GameObject.Find("NetworkManager").GetComponent<NetworkManagerHUD>().showGUI = false;
        }
        
    }

    public void SetInitialSync(bool syncVal)
    {
        initialSync = syncVal;
        CmdUpdateInitialSync(syncVal);
    }

    public void SetName(string newName)
    {
        myName = newName;
        CmdSetName(newName);
    }

    public void SetPlayerNameSet(bool newSet)
    {
        playerNameSet = newSet;
        CmdSetPlayerNameSet(newSet);
    }


    public void ResetMe()
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<UnitRoundReset>().ResetMe();
        }
    }

    public void ShootBullet(Vector3 position, Quaternion rotation, int damage)
    {
        CmdShootBullet(position, rotation, damage);
    }


    public void RequestAuthority()
    {
        //if (GameObject.FindGameObjectWithTag("GameManager").GetComponent<NetworkIdentity>().clientAuthorityOwner != GetComponent<NetworkIdentity>().connectionToClient)
        //{
            Debug.Log("Attempting to assign authority for mode manager");
            CmdAssignNetworkAuthority(GameObject.Find("GameModeManager").GetComponent<NetworkIdentity>(), GetComponent<NetworkIdentity>());
        //}
        //if (GameObject.Find("NetworkedInterfaceHandler").GetComponent<NetworkIdentity>().clientAuthorityOwner != GetComponent<NetworkIdentity>().connectionToClient)
        //{
            Debug.Log("Attempting to assign authority for turn UI");
            CmdAssignNetworkAuthority(GameObject.Find("NetworkedInterfaceHandler").GetComponent<NetworkIdentity>(), GetComponent<NetworkIdentity>());
        //}
    }

    public void HandleUI()
    {
        if (playerNameSet)
        {
            GameObject turnUIbutton = GameObject.Find("TurnUI");
            if (turnUIbutton == null) return;
            if (playerName == gameModeManager.Player.player1)
            {
                if (modeManager.turn == 0)
                {
                    turnUIbutton.transform.GetChild(0).GetComponent<Text>().text = "End Turn";
                }
                else if (modeManager.turn == 1)
                {
                    turnUIbutton.transform.GetChild(0).GetComponent<Text>().text = "Enemy Turn";
                }
            }
            else if (playerName == gameModeManager.Player.player2)
            {
                if (modeManager.turn == 1)
                {
                    turnUIbutton.transform.GetChild(0).GetComponent<Text>().text = "End Turn";
                }
                else if (modeManager.turn == 0)
                {
                    turnUIbutton.transform.GetChild(0).GetComponent<Text>().text = "Enemy Turn";
                }
            }
            
        }
    }


    [Command]
    public void CmdHideUnits(string name)
    {
        RpcHideUnits(name);
    }

    [ClientRpc]
    public void RpcHideUnits(string name)
    {
        foreach(Transform child in transform)
        {
            if (child.gameObject.name != name)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    [Command]
    public void CmdShowUnits()
    {
        RpcShowUnits();
    }

    [ClientRpc]
    public void RpcShowUnits()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    [Command]
    public void CmdSpawnUnit(string unitType, Vector3 pos, Quaternion rotation)
    {
        GameObject instUnit = null;
        if (unitType == "soldier") { instUnit = Instantiate(soldierUnit, pos, rotation, this.transform); }
        else if (unitType == "shieldbot") { instUnit = Instantiate(shieldbotUnit, pos, rotation, this.transform); }
        instUnit.GetComponent<moveUnit>().myPlayer = this.gameObject;
        NetworkServer.SpawnWithClientAuthority(instUnit, connectionToClient);
        //instSoldierUnit.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
        Debug.Log(connectionToClient + ": spawned a soldier");
        RpcSpawnUnit(instUnit);
    }

    [ClientRpc]
    public void RpcSpawnUnit(GameObject instUnit)
    {
        instUnit.GetComponent<moveUnit>().myPlayer = this.gameObject;
    }

    [Command]
    public void CmdUpdateInitialSync(bool syncVal)
    {
        initialSync = syncVal;
    }

    [Command]
    public void CmdSetDestination(Vector3 position, NetworkInstanceId unitID)
    {
        NetworkServer.FindLocalObject(unitID).GetComponent<moveUnit>().navMeshMoveTarget = position;
        Debug.Log(gameObject.name + ": Calling COMMAND to set unit destination to " + position);
    }

    [Command]
    public void CmdSetUnitNames(string name)
    {
        RpcSetUnitNames(name);
    }

    [ClientRpc]
    public void RpcSetUnitNames(string name)
    {
        int incrementSoldierunits = 1;
        int incrementShieldBotUnits = 1;
        foreach (Transform child in transform)
        {
            if (child.name == "SoldierModel(Clone)")
            {
                child.name = "soldier" + incrementSoldierunits;
                incrementSoldierunits++;
            }
            else if (child.name == "ShieldBot(Clone)")
            {
                child.name = "shieldbot" + incrementShieldBotUnits;
                incrementShieldBotUnits++;
            }
        }
    }

    [Command]
    public void CmdSetName(string newName)
    {
        myName = newName;
        RpcSetName(myName);
    }

    [ClientRpc]
    public void RpcSetName(string newName)
    {
        this.transform.name = newName;
        if (myName != playerName.ToString())
        {
            if (myName == gameModeManager.Player.player1.ToString())
            {
                playerName = gameModeManager.Player.player1;
            }
            else if (myName == gameModeManager.Player.player2.ToString())
            {
                playerName = gameModeManager.Player.player2;
            }
        }
    }

    [Command]
    public void CmdSetPlayerNameSet(bool newSet)
    {
        playerNameSet = newSet;
    }

    [Command]
    public void CmdAssignNetworkAuthority(NetworkIdentity cubeId, NetworkIdentity clientId)
    {
        //If -> cube has a owner && owner isn't the actual owner
        if (cubeId.clientAuthorityOwner != null && cubeId.clientAuthorityOwner != clientId.connectionToClient)
        {
            // Remove authority
            if (cubeId.RemoveClientAuthority(cubeId.clientAuthorityOwner))
            {
                Debug.LogWarning("Successfully removed authority");
            } else
            {
                Debug.LogWarning("Failed to remove authority");
            }
        }

        //If -> cube has no owner
        if (cubeId.clientAuthorityOwner == null)
        {
            // Add client as owner
            if (cubeId.AssignClientAuthority(clientId.connectionToClient))
            {
                Debug.LogWarning("Successfully assigned authority");
            }
            else
            {
                Debug.LogWarning("Failed to assign authority");
            }
        }
    }

    [Command]
    public void CmdShootBullet(Vector3 startPos, Quaternion startRot, int damage)
    {
        GameObject bullet = Instantiate(networkBullet, startPos, startRot);
        NetworkServer.Spawn(bullet);
        bullet.GetComponent<laserBulletScript>().SetDamage(damage);
        RpcSetDamage(bullet.GetComponent<NetworkIdentity>(), damage);
        
    }

    public void RpcSetDamage(NetworkIdentity bulletID, int damage)
    {
        NetworkServer.FindLocalObject(bulletID.netId).GetComponent<laserBulletScript>().SetDamage(damage);
    }

    
}
