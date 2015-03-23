using UnityEngine;
using System.Collections;

public class EmSphere : EmBase {

	override public void Awake(){
		base.Awake ();
	}
	
	override public void Start () {
		base.Start ();
	}
	
	override public void Update () {
		base.Update ();
	}
	
	override protected bool updateAI(){
		base.updateAI ();
		bool ret = selCheck ();
		return ret;
	}
	
	override protected Vector3 updatePosition(Vector3 _nextPos){
		return base.updatePosition (_nextPos);
	}

	//----------------------
	private bool selCheck(){
		bool ret = false;
		if (Input.GetMouseButtonDown (0)) {
			int mask = 1; // Default;
			mask |= (1<<LayerMask.NameToLayer("layerNpc"));
			Ray ray = mMainCam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, 100f, mask)){
				ret = true;
				if(hit.transform.gameObject.layer == LayerMask.NameToLayer("layerNpc")){
					if(hit.transform != transform){
						Debug.Log("HitTr");
						mTargetTr = hit.transform;
					}
				}else{
					Debug.Log("HitPos");
					mTargetPos = hit.point;
				}
			}
		}
		return ret;
	}
}
