using UnityEngine;
using System.Collections;

public class PeerPlayer : MonoBehaviour {
    public int m_posX;
    public int m_posY;

    public string username;

    // sprites for moving: W S E N
    public enum Direction
    {
        WEST, SOUTH, EAST, NORTH
    };

    public void Initialize(int x, int y)
    {
        m_posX = x;
        m_posY = y;
        transform.position = new Vector3(m_posX, m_posY, 0);
    }

    public void Move(int posX, int posY)
    {
        int horizontal = posX - m_posX;
        int vertical = posY - m_posY;

        m_posX = posX;
        m_posY = posY;
        transform.position = new Vector3(m_posX, m_posY, 0);

        Animator anim = GetComponent<Animator>();

        if (horizontal > 0)
        {
            anim.Play(Animator.StringToHash("Base Layer.LinkMoveR"));
        }
        else if (horizontal < 0)
        {
            anim.Play(Animator.StringToHash("Base Layer.LinkMoveL"));
        }
        else if (vertical > 0)
        {
            anim.Play(Animator.StringToHash("Base Layer.LinkMoveU"));
        }
        else if (vertical < 0)
        {
            anim.Play(Animator.StringToHash("Base Layer.LinkMoveD"));
        }
    }
}
