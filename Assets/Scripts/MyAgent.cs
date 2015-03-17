﻿#define USE_SEEKER_PATH

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.RVO;

[RequireComponent(typeof(Seeker))]
public class MyAgent{
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

	private NavMeshPath mPath;

	private AstarPath mAsPath;
	private Seeker mSeeker;
	private bool mIsSeeking;

	private Vector3[] mCorners;
	private int mCornerPtr;
	private Vector3 mPosition;
	public Vector3[] corners { get { return mCorners; } }
	public int cornerPtr { get { return mCornerPtr; } }
	public Vector3 position { get { return mPosition; } }
	public float speed { get; set; }
	public int layerMask { get; set; }

	public int layerMaskToTagMask(int _layerMask){
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
	private int getLayerMaskFromName(string _layerStr){
		int mask = 0;
		int layer = NavMesh.GetAreaFromName(_layerStr);
		if (layer >= 0) {
			mask |= (1 << layer);
		} else {
			Debug.Log("warning: bad layer name.");
		}
		return mask;
	}

	
	public MyAgent(GameObject _go){
#if (USE_SEEKER_PATH)
		mSeeker = _go.GetComponent<Seeker> ();
		mAsPath = StageController.instance.astarPath;
#else
		mPath = new NavMeshPath();
#endif
		mCorners = new Vector3[0];
		speed = 0f;
		layerMask = 0;
		Enable ();
	}
	public bool CalculatePath(Vector3 _srcPos, Vector3 _tgtPos){
		bool ret = false;
#if (USE_SEEKER_PATH)
		if (!mIsSeeking) {
			mIsSeeking = true;
			mPosition = _srcPos;
			int tagMask = layerMaskToTagMask(layerMask);
			mSeeker.StartPath (_srcPos, _tgtPos ,onPathComplete, tagMask);
			ret = true;
		}
#else
		if (NavMesh.CalculatePath (_srcPos, _tgtPos, layerMask, mPath)) {
			mCorners = mPath.corners;
			mCornerPtr = 0;
			mPosition = _srcPos;
			if(mCorners.Length>0){
				ret = true;
			}
		}
#endif
		return ret;
	}
	public bool UpdatePosition(){
		bool ret = false;
		if ((mCorners != null) && (mCorners.Length > mCornerPtr) ) {
			if(mCornerPtr<mCorners.Length){
				Vector3 tgtPos = mCorners[mCornerPtr];
				Vector3 dir = tgtPos - mPosition;
				float deltaSpd = speed*Time.deltaTime;
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

	public void Enable () {
//		mSeeker.pathCallback += OnPathComplete;
	}
	
	public void Disable () {
//		mSeeker.pathCallback -= OnPathComplete;
	}
	
	private void onPathComplete (Path _p) {
		mIsSeeking = false;
		ABPath abp = _p as ABPath;
		if (abp == null) throw new System.Exception ("This function only handles ABPaths, do not use special path types");

		//Claim the new path
		abp.Claim (this);

		List<Vector3> vPath = abp.vectorPath;
		mCorners = vPath.ToArray ();
		mCornerPtr = 0;
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
