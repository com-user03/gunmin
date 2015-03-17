using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NpcGenerator : MonoBehaviour {
	[System.Serializable]
	public struct NpcGroupInfo{
		public string name;
		public Color color;
		public Transform spawnTr;
		public Transform destTr;
		public NpcTeam team;
		public string naviLayerStr;
	}

	[System.Serializable]
	public class NpcSpriteInfo{
		public string name;
		public GameObject basePrefab;
		public Sprite sprite;
		public NpcGenerator.NpcTeam team;
		public NpcGenerator.EquipType type;
		public int hp;
		public float spawnTime;
	}
	
	private const int TEAM_NUM = 2;
	public enum NpcTeam{ Red=0, Blue=1 }
	public const int PLAYER_TEAM_NUM = 0;
	public const int ENEMY_TEAM_NUM = 1;
	public enum EquipType{
		Trooper  = 0,
		Archer   = 1,
		Guardian = 2,
		Trooper2 = 3,
		Trooper3 = 4,
	}
	public static string[] nameArr=new string[TEAM_NUM]{"npcRed","npcBlue"};
	public static string[] navLayerArr = new string[4] { "Walkable", "NavLayerHill", "NavLayerBridgeRed", "NavLayerBridgeBlue" };

	public int spawnNum;
	public int liveMax;
    private const float SPAWN_DELAY_TIME = 0.1f;
    private float spawnDelay = 0.0f;
	private GameObject mNpcPrefab;
	public NpcGroupInfo[] npcGpInfo;
	public NpcSpriteInfo[] npcSpriteInfo;

	private List<GameObject>[] mNpcListArr;
	//private int[] mSpawnCnt;
	//private EquipType[] mEquipType;

    private bool m_isButtonHeld;
    private EquipType m_npcToSpawn;

	public List<GameObject> EnemyBaseList;
	public GameObject PlayerBase;

	void Awake(){
	}

	// Use this for initialization
	void Start () {
		//if (spawnNum <= 0) { spawnNum = 1; }
		if (liveMax <= 10) { liveMax = 10; }

		mNpcListArr = new List<GameObject>[nameArr.Length];
		//mSpawnCnt = new int[nameArr.Length];
		//mEquipType = new EquipType[nameArr.Length];
		for (int ii = 0; ii < nameArr.Length; ++ii) {
			mNpcListArr [ii] = new List<GameObject>();
			//mSpawnCnt[ii] = spawnNum;
			//mEquipType[ii] = EquipType.Trooper;
		}

		foreach (GameObject baseObj in EnemyBaseList)
		{
			Base enemyBase = baseObj.GetComponent<Base>();
			if (enemyBase)
			{
				string npcToSpawnName = enemyBase.NpcSpawnName;
				NpcSpriteInfo info = System.Array.Find(npcSpriteInfo, x => x.name == npcToSpawnName);
				enemyBase.Initialize(info);
			}
		}

		Base.BaseDestroyedEvent += OnBaseDestroyed;
	}
	
	// Update is called once per frame
	void Update ()
	{
		foreach (GameObject baseObj in EnemyBaseList)
		{
			Base enemyBase = baseObj.GetComponent<Base>();
			if (enemyBase && enemyBase.IsReadyToSpawn())
			{
				for (int i = 0; i < enemyBase.NpcSpawnNum; ++i)
				{
					if (npcGpInfo[ENEMY_TEAM_NUM].destTr && mNpcListArr[ENEMY_TEAM_NUM].Count <= liveMax)
					{
						spawnNpc(npcGpInfo[ENEMY_TEAM_NUM].team, enemyBase.GetNpcEquipType(), enemyBase.transform.position, npcGpInfo[ENEMY_TEAM_NUM].destTr);
					}
				}
				enemyBase.ResetNpcSpawnDelayTime();
			}
		}

		if (m_isButtonHeld)
		{
			if (spawnDelay >= SPAWN_DELAY_TIME && PlayerBase)
			{
				Transform destTr = EnemyBaseList[Random.Range(0, EnemyBaseList.Count)].transform;
				spawnNpc(npcGpInfo[PLAYER_TEAM_NUM].team, m_npcToSpawn, PlayerBase.transform.position, destTr);
				spawnDelay = 0;
			}
			else
			{
				spawnDelay += Time.deltaTime;
			}
		}
	}

    public void Button1Pressed()
    {
        m_isButtonHeld = true;
        m_npcToSpawn = EquipType.Trooper;
    }

    public void Button2Pressed()
    {
        m_isButtonHeld = true;
        m_npcToSpawn = EquipType.Archer;
    }

    public void Button3Pressed()
    {
        m_isButtonHeld = true;
        m_npcToSpawn = EquipType.Guardian;
    }

    public void Button4Pressed()
    {
        m_isButtonHeld = true;
        m_npcToSpawn = EquipType.Trooper2;
    }

    public void Button5Pressed()
    {
        m_isButtonHeld = true;
        m_npcToSpawn = EquipType.Trooper3;
    }

    public void ButtonReleased()
    {
        m_isButtonHeld = false;
    }

	public List<GameObject> GetEnemyListByMyName(string _name){
		return GetEnemyListByTeam (_name == nameArr [0] ? NpcTeam.Red : NpcTeam.Blue);
	}
	public List<GameObject> GetEnemyListByTeam(NpcTeam _team){
		List<GameObject> retArr = new List<GameObject>();
		int teamId = (int)_team;
		if (teamId < nameArr.Length) {
			for(int ii = 0; ii < mNpcListArr.Length; ++ii){
				if(ii != teamId){
					retArr.AddRange(mNpcListArr[ii]);
				}
			}
		}
		return retArr;
	}

	private void spawnNpc(NpcTeam _team, EquipType _equipType, Vector3 spawnPosition, Transform destTr)
	{
		int id = (int)_team;
		int sprInfoId = (int)_equipType * TEAM_NUM + id;
		mNpcPrefab = npcSpriteInfo [sprInfoId].basePrefab;
		if (mNpcPrefab == null) {
			return;
		}

		GameObject npc = GameObject.Instantiate (mNpcPrefab) as GameObject;
		npc.name = nameArr [id];
		//npc.transform.position = npcGpInfo[id].spawnTr.position;
		npc.transform.position = spawnPosition;
        npc.SendMessage("SM_initializeNpcSprite");
		npc.SendMessage ("SM_setGenerator", gameObject);
		//npc.SendMessage ("SM_addNaviLayer", npcGpInfo[id].naviLayerStr);
		npc.SendMessage ("SM_setDest", destTr);
		npc.SendMessage("SM_setSpriteInfo", npcSpriteInfo[sprInfoId]);
		//npc.SendMessage("SM_setColor", npcGpInfo [id].color);
		SetNpcNaviLayer(npc);

		npc.SendMessage("SM_setHP", npcSpriteInfo[sprInfoId].hp);

		// add to list
		mNpcListArr[id].Add(npc);
	}

	private void SetNpcNaviLayer(GameObject npc)
	{
		npc.SendMessage("SM_addNaviLayer", "Walkable");

		foreach (string layerStr in navLayerArr)
		{
			if (layerStr == "Walkable" || Random.value < 0.5f)
			{
				npc.SendMessage("SM_addNaviLayer", layerStr);
			}
		}
	}

	public static int GetAllLayerMask()
	{
		int allLayerMask = 0;
		foreach (string layerStr in navLayerArr)
		{
			int layer = NavMesh.GetAreaFromName(layerStr);
			allLayerMask |= (1 << layer);
		}
		return allLayerMask;
	}

	private void OnBaseDestroyed(GameObject destroyedBase)
	{
		if (destroyedBase)
		{
			EnemyBaseList.Remove(destroyedBase);
		}
	}

	//--------------------
	private void SM_removeNpc(GameObject _npcObj){
		for (int ii = 0; ii < nameArr.Length; ++ii) {
			if(_npcObj.name==nameArr[ii]){
				for(int jj = 0; jj < mNpcListArr[ii].Count; ++jj){
					if(mNpcListArr[ii][jj]==_npcObj){
						mNpcListArr[ii].Remove(mNpcListArr[ii][jj]);
						break;
					}
				}
			}
		}
	}
}
