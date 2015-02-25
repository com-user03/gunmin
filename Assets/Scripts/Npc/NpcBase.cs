using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NpcBase : MonoBehaviour {
	private const float UP_OFS = 0.1f;
	private const float NEAR_RANGE_SQ = 0.5f*0.5f;

	public GameObject arrowPrefab;
	public Transform destTr;
	private Transform mTempDestTr;
	private float mDefSpeed;
	protected int mLayerMask;
	private NpcGenerator.EquipType mEquipType;
	private NpcGenerator mGenScr;
	private float mCkDistMin,mCkDistMax;
	protected Vector3 mDestinationPos;
	protected Vector3 mNextPos;
	protected NavMeshPath mPath;
	protected float mStoppingDist;

	private Camera m_mainCamera;
	
	public Sprite[] m_npcSprites;
	public GameObject NpcSpriteObject;
	private GameObject m_npcSpriteObject;
	private int SPRITE_CHILD_INDEX = 0;
	private bool m_isEnemy;
	
	public delegate void UnitCreatedDelegate();
	public static event UnitCreatedDelegate UnitCreatedEvent;
	public delegate void UnitRemovedDelegate();
	public static event UnitRemovedDelegate UnitRemovedEvent;
	
	void Awake(){
		mPath = new NavMeshPath ();
		mLayerMask = 0;
		mDefSpeed = 2.0f * (0.8f + Random.value * 0.4f);

		m_mainCamera = Camera.main;
	}
	
	// Use this for initialization
	void Start () {

		switch (mEquipType)
		{
		default:
			mCkDistMin = 0f;
			mCkDistMax = 2.0f;
			break;
		case  NpcGenerator.EquipType.Trooper:
			mCkDistMin = 0f;
			mCkDistMax = 2.0f;
			m_npcSpriteObject.gameObject.GetComponent<SpriteRenderer>().sprite = (m_isEnemy) ? m_npcSprites[3] : m_npcSprites[0];
			break;
		case  NpcGenerator.EquipType.Archer:
			mCkDistMin = 4f;
			mCkDistMax = 6f;
			m_npcSpriteObject.gameObject.GetComponent<SpriteRenderer>().sprite = (m_isEnemy) ? m_npcSprites[3] : m_npcSprites[1];
			break;
		case  NpcGenerator.EquipType.Guardian:
			mCkDistMin = 0f;
			mCkDistMax = 0.5f;
			m_npcSpriteObject.gameObject.GetComponent<SpriteRenderer>().sprite = (m_isEnemy) ? m_npcSprites[3] : m_npcSprites[2];
			break;
		case  NpcGenerator.EquipType.Trooper2:
			mCkDistMin = 0f;
			mCkDistMax = 2.0f;
			this.transform.GetChild(SPRITE_CHILD_INDEX).gameObject.GetComponent<SpriteRenderer>().sprite = (m_isEnemy) ? m_npcSprites[3] : m_npcSprites[4];
			break;
		case  NpcGenerator.EquipType.Trooper3:
			mCkDistMin = 0f;
			mCkDistMax = 2.0f;
			this.transform.GetChild(SPRITE_CHILD_INDEX).gameObject.GetComponent<SpriteRenderer>().sprite = (m_isEnemy) ? m_npcSprites[3] : m_npcSprites[5];
			break;
		}
		
		m_npcSpriteObject.transform.position = this.transform.position;
		
		UnitCreatedEvent();
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = mNextPos;
		bool ck = (Random.value < 0.01f);
		if (ck) {
			updateAI();
			pos = transform.position;
		}
		ck |= ((pos - transform.position).sqrMagnitude < NEAR_RANGE_SQ);
		if (ck) {
			updatePath(pos,destTr.position);
		}

		//カメラに向かう
		Vector3 lookAtPos = this.transform.position + m_mainCamera.transform.rotation * Vector3.forward;
		lookAtPos.y = 0;
		this.transform.LookAt(lookAtPos, Vector3.up);

		updatePosition();
		debugCourseDisp ();
	}
	
	public void KillMe()
	{
		if (m_npcSpriteObject)
		{
			m_npcSpriteObject.GetComponent<Animator>().SetBool("IsDead", true);
		}
	}
	
	private Transform getNearestEm (Vector3 _pos, float _minDist, float _maxDist){
		Transform retTr = null;
		List<GameObject> goList = mGenScr.GetEnemyListByMyName (name);
		foreach (GameObject go in goList) {
			float distSq = (go.transform.position-transform.position).sqrMagnitude;
			if((distSq < _maxDist*_maxDist)&&(distSq > _minDist*_minDist)){
				retTr = go.transform;
				break;
			}
			//Debug.DrawLine(go.transform.position,go.transform.position+Vector3.up*3f,renderer.material.color);
		}
		
		return retTr;
	}

	protected void updateAI(){
		mTempDestTr = getNearestEm(transform.position,mCkDistMin,mCkDistMax);
		if (mTempDestTr != null) {
			mStoppingDist = 0f;
			switch(mEquipType){
			default:
				mDestinationPos = mTempDestTr.position;
				break;
			case  NpcGenerator.EquipType.Trooper:
				mDestinationPos = mTempDestTr.position;
				break;
			case  NpcGenerator.EquipType.Archer:
				mDestinationPos = transform.position;
				break;
			case  NpcGenerator.EquipType.Guardian:
				mDestinationPos = mTempDestTr.position;
				break;
			}
		}
		else
		{
			mStoppingDist = 0.75f;
			mDestinationPos = destTr.position;
		}
		if(mTempDestTr!=null){
			if ((transform.position - mTempDestTr.position).magnitude < 0.2f) {
				KillMe();
				return;
			}
			else if ((transform.position - mTempDestTr.position).magnitude < 0.5f)
			{
				m_npcSpriteObject.GetComponent<Animator>().SetBool("IsHit", true);
			}
		}
	}
	
	private void updatePath(Vector3 _srcPos, Vector3 _tgtPos){
		if (NavMesh.CalculatePath (_srcPos, _tgtPos, mLayerMask, mPath)) {
			if (mPath.corners.Length > 1) {
				Vector3 upOfs = Vector3.up * UP_OFS;
				mNextPos = mPath.corners [1];
				NavMeshHit hit;
				if (NavMesh.Raycast (_srcPos + upOfs, mNextPos + upOfs, out hit, mLayerMask)) {
//					mNextPos = hit.position;
				}
			}
		}
	}
	private void updatePosition(){
		Vector3 dir = (mNextPos - transform.position).normalized;
		float hSpd = Time.deltaTime * mDefSpeed;
		gameObject.rigidbody.MovePosition (transform.position + dir * hSpd);
	}

	//--------------
	private void SM_initializeNpcSprite()
	{
		m_npcSpriteObject = Instantiate(NpcSpriteObject) as GameObject;
		m_npcSpriteObject.transform.localScale = this.transform.localScale;
		m_npcSpriteObject.transform.parent = this.gameObject.transform;
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
		} else {
			Debug.Log("warning: bad layer name.");
		}
	}
	private void SM_setEquipType( NpcGenerator.EquipType _equipType){
		mEquipType = _equipType;
		//Debug.Log("mEquipType="+mEquipType.ToString());
	}
	private void SM_setIsEnemy(bool isEnemy)
	{
		m_isEnemy = isEnemy;
	}
	private void SM_setColor(Color color)
	{
		m_npcSpriteObject.renderer.material.color = color;
	}
	
	private void SM_removeMe()
	{
		UnitRemovedEvent();
		mGenScr.gameObject.SendMessage ("SM_removeNpc", this.gameObject);
	}

	//----------------------
	private void debugCourseDisp(){
		if ((mPath != null)&&(mPath.corners.Length>1)) {
			Vector3 sttPos = mPath.corners [0]+Vector3.up*UP_OFS;
			for (int ii = 1; ii < mPath.corners.Length; ++ii) {
				Vector3 pos = mPath.corners [ii]+Vector3.up*UP_OFS;
				Debug.DrawLine (sttPos, pos, Color.red);
				sttPos = pos;
			}
			Debug.DrawLine (transform.position,mNextPos+Vector3.up * UP_OFS,Color.yellow);
		}
	}

}
