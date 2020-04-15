using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class EnemyControl : EnemyControlBehavior
{
    public GameObject target;

    public Transform waypoint;
    public List<Transform> waypoints;
    public float rayDistance = 2f;
    public bool inPursuit;

    public GameObject projectilePrefab;
    public Transform firingPosition;
    public float interval = 3.5f;
    public bool canFire = true;

    public float transitSpeed = 75f;
    public float speed = 0;

    public int health = 100;

    public Material laserColor;

    public string team = "enemy";

    Vector3 dir;

    bool maneuvering = false;
    private RaycastHit hit;

    public GameObject gameLogic;

    public AudioSource audioLaser;
    public GameObject explosionEffect;

    void Start(){
        //nextFire = Time.time;
        speed = transitSpeed;
        networkObject.speed = speed;
        //selectWaypoint();
        waypoint = null;
        gameLogic = GameObject.Find("GameLogic");
    }

	void FixedUpdate () {
        if(networkObject.IsServer){
            
            if(!maneuvering){
                if(inPursuit)
                {
                    if(!target || Vector3.Distance(target.transform.position,transform.position) > 2000f){ // at 2000m you can escape the enemy ship
                        inPursuit = false;
                        speed = transitSpeed;
                        networkObject.speed = speed;
                        selectWaypoint();
                    } else {
                        dir = (target.transform.position - transform.position).normalized;
                    }
                    // enemy shooting
                    if(canFire){
                        canFire = false;
                        shoot();
                    }
                } else {
                    if(waypoint == null || Vector3.Distance(waypoint.position, transform.position) < 100f){ // picks a new waypoint to patrol to
                        selectWaypoint();
                    }

                }
            }

             Transform leftRay = transform;
             Transform rightRay = transform;
                // Adapted from: http://www.theappguruz.com/blog/unity-3d-enemy-obstacle-awarness-ai-code-sample
             if (Physics.Raycast(leftRay.position + (transform.right * 7), transform.forward, out hit, rayDistance) || Physics.Raycast(rightRay.position - (transform.right * 7), transform.forward, out hit, rayDistance)) {
              
               maneuvering = true;
               transform.Rotate(Vector3.up * Time.deltaTime * 5);
               StartCoroutine(maneuver());
              
             }

             
             if (Physics.Raycast(transform.position - (transform.forward * 4), transform.right, out hit, 10) ||
              Physics.Raycast(transform.position - (transform.forward * 4), -transform.right, out hit, 10)) {

               maneuvering = false;
              
             }
             // Use to debug the Physics.RayCast.
             Debug.DrawRay(transform.position + (transform.right * 7), transform.forward * 20, Color.red);
             Debug.DrawRay(transform.position - (transform.right * 7), transform.forward * 20, Color.red);
             Debug.DrawRay(transform.position - (transform.forward * 4), -transform.right * 20, Color.yellow);
             Debug.DrawRay(transform.position - (transform.forward * 4), transform.right * 20, Color.yellow);
 

            if(!inPursuit){
                transform.LookAt(dir);
            } else {
                var rotate = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotate, Time.fixedDeltaTime);
            }

            
            transform.position += transform.forward * networkObject.speed * Time.fixedDeltaTime;
            networkObject.position = transform.position;
            networkObject.rotation = transform.rotation;

        } else {
            transform.rotation = networkObject.rotation;
            transform.position = networkObject.position;
        }

    }


    /// <summary>
    /// OnCollisionEnter is called when this collider/rigidbody has begun
    /// touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    private void OnTriggerEnter(Collider other) // this needs to follow the shipcontrol on collision enter
    {   
        //Debug.Log("Trigger: " + other.gameObject.name);
        if (other.gameObject.CompareTag("Player") && !inPursuit && networkObject.IsServer)
        {
            //Debug.Log(other.transform.parent.gameObject.GetComponent<ShipControl>().team);

            if((other.transform.parent.gameObject.GetComponent<EnemyControl>() != null && other.transform.parent.gameObject.GetComponent<EnemyControl>().team != team) || 
                (other.transform.parent.gameObject.GetComponent<ShipControl>() != null && other.transform.parent.gameObject.GetComponent<ShipControl>().team != team) ||
                (other.transform.parent.gameObject.GetComponent<TurretControl>() != null && other.transform.parent.gameObject.GetComponent<TurretControl>().team != team)) {
                //Debug.Log("Target: " + other.gameObject.name + " assigned.");
                target = other.gameObject;
                inPursuit = true;
            
                speed = Random.Range(.75f*transitSpeed, 1.25f*transitSpeed);
                networkObject.speed = speed;
            }
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

            if(col.gameObject.CompareTag("Planet")){
                //Debug.Log("Collision with planet");
                networkObject.SendRpc(RPC_SET_HEALTH, Receivers.AllBuffered, -1000);
            }  
        }
    }

    private void shoot(){
        GameObject laser = Instantiate(projectilePrefab, firingPosition.position, firingPosition.rotation);
        MeshRenderer laserMesh = laser.GetComponent<MeshRenderer>();
        laserMesh.material = laserColor;
        Light ptLight = laser.transform.GetChild(0).gameObject.GetComponent<Light>();
        ptLight.color = Color.red;
        laser.GetComponent<LaserControl>().player = gameObject;
        StartCoroutine(cooldown());
        audioLaser.Play(0);
    }

    private IEnumerator cooldown(){
        yield return new WaitForSeconds(interval);
        canFire = true;
    }

    private IEnumerator maneuver(){
        // avoid collision
        yield return new WaitForSeconds(2);
        maneuvering = false;
        selectWaypoint();
    }

    public override void SetHealth(RpcArgs args){
        int damage = args.GetNext<int>();
        health += damage;
        if(health <= 0){
            var explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosion, 6);
            //Debug.Log("You beat the enemy");
            Destroy(gameObject);
        }
    }

    void selectWaypoint(){
        waypoint = waypoints[Random.Range(0, waypoints.Count)];
        dir = waypoint.position;
    }

/*     private void OnDestroy() {
        if(team == "enemy"){
            gameLogic.GetComponent<GameLogic>().decrementEnemy();
        }
        else{
            gameLogic.GetComponent<GameLogic>().decrementFriendly();
        }
    } */
}
