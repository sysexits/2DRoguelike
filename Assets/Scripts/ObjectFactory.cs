using UnityEngine;
using System.Collections;

namespace Roguelike
{
    public class ObjectFactory : MonoBehaviour
    {
        // singleton!
        protected static ObjectFactory instance;

        // Player prefab
        public GameObject playerPrefab;

        // Use this for initialization
        void Start()
        {
            instance = this;
        }

        public static Player createPlayer(int posX, int posY, int HPStatus)
        {
            GameObject playerObject = (Instantiate(
                instance.playerPrefab,
                new Vector3(posX, posY, 0),
                Quaternion.identity
            ) as GameObject);
            Player player = playerObject.GetComponent<Player>();
            player.Initialize(posX, posY, HPStatus);
            return player;
        }
    }
}