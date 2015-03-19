using UnityEngine;
using System.Collections;

public class BridgeGenerator : MonoBehaviour {
	private const float MES_TIME = 10f;
	private const string MES_STR = "Press [Right_Shift] and drag to construct.";
	public GameObject bridgePrefab;
	public float bridgeWidth;
	private Camera mCam;
	private bool mDrag;
	private Vector3 mSttPos;
	private Vector3 mEndPos;
	private Collider mSttColl;
	private Collider mEndColl;
	private StageController mStgCtrl;
	private float mMesTimer;
	// Use this for initialization
	void Start () {
		mCam = Camera.main;
		mDrag = false;
		mStgCtrl = StageController.instance;
		mMesTimer = MES_TIME;
	}
	
	// Update is called once per frame
	void Update () {
		Ray ray;
		RaycastHit hit;
		bool sw = Input.GetKey (KeyCode.LeftShift) | Input.GetKey (KeyCode.RightShift);
		if (sw && Input.GetMouseButtonDown (0)) {
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
					Vector3 scale = new Vector3(bridgeWidth,bridgePrefab.transform.localScale.y,bridgeWidth);
					Vector3 pos = mSttPos + Vector3.up*bridgeWidth;
					Vector3 dir = Vector3.forward;
					if(mSttColl != mEndColl){
						dir = mEndPos-mSttPos;
						pos += dir*0.5f;
						scale.z = dir.magnitude;
					}else{
						scale.y = scale.x*2f;
					}
					scale.x = bridgeWidth;
					GameObject go = Instantiate(bridgePrefab);
					go.transform.position = pos;
					go.transform.rotation = Quaternion.LookRotation(dir);
					go.transform.localScale = scale;
				}
			}
		}
	}

	private void OnGUI(){
		if (mMesTimer > 0f) {
			mMesTimer -= Time.deltaTime;
			GUI.backgroundColor = Color.black;
			GUI.Box (new Rect (0, 0, 400, 24), MES_STR);
		}
	}
}
