using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public List<GameObject> EnemyBaseList;
	public GameObject PlayerBase;

	public GameObject GetNearestBase(Vector3 npcPos)
	{
		return EnemyBaseList[0];
	}

	public void OnBaseDestroyed(GameObject destroyedBase)
	{

	}
}
