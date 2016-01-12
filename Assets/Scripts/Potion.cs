using UnityEngine;
using System.Collections;

public class Potion : MonoBehaviour {

    public int healAmount
    {
        get; private set;
    }

    public void Initialize(int amount)
    {
        healAmount = amount;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
