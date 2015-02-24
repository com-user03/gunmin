using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Npc : MonoBehaviour {
	public enum EquipType{
		Trooper,
		Archer,
		Guardian,
        Trooper2,
        Trooper3,
	}
	public enum NaviLayer{
		BrigeRed =  (1 << 6),
		BrigeBlue = (1 << 7),
	}
	private const int NaviLayer_layerBrigeRed = (1 << 6);
	private const int NaviLayer_layerBrigeBlue = (1 << 7);
	public const int NaviLayer_layerHill = (1 << 8);
	private NavMeshAgent mAg;
	public GameObject arrowPrefab;
	public Transform destTr;
	private Transform mTempDestTr;
	private float mDefSpeed;
	private EquipType mEquipType;
	private NpcGenerator mGenScr;
	private float mCkDistMin,mCkDistMax;

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
		mAg = this.GetComponent<NavMeshAgent>();
		mAg.enabled = false;

        m_mainCamera = Camera.main;
	}

	// Use this for initialization
	void Start () {
		mAg.enabled = true;
		//-------------------------
		mDefSpeed = mAg.speed;
		switch(mEquipType){
		default:                 break;
		case EquipType.Trooper:  mAg.speed = mDefSpeed * 1.3f;  break;
		case EquipType.Archer:   mAg.speed = mDefSpeed * 1.0f;  break;
		case EquipType.Guardian: mAg.speed = mDefSpeed * 0.7f;  break;
		}
		//-------------------------

        switch (mEquipType)
        {
            default:
                mCkDistMin = 0f;
                mCkDistMax = 2.0f;
                break;
            case EquipType.Trooper:
                mCkDistMin = 0f;
                mCkDistMax = 2.0f;
                m_npcSpriteObject.gameObject.GetComponent<SpriteRenderer>().sprite = (m_isEnemy) ? m_npcSprites[3] : m_npcSprites[0];
                break;
            case EquipType.Archer:
                mCkDistMin = 4f;
                mCkDistMax = 6f;
                m_npcSpriteObject.gameObject.GetComponent<SpriteRenderer>().sprite = (m_isEnemy) ? m_npcSprites[3] : m_npcSprites[1];
                break;
            case EquipType.Guardian:
                mCkDistMin = 0f;
                mCkDistMax = 0.5f;
                m_npcSpriteObject.gameObject.GetComponent<SpriteRenderer>().sprite = (m_isEnemy) ? m_npcSprites[3] : m_npcSprites[2];
                break;
            case EquipType.Trooper2:
                mCkDistMin = 0f;
                mCkDistMax = 2.0f;
                this.transform.GetChild(SPRITE_CHILD_INDEX).gameObject.GetComponent<SpriteRenderer>().sprite = (m_isEnemy) ? m_npcSprites[3] : m_npcSprites[4];
                break;
            case EquipType.Trooper3:
                mCkDistMin = 0f;
                mCkDistMax = 2.0f;
                this.transform.GetChild(SPRITE_CHILD_INDEX).gameObject.GetComponent<SpriteRenderer>().sprite = (m_isEnemy) ? m_npcSprites[3] : m_npcSprites[5];
                break;
        }

        m_npcSpriteObject.transform.position = this.transform.position;

		mDefSpeed = mAg.speed;
		if (mAg.pathStatus != NavMeshPathStatus.PathInvalid) {
			if ((mAg.gameObject.activeSelf)&&(destTr!=null)) {
				mAg.SetDestination (destTr.position);
				mAg.speed = mDefSpeed*(0.8f+Random.value*0.4f);
			}
		}

        UnitCreatedEvent();
	}
	
	// Update is called once per frame
	void Update () {
		if (Random.value < 0.01f) {
			mTempDestTr = getNearestEm(transform.position,mCkDistMin,mCkDistMax);
			if (mTempDestTr != null) {
                mAg.stoppingDistance = 0;
				switch(mEquipType){
				default:
					mAg.SetDestination (mTempDestTr.position);
					break;
				case EquipType.Trooper:
					mAg.SetDestination (mTempDestTr.position);
					break;
				case EquipType.Archer:
					mAg.SetDestination (transform.position);
					if (Random.value < 0.2f) {
						setArrow(mTempDestTr);
					}
					break;
				case EquipType.Guardian:
					mAg.SetDestination (mTempDestTr.position);
					break;
				}
			}
            else
            {
                mAg.SetDestination(destTr.position);
                mAg.stoppingDistance = 0.75f;
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
		if (Random.value < 0.05f) { // 再検索頻度を下げる 
			if (mTempDestTr != null) {
				mAg.CalculatePath (mTempDestTr.position, mAg.path);
				/*Vector3 nowPos = transform.position; //mAg.updatePosition;
			Vector3 nextPos = nowPos + (mAg.nextPosition - nowPos) * 1000f;
			Debug.DrawLine (nowPos, nextPos, Color.red);*/
			}
		}
		

        //カメラに向かう
        Vector3 lookAtPos = this.transform.position + m_mainCamera.transform.rotation * Vector3.forward;
        lookAtPos.y = 0;
        this.transform.LookAt(lookAtPos, Vector3.up);
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

	private void setArrow(Transform _tgtTr){
		Vector3 _pos = _tgtTr.transform.position;
		float agentSpd=0f;
		NavMeshAgent na = mTempDestTr.gameObject.GetComponent<NavMeshAgent>();
		if(na!=null){
			agentSpd = na.velocity.magnitude;
		}
		Vector3 _spd = mTempDestTr.forward.normalized * agentSpd;

		float spd = 6f;
		float[] time = TmMath.CollideTime (_pos, _spd, transform.position, spd);
		if (time != null) {
//			Debug.Log ("reachTIme=" + time [0]);
			Vector3 futurePos = _pos+_spd*time[0];
			Vector3 spdVec = (futurePos-transform.position).normalized * spd;
			float diffY = _pos.y - transform.position.y;
			spdVec.y += TmMath.ParabolicSpeed(time[0],diffY,Physics.gravity.y);
			GameObject bulletGo = GameObject.Instantiate (arrowPrefab) as GameObject;
			bulletGo.transform.position = transform.position;
			bulletGo.transform.rotation = Quaternion.FromToRotation(Vector3.forward,spdVec.normalized);
			bulletGo.rigidbody.velocity=Vector3.zero;
			bulletGo.rigidbody.AddForce(spdVec,ForceMode.VelocityChange);
			Destroy (bulletGo, 2f);
		}
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
	private void SM_addNaviLayer(int _layer){
		mAg.walkableMask |= _layer;
	}
	private void SM_setEquipType(EquipType _equipType){
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
}
