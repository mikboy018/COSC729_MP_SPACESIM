using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated{
    public partial class ShipControlNetworkObject : NetworkObject
    {
        protected override bool AllowOwnershipChange(NetworkingPlayer newOwner){
            Debug.Log("returning true for");
            Debug.Log(newOwner);
            return true;
        }

 		private void OnPlayerAccepted(NetworkingPlayer player, NetWorker serverNetworker){
		MainThreadManager.Run(()=> {
			var behavior = NetworkManager.Instance.InstantiateShipControl() as ShipControl;
			behavior.networkObject.AssignOwnership(player);
		});
        }
    }
}
