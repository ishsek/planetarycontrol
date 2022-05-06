using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseLogicScript : MonoBehaviour
{
    [Header("Tuneable Values")]
    public float fullHealth = 100f;
    public float healthBarHeight = 0f;

    [Header("Health Variables")]
    public float currentHealth;
    public GameObject healthBarPrefab;
    private Image healthBarImg;
    public GameObject myHealthBar;
    private GameObject healthbarLocation;

    // Start is called before the first frame update
    void Start()
    {
        healthbarLocation = transform.Find("healthBarLocation").gameObject;
        myHealthBar = Instantiate(healthBarPrefab, healthbarLocation.transform.position + new Vector3(0, healthBarHeight, 0), Quaternion.Euler(-45, 45, 0), this.transform);
        healthBarImg = myHealthBar.transform.Find("healthBar").Find("healthFilled").GetComponent<Image>();
        healthBarImg.fillAmount = 1.0f;
        currentHealth = fullHealth;
    }

    // Update is called once per frame
    void Update()
    {
        healthBarImg.fillAmount = currentHealth / fullHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Mathf.Clamp(currentHealth, 0, fullHealth);
        
    }
}
