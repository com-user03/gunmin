using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour {

    [SerializeField]
    private NpcGenerator m_npcGenerator;

    [SerializeField]
    private GameObject m_selectionCircleObj;

	[SerializeField]
	private GameObject m_npcSelectedObj;

	[SerializeField]
	private GameObject m_selectionMoveObj;

    [SerializeField]
    private GameObject m_moveArrowObj;

    private Vector3 m_selectPosOrigin;
    private Vector3 m_selectPosDestination;
	private const float MIN_MOVE_DISTANCE = 10;
	private float m_canvasReferenceResolutionRatio;

	private List<GameObject> m_selectedObjects;
	public static bool IsNpcSelected;

	// Use this for initialization
	void Start () {
        m_selectionCircleObj = Instantiate(m_selectionCircleObj) as GameObject;
        m_selectionCircleObj.transform.SetParent(this.transform, false);
        m_selectionCircleObj.SetActive(false);
        m_moveArrowObj = Instantiate(m_moveArrowObj) as GameObject;
        m_moveArrowObj.transform.SetParent(this.transform, false);
        m_moveArrowObj.SetActive(false);
		m_selectionMoveObj = Instantiate(m_selectionMoveObj) as GameObject;
		m_selectionMoveObj.transform.SetParent(this.transform, false);
		m_selectionMoveObj.SetActive(false);

		// screen -> canvas UI計算の為
		m_canvasReferenceResolutionRatio = this.GetComponent<CanvasScaler>().referenceResolution.x / Screen.width;
	}
	
	// Update is called once per frame
	void Update () {
#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER)

		Vector3 mousePos = Input.mousePosition;

        // 移動機能（マウス）
        if (Input.GetMouseButtonDown(0))
        {
			DisplaySelectionCircle(mousePos);
			SelectNPC(mousePos);
        }
		else if (Input.GetMouseButton(0))
		{
			UpdateMoveArrow(mousePos);
		}
        else if (Input.GetMouseButtonUp(0))
        {
			MoveNPC(mousePos);
			HideMoveArrow();
			ShowMoveSelectedUI(m_selectPosDestination);
			HideSelectionCircle();
        }
#endif

#if !(UNITY_STANDALONE && UNITY_WEBPLAYER)
        // 携帯端末のnpc選択機能
		if (Input.touchCount == 1)
        {
            Vector3 touchPos = Input.GetTouch(0).position;

			if (Input.GetTouch(0).phase == TouchPhase.Began)
			{
				DisplaySelectionCircle(touchPos);
				SelectNPC(touchPos);
			}
			else if (m_selectedObjects.Count > 0)
			{
				if (Input.GetTouch(0).phase == TouchPhase.Moved)
				{
					UpdateMoveArrow(touchPos);
				}
				else if (Input.GetTouch(0).phase == TouchPhase.Ended)
				{
					MoveNPC(touchPos);
					HideMoveArrow();
					ShowMoveSelectedUI(m_selectPosDestination);
				}
			}

			if (Input.GetTouch(0).phase == TouchPhase.Ended)
			{
				HideSelectionCircle();
			}
        }
#endif
	}

	private void SelectNPC(Vector3 selectPos)
	{
		//　npcを選択する
		m_selectPosOrigin = selectPos;
		m_selectedObjects = new List<GameObject>();
		List<GameObject> npcList = m_npcGenerator.GetEnemyListByMyName(NpcGenerator.nameArr[1]);
		RectTransform selectCircle = m_selectionCircleObj.GetComponent<RectTransform>();
		foreach (GameObject npcObj in npcList)
		{
			Vector3 npcScreenPos = Camera.main.WorldToScreenPoint(npcObj.transform.position);
			npcScreenPos.z = 0;
			float distance = (npcScreenPos - m_selectPosOrigin).sqrMagnitude;
			float selectRadius = selectCircle.rect.width * 2.5f;
			if (distance <= selectRadius)
			{
				m_selectedObjects.Add(npcObj);

				//　選択したnpcのＵＩ表示
				npcObj.GetComponent<NpcBase>().SetSelectedUI(m_npcSelectedObj);
			}
		}

		if (m_selectedObjects.Count > 0)
		{
			IsNpcSelected = true;
		}
	}

	private void UpdateMoveArrow(Vector3 movePos)
	{
		float moveDistance = Vector3.Distance(movePos, m_selectPosOrigin);
		if (moveDistance > MIN_MOVE_DISTANCE)
		{
			// 矢印の表示
			DisplayMoveArrow(m_selectPosOrigin);

			RectTransform rectTr = m_moveArrowObj.GetComponent<RectTransform>();
			float angle = Vector3.Angle(Vector3.up, movePos - m_selectPosOrigin);
			angle *= Mathf.Sign(Vector3.Cross(Vector3.up, movePos - m_selectPosOrigin).z);
			rectTr.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
			rectTr.sizeDelta = new Vector2(rectTr.sizeDelta.x, moveDistance * m_canvasReferenceResolutionRatio / rectTr.localScale.x);
		}
	}

	private void MoveNPC(Vector3 targetPos)
	{
		m_selectPosDestination = targetPos;

		foreach (GameObject npc in m_selectedObjects)
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(m_selectPosDestination);
			if (Physics.Raycast(ray, out hit))
			{
				npc.GetComponent<NpcBase>().SetDestination(hit.point);
			}
			npc.GetComponent<NpcBase>().RemoveSelectedUI();
		}

		IsNpcSelected = false;
	}

    private void DisplaySelectionCircle(Vector3 position)
    {
        m_selectionCircleObj.SetActive(true);
        m_selectionCircleObj.GetComponent<RectTransform>().position = position;
    }

    private void HideSelectionCircle()
    {
        m_selectionCircleObj.SetActive(false);
    }

    private void DisplayMoveArrow(Vector3 position)
    {
        m_moveArrowObj.SetActive(true);
        m_moveArrowObj.GetComponent<RectTransform>().position = position;
    }

    private void HideMoveArrow()
    {
        m_moveArrowObj.SetActive(false);
    }

	private void ShowMoveSelectedUI(Vector3 position)
	{
		m_selectionMoveObj.SetActive(true);
		m_selectionMoveObj.GetComponent<RectTransform>().position = position;
	}
}
