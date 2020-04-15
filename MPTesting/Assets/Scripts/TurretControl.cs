using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class TurretControl : TurretControlBehavior
{
    public GameObject target; // just like enemy ship, this will hold the target gameObject

    public Transform firingPosition; // where projectile is instatiated
    public float interval = 3.5f; // rate of fire
    public float nextFire;
    public int health = 1000; // can be destroyed
    public GameObject projectilePrefab;
    public Material laserColor;
    
    Quaternion originalRotation;

    public bool defensesOnline;
    public bool canFire;

    public string team = "enemy";

    public GameObject turretStation; // onDestroy destroys this as well as spawns a particle blast
    public GameObject turretOperator; // onDestroy spawns this little guy who wanders around the NavMesh with his hands in the air in a panic

    private IEnumerator shootRoutine;

    public GameObject gameLogic;

    public AudioSource audioLaser;
    public GameObject explosionEffect;

    void Start()
    {
        nextFire = Time.time;
        originalRotation = transform.rotation;
        defensesOnline = false;
        canFire = true;
        gameLogic = GameObject.Find("GameLogic");
        audioLaser = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if(networkObject.IsServer){
            var rot = originalRotation;
            if(defensesOnline){
                if(!target || Vector3.Distance(firingPosition.position, target.transform.position)>2000f){
                    defensesOnline = false;
                }
                var dir = (target.transform.position - transform.position);
                rot = Quaternion.LookRotation(dir);
                var rotEuler = rot.eulerAngles;

                rotEuler = new Vector3(-rotEuler.x, rotEuler.y - 180, rotEuler.z);

                rot.eulerAngles = rotEuler;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rot,80*Time.fixedDeltaTime);
                
                
                if(canFire){
                    canFire = false;
                    shoot();
                }
            } else { 
                transform.rotation = originalRotation;
            }
            networkObject.rotation = transform.rotation;
        }
        else 
        {
            transform.rotation = networkObject.rotation;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {   
        if(networkObject.IsServer){
            //Debug.Log("Trigger: " + other.gameObject.name);
            if (!defensesOnline && other.gameObject.CompareTag("Player"))
            {
                // teams must be assigned to either enemycontrol or shipcontrol
                if( other.transform.parent.gameObject.GetComponent<EnemyControl>() != null && other.transform.parent.gameObject.GetComponent<EnemyControl>().team != team || 
                    other.transform.parent.gameObject.GetComponent<ShipControl>() != null && other.transform.parent.gameObject.GetComponent<ShipControl>().team != team){
                        Debug.Log("Target: " + other.gameObject.name + " assigned.");
                        target = other.gameObject;
                        defensesOnline = true;
                }
            }
        }
    }

    private void shoot(){ 
        audioLaser.Play(0);
        turretOperator.GetComponent<AlienController>().animator.SetTrigger("armAction");
        GameObject laser = Instantiate(projectilePrefab, firingPosition.position, firingPosition.rotation);
        MeshRenderer laserMesh = laser.GetComponent<MeshRenderer>();
        laserMesh.material = laserColor;
        Light ptLight = laser.transform.GetChild(0).gameObject.GetComponent<Light>();
        ptLight.color = Color.blue;
        laser.GetComponent<LaserControl>().speed += laser.GetComponent<LaserControl>().speed*2f;
        laser.GetComponent<LaserControl>().player = gameObject;
        StartCoroutine(cooldown());
    }

    private IEnumerator cooldown(){
        yield return new WaitForSeconds(interval);
        canFire = true;
    }

    public override void SetHealth(RpcArgs args){
        int damage = args.GetNext<int>();
        health += damage;
        
        if(health <= 0){
            Debug.Log("Turret Destroyed");
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision col){ 
        if(networkObject.IsServer){
            if(col.gameObject.CompareTag("Laser")){
                //Debug.Log("Laser hit enemy");
                Destroy(col.gameObject);
                //SetHealth(col.gameObject.GetComponent<LaserControl>().damage);
                networkObject.SendRpc(RPC_SET_HEALTH, Receivers.AllBuffered,col.gameObject.GetComponent<LaserControl>().damage);
            }
        }
    }

    void OnDestroy(){
        var explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        var explosion1 = Instantiate(explosionEffect, transform.position + new Vector3(0,1,0), Quaternion.identity);
        explosion1.GetComponentInChildren<ParticleSystem>().transform.localScale = new Vector3(25,25,25);
        Destroy(explosion, 6);
        Destroy(explosion1, 6);
        Destroy(turretStation);
        turretOperator.GetComponent<AlienController>().panic = true;

/*         if(team == "enemy"){
            gameLogic.GetComponent<GameLogic>().decrementEnemyTurret();
        }
        else{
            gameLogic.GetComponent<GameLogic>().decrementFriendlyTurret();
        } */
        
    }
}
