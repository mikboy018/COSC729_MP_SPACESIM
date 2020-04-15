using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;

public class LaserControl : LaserBehavior
{
    public int damage = -25;
    public float speed = 500f;
    public GameObject player;

    void Start(){
        Destroy(gameObject, 25); // destroy after 15 seconds
    }

    private void FixedUpdate(){
/*         if(!networkObject.IsOwner){
            transform.position = networkObject.position;
            //transform.rotation = networkObject.rotation;
            return;
        } */

        // move laser beam forward - local
        transform.Translate(Vector3.forward*speed*Time.deltaTime);

        // server
        //networkObject.position = transform.position;
        //networkObject.rotation = transform.rotation;
    }

    private void OnCollisionEnter(Collision col) {
        //if(!col.gameObject.CompareTag("Player")){
        Destroy(gameObject);
        //}
    }
}
