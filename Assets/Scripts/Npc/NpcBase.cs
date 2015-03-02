using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NpcBase : MonoBehaviour {
	private const float UP_OFS = 0.1f;
	private const float NEAR_RANGE_SQ = 0.5f*0.5f;
	private const float DEF_SPEED = 0.4f;

	public class MyAgent{
		private NavMeshPath mPath;
		private Vector3[] mCorners;
		private int mCornerPtr;
		private Vector3 mPosition;
		public Vector3[] corners { get { return mCorners; } }
		public int cornerPtr { get { return mCornerPtr; } }
		public Vector3 position { get { return mPosition; } }
		public float speed { get; set; }
		public int layerMask { get; set; }

		public MyAgent(){
			mPath = new NavMeshPath();
			mCorners = new Vector3[0];
			speed = 0f;
			layerMask = 0;
		}
		public bool CalculatePath(Vector3 _srcPos, Vector3 _tgtPos, int _mask){
			bool ret = false;
			if (NavMesh.CalculatePath (_srcPos, _tgtPos, layerMask, mPath)) {
				mCorners = mPath.corners;
				mCornerPtr = 0;
				mPosition = _srcPos;
				if(mCorners.Length>0){
					ret = true;
				}
			}
			return ret;
		}
		public bool UpdatePosition(){
			bool ret = false;
			if ((mCorners != null) && (mCorners.Length > mCornerPtr) ) {
				if(mCornerPtr<mCorners.Length){
					Vector3 tgtPos = mCorners[mCornerPtr];
					Vector3 dir = tgtPos - mPosition;
					float spd = Mathf.Min (dir.magnitude,speed*Time.deltaTime);
					mPosition += dir.normalized * spd;
					if(spd==0f){
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
	}

	public Transform destTr;
	private Transform mTempDestTr;
	protected float mDefSpeed;
	protected int mLayerMask;
	private NpcGenerator.EquipType mEquipType;
	private NpcGenerator mGenScr;
	protected float mCkDistMin,mCkDistMax;
	protected Vector3 mDestinationPos;
	protected Vector3 mNextPos;
	protected NpcGenerator.NpcTeam mTeam;
	protected NpcGenerator.NpcSpriteInfo mSpriteInfo;
	public Sprite EnemySprite;

	// for myAgent
	protected MyAgent mAg = new MyAgent ();

	private Camera m_mainCamera;
	
	public GameObject NpcSpriteContainer;
	private GameObject m_npcSpriteObject;
	private GameObject m_selectedUI;
	private int SPRITE_CHILD_INDEX = 0;

	public delegate void UnitCreatedDelegate();
	public static event UnitCreatedDelegate UnitCreatedEvent;
	public delegate void UnitRemovedDelegate();
	public static event UnitRemovedDelegate UnitRemovedEvent;
	
	virtual public void Awake(){
		mLayerMask = 0;

		m_mainCamera = Camera.main;
	}
	
	// Use this for initialization
	virtual public void Start () {

		bool isEnemy = (mTeam != NpcGenerator.NpcTeam.Red);
		switch (mEquipType)
		{
		default:
			mCkDistMin = 0f;
			mCkDistMax = 2.0f;
			break;
		case  NpcGenerator.EquipType.Trooper:
			mCkDistMin = 0f;
			mCkDistMax = 2.0f;
			break;
		case  NpcGenerator.EquipType.Archer:
			mCkDistMin = 4f;
			mCkDistMax = 6f;
			break;
		case  NpcGenerator.EquipType.Guardian:
			mCkDistMin = 0f;
			mCkDistMax = 0.5f;
			break;
		case  NpcGenerator.EquipType.Trooper2:
			mCkDistMin = 0f;
			mCkDistMax = 2.0f;
			break;
		case  NpcGenerator.EquipType.Trooper3:
			mCkDistMin = 0f;
			mCkDistMax = 2.0f;
			break;
		}
		Sprite npcSprite = (isEnemy) ? EnemySprite : mSpriteInfo.sprite;
		NpcSpriteContainer.GetComponent<NpcSpriteController>().SetSprite(npcSprite);

		mAg.speed = DEF_SPEED * (0.8f + Random.value * 0.4f);
		mDefSpeed = mAg.speed;
		mAg.CalculatePath(transform.position,destTr.position,mAg.layerMask);
		
		UnitCreatedEvent();
	}
	
	// Update is called once per frame
	virtual public void Update () {
		if (mAg.corners.Length == 0) {
			NavMeshHit hit;
			Vector3 tmpPos = transform.position;
			Vector3 tmpOfs = Vector3.up*1f;
			if(NavMesh.Raycast(tmpPos-tmpOfs,tmpPos+tmpOfs,out hit,mAg.layerMask)){
				tmpPos = hit.position;
			}
			mAg.CalculatePath(tmpPos,mDestinationPos,mLayerMask);
		}

		bool ck = (mAg.corners==null);

		Vector3 pos = mNextPos;
		ck |= (Random.value < 0.01f);
		if (ck) {
			updateAI();
			pos = transform.position;
			mAg.CalculatePath(transform.position,mDestinationPos,mLayerMask);
		}

		if (mAg.corners.Length > 0) {
			mNextPos = updatePosition (mNextPos);
		}

		//test
		if ((transform.position - destTr.position).sqrMagnitude < 2f) {
			KillMe();
		}
		debugCourseDisp ();
	}
	
	public void KillMe()
	{
		NpcSpriteContainer.GetComponent<NpcSpriteController>().IsDead();
	}
	
	private Transform getNearestEm (Vector3 _pos, float _minDist, float _maxDist){
		Transform retTr = null;
		List<GameObject> goList = mGenScr.GetEnemyListByTeam (mTeam);
		foreach (GameObject go in goList) {
			if(go!=this.gameObject){
				float distSq = (go.transform.position-transform.position).sqrMagnitude;
				if((distSq < _maxDist*_maxDist)&&(distSq > _minDist*_minDist)){
					retTr = go.transform;
					break;
				}
			}
		}
		
		return retTr;
	}

	protected virtual bool updateAI(){
		mTempDestTr = getNearestEm(transform.position,mCkDistMin,mCkDistMax);
		if (mTempDestTr != null) {
			mDestinationPos = mTempDestTr.position;
		}
		else
		{
			mDestinationPos = destTr.position;
		}
		if(mTempDestTr!=null){
			if ((transform.position - mTempDestTr.position).magnitude < 0.2f) {
				KillMe();
				return false;
			}
			else if ((transform.position - mTempDestTr.position).magnitude < 0.5f)
			{
				NpcSpriteContainer.GetComponent<NpcSpriteController>().IsHit();
			}
		}
		return true;
	}
	
	private Vector3 updatePosition(Vector3 _nextPos){
		mAg.UpdatePosition ();
		transform.position = mAg.position;
		return _nextPos;


		/*Vector3 ret = _nextPos;
		Vector3 dir = mAg.position - transform.position;
		if (dir.magnitude < mAg.speed) {
			Vector3 nextPos = transform.position + dir.normalized * mAg.speed * Time.deltaTime;
			gameObject.rigidbody.MovePosition (nextPos+Vector3.up*UP_OFS);
			if (mAg.corners.Length > mAg.cornerPtr) {
				ret = mAg.corners[mAg.cornerPtr];
			}
		}
		return ret;*/
	}

	public void SetDestination(Vector3 dest)
	{
		mDestinationPos = dest;
	}

	public void SetSelectedUI(GameObject selectedUiObj)
	{
		m_selectedUI = Instantiate(selectedUiObj) as GameObject;
		m_selectedUI.transform.SetParent(this.gameObject.transform, true);
		m_selectedUI.transform.localPosition = Vector3.zero;
	}

	public void RemoveSelectedUI()
	{
		Destroy(m_selectedUI.gameObject);
	}

	//--------------
	private void SM_initializeNpcSprite()
	{
		/*m_npcSpriteObject = Instantiate(NpcSpriteObject) as GameObject;
		m_npcSpriteObject.transform.parent = this.gameObject.transform;
		m_npcSpriteObject.transform.localPosition = Vector3.zero;
		m_npcSpriteObject.transform.localScale = Vector3.one;*/
	}
	
	private void SM_setGenerator(GameObject _genObj){
		mGenScr = _genObj.GetComponent<NpcGenerator>();
	}
	private void SM_setDest(Transform _tr){
		destTr = _tr;
		mTempDestTr = destTr;
	}
	private void SM_addNaviLayer(string _layerStr){
		int layer = NavMesh.GetNavMeshLayerFromName(_layerStr);
		if (layer >= 0) {
			mLayerMask |= (1 << layer);
			mAg.layerMask |= (1 << layer);
		} else {
			Debug.Log("warning: bad layer name.");
		}
	}
	private void SM_setEquipType( NpcGenerator.EquipType _equipType){}
	private void SM_setTeam(NpcGenerator.NpcTeam _team)	{}
	private void SM_setSpriteInfo(NpcGenerator.NpcSpriteInfo _info)
	{
		mSpriteInfo = _info;
		mTeam = _info.team;
		mEquipType = _info.type;
	}
	
	private void SM_removeMe()
	{
		UnitRemovedEvent();
		mGenScr.gameObject.SendMessage ("SM_removeNpc", this.gameObject);
	}

	//----------------------
	private void debugCourseDisp(){
		Debug.DrawLine (transform.position - Vector3.up, transform.position + Vector3.up, Color.white);
		if ((mAg != null)&&(mAg.corners.Length>0)) {
			Vector3 sttPos = transform.position+Vector3.up*UP_OFS;
			for (int ii = 0; ii < mAg.corners.Length; ++ii) {
				float rate = ((float)ii/(float)mAg.corners.Length);
				Vector3 pos = mAg.corners [ii]+Vector3.up*UP_OFS;
				Debug.DrawLine (sttPos, pos, Color.Lerp(Color.red,Color.blue,rate));
				sttPos = pos;
			}
		}
	}

}
