using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class laserBulletScript : NetworkBehaviour
{
    public float damage;
    public float aliveduration = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * 20 *Time.deltaTime);

        aliveduration += 1 * Time.deltaTime;
        if (aliveduration > 12)
        {
            NetworkServer.Destroy(gameObject);
            Destroy(gameObject);
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.tag == "Enemy")
    //    {
    //        Debug.Log(transform.name + ": ENEMY HIT!");
    //        // do some damage
    //        collision.gameObject.GetComponent<enemySoldierAI>().TakeDamage(damage);
    //        Destroy(this.gameObject);
    //    }
    //    else if (collision.gameObject.tag == "Player")
    //    {
    //        Debug.Log(transform.name + ": PLAYER HIT!");
    //        collision.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
    //        Destroy(this.gameObject);
    //    }
    //    else
    //    {
    //        Debug.Log(transform.name + ": WALL HIT");
    //        Destroy(this.gameObject);
    //    }
    //}

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            Debug.Log(transform.name + ": ENEMY HIT!");
            // do some damage
            other.GetComponent<enemySoldierAI>().TakeDamage(damage);
            Destroy(this.gameObject);
        }
        else if (other.tag == "Player")
        {
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2"))
            {
                Debug.Log("HIT A PLAYER");
                if (isServer)
                {
                    Debug.Log("HIT ON SERVER");
                    other.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
                    NetworkServer.Destroy(gameObject);
                }
            }
            else
            {
                Debug.Log(transform.name + ": PLAYER HIT!");
                other.GetComponent<PlayerControllerLocal>().TakeDamage(damage);
                Destroy(this.gameObject);
            }
        }
        else if (other.tag == "Projectile")
        {
            Debug.Log(transform.name + ": ignoring projectile");
        }
        else
        {
            Debug.Log(transform.name + ": WALL HIT");
            Destroy(this.gameObject);
        }
    }

    public void SetDamage(float amount)
    {
        damage = amount;
    }
}
