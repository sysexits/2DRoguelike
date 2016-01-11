﻿using UnityEngine;
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

        // the layer where the collision detection is checked
        public LayerMask objectLayer;

        // the box collider 2d attached to this game object
        private BoxCollider2D boxCollider;

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

            RaycastHit2D hit;
            if (TryToMove(horizontal, vertical, out hit))
            {
                transform.Translate(horizontal, vertical, 0);
            }

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

        private bool TryToMove(int dirX, int dirY, out RaycastHit2D hit)
        {
            Vector2 start = transform.position;
            Vector2 end = start + new Vector2(dirX, dirY);

            // disable this object's box collider so that the ray cast does not hit this player
            boxCollider.enabled = false;

            hit = Physics2D.Linecast(start, end, objectLayer);

            // re-enable the box collider
            boxCollider.enabled = true;

            // the case when the line cast did not hit anything
            if (hit.transform == null)
            {
                return true;
            }

            // the casw when the line cast hit something
            return false;
        }

        // initialization method
        public void Initialize(int posX, int posY)
        {
            m_posX = posX;
            m_posY = posY;
            boxCollider = GetComponent<BoxCollider2D>();
        }
    }
}
