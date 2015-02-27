using UnityEngine;
using System.Collections;

public class NpcArcher : NpcBase {
	public GameObject arrowPrefab;

	virtual public void Awake(){
		base.Awake ();
	}
	
	// Use this for initialization
	virtual public void Start () {
		base.Start ();
		mDefSpeed = 2.0f * (0.8f + Random.value * 0.4f);
		mCkDistMin = 15f;
		mCkDistMax = 30f;
	}
	
	// Update is called once per frame
	virtual public void Update () {
		base.Update ();
	}
	
	override protected bool updateAI(){
		if (base.updateAI ()) {
			if(Random.value>0.95f){
				setArrow(destTr,10f); //仮 
			}
		}
		return true;
	}

	//---------------------------------
	private void setArrow(Transform _tgtTr, float _arrowSpd){
		Vector3 _pos = _tgtTr.transform.position;
		Vector3 npcSpdVec = destTr.forward.normalized * mDefSpeed; //仮 
		float[] time = TmMath.CollideTime (_pos, npcSpdVec, transform.position, _arrowSpd);
		if (time != null) {
			//			Debug.Log ("reachTIme=" + time [0]);
			Vector3 futurePos = _pos+npcSpdVec*time[0];
			Vector3 spdVec = (futurePos-transform.position).normalized * _arrowSpd;
			float diffY = _pos.y - transform.position.y;
			spdVec.y += TmMath.ParabolicSpeed(time[0],diffY,Physics.gravity.y);
			GameObject bulletGo = GameObject.Instantiate (arrowPrefab) as GameObject;
			bulletGo.transform.position = transform.position;
			bulletGo.transform.rotation = Quaternion.FromToRotation(Vector3.forward,spdVec.normalized);
			bulletGo.rigidbody.velocity=Vector3.zero;
			bulletGo.rigidbody.AddForce(spdVec,ForceMode.VelocityChange);
			Destroy (bulletGo, 2f);
		}
	}
	
	
}
