using UnityEngine;
using System.Collections;

public class Bridge : MonoBehaviour {
	private const float TO_DESTROY_TIME = 20f;
	private bool mToDestroy;
	private bool mFinished;
	private float mToDestroyTimer;
	private Rigidbody mRidgit;
	private Collider mColl;
	private Color mSttCol;
	private readonly Color mEndCol = Color.red;
	private readonly Color mFinishedCol = Color.black;
	private Pathfinding.GraphUpdateScene mGUScene;
	private Vector3[] mPoints;
	private AstarPath mASPath;
	
	// Use this for initialization
	void Start () {
		GameObject asGo = GameObject.Find ("A*");
		mASPath = asGo.GetComponent<AstarPath>();
		mToDestroy = false;
		mFinished = false;
		mToDestroyTimer = TO_DESTROY_TIME;
		mRidgit = gameObject.GetComponent<Rigidbody> ();
		mColl = gameObject.GetComponent<Collider> ();
		mGUScene = gameObject.GetComponent<Pathfinding.GraphUpdateScene> ();
		mSttCol = GetComponent<Renderer> ().material.color;
		int divNum = 10;
		mPoints = new Vector3[divNum];
		for(int ii = 0; ii < mPoints.Length; ++ii) {
			Vector3 ppos = new Vector3();
			ppos.z -= 0.5f; //* transform.localScale.z;
			ppos.z += ((float)ii/(float)mPoints.Length); // * transform.localScale.z;
			mPoints[ii] = ppos;
		}
		//		mGUScene.points = mPoints;
	}
	
	// Update is called once per frame
	void Update () {
		if (mFinished)
			return;
		
		if (!mToDestroy) {
			if (mRidgit.IsSleeping()) {
				mRidgit.isKinematic = true;
				mToDestroy = true;
				mGUScene.Apply();
			}
		} else {
			if(!mRidgit.IsSleeping()){
				mToDestroy = false;
				mRidgit.isKinematic = false;
			}
			mToDestroyTimer -= Time.deltaTime;
			float rate = 1f-mToDestroyTimer/TO_DESTROY_TIME;
			GetComponent<Renderer> ().material.color = Color.Lerp(mSttCol,mEndCol,rate);
			if(mToDestroyTimer<=0f){
				mFinished = true;
				GetComponent<Renderer> ().material.color = mFinishedCol;
				Debug.Log("finished");
//				mColl.enabled = false;
				gameObject.layer = LayerMask.NameToLayer("layerIgnorePathfind");
				mGUScene.Apply();
				Destroy(gameObject);
//				mASPath.Scan();
			}
		}
	}
	
}
