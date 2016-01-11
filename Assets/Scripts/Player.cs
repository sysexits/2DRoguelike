using UnityEngine;
using System.Collections;
using CnControls;

namespace Roguelike
{
    public class Player : MonoBehaviour
    {
        public bool canMove = false;

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

            if (Time.frameCount % 20 == 0)
            {
                canMove = true;
            }
            if(canMove)
            {
                canMove = false;
                var movementVector = new Vector3(CnInputManager.GetAxis("Horizontal"), CnInputManager.GetAxis("Vertical"), 0f);
                if (movementVector.sqrMagnitude >= 0.00001f)
                {
                    movementVector.Normalize();
                    double rad = Mathf.Atan2(movementVector.y, movementVector.x);
                    double degree = rad * Mathf.Rad2Deg;
                    if (degree >= 45 && degree <= 135)
                    {
                        vertical = 1;
                    }
                    else if (degree > 135 || degree < -135)
                    {
                        horizontal = -1;
                    }
                    else if (degree <= -45 && degree >= -135)
                    {
                        vertical = -1;
                    }
                    else if (degree < 45 || degree < -45)
                    {
                        horizontal = 1;
                    }
                }
            }
            transform.Translate(horizontal, vertical, 0f);

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
