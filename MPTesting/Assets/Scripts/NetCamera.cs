using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;

public class NetCamera : NetworkCameraBehavior
{
    protected override void NetworkStart(){
        base.NetworkStart();
        Camera cameraRef = GetComponent<Camera>();
        networkObject.UpdateInterval = 100;
        if(!networkObject.IsOwner)
            cameraRef.enabled = false;
    }
}
