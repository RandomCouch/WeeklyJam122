using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Fist : MonoBehaviour {

    public Action<EnemyScript, Vector3> OnHitEnemy;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.gameObject.tag == "Enemy")
        {
            //Hit enemy
            //Debug.Log("Hit enemy");
            EnemyScript enemyScript = collision.collider.gameObject.GetComponent<EnemyScript>();
            if(OnHitEnemy != null)
            {
                OnHitEnemy(enemyScript, transform.position);
            }
        }
    }
}
