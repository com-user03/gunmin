using UnityEngine;
using System.Collections;

public class Base : MonoBehaviour {

	[SerializeField]
	private bool m_isEnemy;
	[SerializeField]
	private float m_enemySpawnTime;
	[SerializeField]
	private int m_enemySpawnNum;
	[SerializeField]
	private string m_enemySpawnName;
	[SerializeField]
	private int m_hp;

	public delegate void BaseDestroyedDelegate(GameObject destroyedBase);
	public static event BaseDestroyedDelegate BaseDestroyedEvent;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
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
}
