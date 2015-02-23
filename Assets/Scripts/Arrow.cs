using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {
	private float mNoHitTimer;
	// Use this for initialization
	void Start () {
		mNoHitTimer = 0.1f;
		gameObject.collider.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (mNoHitTimer > 0f) {
			mNoHitTimer -= Time.deltaTime;
			if(mNoHitTimer<=0f){
				gameObject.collider.enabled = true;
			}
		}

		if (gameObject.rigidbody.velocity.sqrMagnitude > 0.01f) {
			transform.rotation = Quaternion.LookRotation (gameObject.rigidbody.velocity);
		}
	}

	private void OnCollisionEnter(Collision _coll){
		Destroy (gameObject);
	}

	private void OnTriggerEnter(Collider _coll){
		if (_coll.gameObject.tag == "tagNpc") {
//			Debug.Log("hit");
            Npc npcObj = _coll.gameObject.GetComponent<Npc>();
            if (npcObj)
            {
                npcObj.KillMe();
            }
		}
		Destroy (gameObject);
	}
	
}
