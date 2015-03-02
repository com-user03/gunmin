using UnityEngine;
using System.Collections;

public class NpcSprite : MonoBehaviour {

	void RemoveMe()
	{
		this.transform.parent.SendMessage("RemoveMe");
	}

	void EndHit()
	{
		this.GetComponent<Animator>().SetBool("IsHit", false);
	}
}
