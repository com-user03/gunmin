using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.RVO;

[RequireComponent(typeof(Seeker))]
public class MyAgent{
	public const bool USE_SEEKER_PATH = true;
	public const float DEF_SPEED = 1.0f;

	public enum TagNameLayer{ // A* TagNameにあわせる 
		BasicGround = 0,           // NavLayer 0
		AStarTagNotWalkable = 1,   // NavLayer 1
		AStarTagJump = 2,          // NavLayer 2
		AStarTagUserLayer = 3,     // NavLayer 3
		AStarTagThresholdRed = 4,  // NavLayer 4
		AStarTagThresholdBlue = 5, // NavLayer 5
		AStarTagBridgeRed = 6,     // NavLayer 6  
		AStarTagBridgeBlue = 7,    // NavLayer 7
		AStarTagHill = 8,          // NavLayer 8
	}

	//------------------------------------------------
	// NavMeshPathStatus,PathCompleteState の統一用 
	public enum PathState{
		None,
		Complete,
		Seeking,
		Partial,
		Error
	}
	public class PathCompleteResult{
		public PathState state;
		public Vector3[] corners;
		public Vector3 sttPos;
		public Vector3 tgtPos;
		public PathCompleteResult(){
			state = PathState.None;
			corners = new Vector3[0];
		}
	}
	//------------------------------------------------

	public delegate void OnCalculatePathComplete(PathCompleteResult result);

	private AstarPath mAsPath;
	private Seeker mSeeker;
	private Path mPath;
	private PathState mPathState;
	public PathState pathState{ get { return mPathState; } }
	private bool mIsSeeking;

	private Vector3[] mCorners;
	private int mCornerPtr;
	private Vector3 mPosition;
	public Vector3[] corners { get { return mCorners; } }
	public int cornerPtr { get { return mCornerPtr; } }
	public Vector3 position { get { return mPosition; } }
	public float speed { get; set; }
	public int layerMask { get; set; }

	public static int LayerMaskToTagMask(int _layerMask){
		int mask = (1<<((int)TagNameLayer.BasicGround));
		if ((getLayerMaskFromName ("NavLayerBridgeRed") & _layerMask)!=0) {
			mask |= (1<<((int)TagNameLayer.AStarTagBridgeRed));
		}
		if ((getLayerMaskFromName ("NavLayerBridgeBlue") & _layerMask)!=0) {
			mask |= (1<<((int)TagNameLayer.AStarTagBridgeBlue));
		}
		if ((getLayerMaskFromName ("NavLayerHill") & _layerMask)!=0) {
			mask |= (1<<((int)TagNameLayer.AStarTagHill));
		}
		return mask;
	}
	private static int getLayerMaskFromName(string _layerStr){
		int mask = 0;
		int layer = NavMesh.GetAreaFromName(_layerStr);
		if (layer >= 0) {
			mask |= (1 << layer);
		} else {
			Debug.Log("warning: bad layer name.");
		}
		return mask;
	}

	
	public MyAgent(GameObject _go, bool _tagClear){
		mIsSeeking = false;
		if (USE_SEEKER_PATH) {
			mSeeker = _go.GetComponent<Seeker> ();
			if(mSeeker==null){
				mSeeker = _go.AddComponent<Seeker> ();
			}
			mSeeker.drawGizmos = false;
			mAsPath = StageController.instance.astarPath;
			mPath = null;
		} else {
		}
		if(_tagClear){
			SetNaviLayer(0);
		}
		mPathState = PathState.None;
		mCorners = new Vector3[0];
		speed = DEF_SPEED;
		layerMask = 0;
	}
	public bool CalculatePath(Vector3 _sttPos, Vector3 _tgtPos, OnCalculatePathComplete _callback=null){
		bool ret = false;
		if(mIsSeeking){
			return ret;
		}
		mIsSeeking=true;

		PathCompleteResult result = new PathCompleteResult ();
		result.state = PathState.Seeking;
		result.sttPos = _sttPos;
		result.tgtPos = _tgtPos;
		if (_callback == null) {
			_callback = defOnPathComplete;
		} else {
//			Debug.Log("EM");
		}
		if (USE_SEEKER_PATH) {
			mSeeker.StartPath (_sttPos, _tgtPos ,(Path _p)=>{
				ABPath abp = _p as ABPath;
				if (abp == null) throw new System.Exception ("This function only handles ABPaths, do not use special path types");
				
				if (abp.CompleteState == PathCompleteState.Complete) {
					//Claim the new path
					abp.Claim (this);
					
					if (abp.error) {
						mPath = null;
						abp.Release (this);
					}else{
						mPath = abp;
					}

					if ((mPath!=null)&&(mPath.vectorPath != null)) {
						result.corners = mPath.vectorPath.ToArray ();
					}

					mPosition = result.sttPos;
					mPathState = result.state;
					mCorners = result.corners;
					mCornerPtr = 0;
				}

				switch (abp.CompleteState) {
				case PathCompleteState.Complete:      result.state = PathState.Complete; break;
				case PathCompleteState.Error:         result.state = PathState.Error;    break;
				case PathCompleteState.NotCalculated: result.state = PathState.Error;    break;
				case PathCompleteState.Partial:       result.state = PathState.Partial;  break;
				default:                              result.state = PathState.Error;    break;
				}
				
				_callback(result);
				mIsSeeking=false;
			}, mSeeker.traversableTags.tagsChange);
			ret = true;
		} else {
			NavMeshPath path = new NavMeshPath();
			if (NavMesh.CalculatePath (_sttPos, _tgtPos, layerMask, path)) {
				switch (path.status) {
				case NavMeshPathStatus.PathComplete:  result.state = PathState.Complete; break;
				case NavMeshPathStatus.PathInvalid:   result.state = PathState.Error;    break;
				case NavMeshPathStatus.PathPartial:   result.state = PathState.Partial;  break;
				default:                              result.state = PathState.Error;    break;
				}
				result.corners = path.corners;

				mPosition = result.sttPos;
				mPathState = result.state;
				mCorners = result.corners;
				mCornerPtr = 0;
				if(mCorners.Length>0){
					ret = true;
				}
			}else{
				Debug.Log("Warning:can't find path from "+_sttPos+" to "+_tgtPos+":layer:"+layerMask);
			}
			mIsSeeking=false;
		}
		return ret;
	}
	public bool UpdatePosition(float _spd=0f){
		bool ret = false;
		if (_spd == 0f) {
			_spd = speed;
		}
		if ((mCorners != null) && (mCorners.Length > mCornerPtr) ) {
			if(mCornerPtr<mCorners.Length){
				Vector3 tgtPos = mCorners[mCornerPtr];
				Vector3 dir = tgtPos - mPosition;
				float deltaSpd = _spd*Time.deltaTime;
				float spd = Mathf.Min (dir.magnitude,deltaSpd);
				mPosition += dir.normalized * spd;
				NavMeshHit hit;
				Vector3 ofsH = Vector3.up * 1f;
				if(NavMesh.Raycast(mPosition+ofsH,mPosition-ofsH,out hit,layerMask)){
					mPosition.y = hit.position.y;
				}
				if(spd<deltaSpd){
					if(mCornerPtr < (mCorners.Length-1) ){
						mCornerPtr++;
						ret = true;
					}
				}else{
					ret = true;
				}
			}
		}
		return ret;
	}

	private void defOnPathComplete (PathCompleteResult _result) {
//		Debug.Log ("RR:" + _result.corners.Length);
	}

	public void SetNaviLayer(int _layerMask){
		if (USE_SEEKER_PATH) {
			int tagMask = LayerMaskToTagMask(_layerMask);
			mSeeker.traversableTags = new TagMask(tagMask,tagMask);
		} else {
			layerMask = _layerMask;
		}
	}
	public void AddNaviLayer(string _layerStr){
		int layer = NavMesh.GetAreaFromName(_layerStr);
		if (layer >= 0) {
			if (USE_SEEKER_PATH) {
				int tagMask = mSeeker.traversableTags.tagsChange;
				tagMask |= LayerMaskToTagMask(1 << layer);
				mSeeker.traversableTags = new TagMask(tagMask,tagMask);
			} else {
				layerMask |= (1 << layer);
			}
		} else {
			Debug.Log("warning: bad layer name.");
		}
	}

	public void RemoveNaviLayer(string _layerStr){
		int layer = NavMesh.GetAreaFromName(_layerStr);
		if (layer >= 0) {
			if (USE_SEEKER_PATH) {
				int tagMask = mSeeker.traversableTags.tagsChange;
				tagMask &= (~MyAgent.LayerMaskToTagMask(1 << layer));
				mSeeker.traversableTags = new TagMask(tagMask,tagMask);
			} else {
				layerMask &= ~(1 << layer);
			}
		} else {
			Debug.Log("warning: bad layer name.");
		}
	}

	//----------------------
	public bool debugCourseDisp(Color _sttCol, Color _endCol){
		bool ret = false;
		if (corners.Length>0) {
			Debug.DrawLine (mPosition-Vector3.up, mPosition+Vector3.up, Color.white);
			Vector3 sttPos = corners [0];
			for (int ii = 0; ii < corners.Length; ++ii) {
				Vector3 pos = corners [ii];
				if(ii>0){
					float rate = ((float)(ii-1)/(float)(corners.Length-1));
					Color col = Color.Lerp(_sttCol,_endCol,rate);
//					Color hCol = new Color(col.r,col.g,col.b,col.a*0.3f);
//					Debug.DrawLine (sttPos, pos, hCol,0f,false);
					Debug.DrawLine (sttPos, pos, col);
				}
				sttPos = pos;
			}
			ret = true;
		}
		return ret;
	}
	
}
