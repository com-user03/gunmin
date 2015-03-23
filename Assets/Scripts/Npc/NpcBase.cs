using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class NpcBase : MonoBehaviour {
	private Bounds VIABLE_AREA{get{
		return new Bounds(Vector3.zero,Vector3.one*10f);
	}}
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

	protected int m_maxHP;
	protected int m_hp;
	protected NpcBase m_enemyTarget;
	private const float CHECK_ATTACK_TIME = 1.0f;
	protected float m_checkAttackTime;

	// for myAgent
	protected MyAgent mAg;
	
//	[HideInInspector]
	public TagMask traversableTags = new TagMask (-1,-1);

	public GameObject NpcSpriteContainer;
	private GameObject m_npcSpriteObject;
	private GameObject m_selectedUI;

	public delegate void UnitCreatedDelegate();
	public static event UnitCreatedDelegate UnitCreatedEvent;
	public delegate void UnitRemovedDelegate();
	public static event UnitRemovedDelegate UnitRemovedEvent;
	
	virtual public void Awake(){
		mAg = new MyAgent (gameObject,true);
	}
	
	// Use this for initialization
	virtual public void Start () {
		mTargetTr = null;
		mAIUpdateFlag = false;
		prepareParam ();
		UnitCreatedEvent();
		m_checkAttackTime = 0;
		mCkDistMin = 0f;
		mCkDistMax = 2.0f;
	}
	
	// Update is called once per frame
	virtual public void Update () {
		if (!VIABLE_AREA.Contains (transform.position)) {
			SM_removeMe();
			return;
		}
		
		if (mAg.corners.Length == 0) {
			NavMeshHit hit;
			Vector3 tmpPos = transform.position;
			Vector3 tmpOfs = Vector3.up*1f;
//			if(NavMesh.Raycast(tmpPos-tmpOfs,tmpPos+tmpOfs,out hit,mAg.layerMask)){
//				tmpPos = hit.position;
//			}
			mAg.CalculatePath(tmpPos,mDestinationPos);
		}
		else if (m_hasMoveTargetPos && (transform.position - mDestinationPos).magnitude < 0.1f)
		{
			m_hasMoveTargetPos = false;
			mAIUpdateFlag = true;
		}

		checkAttack();

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

#if UNITY_EDITOR
		if (mAg != null) {
			mAg.debugCourseDisp (Color.red, Color.blue);
		}
#endif
	}
	
	public void KillMe()
	{
		NpcSpriteContainer.GetComponent<NpcSpriteController>().IsDead();
	}

	private NpcBase getNearestEm(Vector3 _pos, float _minDist, float _maxDist)
	{
		List<GameObject> goList = mGenScr.GetEnemyListByTeam (mTeam);
		foreach (GameObject go in goList) {
			if(go!=this.gameObject){
				float distSq = (go.transform.position-transform.position).sqrMagnitude;
				if((distSq < _maxDist*_maxDist)&&(distSq > _minDist*_minDist)){
					return go.GetComponent<NpcBase>();
				}
			}
		}
		
		return null;
	}

	public virtual void AttackMe(int damage)
	{
		m_hp -= damage;
		NpcSpriteContainer.GetComponent<NpcSpriteController>().IsHit();

		if (m_hp <= 0)
		{
			KillMe();
		}
	}

	protected virtual void checkAttack()
	{
		if (m_checkAttackTime >= CHECK_ATTACK_TIME)
		{
			m_checkAttackTime = 0;
			if (m_enemyTarget)
			{
				if (m_enemyTarget && (transform.position - m_enemyTarget.transform.position).magnitude < 0.2f)
				{
					m_enemyTarget.AttackMe(Random.Range(0, 2));
					if (!m_enemyTarget)
					{
						mAIUpdateFlag = true;
					}
				}
			}
			else if (destTr && (transform.position - destTr.position).sqrMagnitude < 2f)
			{
				// ベースを攻撃する
				destTr.gameObject.GetComponent<Base>().AttackMe(1);
			}
		}
		else
		{
			m_checkAttackTime += Time.deltaTime;
		}
	}

	protected virtual bool updateAI(){
		Transform tempDestTr;
		m_enemyTarget = getNearestEm(transform.position,mCkDistMin,mCkDistMax);
		tempDestTr = (m_enemyTarget) ? m_enemyTarget.transform : null;

		if (tempDestTr != null) {
			mTargetTr = tempDestTr;
			mDestinationPos = tempDestTr.position;
		}
		else if (m_hasMoveTargetPos)
		{
			mDestinationPos = m_moveTargetPos;
		}
		else if (destTr)
		{
			mDestinationPos = destTr.position;
			mNextPos = destTr.position;
		}
		return true;
	}
	
	protected virtual Vector3 updatePosition(Vector3 _nextPos){
		Vector3 dir = mAg.position - transform.position;
		dir.y = 0f;
//		gameObject.GetComponent<Rigidbody> ().velocity = dir/Time.deltaTime;
		gameObject.GetComponent<Rigidbody>().MovePosition (transform.position + dir);
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
		mAg.AddNaviLayer (_layerStr);
	}
	private void SM_removeNaviLayer(string _layerStr){
		mAg.RemoveNaviLayer (_layerStr);
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

	private void SM_setHP(int maxHP)
	{
		m_maxHP = maxHP;
		m_hp = maxHP;
	}
}
