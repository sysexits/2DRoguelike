using UnityEngine;
using System.Collections;

namespace Roguelike
{
    public class Player : MonoBehaviour
    {
        // sprites for moving: N E S W
        public enum Direction
        {
            NORTH, EAST, SOUTH, WEST
        };
        public Sprite[] moveSprite;

        // member variables for this player
        private int m_posX;
        private int m_posY;

        // Update is called once per frame
        void Update()
        {
            int horizontal = 0;
            int vertical = 0;
            
            if (Input.GetKeyUp(KeyCode.W))
            {
                vertical = 1;
            }
            else if (Input.GetKeyUp(KeyCode.S))
            {
                vertical = -1;
            }
            else if (Input.GetKeyUp(KeyCode.D))
            {
                horizontal = 1;
            }
            else if (Input.GetKeyUp(KeyCode.A))
            {
                horizontal = -1;
            }

            // if moving horizontally, do not move vertically
            if (horizontal != 0)
            {
                vertical = 0;
            }

            transform.Translate(horizontal, vertical, 0);

            if (horizontal > 0)
            {
                GetComponent<SpriteRenderer>().sprite = moveSprite[(int)(Direction.EAST)];
            }
            else if (horizontal < 0)
            {
                GetComponent<SpriteRenderer>().sprite = moveSprite[(int)(Direction.WEST)];
            }
            else if (vertical > 0)
            {
                GetComponent<SpriteRenderer>().sprite = moveSprite[(int)(Direction.NORTH)];
            }
            else if (vertical < 0)
            {
                GetComponent<SpriteRenderer>().sprite = moveSprite[(int)(Direction.SOUTH)];
            }
        }

        // initialization method
        public void Initialize(int posX, int posY)
        {
            m_posX = posX;
            m_posY = posY;
        }
    }
}
