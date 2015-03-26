using UnityEngine;
using System.Collections;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class EmBase : MonoBehaviour {
	protected MyAgent mAg;
	protected Transform mTargetTr;
	protected Vector3 mGndOfs; // Centerからのオフセット 
	protected Vector3 mTargetPos;
	protected Vector3 mNextPos;
	protected Camera mMainCam;

	public virtual void Awake(){
		mAg = new MyAgent (gameObject,false);
		mTargetTr = null;
		mGndOfs = Vector3.zero;
		mTargetPos = transform.position;
		mNextPos = transform.position;
		mMainCam = Camera.main;
		if (MyAgent.USE_SEEKER_PATH == false) {
			mAg.AddNaviLayer("Walkable");
		}
	}

	public virtual void Start () {
	}

	public virtual void Update () {
		if(updateAI()){
			Vector3 srcPos;
			Vector3 destPos;
			srcPos = transform.position+mGndOfs;
			if(mTargetTr != null){
				destPos = mTargetTr.position;
			}else{
				destPos = mTargetPos;
			}
			if((destPos-transform.position).sqrMagnitude > (1f*1f)){
				mAg.CalculatePath(srcPos,destPos,onCalculatePathComplete);
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
		Vector3 dir = mAg.position - (transform.position+mGndOfs);
		gameObject.GetComponent<Rigidbody>().MovePosition (transform.position + dir);
		mAg.UpdatePosition ();
		return mAg.position;
	}

	protected void onCalculatePathComplete(MyAgent.PathCompleteResult _result){
		Debug.Log ("callback:"+_result.state.ToString());
	}

}
