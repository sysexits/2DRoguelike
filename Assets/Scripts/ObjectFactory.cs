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

        // Potion prefab
        public GameObject potionPrefab;

        // Weapon prefab
        public GameObject weaponPrefab;

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

        public static Potion createPotion(int posX, int posY, int healAmount)
        {
            GameObject potionObject = (Instantiate(
                instance.potionPrefab,
                new Vector3(posX, posY, 0),
                Quaternion.identity
            ) as GameObject);
            Potion potion = potionObject.GetComponent<Potion>();
            potion.Initialize(posX, posY, healAmount);
            return potion;
        }

        public static Weapon createWeapon(int posX, int posY, int value)
        {
            GameObject weaponObject = (Instantiate(
                instance.weaponPrefab,
                new Vector3(posX, posY, 0),
                Quaternion.identity
            ) as GameObject);
            Weapon weapon = weaponObject.GetComponent<Weapon>();
            weapon.Initialize(posX, posY, value);
            return weapon;
        }
    }
}