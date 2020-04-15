using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;
using System.Linq;

namespace BeardedManStudios.Forge.Networking.Unity
{


    public class GameLogic : MonoBehaviour
    {
        public List<GameObject> enemyShips;
        public List<GameObject> enemyTurrets;
        public List<GameObject> friendlyShips;
        public List<GameObject> friendlyTurrets;
        public int enemyCount; // this will be the total # of enemies in the scene
        public int enemyTurretCount; // same as enemycount but for the turrets

        public int friendlyCount; // number of allied ships
        public int friendlyTurretCount; // same as above, but for the turrets

        public bool winCondition = false;
        public bool lossCondition = false;

        // Start is called before the first frame update
        void Start()
        {
            var ball = NetworkManager.Instance.InstantiateShipControl();
            Debug.Log(ball.gameObject.name);

        }  

        private void FixedUpdate() {

            enemyShips = enemyShips.Where(item => item != null).ToList();
            enemyTurrets = enemyTurrets.Where(item => item != null).ToList();
            friendlyShips = friendlyShips.Where(item => item != null).ToList();
            friendlyTurrets = friendlyTurrets.Where(item => item != null).ToList();

            enemyCount = enemyShips.Count;
            enemyTurretCount = enemyTurrets.Count;
            friendlyCount = friendlyShips.Count;
            friendlyTurretCount = friendlyTurrets.Count;

            if(enemyShips.Count <= 0 && enemyTurrets.Count <= 0){
                winCondition = true;
            }    

            if(friendlyShips.Count <= 0 && friendlyTurrets.Count <= 0){
                lossCondition = true;
            }
        }

    }
}
