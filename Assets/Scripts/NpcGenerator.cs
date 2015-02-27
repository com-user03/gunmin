using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NpcGenerator : MonoBehaviour {
	public enum EquipType{
		Trooper,
		Archer,
		Guardian,
		Trooper2,
		Trooper3,
	}
	[System.Serializable]
	public struct NpcGroupInfo{
		public string name;
		public Color color;
		public Transform spawnTr;
		public Transform destTr;
		public NpcTeam team;
		public string naviLayerStr;
	}
	private const int TEAM_NUM = 2;
	public enum NpcTeam{ Red=0, Blue=1 }
	private static string[] nameArr;
	//private static string[,] typeNameArr=new string[3,2]{{"赤兵","青兵"},{"赤弓兵","青弓兵"},{"赤重兵","青重兵"}};
    
    //private int m_guiButtonWidth;
    //private int m_guiButtonHeight;
    //private const float GUI_BUTTON_WIDTH_SCREEN_PERCENT = 0.1f;
    //private const float GUI_BUTTON_HEIGHT_SCREEN_PERCENT = 0.033f;
    //private const int GUI_BUTTON_SPACING = 10;

	public int spawnNum;
	public int liveMax;
    private const float SPAWN_DELAY_TIME = 0.1f;
    private float spawnDelay = 0.0f;
	public GameObject npcPrefab;
	public NpcGroupInfo[] npcGpInfo;

	private List<GameObject>[] mNpcListArr;
	private int[] mSpawnCnt;
	private EquipType[] mEquipType;

    private bool m_isButtonHeld;
    private EquipType m_npcToSpawn;
    private int m_npcToSpawnTeamIndex;

	void Awake(){
		nameArr=new string[TEAM_NUM]{"npcRed","npcBlue"};
	}

	// Use this for initialization
	void Start () {
		if (spawnNum <= 0) { spawnNum = 1; }
		if (liveMax <= 10) { liveMax = 10; }

		mNpcListArr = new List<GameObject>[nameArr.Length];
		mSpawnCnt = new int[nameArr.Length];
		mEquipType = new EquipType[nameArr.Length];
		for (int ii = 0; ii < nameArr.Length; ++ii) {
			mNpcListArr [ii] = new List<GameObject>();
			mSpawnCnt[ii] = spawnNum;
			mEquipType[ii] = EquipType.Trooper;
		}

        //m_guiButtonWidth = System.Convert.ToInt32(Screen.width * GUI_BUTTON_WIDTH_SCREEN_PERCENT);
        //m_guiButtonHeight = System.Convert.ToInt32(Screen.width * GUI_BUTTON_HEIGHT_SCREEN_PERCENT);
	}
	
	// Update is called once per frame
	void Update () {

        if (m_isButtonHeld)
        {
            PrepareNpc(m_npcToSpawn, m_npcToSpawnTeamIndex);

            // （仮）CPUチームのユニットを作る
            PrepareNpc(m_npcToSpawn, m_npcToSpawnTeamIndex + 1);
        }

        if (spawnDelay >= SPAWN_DELAY_TIME)
        {
            //if (Random.value < 0.1f) {
            for (int ii = 0; ii < nameArr.Length; ++ii)
            {
                if (mSpawnCnt[ii] > 0)
                {
                    mSpawnCnt[ii]--;
                    spawnNpc(npcGpInfo[ii].team, mEquipType[ii]);
                }
            }
            //}
            spawnDelay = 0;
        }
        else
        {
            spawnDelay += Time.deltaTime;
        }
	}

	/*private void OnGUI()
    {
		for (int ii = 1; ii < nameArr.Length; ++ii)
        {
			if(mSpawnCnt[ii]<=0)
            {
				//if(mNpcListArr[ii].Count < liveMax)
                {
                    if (GUI.RepeatButton(new Rect((ii - 1) * (m_guiButtonWidth + GUI_BUTTON_SPACING) + 10, GUI_BUTTON_SPACING, m_guiButtonWidth, m_guiButtonHeight), typeNameArr[0, ii]))
                    {
                        PrepareNpc(EquipType.Trooper, ii);
					}
                    else if (GUI.RepeatButton(new Rect((ii - 1) * (m_guiButtonWidth + GUI_BUTTON_SPACING) + 10, m_guiButtonHeight + GUI_BUTTON_SPACING * 1.5f, m_guiButtonWidth, m_guiButtonHeight), typeNameArr[1, ii]))
                    {
                        PrepareNpc(EquipType.Archer, ii);
					}
                    else if (GUI.RepeatButton(new Rect((ii - 1) * (m_guiButtonWidth + GUI_BUTTON_SPACING) + 10, m_guiButtonHeight * 2 + GUI_BUTTON_SPACING * 2, m_guiButtonWidth, m_guiButtonHeight), typeNameArr[2, ii]))
                    {
                        PrepareNpc(EquipType.Guardian, ii);
					}
				//}
			//}
		}
	}*/

    private void PrepareNpc(EquipType npcType, int teamIndex)
    {
        mEquipType[teamIndex] = npcType;
        mSpawnCnt[teamIndex] = Mathf.Min(spawnNum, liveMax - mNpcListArr[teamIndex].Count);
    }

    public void Button1Pressed()
    {
        m_isButtonHeld = true;
        m_npcToSpawn = EquipType.Trooper;
        m_npcToSpawnTeamIndex = 0;
    }

    public void Button2Pressed()
    {
        m_isButtonHeld = true;
        m_npcToSpawn = EquipType.Archer;
        m_npcToSpawnTeamIndex = 0;
    }

    public void Button3Pressed()
    {
        m_isButtonHeld = true;
        m_npcToSpawn = EquipType.Guardian;
        m_npcToSpawnTeamIndex = 0;
    }

    public void Button4Pressed()
    {
        m_isButtonHeld = true;
        m_npcToSpawn = EquipType.Trooper2;
        m_npcToSpawnTeamIndex = 0;
    }

    public void Button5Pressed()
    {
        m_isButtonHeld = true;
        m_npcToSpawn = EquipType.Trooper3;
        m_npcToSpawnTeamIndex = 0;
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


	private void spawnNpc(NpcTeam _team, EquipType _equipType){
		int id = (int)_team;
		GameObject npc = GameObject.Instantiate (npcPrefab) as GameObject;
		npc.name = nameArr [id];
		npc.transform.position = npcGpInfo[id].spawnTr.position + Vector3.up * 0.5f;
        npc.SendMessage("SM_initializeNpcSprite");
		npc.SendMessage ("SM_setGenerator", gameObject);
		npc.SendMessage ("SM_addNaviLayer", npcGpInfo[id].naviLayerStr);
		npc.SendMessage ("SM_setDest", npcGpInfo[id].destTr);
		npc.SendMessage ("SM_setEquipType", _equipType);
		npc.SendMessage("SM_setTeam", npcGpInfo[id].team);
        //npc.SendMessage("SM_setColor", npcGpInfo [id].color);

		// add to list
		mNpcListArr[id].Add(npc);

		npc.SendMessage ("SM_addNaviLayer","Default");
		if (Random.value < 0.5f) {
			npc.SendMessage ("SM_addNaviLayer","layerHill");
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
