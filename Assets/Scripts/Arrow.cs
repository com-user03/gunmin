using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {
	private float mNoHitTimer;
	private float mLifeTimer;
	void Awake () {
		gameObject.GetComponent<Collider>().enabled = false;
	}
	// Use this for initialization
	void Start () {
		mNoHitTimer = 0.1f;
		mLifeTimer = 2f;
	}
	
	// Update is called once per frame
	void Update () {
		mLifeTimer -= Time.deltaTime;
		if (mLifeTimer < 0f) {
			Destroy (gameObject);
			return;
		}

		if (mNoHitTimer > 0f) {
			mNoHitTimer -= Time.deltaTime;
			if(mNoHitTimer<=0f){
				gameObject.GetComponent<Collider>().enabled = true;
			}
		}

		if (gameObject.GetComponent<Rigidbody>().velocity.sqrMagnitude > 0.01f) {
			transform.rotation = Quaternion.LookRotation (gameObject.GetComponent<Rigidbody>().velocity);
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

	private void debugDraw(){
		Vector3 sttPos = transform.position;
		Vector3 spd = gameObject.GetComponent<Rigidbody>().velocity;
		float timer = mLifeTimer;
		float divTime = 0.1f;
		while(timer>0f){
			Vector3 pos = sttPos + spd * divTime;
			spd += Physics.gravity * divTime;
			Debug.DrawLine(sttPos,pos,Color.red);
			sttPos = pos;
			timer -= divTime;
		}
	}
}
