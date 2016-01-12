using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    public int value
    {
        get; private set;
    }
    public int m_posX;
    public int m_posY;
    public int m_itemID
    {
        get; private set;
    }

    public void Initialize(int posX, int posY, int _value, int itemID)
    {
        value = _value;
        m_posX = posX;
        m_posY = posY;
        m_itemID = itemID;
        transform.position = new Vector3(m_posX, m_posY, 0);
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
