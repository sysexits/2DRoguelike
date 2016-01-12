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
    public Sprite[] moveSprite;

    public void Initialize(int x, int y)
    {
        m_posX = x;
        m_posY = y;
        transform.position = new Vector3(m_posX, m_posY, 0);
    }
}
