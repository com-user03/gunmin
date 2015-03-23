using UnityEngine;
using System.Collections;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class EmBase : MonoBehaviour {
	protected MyAgent mAg;
	protected Transform mTargetTr;
	protected Vector3 mTargetPos;
	protected Vector3 mNextPos;
	protected Camera mMainCam;

	public virtual void Awake(){
		mAg = new MyAgent (gameObject,false);
		mTargetTr = null;
		mTargetPos = transform.position;
		mNextPos = transform.position;
		mMainCam = Camera.main;
	}

	public virtual void Start () {
	}

	public virtual void Update () {
		if(updateAI()){
			Vector3 destPos;
			if(mTargetTr != null){
				destPos = mTargetTr.position;
			}else{
				destPos = mTargetPos;
			}
			destPos.y = transform.position.y;
			if((destPos-transform.position).sqrMagnitude > (1f*1f)){
				mAg.CalculatePath(transform.position,destPos);
			}
		}
		if (mAg.corners.Length > 0) {
			mNextPos = updatePosition (mNextPos);
		}

#if UNITY_EDITOR
		if (mAg != null) {
			mAg.debugCourseDisp (Color.red, Color.blue);
		}
#endif
	}

	protected virtual bool updateAI(){
		return false;
	}

	protected virtual Vector3 updatePosition(Vector3 _nextPos){
		Vector3 dir = mAg.position - transform.position;
		dir.y = 0f;
		gameObject.GetComponent<Rigidbody>().MovePosition (transform.position + dir);
		mAg.UpdatePosition ();
		return mAg.position;
	}
}
