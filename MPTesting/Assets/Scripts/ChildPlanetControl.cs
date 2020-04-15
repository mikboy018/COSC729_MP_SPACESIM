using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;

public class ChildPlanetControl : ChildPlanetBehavior
{
    public GameObject parentPlanet;

    void FixedUpdate()
    {
        if(networkObject.IsServer){

            transform.RotateAround(parentPlanet.transform.position, Vector3.up, 1.5f*Time.fixedDeltaTime);

            networkObject.rotation = transform.rotation;
            networkObject.position = transform.position;

        } else {
            transform.rotation = networkObject.rotation;
            transform.position = networkObject.position;
        }
    }
}
