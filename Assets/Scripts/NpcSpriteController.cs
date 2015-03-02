using UnityEngine;
using System.Collections;

public class NpcSpriteController : MonoBehaviour {

	[SerializeField]
	private GameObject m_spriteObject;
	private GameObject m_selectedUI;

	void LateUpdate()
	{
		//カメラに向かう
		Vector3 lookAtPos = this.transform.position + Camera.main.transform.rotation * Vector3.forward;
		lookAtPos.y = 0;
		this.transform.LookAt(lookAtPos, Vector3.up);
	}

	public void SetSprite(Sprite sprite)
	{
		m_spriteObject.GetComponent<SpriteRenderer>().sprite = sprite;
	}

	public void IsHit()
	{
		m_spriteObject.GetComponent<Animator>().SetBool("IsHit", true);
	}

	public void IsDead()
	{
		m_spriteObject.GetComponent<Animator>().SetBool("IsDead", true);
	}

    void RemoveMe()
    {
		this.transform.parent.SendMessage ("SM_removeMe");
        Destroy(this.transform.parent.gameObject);
    }
}
