using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PlayerController : NetworkBehaviour {
    private gameModeManager modeManager;
    private EnemyControllerAI myControllerAI;

    private moveUnit myMoveUnit;

    [Header("Attributes upgrade")]
    public string unit_type;
    Dictionary<string,float> attributes;
    public bool if_attribute_update = false;

    [Header("Third Person Movement Variables")]
    public bool grounded = false;
    public float walkSpeed = 2;
    public float runSpeed = 6;
    public float gravity = -12;
    public float jumpHeight = 1;

    public bool jumping = false;

    [Header("Boostable Variables")]
    public float boostScale = 1.2f;
    public List<boostableStats> boostedStats;
    public enum boostableStats { boostedWalkSpeed, boostedRunSpeed, boostedJumpHeight, boostedActionPoints, boostedhealth, boostedDamage }
    public float turnSmoothTime = 0.2f;
    float turnSmoothVelocity;

    public float speedSmoothTime = 0.1f;

	float speedSmoothVelocity;
    float currentSpeed;
    float velocityY;
    public Vector3 velocity;

    Animator animator;
    public Transform cameraThirdPerson;
    CharacterController controller;
    
    //health bar and action bar
    [Header("UI Variables")]
    public float healthBarHeight = 2.93f;
    public float actionBarHeight = 1.96f;

    public float attackPower;
    public float armor;
    public float jump_iso;
    public float speed_iso;
    public float vision;
    public float fullHealth;
    
    [SyncVar]
    public float currentHealth;
    public float fullActionPoints = 20;
    public float previewActionPoints;
	public float currentActionPoints;
	public GameObject healthBarPrefab;
	public GameObject actionPointsBarPrefab;
    private Image healthBarImg;
    private Image ActionBarImg;
    public GameObject myHealthBar;
    public GameObject myActionPointsBar;

    // Use this for initialization
    void Start () {

        modeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>();
        //myControllerAI = GameObject.FindWithTag("EnemyController").GetComponent<EnemyControllerAI>();
        animator = GetComponent<Animator>();

        controller = GetComponent<CharacterController>();
        velocity = Vector3.zero;
        myMoveUnit = GetComponent<moveUnit>();
        
        //load attributes for each of unit
        attributes = initStatus();
        
        //intialize and fill health bar
        //ToDo:Here should find a current game object[this]
        Transform character = transform.gameObject.transform;
		myHealthBar = Instantiate(healthBarPrefab, this.transform.position + new Vector3(0, healthBarHeight, 0), Quaternion.Euler(-45, 45, 0), character);
		myActionPointsBar = Instantiate(actionPointsBarPrefab, this.transform.position + new Vector3(0, actionBarHeight, 0), Quaternion.Euler(-45, 45, 0), character);
		currentHealth = fullHealth;
        currentActionPoints = fullActionPoints;
        previewActionPoints = fullActionPoints;

        healthBarImg = myHealthBar.transform.Find("healthBar").Find("healthFilled").GetComponent<Image>();
        healthBarImg.fillAmount = 1.0f;
        ActionBarImg = myActionPointsBar.transform.Find("ActionPoints").Find("ActionFilled").GetComponent<Image>();
        ActionBarImg.fillAmount = 1.0f;
    }
	
	// Update is called once per frame
	void Update () {
        //if (hasAuthority)
        //{

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2") && !hasAuthority)
        {
            healthBarImg.fillAmount = currentHealth / fullHealth;
            return;
        }

        

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2") && modeManager.currentMode == gameModeManager.Mode.thirdperson && myMoveUnit.selected)
        {
            bool enable = true;
            CmdSetNetTransform(enable);
        } else
        {
            bool enable = false;
            CmdSetNetTransform(enable);
        }

        if (cameraThirdPerson == null)
        {
            cameraThirdPerson = Camera.main.transform;
            return;
        }

        //testing
        if (myHealthBar != null && myActionPointsBar != null)
        {
            //currentHealth -= Time.deltaTime;
            healthBarImg.fillAmount = currentHealth / fullHealth;
            if (previewActionPoints != currentActionPoints)
                ActionBarImg.fillAmount = previewActionPoints / fullActionPoints;
            else
                ActionBarImg.fillAmount = currentActionPoints / fullActionPoints;
            //myHealthBar.transform.LookAt(cameraThirdPerson);
        }

        if (modeManager.currentMode != gameModeManager.Mode.thirdperson)
        {
            ResetTriggers();
            animator.SetTrigger("landed");
        }

        

        if (modeManager.currentMode == gameModeManager.Mode.thirdperson && myMoveUnit.selected)
        {
            
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector2 inputDir = input.normalized;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }

            //if (inputDir != Vector2.zero)
            //{
            float targetRotation = Mathf.Atan2(0, 1) * Mathf.Rad2Deg + cameraThirdPerson.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
            //}

            bool running = Input.GetKey(KeyCode.LeftShift);
            float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

            velocityY += Time.deltaTime * gravity;
            velocity = Vector3.zero;
            if (inputDir.x > 0)
            {
                velocity = transform.right * currentSpeed + Vector3.up * velocityY;
            }
            else if (inputDir.x < 0)
            {
                velocity = -1 * transform.right * currentSpeed + Vector3.up * velocityY;
            }
            if (inputDir.y > 0)
            {
                if (velocity == Vector3.zero)
                {
                    velocity = transform.forward * currentSpeed + Vector3.up * velocityY;
                }
                else
                {
                    velocity /= 2;
                    velocity += transform.forward * currentSpeed / 1.2f;
                }
            }
            else if (inputDir.y < 0)
            {
                if (velocity == Vector3.zero)
                {
                    velocity = -1 * transform.forward * currentSpeed + Vector3.up * velocityY;
                }
                else
                {
                    velocity /= 2;
                    velocity += -1 * transform.forward * currentSpeed / 1.2f;
                }
            }
            if (inputDir.x == 0 && inputDir.y == 0)
            {
                velocity = transform.forward * currentSpeed + Vector3.up * velocityY;
            }
            //Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;

            controller.Move(velocity * Time.deltaTime);

            currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

            grounded = controller.isGrounded;

            if (jumping)
            {
                if (controller.isGrounded)
                {
                    jumping = false;
                    ResetTriggers();
                    animator.SetTrigger("landed");
                    Debug.Log(this.gameObject.name + "setting anim to grounded");
                }
            }

            if (controller.isGrounded)
            {
                velocityY = 0;
                float animationSpeedPercent = ((running) ? currentSpeed / runSpeed : currentSpeed / walkSpeed * .5f);
                animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
                if (inputDir.y > 0) { ResetTriggers(); animator.SetTrigger("landed"); }
                else if (inputDir.y < 0) { ResetTriggers(); animator.SetTrigger("Backward"); }
                else if (inputDir.x > 0) { ResetTriggers(); animator.SetTrigger("StrafeRight"); }
                else if (inputDir.x < 0) { ResetTriggers(); animator.SetTrigger("StrafeLeft"); }
                else if (velocity.magnitude < 0.01) { ResetTriggers(); animator.SetTrigger("landed"); }
            }
        }
        //}
    }

    void Jump()
    {
        if (controller.isGrounded)
        {
            jumping = true;
            Debug.Log(this.gameObject.name + "setting anim to jump");
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight);
            velocityY = jumpVelocity;
            ResetTriggers();
            animator.SetTrigger("jumpTrigger");
        }
    }

    private void ResetTriggers()
    {
        animator.ResetTrigger("jumpTrigger");
        animator.ResetTrigger("landed");
        animator.ResetTrigger("StrafeLeft");
        animator.ResetTrigger("StrafeRight");
        animator.ResetTrigger("Backward");
    }

    public void BoostStatsFixed()
    {
        //boostedWalkSpeed, boostedRunSpeed, boostedJumpHeight, boostedActionPoints, boostedhealth, boostedDamage;
        walkSpeed *= boostScale;
        boostedStats.Add(boostableStats.boostedWalkSpeed);
        runSpeed *= boostScale;
        boostedStats.Add(boostableStats.boostedRunSpeed);
        jumpHeight *= boostScale;
        boostedStats.Add(boostableStats.boostedJumpHeight);
        fullActionPoints *= boostScale;
        currentActionPoints *= boostScale;
        boostedStats.Add(boostableStats.boostedActionPoints);
        fullHealth *= boostScale;
        currentHealth *= boostScale;
        boostedStats.Add(boostableStats.boostedhealth);
        myMoveUnit.myGun.GetComponent<PlayerShoot>().gunDamage *= 2;
        boostedStats.Add(boostableStats.boostedDamage);
    }

    public void DebuffStats()
    {
        //boostedWalkSpeed, boostedRunSpeed, boostedJumpHeight, boostedActionPoints, boostedhealth, boostedDamage;
        walkSpeed /= boostScale;
        boostedStats.Remove(boostableStats.boostedWalkSpeed);
        runSpeed /= boostScale;
        boostedStats.Remove(boostableStats.boostedRunSpeed);
        jumpHeight /= boostScale;
        boostedStats.Remove(boostableStats.boostedJumpHeight);
        fullActionPoints /= boostScale;
        currentActionPoints /= boostScale;
        boostedStats.Remove(boostableStats.boostedActionPoints);
        fullHealth /= boostScale;
        currentHealth /= boostScale;
        boostedStats.Remove(boostableStats.boostedhealth);
        myMoveUnit.myGun.GetComponent<PlayerShoot>().gunDamage /= 2;
        boostedStats.Remove(boostableStats.boostedDamage);
    }

    public void TakeDamage(float damageAmount)
    {
        if (currentHealth > 0) { 
            currentHealth -= damageAmount;
            Mathf.Clamp(currentHealth, 0, fullHealth);
            CmdSetHealth(currentHealth);
            if (currentHealth <= 0)
            {
                CmdDestroyMe();
                myControllerAI.RemoveFromList(this.gameObject);
                Destroy(this.gameObject);
            }
        }
    }

    [Command]
    public void CmdDestroyMe()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    [Command]
    public void CmdSetHealth(float health)
    {
        Debug.Log("COMMAND setting health");
        currentHealth = health;
        RpcSetHealth(health);
    }

    [ClientRpc]
    public void RpcSetHealth(float health)
    {
        Debug.Log("RPC Setting health");
        currentHealth = health;
    }

    [Command]
    public void CmdSetNetTransform(bool status)
    {
        RpcSetNetTransform(status);
    }

    [ClientRpc]
    public void RpcSetNetTransform(bool status)
    {
        if (status == false)
        {
            //controller.SimpleMove(Vector3.zero);
        }
        GetComponent<NetworkTransform>().enabled = status;
    }
    
     public Dictionary<string,float> initStatus(){
        UnitAttributesList unit_attribute = new UnitAttributesList();
        Dictionary<string,float> attributes = unit_attribute.initialize(unit_type);
    
        attackPower = attributes["attackPower"];
        armor   = attributes["armor"];
        speed_iso = attributes["speed"];
        jump_iso = attributes["jumpHeight"];
        fullHealth = attributes["fullHealth"];
        vision = attributes["vision"];
        return attributes;
    }
    
    public void updateStatus(string attribute_type){
        if(attributes.ContainsKey(attribute_type)){
            if_attribute_update = true;
            
            switch(attribute_type)
            {
                case "attackPower":
                    attackPower+=1.5f;
                    attributes["attackPower"]+=1.5f;
                    break;
                case "armor":
                    armor+=1f;
                    attributes["armor"]+=1f;
                    break;
                case "speed":
                    speed_iso+=1f;
                    attributes["speed"]+=1f;
                    break;
                case "jumpHeight":
                    attributes["jumpHeight"]+=1f;
                    jump_iso+=1f;
                    break;
                case "fullHealth":
                    fullHealth+=5f;
                    attributes["fullHealth"]+=5f;
                    break;
                case "vision":
                    vision+=1f;
                    attributes["vision"]+=1f;
                    break;
            }
            
        }
        if_attribute_update = true;
    }
    
    public bool check_if_need_update(){
        return if_attribute_update;
    }
    
    public Dictionary<string,float> getAttributesDic(){
        return attributes;
    }
    
}
