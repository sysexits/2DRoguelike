using UnityEngine;
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

        // sprites for moving: N E S W
        public enum Direction
        {
            NORTH, EAST, SOUTH, WEST
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
