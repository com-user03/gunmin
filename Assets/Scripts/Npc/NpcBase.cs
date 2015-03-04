using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NpcBase : MonoBehaviour {
	private const float UP_OFS = 0.1f;
	private const float NEAR_RANGE_SQ = 0.5f*0.5f;
	private const float DEF_SPEED = 0.4f;

	public Transform destTr;
	public Sprite EnemySprite;
	public int layerMask {
		get { return (mAg!=null) ? mAg.layerMask : 0; }
		set { if(mAg!=null){ mAg.layerMask = value; } }
	}

	protected float mDefSpeed;
	private NpcGenerator.EquipType mEquipType;
	private NpcGenerator mGenScr;
	protected float mCkDistMin,mCkDistMax;
	protected Transform mTargetTr;
	protected bool m_hasMoveTargetPos;
	protected Vector3 m_moveTargetPos;
	protected Vector3 mDestinationPos;
	protected Vector3 mNextPos;
	protected NpcGenerator.NpcTeam mTeam;
	protected NpcGenerator.NpcSpriteInfo mSpriteInfo;
	private bool mAIUpdateFlag;


	// for myAgent
	protected MyAgent mAg;

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
		m_mainCamera = Camera.main;
		mAg = new MyAgent ();
	}
	
	// Use this for initialization
	virtual public void Start () {
		mTargetTr = null;
		mAIUpdateFlag = false;
		prepareParam ();
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
			mAg.CalculatePath(tmpPos,mDestinationPos);
		}
		else if (m_hasMoveTargetPos && (transform.position - mDestinationPos).magnitude < 0.1f)
		{
			m_hasMoveTargetPos = false;
			mAIUpdateFlag = true;
		}

		mAIUpdateFlag |= (mAg.corners==null);
		mAIUpdateFlag |= (Random.value < 0.01f);
		if (mAIUpdateFlag) {
			mAIUpdateFlag = false;
			updateAI();
			mAg.CalculatePath(transform.position,mDestinationPos);
		}

		if (mAg.corners.Length > 0) {
			mNextPos = updatePosition (mNextPos);
		}

		//test
		if ((transform.position - destTr.position).sqrMagnitude < 2f) {
			KillMe();
		}

		if (mAg != null) {
			mAg.debugCourseDisp (Color.red, Color.blue);
		}
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
		Transform tempDestTr;
		tempDestTr = getNearestEm(transform.position,mCkDistMin,mCkDistMax);
		if (tempDestTr != null) {
			mTargetTr = tempDestTr;
			mDestinationPos = tempDestTr.position;

			if ((transform.position - tempDestTr.position).magnitude < 0.2f) {
				KillMe();
				return false;
			}
			else if ((transform.position - tempDestTr.position).magnitude < 0.5f)
			{
				NpcSpriteContainer.GetComponent<NpcSpriteController>().IsHit();
			}
		}
		else if (m_hasMoveTargetPos)
		{
			mDestinationPos = m_moveTargetPos;
		}
		else
		{
			mDestinationPos = destTr.position;
			mNextPos = destTr.position;
		}
		return true;
	}
	
	protected virtual Vector3 updatePosition(Vector3 _nextPos){
		Vector3 dir = mAg.position - transform.position;
		dir.y = 0f;
		gameObject.rigidbody.MovePosition (transform.position + dir);

		if (dir.sqrMagnitude < NEAR_RANGE_SQ) {
			mAg.UpdatePosition ();
		}

		/*
		RaycastHit hit;
		Ray ray = new Ray (transform.position + Vector3.up*UP_OFS, mAg.position - transform.position);
		if (Physics.Raycast (ray, out hit)) {
			transform.position = hit.point;
		}
		*/
		return mAg.position;
	}

	private void prepareParam(){
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
		mAg.CalculatePath(transform.position,destTr.position);
	}

	public void SetDestination(Vector3 dest)
	{
		mAg.layerMask = NpcGenerator.GetAllLayerMask();
		mDestinationPos = dest;
		m_moveTargetPos = dest;
		m_hasMoveTargetPos = true;
		mTargetTr = null;
		mAIUpdateFlag = true;
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
	}
	private void SM_addNaviLayer(string _layerStr){
		int layer = NavMesh.GetNavMeshLayerFromName(_layerStr);
		if (layer >= 0) {
			mAg.layerMask |= (1 << layer);
		} else {
			Debug.Log("warning: bad layer name.");
		}
	}
	private void SM_removeNaviLayer(string _layerStr){
		int layer = NavMesh.GetNavMeshLayerFromName(_layerStr);
		if (layer >= 0) {
			mAg.layerMask &= ~(1 << layer);
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

}
