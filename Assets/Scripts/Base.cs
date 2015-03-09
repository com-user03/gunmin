using UnityEngine;
using System.Collections;

public class Base : MonoBehaviour {

	[SerializeField]
	private bool m_isEnemy;
	[SerializeField]
	private int m_npcSpawnNum;
	public int NpcSpawnNum { get { return m_npcSpawnNum; } }
	[SerializeField]
	private string m_npcSpawnName;
	public string NpcSpawnName { get { return m_npcSpawnName; } }
	[SerializeField]
	private int m_hp;

	private NpcGenerator.NpcSpriteInfo m_spawnNpcInfo;
	public NpcGenerator.EquipType GetNpcEquipType()
	{
		return m_spawnNpcInfo.type;
	}
	private float m_spawnDelayTime;

	public delegate void BaseDestroyedDelegate(GameObject destroyedBase);
	public static event BaseDestroyedDelegate BaseDestroyedEvent;

	// Use this for initialization
	void Start()
	{
		m_spawnDelayTime = 0;
	}

	// Update is called once per frame
	void Update()
	{
		m_spawnDelayTime += Time.deltaTime;
	}

	public void Initialize(NpcGenerator.NpcSpriteInfo spawnNpcInfo)
	{
		m_spawnNpcInfo = spawnNpcInfo;
	}

	public void AttackMe(int damage)
	{
		m_hp -= damage;

		if (m_hp <= 0)
		{
			BaseDestroyedEvent(this.gameObject);
			Destroy(this.gameObject);
		}
	}

	public bool IsReadyToSpawn()
	{
		return (m_spawnDelayTime >= m_spawnNpcInfo.spawnTime);
	}

	public void ResetNpcSpawnDelayTime()
	{
		m_spawnDelayTime = 0;
	}
}
