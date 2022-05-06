using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class unitUIlogic : MonoBehaviour
{
    public GameObject myUnit;
    public PlayerManager playerManager;
    public moveUnitLocal myMoveUnit;
    public PlayerControllerLocal myPlayerController;

    public static int upgradeCost = 50;
    public static int upgradeCap = 6;

    public Text damageValue;
    public Button damageUpgradeButton;
    public static int damageUpgradePer = 4;
    public int damageUpgradesUsed;

    public Text firerateValue;
    public Button firerateUpgradeButton;
    public static float firerateUpgradePer = 0.05f;
    public int firerateUpgradesUsed;

    public Text movespeedValue;
    public Button movespeedUpgradeButton;
    public static float movespeedUpgradePer = 1f;
    public int movespeedUpgradesUsed;

    public Text jumpValue;
    public Button jumpUpgradeButton;
    public static float jumpUpgradePer = 0.5f;
    public int jumpUpgradesUsed;

    private void Start()
    {
        // Initialize fields
        myMoveUnit = myUnit.GetComponent<moveUnitLocal>();
        myPlayerController = myUnit.GetComponent<PlayerControllerLocal>();
        damageValue = transform.Find("AttributeList").Find("Damage").Find("AttributeValueText").GetComponent<Text>();
        damageUpgradeButton = transform.Find("AttributeList").Find("Damage").Find("UpgradeButton").GetComponent<Button>();
        firerateValue = transform.Find("AttributeList").Find("FireRate").Find("AttributeValueText").GetComponent<Text>();
        firerateUpgradeButton = transform.Find("AttributeList").Find("FireRate").Find("UpgradeButton").GetComponent<Button>();
        movespeedValue = transform.Find("AttributeList").Find("MoveSpeed").Find("AttributeValueText").GetComponent<Text>();
        movespeedUpgradeButton = transform.Find("AttributeList").Find("MoveSpeed").Find("UpgradeButton").GetComponent<Button>();
        jumpValue = transform.Find("AttributeList").Find("JumpHeight").Find("AttributeValueText").GetComponent<Text>();
        jumpUpgradeButton = transform.Find("AttributeList").Find("JumpHeight").Find("UpgradeButton").GetComponent<Button>();

        //Initialize Values
        transform.Find("UnitName").GetComponent<Text>().text = myPlayerController.unit_type;
        damageValue.text = myMoveUnit.myGun.GetComponent<PlayerShoot>().gunDamage.ToString();
        firerateValue.text = myMoveUnit.myGun.GetComponent<PlayerShoot>().fireRate.ToString();
        movespeedValue.text = myPlayerController.runSpeed.ToString();
        jumpValue.text = myPlayerController.jumpHeight.ToString();
    }

    private void Update()
    {
        if (playerManager.currency >= upgradeCost)
        {
            if (damageUpgradesUsed < upgradeCap)
                damageUpgradeButton.interactable = true;
            if (firerateUpgradesUsed < upgradeCap)
                firerateUpgradeButton.interactable = true;
            if (movespeedUpgradesUsed < upgradeCap)
                movespeedUpgradeButton.interactable = true;
            if (jumpUpgradesUsed < upgradeCap)
                jumpUpgradeButton.interactable = true;
        }
        else
        {
            damageUpgradeButton.interactable = false;
            firerateUpgradeButton.interactable = false;
            movespeedUpgradeButton.interactable = false;
            jumpUpgradeButton.interactable = false;
        }

        // Update Values
        damageValue.text = myMoveUnit.myGun.GetComponent<PlayerShoot>().gunDamage.ToString();
        firerateValue.text = myMoveUnit.myGun.GetComponent<PlayerShoot>().fireRate.ToString();
        movespeedValue.text = myPlayerController.runSpeed.ToString();
        jumpValue.text = myPlayerController.jumpHeight.ToString();
    }

    public void UpgradeDamage()
    {
        if (damageUpgradesUsed < upgradeCap)
        {
            myMoveUnit.myGun.GetComponent<PlayerShoot>().gunDamage += damageUpgradePer;
            damageUpgradesUsed++;
            playerManager.currency -= upgradeCost;
        }
    }

    public void UpgradeFirerate()
    {
        if (firerateUpgradesUsed < upgradeCap)
        {
            myMoveUnit.myGun.GetComponent<PlayerShoot>().fireRate -= firerateUpgradePer;
            firerateUpgradesUsed++;
            playerManager.currency -= upgradeCost;
        }
    }

    public void UpgradeMovespeed()
    {
        if (movespeedUpgradesUsed < upgradeCap)
        {
            myPlayerController.walkSpeed += movespeedUpgradePer;
            myPlayerController.runSpeed += movespeedUpgradePer;
            movespeedUpgradesUsed++;
            playerManager.currency -= upgradeCost;
        }
    }

    public void UpgradeJump()
    {
        if (jumpUpgradesUsed < upgradeCap)
        {
            myPlayerController.jumpHeight += jumpUpgradePer;
            jumpUpgradesUsed++;
            playerManager.currency -= upgradeCost;
        }
    }
}
