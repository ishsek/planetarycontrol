using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class moveUnit : NetworkBehaviour
{
    [Header("Network Variables")]
    [SyncVar]
    public Vector3 navMeshMoveTarget;
    [SyncVar]
    public GameObject myPlayer;

    private gameModeManager modeManager;
    private CharacterController myCharacterController;

    [Header("Regular Variables")]
    public GameObject myGun;
    //public float maxMoveDistance = 10f;
    public float attackDistance = 10f;
    public bool attackedThisTurn;
    [SerializeField] private float pathDistance;

    private Animator myAnim;
    private PlayerController myContrl;
    [SerializeField] private EnemyControllerAI enemiesController;

    public GameObject myPointer;
    private GameObject pointer;
    public Material defaultPointerMat;
    public Material enemyPointerMat;
    public Material grayedPointerMat;
    private LineRenderer myLineR;
    public Material lineActive;
    public Material lineDeactive;
    private drawNavLine myNavLine;

    public GameObject camParent;
    public Camera cam;
    public bool selected = false;
    public bool mouseHovering = false;
    public Outline myOutline;
    public float outlineWidth = 0f;
    public Color outlineHoverColor = Color.white;
    public Color outlineSelectedColor = Color.blue;

    [SerializeField] Transform _destination;

    [SerializeField] NavMeshAgent _navMeshAgent;

    public override void OnStartAuthority()
    {
        InitializeMe();
    }

    // Not currently used
    private void SetDestination()
    {
        if(_destination != null)
        {
            Vector3 targetVector = _destination.transform.position;
            _navMeshAgent.SetDestination(targetVector);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_navMeshAgent == null || camParent == null) InitializeMe();
        if (cam == null)
        {
            cam = camParent.GetComponentInChildren<Camera>();
        }

        if (!hasAuthority && SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2"))
        {
            if (modeManager.currentMode == gameModeManager.Mode.strategy && navMeshMoveTarget != Vector3.zero)
            {
                if (Vector3.Distance(transform.position, navMeshMoveTarget) > 0.1)
                {
                    _navMeshAgent.SetDestination(navMeshMoveTarget);
                    _navMeshAgent.isStopped = false;
                }
            }
            if (Vector3.Distance(transform.position, navMeshMoveTarget) < 0.1)
            {
                _navMeshAgent.isStopped = true;
                navMeshMoveTarget = Vector3.zero;
                //    _navMeshAgent.ResetPath();
            }
        }

        if (hasAuthority)
        {
            //{
            // Set camera after it's been spawned
            if (camParent == null) return;
            if (cam == null)
            {
                cam = camParent.GetComponentInChildren<Camera>();
            }

            if (pointer == null)
            {
                pointer = myPointer.transform.Find("pointer").gameObject;
            }

            // If its my turn, do strategy stuff
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2"))
            {
                if (transform.parent.GetComponent<playerNetworkObjectScript>().playerName == gameModeManager.Player.player1 && modeManager.turn != 0) return;
                if (transform.parent.GetComponent<playerNetworkObjectScript>().playerName == gameModeManager.Player.player2 && modeManager.turn != 1) return;
            }
            else if (modeManager.turn != 0)
            {
                return;
            }

            //if (modeManager.turn == 0)
            if (true)
            {
                if (modeManager.currentMode == gameModeManager.Mode.thirdperson && selected)
                {
                    if (!myContrl.jumping) UpdateNavMeshDesync();
                }

                // Cast rays from mouse to see if player clicked on me
                if (modeManager.currentMode == gameModeManager.Mode.strategy && _navMeshAgent != null)
                {
                    // If not already selected, check to see if I was clicked on and make me selected
                    if (!selected)
                    {
                        if (_navMeshAgent.hasPath) _navMeshAgent.ResetPath();

                        Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hitCheck;


                        if (Physics.Raycast(camRay, out hitCheck))
                        {
                            if (hitCheck.transform.gameObject == this.gameObject)
                            {

                                //Debug.Log("Hovering on player select");

                                //myOutline.OutlineMode = Outline.Mode.OutlineAll;
                                //myOutline.OutlineColor = Color.white;
                                if (myOutline.OutlineWidth == 0f) myOutline.OutlineWidth = outlineWidth;
                                if (Input.GetMouseButtonDown(0))
                                {
                                    //Debug.Log(this.gameObject.name + ": selected = true");
                                    selected = true;
                                    _navMeshAgent.isStopped = true;
                                    cam.GetComponent<isometricCamera>().selectedPlayer = this.gameObject;
                                    cam.GetComponent<ThirdPersonCamera>().selectedPlayer = this.gameObject;
                                    return;
                                }
                            }
                            else
                            {
                                if (myOutline.OutlineWidth > 0f)
                                {
                                    myOutline.OutlineWidth = 0f;
                                }
                            }
                        }
                    }

                    // If I am selected, then check to see if I click on a location to move me
                    else if (selected && _navMeshAgent.isStopped)
                    {
                        if (myOutline.OutlineWidth == 0f) myOutline.OutlineWidth = outlineWidth;
                        if (myOutline.OutlineColor != outlineSelectedColor) myOutline.OutlineColor = outlineSelectedColor;

                        preDrawPath();

                        if (Input.GetMouseButtonDown(0))
                        {
                            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                            RaycastHit hit;

                            if (Physics.Raycast(ray, out hit))
                            {
                                if (hit.transform.tag == "Enemy")
                                {
                                    if (Vector3.Distance(this.transform.position, hit.transform.position) < attackDistance && !attackedThisTurn)
                                    {
                                        //Debug.Log("Fight engaged!");
                                        modeManager.fightDuration = modeManager.defaultFightDuration + myContrl.currentActionPoints;
                                        myContrl.previewActionPoints = 0f;
                                        myContrl.currentActionPoints = 0f;
                                        modeManager.currentTime = modeManager.fightDuration;
                                        myOutline.OutlineWidth = 0f;
                                        attackedThisTurn = true;
                                        enemiesController.currentEnemyRef = hit.transform.gameObject;
                                        enemiesController.currentState = EnemyControllerAI.EnemyAiStates.tpPlayerSearch;
                                        enemiesController.target = this.gameObject;
                                        modeManager.ChangeMode(gameModeManager.Mode.transitionToThirdPerson);
                                        return;
                                    }
                                }
                                else if (hit.transform.tag == "EnemyBase")
                                {
                                    if (Vector3.Distance(this.transform.position, hit.transform.position) < attackDistance && !attackedThisTurn)
                                    {
                                        //Debug.Log("Fight engaged!");
                                        myContrl.previewActionPoints = 0f;
                                        myContrl.currentActionPoints = 0f;
                                        myOutline.OutlineWidth = 0f;
                                        attackedThisTurn = true;
                                        hit.transform.GetComponent<BaseLogicScript>().TakeDamage(myGun.GetComponent<PlayerShoot>().gunDamage);
                                        return;
                                    }
                                }
                                else if (hit.transform.tag == "Player")
                                {
                                    // Remove this if networking fails
                                    if (hit.transform.parent != this.transform.parent && SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2"))
                                    {
                                        if (Vector3.Distance(this.transform.position, hit.transform.position) < attackDistance && !attackedThisTurn)
                                        {
                                            //Debug.Log("Fight engaged!");
                                            modeManager.fightDuration = modeManager.defaultFightDuration; //+ myContrl.currentActionPoints;
                                            myContrl.previewActionPoints = 0f;
                                            myContrl.currentActionPoints = 0f;
                                            modeManager.currentTime = modeManager.fightDuration;
                                            myOutline.OutlineWidth = 0f;
                                            attackedThisTurn = true;
                                            //enemiesController.currentEnemyRef = hit.transform.gameObject;
                                            //enemiesController.currentState = EnemyControllerAI.EnemyAiStates.tpPlayerSearch;
                                            //enemiesController.target = this.gameObject;
                                            //modeManager.RequestAuthority(this.transform.parent.GetComponent<NetworkIdentity>());
                                            //transform.parent.GetComponent<playerNetworkObjectScript>().RequestAuthority();
                                            if (transform.parent.name == "player1") modeManager.CmdSetFightUnits(this.transform.name, hit.transform.name);
                                            else if (transform.parent.name == "player2") modeManager.CmdSetFightUnits(hit.transform.name, this.transform.name);
                                            modeManager.ChangeMode(gameModeManager.Mode.transitionToThirdPerson);
                                            return;
                                        }
                                    }
                                    else if (hit.transform.gameObject != this.gameObject)
                                    {
                                        //Debug.Log("unselect me");
                                        selected = false;
                                        myOutline.OutlineWidth = 0f;
                                        myOutline.OutlineColor = outlineHoverColor;
                                        return;
                                    }
                                }

                                NavMeshHit closestPoint;

                                if (NavMesh.SamplePosition(hit.point, out closestPoint, 1.0f, NavMesh.AllAreas))
                                {
                                    _navMeshAgent.SetDestination(closestPoint.position);
                                    if (myNavLine.pathLength < myContrl.currentActionPoints)
                                    {
                                        _navMeshAgent.isStopped = false;
                                        myContrl.currentActionPoints = myContrl.previewActionPoints;
                                        myPlayer.GetComponent<playerNetworkObjectScript>().CmdSetDestination(closestPoint.position, netId);
                                    }
                                }
                            }
                        }
                        else if (Input.GetKeyDown(KeyCode.C))
                        {
                            selected = false;
                            myOutline.OutlineWidth = 0f;
                            myOutline.OutlineColor = outlineHoverColor;
                            myPointer.transform.position = new Vector3(0, -20, 0);
                            myContrl.previewActionPoints = myContrl.currentActionPoints;
                        }
                    }
                    else if (selected && !_navMeshAgent.isStopped)
                    {
                        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
                        {
                            if (!_navMeshAgent.hasPath || _navMeshAgent.velocity.sqrMagnitude == 0f)
                            {
                                _navMeshAgent.isStopped = true;
                                //Debug.Log(this.gameObject.name + ": navmeshstopped, unselect me");
                                selected = false;
                                myOutline.OutlineColor = outlineHoverColor;
                                myOutline.OutlineWidth = 0f;
                            }
                        }
                    }
                    float animationSpeedPercent = Mathf.Clamp(Mathf.Abs(_navMeshAgent.velocity.magnitude) / myContrl.runSpeed, 0f, 1f);
                    myAnim.SetFloat("speedPercent", animationSpeedPercent, myContrl.speedSmoothTime, Time.deltaTime);
                }
            }
            //else if (modeManager.turn == 1)
            //{
            //    if (modeManager.currentMode == gameModeManager.Mode.strategy)
            //    {
            //        float animationSpeedPercent = Mathf.Clamp(Mathf.Abs(_navMeshAgent.velocity.magnitude) / myContrl.runSpeed, 0f, 1f);
            //        myAnim.SetFloat("speedPercent", animationSpeedPercent, myContrl.speedSmoothTime, Time.deltaTime);
            //    }
            //}
        }
        // If the player doesn't have local authority then update its movement if it has a navigation target
        else
        {
            if (modeManager.currentMode == gameModeManager.Mode.strategy && navMeshMoveTarget != Vector3.zero)
            {
                if (_navMeshAgent.isStopped && Vector3.Distance(transform.position, navMeshMoveTarget) > 0.1)
                {
                    _navMeshAgent.SetDestination(navMeshMoveTarget);
                    _navMeshAgent.isStopped = false;
                }
            }
            if (Vector3.Distance(transform.position, navMeshMoveTarget) < 0.1)
            {
                _navMeshAgent.isStopped = true;
                navMeshMoveTarget = Vector3.zero;
            //    _navMeshAgent.ResetPath();
            }
            
        }

        // Animate the player regardless
        //if (modeManager.currentMode == gameModeManager.Mode.strategy)
        //{
        //    float animationSpeedPercent = Mathf.Clamp(Mathf.Abs(_navMeshAgent.velocity.magnitude) / myContrl.runSpeed, 0f, 1f);
        //    myAnim.SetFloat("speedPercent", animationSpeedPercent, myContrl.speedSmoothTime, Time.deltaTime);
        //}
    }

    //[Command]
    //public void CmdSetDestination(Vector3 position)
    //{
    //    navMeshMoveTarget = position;
    //    Debug.Log(gameObject.name + ": Calling COMMAND to set destination to " + navMeshMoveTarget);
    //}

    private Vector3 prevMousPos;
    private Vector3 currentMousPos;
    public float minMouseChangeDist = 5f;
    Vector3 lastViablePath;

    // This is just to draw the path with the pointer and line but does not move the unit
    private void preDrawPath()
    {
        // Starting mouse posiiton
        if (currentMousPos == null)
        {
            currentMousPos = Input.mousePosition;
            prevMousPos = currentMousPos;
        }
        // If mouse position has changed
        else if (Input.mousePosition != prevMousPos)
        {
            currentMousPos = Input.mousePosition;
            
            // Only update the path if the mouse position has changed a significant amount to limit processing of paths
            if ((Math.Abs(currentMousPos.x - prevMousPos.x) > minMouseChangeDist) || (Math.Abs(currentMousPos.y - prevMousPos.y) > minMouseChangeDist))
            {
                prevMousPos = currentMousPos;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Use raycast to determine new path target
                if (Physics.Raycast(ray, out hit))
                {

                    NavMeshHit closestPoint;

                    // If the raycast hit an enemy, show that to the user visually
                    if (hit.transform.tag == "Enemy")
                    {
                        if (Vector3.Distance(this.transform.position, hit.transform.position) < attackDistance && !attackedThisTurn)
                        {
                            pointer.GetComponent<Renderer>().material = enemyPointerMat;
                            myPointer.transform.position = hit.transform.position;
                            myLineR.material = lineActive;
                            myContrl.previewActionPoints = 0f;
                        } else
                        {
                            pointer.GetComponent<Renderer>().material = grayedPointerMat;
                            myPointer.transform.position = hit.transform.position;
                            myLineR.material = lineDeactive;
                        }
                    }
                    else if (hit.transform.tag == "Player")
                    {
                        if (hit.transform.parent != this.transform.parent && SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2"))
                        {
                            if (Vector3.Distance(this.transform.position, hit.transform.position) < attackDistance && !attackedThisTurn)
                            {
                                pointer.GetComponent<Renderer>().material = enemyPointerMat;
                                myPointer.transform.position = hit.transform.position;
                                myLineR.material = lineActive;
                                myContrl.previewActionPoints = 0f;
                            }
                            else
                            {
                                pointer.GetComponent<Renderer>().material = grayedPointerMat;
                                myPointer.transform.position = hit.transform.position;
                                myLineR.material = lineDeactive;
                            }
                        }
                    }
                    else if (hit.transform.tag == "EnemyBase")
                    {
                        //Debug.Log("basehit");
                        if (Vector3.Distance(this.transform.position, hit.transform.position) < attackDistance && !attackedThisTurn)
                        {
                            //Debug.Log("changepointercolorssssss");
                            pointer.GetComponent<Renderer>().material = enemyPointerMat;
                            myLineR.material = lineActive;
                            myContrl.previewActionPoints = 0f;
                        }
                        else
                        {
                            pointer.GetComponent<Renderer>().material = grayedPointerMat;
                            myPointer.transform.position = hit.transform.position;
                            myLineR.material = lineDeactive;
                        }
                    }
                    // Else if the raycast hit a point near the navmesh, calculate the path to that nearest point
                    else if (NavMesh.SamplePosition(hit.point, out closestPoint, 1.0f, NavMesh.AllAreas))
                    {
                        //Debug.Log("navmeshhit");
                        _navMeshAgent.SetDestination(closestPoint.position);
                        // If the path length is not within my move distance, visually show that
                        if (myNavLine.pathLength > myContrl.currentActionPoints)
                        {
                            pointer.GetComponent<Renderer>().material = grayedPointerMat;
                            myLineR.material = lineDeactive;
                            myContrl.previewActionPoints = 0;
                        }
                        // If the path length is within my move distance, visually show that
                        else
                        {
                            pointer.GetComponent<Renderer>().material = defaultPointerMat;
                            myLineR.material = lineActive;
                            myContrl.previewActionPoints = myContrl.currentActionPoints - myNavLine.pathLength;
                        }

                        myPointer.transform.position = closestPoint.position;

                        _navMeshAgent.isStopped = true;
                        pathDistance = myNavLine.pathLength;
                    }
                    // If the raycast didnt hit an enemy or a navmesh point, then show that
                    else
                    {
                        pointer.GetComponent<Renderer>().material = grayedPointerMat;
                    }
                }
            }
        }
    }

    void UpdateNavMeshDesync()
    {
        //_navMeshAgent.updatePosition = false;
        //_navMeshAgent.isStopped = true;
        //_navMeshAgent.ResetPath();
        NavMeshHit testpoint;
        NavMesh.SamplePosition(this.transform.position, out testpoint, 5f, NavMesh.AllAreas);

        //_navMeshAgent.SetDestination(testpoint.position);
        //_navMeshAgent.isStopped = false;
        _navMeshAgent.Warp(testpoint.position);
    }


    // For network code
    public void InitializeMe()
    {
        //if (hasAuthority)
        //{
        if (transform.parent != myPlayer.transform) { transform.SetParent(myPlayer.transform); return; }
        Debug.Log("I have authority over: " + gameObject.name + ", with NETID of: " + gameObject.GetComponent<NetworkIdentity>().netId);

        modeManager = GameObject.FindWithTag("GameManager").GetComponent<gameModeManager>();
        camParent = GameObject.Find("CameraLoc");
        myPointer = GameObject.Find("pivotPointParent");
        myCharacterController = GetComponent<CharacterController>();

        //enemiesController = GameObject.FindWithTag("EnemyController").GetComponent<EnemyControllerAI>();

        myLineR = GetComponent<LineRenderer>();
        _navMeshAgent = this.GetComponent<NavMeshAgent>();
        myAnim = GetComponent<Animator>();
        myContrl = GetComponent<PlayerController>();
        myNavLine = GetComponent<drawNavLine>();

        if (_navMeshAgent == null)
        {
            Debug.LogError("The nav mesh agent component is not attached to " + gameObject.name);
        }
        else
        {
            _navMeshAgent.SetDestination(this.transform.position);
            _navMeshAgent.isStopped = true;
        }

        myOutline = gameObject.AddComponent<Outline>();
        myOutline.OutlineMode = Outline.Mode.OutlineAll;
        myOutline.OutlineColor = Color.white;
        myOutline.OutlineWidth = 0f;
        //}
    }
}
