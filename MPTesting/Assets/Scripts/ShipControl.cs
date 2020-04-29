using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine.UI;

public class ShipControl : ShipControlBehavior
{   
    public int accel = 0;
    public int accelRate = 1;
    public GameObject projectilePrefab;
    public Transform FiringPosition;

    private int fireDelay = 0;

    Vector3 fwd = new Vector3(0,0,0);

    // health
    public int health = 100;

    public string team = "player";

    public Text healthStatus;
    public Text friendlyStatus;
    public Text enemyStatus;
    public GameObject gameLogic;

    int delayHudUpdate = 0;

    public AudioSource audioLaser;

    public AudioListener ears;

    public GameObject explosionEffect;
    public GameObject myCamera;

    public Text timer;
    private float startTime;

    void Start(){
        spawn();
        healthStatus.text = "Health: " + health.ToString();
        //https://answers.unity.com/questions/249678/how-to-halt-rotation-after-object-bounce-offs.html
        GetComponent<Rigidbody>().freezeRotation = true;
        gameLogic = GameObject.Find("GameLogic");
        ears = GetComponent<AudioListener>();
        myCamera.SetActive(false);
        if(networkObject.IsOwner){
            ears.enabled = true;
            myCamera.SetActive(true);
        }
        startTime = Time.time;
    }

    private void spawn(){
        gameObject.SetActive(false);
        GetComponentsInChildren<Rigidbody>()[0].velocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0,360),0));
        // set location to area in unit sphere
        transform.position = new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10,10)); // select a random point around 0,0,0 starting point
        gameObject.SetActive(true);
    }

    public override void FireLaser(RpcArgs args){   
        GameObject laser = Instantiate(projectilePrefab, FiringPosition.position, FiringPosition.rotation);
        laser.GetComponent<LaserControl>().player = gameObject;
        audioLaser.Play(0);
    }

    public override void SetHealth(RpcArgs args){
        int dmg = args.GetNext<int>();
        health += dmg;
        healthStatus.text = "Health: " + health.ToString();
        if(health <= 0){
            var explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosion, 6);
            // respawn
            spawn();
            health = 100;
            if(networkObject.IsOwner){
                healthStatus.text = "Health: " + health.ToString();
            }
        }
    }

    private void FixedUpdate(){
        // other clients
        if(!networkObject.IsOwner){
            transform.position = networkObject.shipPos;
            transform.rotation = networkObject.shipRot;
            return;
        }

        if(gameLogic.GetComponent<GameLogic>().winCondition){
            enemyStatus.text = "Congratulations! You have won!";
        } else if(gameLogic.GetComponent<GameLogic>().lossCondition){
            friendlyStatus.text = "You Lost! All allied ships and turrets have been eliminated!";
            enemyStatus.text = ""; // losing all friendlies still results in loss
        } else {
            delayHudUpdate++;
            if(delayHudUpdate > 180){
                delayHudUpdate = 0;
                friendlyStatus.text = "Friendly Turrets: " + gameLogic.GetComponent<GameLogic>().friendlyTurretCount.ToString() +
                                      " // Friendly Ships: " + gameLogic.GetComponent<GameLogic>().friendlyCount.ToString();
                enemyStatus.text = "Enemy Turrets: " + gameLogic.GetComponent<GameLogic>().enemyTurretCount.ToString() +
                                      " // Enemy Ships: " + gameLogic.GetComponent<GameLogic>().enemyCount.ToString();
            }
        }
    

        // move ship - local Key Z Accelerates, Key C Deccelerates, w/s/a/d controls 
        if(Input.GetKey(KeyCode.Z)){
            accel += accelRate;
            if(accel > 100){
                accel = 100;
            }
        }
        if(Input.GetKey(KeyCode.C)){
            accel -= accelRate;
            if(accel < 0){
                accel = 0;
            }
        }

        transform.Translate(Vector3.forward*accel*Time.deltaTime);
        transform.Rotate(Vector3.up, Time.deltaTime*90.0f*Input.GetAxis("Horizontal"));
        transform.Rotate(Vector3.right, Time.deltaTime*90.0f*Input.GetAxis("Vertical"));

        // space bar fires
        if(Input.GetKey(KeyCode.Space) && fireDelay == 0){
           networkObject.SendRpc(RPC_FIRE_LASER, Receivers.AllBuffered);
           fireDelay = 15;
        }

        if(fireDelay != 0){
            fireDelay--;
        }

        // server
        networkObject.shipPos = transform.position;
        networkObject.shipRot = transform.rotation;

    }

    void Update(){
        if(!gameLogic.GetComponent<GameLogic>().winCondition || gameLogic.GetComponent<GameLogic>().lossCondition){
            float delta = Time.time - startTime;
            string min = ((int)delta / 60).ToString();
            string sec = (delta % 60).ToString("f2");
            if((delta%60) < 10)
                timer.text = min + ":0" + sec;
            else
                timer.text = min + ":" + sec;
        }
    }

    private void OnCollisionEnter(Collision col) {
        if(networkObject.IsServer){
            if(col.gameObject.CompareTag("Laser")){
                networkObject.SendRpc(RPC_SET_HEALTH, Receivers.AllBuffered, col.gameObject.GetComponent<LaserControl>().damage);
            } 

            if(col.gameObject.CompareTag("Planet")){
                networkObject.SendRpc(RPC_SET_HEALTH, Receivers.AllBuffered, -1000);
            }  
        }
    }
 
}
