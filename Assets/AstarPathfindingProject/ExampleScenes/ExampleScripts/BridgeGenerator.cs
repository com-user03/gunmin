using UnityEngine;
using System.Collections;

public class BridgeGenerator : MonoBehaviour {
	public GameObject bridgePrefab;
	private Camera mCam;
	private bool mDrag;
	private Vector3 mSttPos;
	private Vector3 mEndPos;
	private Collider mSttColl;
	private Collider mEndColl;
	// Use this for initialization
	void Start () {
		mCam = Camera.main;
		mDrag = false;
	}
	
	// Update is called once per frame
	void Update () {
		Ray ray;
		RaycastHit hit;
		if (Input.GetMouseButtonDown (0)) {
			if (!mDrag) {
				ray = mCam.ScreenPointToRay (Input.mousePosition);
				if (Physics.Raycast (ray, out hit)) {
					mDrag = true;
					mSttColl = hit.collider;
					mSttPos = hit.point;
				}
			}
		} else if (Input.GetMouseButtonUp (0)) {
			if (mDrag) {
				mDrag = false;
				ray = mCam.ScreenPointToRay (Input.mousePosition);
				if (Physics.Raycast (ray, out hit)) {
					mEndColl = hit.collider;
					mEndPos = hit.point;
					Vector3 scale = bridgePrefab.transform.localScale;
					if(mSttColl != mEndColl){
						Vector3 dir = mEndPos-mSttPos;
						Vector3 pos = mSttPos + dir*0.5f + Vector3.up*5f;
						scale.z = dir.magnitude;
						GameObject go = Instantiate(bridgePrefab);
						go.transform.position = pos;
						go.transform.rotation = Quaternion.LookRotation(dir);
						go.transform.localScale = scale;
					}else{
						GameObject go = Instantiate(bridgePrefab);
						go.transform.position = mSttPos + Vector3.up*5f;
						go.transform.localScale = new Vector3(scale.x,scale.x,scale.x);
					}
				}
			}
		}
	
	}
}
