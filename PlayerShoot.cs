using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PlayerShoot : MonoBehaviour {
    private gameModeManager modeManager;

    public GameObject playerRef;
    [SerializeField] Transform hand;
    private PlayerController myUnitsController;
    private PlayerControllerLocal myUnitsControllerLocal;

    [Header("Weapon Stats")]
    public int gunDamage = 1;
    public float fireRate = .25f;
    public float weaponRange = 50f;
    public float hitForce = 100f;
    public int shotgunPellets = 4;
    public int shotgunSpread = 5;

    public Transform gunEnd;
    public Camera tpCam;
    public GameObject projectile;
    public GameObject crosshair;

    private WaitForSeconds shotDuration = new WaitForSeconds(.07f);
    private AudioSource gunAudio;
    private LineRenderer laserLine;
    private float nextFire;

	// Use this for initialization
	void Start () {
        laserLine = GetComponent<LineRenderer>();
        gunAudio = GetComponent<AudioSource>();
        modeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>();
        if (playerRef.tag == "Player") myUnitsController = playerRef.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update ()
    {
        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Level2"))
        {
            myUnitsControllerLocal = playerRef.GetComponent<PlayerControllerLocal>();
        }
        //if (hasAuthority)
        //{

        if (crosshair == null) crosshair = GameObject.Find("Crosshairs");

        if (playerRef == null)
        {
            Debug.LogError(gameObject.name + ": PLAYER REF NULL");
        }

        if (tpCam == null) tpCam = Camera.main;

        if (modeManager == null) { modeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>(); return; }

        if (playerRef.tag == "Player")
        {
            if (modeManager.currentMode == gameModeManager.Mode.thirdperson)
            {
                if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2"))
                {
                    if (!playerRef.GetComponent<moveUnit>().selected) return;
                } else
                {
                    if (!playerRef.GetComponent<moveUnitLocal>().selected) return;
                }
                
                if (crosshair.activeSelf == false) crosshair.SetActive(true);
                if (Input.GetButton("Fire1") && Time.time > nextFire)
                {
                    Debug.Log(gameObject.name + ": SHOOT PLEASE");
                    nextFire = Time.time + fireRate;

                    StartCoroutine(ShotEffect());

                    Vector3 rayOrigin = tpCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
                    RaycastHit hit;

                    //laserLine.SetPosition(0, gunEnd.position);

                    if (Physics.Raycast(rayOrigin, tpCam.transform.forward, out hit, weaponRange))
                    {
                        //    laserLine.SetPosition(1, hit.point);
                        gunEnd.LookAt(hit.point);
                    }
                    else
                    {
                        //    laserLine.SetPosition(1, rayOrigin + (tpCam.transform.forward * weaponRange));
                        gunEnd.LookAt(rayOrigin + tpCam.transform.forward * weaponRange);
                    }

                    if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2"))
                    {
                        if (myUnitsController.unit_type == "soldier")
                        {
                            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2"))
                            {
                                Vector3 position = gunEnd.position + gunEnd.transform.forward * 1.5f;
                                Quaternion rotation = gunEnd.rotation;
                                playerRef.transform.parent.GetComponent<playerNetworkObjectScript>().ShootBullet(position, rotation, gunDamage);
                            }
                            else
                            {
                                GameObject tempProjectile = Instantiate(projectile, gunEnd.position + gunEnd.transform.forward * 2f, gunEnd.rotation);
                                tempProjectile.GetComponent<laserBulletScript>().SetDamage(gunDamage);
                            }
                        }
                        else if (myUnitsController.unit_type == "tank")
                        {
                            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2"))
                            {
                                for (int i = 0; i < shotgunPellets; i++)
                                {
                                    GameObject emptyGO = new GameObject();

                                    Vector3 position = gunEnd.position + gunEnd.transform.forward * 1.5f;
                                    Transform rotation = emptyGO.transform;
                                    rotation.position = gunEnd.position;
                                    rotation.rotation = gunEnd.rotation;
                                    rotation.Rotate(new Vector3(Random.Range(-shotgunSpread, shotgunSpread), Random.Range(-shotgunSpread, shotgunSpread), Random.Range(-shotgunSpread, shotgunSpread)), Space.Self);
                                    playerRef.transform.parent.GetComponent<playerNetworkObjectScript>().ShootBullet(position, rotation.rotation, gunDamage);
                                    Destroy(emptyGO);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < shotgunPellets; i++)
                                {
                                    GameObject tempProjectile = Instantiate(projectile, gunEnd.position + gunEnd.transform.forward * 2f, gunEnd.rotation);
                                    tempProjectile.GetComponent<laserBulletScript>().SetDamage(gunDamage);
                                    tempProjectile.transform.Rotate(new Vector3(Random.Range(-shotgunSpread, shotgunSpread), Random.Range(-shotgunSpread, shotgunSpread), Random.Range(-shotgunSpread, shotgunSpread)), Space.Self);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (myUnitsControllerLocal.unit_type == "soldier")
                        {
                            GameObject tempProjectile = Instantiate(projectile, gunEnd.position + gunEnd.transform.forward * 2f, gunEnd.rotation);
                            tempProjectile.GetComponent<laserBulletScript>().SetDamage(gunDamage);
                            
                        }
                        else if (myUnitsControllerLocal.unit_type == "tank")
                        {
                            for (int i = 0; i < shotgunPellets; i++)
                            {
                                GameObject tempProjectile = Instantiate(projectile, gunEnd.position + gunEnd.transform.forward * 2f, gunEnd.rotation);
                                tempProjectile.GetComponent<laserBulletScript>().SetDamage(gunDamage);
                                tempProjectile.transform.Rotate(new Vector3(Random.Range(-shotgunSpread, shotgunSpread), Random.Range(-shotgunSpread, shotgunSpread), Random.Range(-shotgunSpread, shotgunSpread)), Space.Self);
                            }
                            
                        }
                    }
                }
            }
        } 
        else if (playerRef.tag == "Enemy")
        {
            // Enemy weapon behaviour
            
        }
        //}
	}

    public void shootForEnemy(Transform targetLoc)
    {
        if (Time.time > nextFire)
        {
            StartCoroutine(ShotEffect());
            gunEnd.LookAt(targetLoc);
            if (playerRef.GetComponent<enemySoldierAI>().unit_type == "soldier")
            {
                GameObject tempProjectile = Instantiate(projectile, gunEnd.position + gunEnd.transform.forward * 2f, gunEnd.rotation);
                tempProjectile.GetComponent<laserBulletScript>().SetDamage(gunDamage);
            }
            else if (playerRef.GetComponent<enemySoldierAI>().unit_type == "tank")
            {
                for (int i = 0; i < shotgunPellets; i++)
                {
                    GameObject tempProjectile = Instantiate(projectile, gunEnd.position + gunEnd.transform.forward * 2f, gunEnd.rotation);
                    tempProjectile.GetComponent<laserBulletScript>().SetDamage(gunDamage);
                    tempProjectile.transform.Rotate(new Vector3(Random.Range(-shotgunSpread, shotgunSpread), Random.Range(-shotgunSpread, shotgunSpread), Random.Range(-shotgunSpread, shotgunSpread)), Space.Self);
                }
            }
            nextFire = Time.time + fireRate;
        }
    }

    private IEnumerator ShotEffect()
    {
        gunAudio.Play();

        //laserLine.enabled = true;
        yield return shotDuration;
        //laserLine.enabled = false;

    }
}
