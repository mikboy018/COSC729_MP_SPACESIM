using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;

public class ParentPlanetControl : ParentPlanetBehavior
{
    float timer = 0;

    public float rotateSpeed = 0.05f;

    
    void FixedUpdate()
    {
        if(networkObject.IsServer){
            timer += Time.fixedDeltaTime;
            if(timer > 360)
                timer = 0;
                
            transform.rotation = Quaternion.AngleAxis(rotateSpeed*timer, Vector3.up);
        } else {
            transform.rotation = networkObject.rotation;
        }
    }

}
