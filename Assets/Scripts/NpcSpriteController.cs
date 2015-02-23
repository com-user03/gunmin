﻿using UnityEngine;
using System.Collections;

public class NpcSpriteController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void RemoveMe()
    {
		this.transform.parent.SendMessage ("SM_removeMe");
        Destroy(this.transform.parent.gameObject);
    }

    void EndHit()
    {
        this.GetComponent<Animator>().SetBool("IsHit", false);
    }
}
