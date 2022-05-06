using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyControllerAI : MonoBehaviour
{
    [Header("Tunable Variables")]
    public float baseEnemySearchRange = 40f;
    public float movementBuffer = 3f;
    public float widenSearchRange = 10f;

    [Header("Non-tunable Variables")]
    public List<GameObject> enemyUnits;
    public List<GameObject> playerUnits;
    private gameModeManager modeManager;
    [SerializeField] private GameObject playerManager;
    [SerializeField] private GameObject playerBase;
    [SerializeField] private GameObject enemyBase;
    private int currentEnemy;
    [SerializeField] public GameObject currentEnemyRef;
    [HideInInspector] public enum EnemyAiStates {isoSearchMove, isoMoving, tpPlayerSearch, tpMoving, tpAttack}
    public EnemyAiStates currentState;
    [SerializeField] private float widenSearchValue = 0f;
    [SerializeField] public GameObject target;
    public GameObject gameovertext;

    // Start is called before the first frame update
    void Start()
    {
        modeManager = GameObject.FindWithTag("GameManager").GetComponent<gameModeManager>();
        foreach(Transform child in transform)
        {
            enemyUnits.Add(child.gameObject);
        }
        foreach(Transform child in playerManager.transform)
        {
            playerUnits.Add(child.gameObject);
        }
        currentState = EnemyAiStates.isoSearchMove;
    }

    // Update is called once per frame
    void Update()
    {
        if (modeManager.turn == 1)
        {
            // if all enemies used up then end turn
            if (currentEnemy > enemyUnits.Count - 1)
            {
                modeManager.EndTurn();
                currentEnemy = 0;
                return;
            }
            // if not used up then start at first enemy and go through states
            else
            {
                currentEnemyRef = enemyUnits[currentEnemy];
                foreach (GameObject unit in enemyUnits)
                {
                    if (unit == currentEnemyRef) unit.GetComponent<enemySoldierAI>().selected = true;
                    else unit.GetComponent<enemySoldierAI>().selected = false;
                }
                switch(currentState)
                {
                    case EnemyAiStates.isoSearchMove:
                        StateIsoSearchMove();
                        break;
                    case EnemyAiStates.isoMoving:
                        StateIsoMoving();
                        break;
                    case EnemyAiStates.tpPlayerSearch:
                        StateTpPlayerSearch();
                        break;
                    case EnemyAiStates.tpMoving:
                        break;
                    case EnemyAiStates.tpAttack:
                        StateTpAttack();
                        break;
                }
            }
        }
        else if (modeManager.turn == 0)
        {
            for(int i = 0; i < enemyUnits.Count; i++)
            {
                if (enemyUnits[i] == null) break;
                if (enemyUnits[i] == currentEnemyRef) { enemyUnits[i].GetComponent<enemySoldierAI>().selected = true; }
                else enemyUnits[i].GetComponent<enemySoldierAI>().selected = false;
            }
            switch (currentState)
            {
                case EnemyAiStates.tpPlayerSearch:
                    StateTpPlayerSearch();
                    break;
                case EnemyAiStates.tpAttack:
                    StateTpAttack();
                    break;
            }
        }
    }


    // Search for the closest player unit, if all too far then move towards enemy base
    private void StateIsoSearchMove()
    {
        target = null;
        float distanceLeast = 0f;
        float distanceCheck = 0f;
        for (int i = 0; i < playerUnits.Count; i++)
        {
            distanceCheck = Vector3.Distance(currentEnemyRef.transform.position, playerUnits[i].transform.position);
            if (distanceLeast == 0f)
            {
                distanceLeast = distanceCheck;
                Debug.Log(currentEnemyRef.name + ": SETTING LEAST DISTANCE INITIALIZE TO " + distanceLeast);
            }
            if (distanceCheck <= distanceLeast)
            {
                distanceLeast = distanceCheck;
                // If the closest player is within range then make them the target
                if (distanceLeast < baseEnemySearchRange)
                {
                    Debug.Log(currentEnemyRef.name + ": FOUND PLAYER IN RANGE");
                    target = playerUnits[i].gameObject;
                }
            }
        }
        // If no target within range, set target to players base
        if (target == null || distanceLeast > Vector3.Distance(playerBase.transform.position, currentEnemyRef.transform.position))
        {
            target = playerBase;
        }
        // If the current unit is already in attack range
        if (Vector3.Distance(currentEnemyRef.transform.position, target.transform.position) < currentEnemyRef.GetComponent<enemySoldierAI>().attackRange)
        {
            if (target.tag == "Player")
            {
                moveUnitLocal tempMoveUnit = target.GetComponent<moveUnitLocal>();
                tempMoveUnit.selected = true;
                tempMoveUnit.cam.GetComponent<isometricCamera>().selectedPlayer = target;
                tempMoveUnit.cam.GetComponent<ThirdPersonCamera>().selectedPlayer = target;
                tempMoveUnit.myOutline.OutlineColor = tempMoveUnit.outlineHoverColor;
                tempMoveUnit.myOutline.OutlineWidth = 0f;
                modeManager.fightDuration = modeManager.defaultFightDuration;
                modeManager.ChangeMode(gameModeManager.Mode.transitionToThirdPerson);
                currentState = EnemyAiStates.tpPlayerSearch;
                return;
            } else if (target.tag == "PlayerBase")
            {
                Debug.Log(currentEnemyRef.name + ": ATTACK BASE");
                target.GetComponent<BaseLogicScript>().TakeDamage(currentEnemyRef.GetComponent<enemySoldierAI>().myGun.GetComponent<PlayerShoot>().gunDamage);
                currentEnemy++;
                currentState = EnemyAiStates.isoSearchMove;
                return;
            }
        } 
        // If not in attack range then move towards it
        else
        {
            NavMeshHit meshHitCheck;
            Vector3 targetDirection = target.transform.position - currentEnemyRef.transform.position;
            targetDirection.Normalize();
            Vector3 targetPoint = currentEnemyRef.transform.position + (targetDirection * currentEnemyRef.GetComponent<enemySoldierAI>().moveRange - targetDirection * movementBuffer);
            // If you find a point on the nav mesh in the direction of the target, move there
            if (NavMesh.SamplePosition(targetPoint, out meshHitCheck, 10f, NavMesh.AllAreas))
            {
                NavMeshAgent tempAgent = currentEnemyRef.GetComponent<enemySoldierAI>().myAgent;
                NavMeshPath tempPath = new NavMeshPath();
                tempAgent.CalculatePath(meshHitCheck.position, tempPath);
                widenSearchValue = 0f;

                // Check if the path is valid, otherwise dont move
                while (tempPath.status == NavMeshPathStatus.PathPartial)
                {
                    Debug.Log(currentEnemyRef.name + ": Couldn't find a valid path, trying again");
                    // Retest with a random x range;
                    if (Mathf.Abs(widenSearchValue) < widenSearchRange)
                    {
                        widenSearchValue = (Random.Range(-1, 1) >= 0) ? widenSearchValue * 1 : widenSearchValue * -1;
                        if (widenSearchValue >= 0) widenSearchValue++;
                        else widenSearchValue--;
                    }
                    else
                    {
                        Debug.Log(currentEnemyRef.name + ": Unable to find any valid path");
                        currentEnemy++;
                        currentState = EnemyAiStates.isoSearchMove;
                        return;
                    }
                    targetPoint += new Vector3(widenSearchValue, 0, 0);
                    NavMeshHit reCheck;
                    if (NavMesh.SamplePosition(targetPoint, out meshHitCheck, 10f, NavMesh.AllAreas))
                    {
                        tempAgent.CalculatePath(meshHitCheck.position, tempPath);
                    }
                }

                // If it found a valid path then set it as the destination
                tempAgent.SetDestination(meshHitCheck.position);
                tempAgent.isStopped = false;
                currentState = EnemyAiStates.isoMoving;
            }
        }
    }

    private NavMeshAgent currentEnemyAgent;

    // If the unit is moving then stop the unit when it reaches its destination
    private void StateIsoMoving()
    {
        if (currentEnemyAgent == null) currentEnemyAgent = currentEnemyRef.GetComponent<enemySoldierAI>().myAgent;
        else if (currentEnemyAgent != null && currentEnemyAgent != currentEnemyRef.GetComponent<enemySoldierAI>().myAgent)
            currentEnemyAgent = currentEnemyRef.GetComponent<enemySoldierAI>().myAgent;

        if (currentEnemyAgent.remainingDistance <= currentEnemyAgent.stoppingDistance)
        {
            Debug.Log(currentEnemyRef.name + ": Destination Reached");
            currentEnemyAgent.isStopped = true;
            currentEnemy++;
            currentState = EnemyAiStates.isoSearchMove;
            if (!currentEnemyAgent.hasPath || currentEnemyAgent.velocity.sqrMagnitude == 0f)
            {
                    
            }
        }
        
    }

    GameObject myEyes;
    public bool playerVisible = false;

    // Search for the player in third person mode
    private void StateTpPlayerSearch()
    {
        if (currentEnemyRef == null) return;
        playerVisible = false;
        if (modeManager.turn == 1)
        {
            if (modeManager.currentMode == gameModeManager.Mode.thirdperson)
            {
                if (myEyes != currentEnemyRef.transform.Find("Eyes").gameObject) myEyes = currentEnemyRef.transform.Find("Eyes").gameObject;
                if (!playerVisible)
                {
                    Vector3 raycastTarget = target.transform.Find("bodyTarget").transform.position;
                    Vector3 raycastDirection = raycastTarget - myEyes.transform.position;
                    raycastDirection.Normalize();
                    RaycastHit sightCheck;

                    if (Physics.Raycast(myEyes.transform.position, raycastDirection, out sightCheck, 100f))
                    {
                        if (sightCheck.transform.tag == "Player")
                        {
                            playerVisible = true;
                            currentState = EnemyAiStates.tpAttack;
                            return;
                        }
                    }
                }

            }
            else if (modeManager.currentMode == gameModeManager.Mode.strategy)
            {
                currentEnemy++;
                currentState = EnemyAiStates.isoSearchMove;
                return;
            }
        }
        else if (modeManager.turn == 0 && currentEnemyRef.GetComponent<enemySoldierAI>().selected)
        {
            if (modeManager.currentMode == gameModeManager.Mode.thirdperson)
            {
                if (myEyes != currentEnemyRef.transform.Find("Eyes").gameObject) myEyes = currentEnemyRef.transform.Find("Eyes").gameObject;
                if (!playerVisible)
                {
                    Vector3 raycastTarget = target.transform.Find("bodyTarget").transform.position;
                    Vector3 raycastDirection = raycastTarget - myEyes.transform.position;
                    raycastDirection.Normalize();
                    RaycastHit sightCheck;

                    if (Physics.Raycast(myEyes.transform.position, raycastDirection, out sightCheck, 100f))
                    {
                        if (sightCheck.transform.tag == "Player")
                        {
                            playerVisible = true;
                            currentState = EnemyAiStates.tpAttack;
                            return;
                        }
                    }
                }

            }
        }
    }

    // Player is visible so attack them
    private void StateTpAttack()
    {
        if (currentEnemyRef == null) return;
        if (modeManager.turn == 1)
        {
            if (modeManager.currentMode == gameModeManager.Mode.thirdperson)
            {
                if (playerVisible)
                {
                    currentEnemyRef.transform.LookAt(new Vector3(target.transform.position.x, currentEnemyRef.transform.position.y, target.transform.position.z));
                    Vector3 raycastTarget = target.transform.Find("bodyTarget").transform.position;
                    Vector3 raycastDirection = raycastTarget - myEyes.transform.position;
                    raycastDirection.Normalize();
                    RaycastHit shootCheck;

                    if (Physics.Raycast(myEyes.transform.position, raycastDirection, out shootCheck, 100f))
                    {
                        if (shootCheck.transform.tag == "Player")
                        {
                            playerVisible = true;
                            currentEnemyRef.GetComponent<enemySoldierAI>().myGun.GetComponent<PlayerShoot>().shootForEnemy(shootCheck.transform.Find("bodyTarget"));
                        }
                        else
                        {
                            playerVisible = false;
                            currentState = EnemyAiStates.tpPlayerSearch;
                            return;
                        }
                    }
                    else
                    {
                        playerVisible = false;
                        currentState = EnemyAiStates.tpPlayerSearch;
                        return;
                    }
                }
            }
            else if (modeManager.currentMode == gameModeManager.Mode.strategy)
            {
                currentEnemy++;
                currentState = EnemyAiStates.isoSearchMove;
                return;
            }
        }
        else if (modeManager.turn == 0 && currentEnemyRef.GetComponent<enemySoldierAI>().selected)
        {
            if (modeManager.currentMode == gameModeManager.Mode.thirdperson)
            {
                if (playerVisible)
                {
                    currentEnemyRef.transform.LookAt(new Vector3(target.transform.position.x, currentEnemyRef.transform.position.y, target.transform.position.z));
                    Vector3 raycastTarget = target.transform.Find("bodyTarget").transform.position;
                    Vector3 raycastDirection = raycastTarget - myEyes.transform.position;
                    raycastDirection.Normalize();
                    RaycastHit shootCheck;

                    if (Physics.Raycast(myEyes.transform.position, raycastDirection, out shootCheck, 100f))
                    {
                        if (shootCheck.transform.tag == "Player")
                        {
                            playerVisible = true;
                            currentEnemyRef.GetComponent<enemySoldierAI>().myGun.GetComponent<PlayerShoot>().shootForEnemy(shootCheck.transform.Find("bodyTarget"));
                        }
                        else
                        {
                            playerVisible = false;
                            currentState = EnemyAiStates.tpPlayerSearch;
                            return;
                        }
                    }
                    else
                    {
                        playerVisible = false;
                        currentState = EnemyAiStates.tpPlayerSearch;
                        return;
                    }
                }
            }
        }
    }

    public void RemoveFromList(GameObject objToRemove)
    {
        for (int i = enemyUnits.Count-1; i >= 0; i--)
        {
            if (enemyUnits[i] == objToRemove)
            {
                enemyUnits.RemoveAt(i);
                if (enemyUnits.Count == 0) { modeManager.GameWon(); }
                playerVisible = false;
                modeManager.ChangeMode(gameModeManager.Mode.transitionToStrategy);
                return;
            }
        }

        for (int i = playerUnits.Count - 1; i >= 0; i--)
        {
            if (playerUnits[i] == objToRemove)
            {
                playerUnits.RemoveAt(i);
                if (playerUnits.Count == 0) { modeManager.GameLost(); }
                playerVisible = false;
                modeManager.ChangeMode(gameModeManager.Mode.transitionToStrategy);
                return;
            }
        }
    }

    public void ResetMe()
    {
        currentEnemy = 0;
        currentState = EnemyAiStates.isoSearchMove;
        // reset values for me and children
    }
}
