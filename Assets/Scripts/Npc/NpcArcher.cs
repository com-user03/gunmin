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
		mCkDistMin = 4f;
		mCkDistMax = 8f;
	}
	
	// Update is called once per frame
	virtual public void Update () {
		base.Update ();
	}
	
	override protected bool updateAI(){
		if (base.updateAI ()) {
			if(Random.value>0.95f){
				if(mTargetTr!=null){
					setArrow(mTargetTr,5f); //仮 
				}
			}
		}
		return true;
	}
	override protected Vector3 updatePosition(Vector3 _nextPos){
		return base.updatePosition (_nextPos);
	}

	//---------------------------------
	private bool setArrow(Transform _tgtTr, float _arrowSpd){
		bool ret = false;
		Vector3 _pos = _tgtTr.transform.position;
		Vector3 npcSpdVec = Vector3.zero;
		if (_tgtTr != null){
			Rigidbody rb = _tgtTr.GetComponent<Rigidbody> ();
			if(rb!=null){
				npcSpdVec = rb.velocity;
			}
		}
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
			ret = true;
		}
		return ret;
	}
	
	
}
