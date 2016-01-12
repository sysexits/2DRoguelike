﻿using UnityEngine;
using System.Collections;

public class Potion : MonoBehaviour {

    public int healAmount
    {
        get; private set;
    }
    public int m_posX;
    public int m_posY;

    public void Initialize(int posX, int posY, int amount)
    {
        healAmount = amount;
        m_posX = posX;
        m_posY = posY;
        transform.position = new Vector3(m_posX, m_posY, 0);
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
