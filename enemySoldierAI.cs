using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class enemySoldierAI : MonoBehaviour
{
    [Header("Tuneable Values")]
    public string unit_type;
    public float fullHealth = 100f;
    public float healthBarHeight = 1.96f;
    public float attackRange = 10f;
    public float moveRange = 20f;

    [Header("Health Variables")]
    public float currentHealth;
    public bool selected = false;
    public GameObject healthBarPrefab;
    private Image healthBarImg;
    public GameObject myHealthBar;

    [Header("Non-tuneable Values")]
    public NavMeshAgent myAgent;
    private EnemyControllerAI myControllerAI;
    private Animator myAnim;
    public GameObject myGun;

    // Start is called before the first frame update
    void Start()
    {
        myControllerAI = GameObject.FindWithTag("EnemyController").GetComponent<EnemyControllerAI>();
        myAnim = GetComponent<Animator>();
        myAgent = GetComponent<NavMeshAgent>();
        myHealthBar = Instantiate(healthBarPrefab, this.transform.position + new Vector3(0, healthBarHeight, 0), Quaternion.Euler(-45, 45, 0), this.transform);
        healthBarImg = myHealthBar.transform.Find("healthBar").Find("healthFilled").GetComponent<Image>();
        healthBarImg.fillAmount = 1.0f;
        currentHealth = fullHealth;
    }

    // Update is called once per frame
    void Update()
    {
        myAnim.SetFloat("speedPercent", myAgent.velocity.magnitude);
        healthBarImg.fillAmount = currentHealth / fullHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Mathf.Clamp(currentHealth, 0, fullHealth);
        if (currentHealth <= 0)
        {
            //kill this object
            myControllerAI.RemoveFromList(this.gameObject);
            this.gameObject.SetActive(false);
            Destroy(this.gameObject);
        }
    }
}
