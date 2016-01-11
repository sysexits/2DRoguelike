﻿using UnityEngine;
using System.Collections;
using CnControls;

namespace Roguelike
{
    public class Player : MonoBehaviour
    {
        public bool canMove = false;
        private int count = 0;
        private int dirX = 0;
        private int dirY = 1;

        private Animator anim;

        // sprites for moving: W S E N
        public enum Direction
        {
            WEST, SOUTH, EAST, NORTH
        };
        public Sprite[] moveSprite;
        public Animation[] animations;

        // the layer where the collision detection is checked
        public LayerMask objectLayer;

        // the box collider 2d attached to this game object
        private BoxCollider2D boxCollider;

        // member variables for this player
        private int m_posX;
        private int m_posY;
        private int m_HPStatus;

        private BoardManager boardManager = null;

        void Start()
        {
            anim = (Animator)GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            int horizontal = 0;
            int vertical = 0;

            // if moving horizontally, do not move vertically
            if (Time.frameCount % 20 == 0)
            {
                canMove = true;
            }
            
            if (Mathf.Abs(CnInputManager.GetAxis("Horizontal")) > 0 || Mathf.Abs(CnInputManager.GetAxis("Vertical")) > 0)
            {
                count += 1;
                if(count > 60)
                {
                    count = 1;
                }
            } else
            {
                count = 0;

                if(dirX == -1)
                {
                    GetComponent<SpriteRenderer>().sprite = moveSprite[(int)(Direction.WEST)];
                } else if(dirX == 1)
                {
                    GetComponent<SpriteRenderer>().sprite = moveSprite[(int)(Direction.EAST)];
                } else if(dirY == -1)
                {
                    GetComponent<SpriteRenderer>().sprite = moveSprite[(int)(Direction.NORTH)];
                } else if(dirY == 1)
                {
                    GetComponent<SpriteRenderer>().sprite = moveSprite[(int)(Direction.SOUTH)];
                }
            }
            
            if (canMove)
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

            RaycastHit2D hit;
            if (TryToMove(horizontal, vertical, out hit))
            {
                transform.Translate(horizontal, vertical, 0f);
                m_posX += horizontal;
                m_posY += vertical;
                if (horizontal != 0 || vertical != 0)
                {
                    if (m_posX == 0)
                        boardManager.gotoNextStage(Direction.WEST);
                    else if (m_posX == boardManager.columns - 1)
                        boardManager.gotoNextStage(Direction.EAST);
                    else if (m_posY == boardManager.rows - 1)
                        boardManager.gotoNextStage(Direction.NORTH);
                    else if (m_posY == 0)
                        boardManager.gotoNextStage(Direction.SOUTH);
                }
            }

            // count 2++ => run

            if (horizontal > 0)
            {
                if (count < 2)
                    GetComponent<SpriteRenderer>().sprite = moveSprite[(int)(Direction.EAST)];
                else
                    anim.Play(Animator.StringToHash("Base Layer.LinkMoveR"));
                dirX = 1; dirY = 0;
            }
            else if (horizontal < 0)
            {
                if (count < 2)
                    GetComponent<SpriteRenderer>().sprite = moveSprite[(int)(Direction.WEST)];
                else
                    anim.Play(Animator.StringToHash("Base Layer.LinkMoveL"));
                dirX = -1; dirY = 0;
            }
            else if (vertical > 0)
            {
                if (count < 2)
                    GetComponent<SpriteRenderer>().sprite = moveSprite[(int)(Direction.NORTH)];
                else
                    anim.Play(Animator.StringToHash("Base Layer.LinkMoveU"));
                dirX = 0; dirY = -1;
            }
            else if (vertical < 0)
            {
                if (count < 2)
                    GetComponent<SpriteRenderer>().sprite = moveSprite[(int)(Direction.SOUTH)];
                else
                    anim.Play(Animator.StringToHash("Base Layer.LinkMoveD"));
                dirX = 0; dirY = 1;
            }
        }

        // initialization method
        public void Initialize(int posX, int posY, int HPStatus)
        {
            m_posX = posX;
            m_posY = posY;
            m_HPStatus = HPStatus;
            boxCollider = GetComponent<BoxCollider2D>();
            if (boardManager == null)
            {
                boardManager = GameObject.Find("BoardManager").GetComponent<BoardManager>();
            }
            transform.position = new Vector3(posX, posY, 0);
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
    }
}
