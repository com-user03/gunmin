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

    private string m_playerTeamName;
    private Vector3 m_selectPosOrigin;
    private Vector3 m_selectPosDestination;
	private const float MIN_MOVE_DISTANCE = 10;
	private float m_canvasReferenceResolutionRatio;

	private List<GameObject> m_selectedObjects;

	// Use this for initialization
	void Start () {
        m_playerTeamName = m_npcGenerator.npcGpInfo[0].name;

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

        // ズーム機能（マウス）
        if (Input.mouseScrollDelta.y != 0)
        {
        }

        // 移動機能（マウス）
        if (Input.GetMouseButtonDown(0))
        {
            DisplaySelectionCircle(mousePos);
            m_selectPosOrigin = mousePos;

			//　npcを選択する
			m_selectedObjects = new List<GameObject>();
			List<GameObject> npcList = m_npcGenerator.GetEnemyListByMyName(NpcGenerator.nameArr[1]);
			RectTransform selectCircle = m_selectionCircleObj.GetComponent<RectTransform>();
			foreach (GameObject npcObj in npcList)
			{
				Vector3 npcScreenPos = Camera.main.WorldToScreenPoint(npcObj.transform.position);
				npcScreenPos.z = 0;
				float distance = (npcScreenPos - m_selectPosOrigin).sqrMagnitude;
				float selectRadius = selectCircle.rect.width;
				if (distance <= selectRadius)
				{
					m_selectedObjects.Add(npcObj);

					//　選択したnpcのＵＩ表示
					npcObj.GetComponent<NpcBase>().SetSelectedUI(m_npcSelectedObj);
				}
			}
        }
		else if (Input.GetMouseButton(0))
		{
			float moveDistance = Vector3.Distance(mousePos, m_selectPosOrigin);
			if (moveDistance > MIN_MOVE_DISTANCE)
			{
				// 矢印の表示
				DisplayMoveArrow(m_selectPosOrigin);

				RectTransform rectTr = m_moveArrowObj.GetComponent<RectTransform>();
				float angle = Vector3.Angle(Vector3.up, mousePos - m_selectPosOrigin);
				angle *= Mathf.Sign(Vector3.Cross(Vector3.up, mousePos - m_selectPosOrigin).z);
				rectTr.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
				rectTr.sizeDelta = new Vector2(rectTr.sizeDelta.x, moveDistance * m_canvasReferenceResolutionRatio / rectTr.localScale.x);
			}
		}
        else if (Input.GetMouseButtonUp(0))
        {
			m_selectPosDestination = mousePos;
			
            HideSelectionCircle();
            HideMoveArrow();
			ShowMoveSelectedUI(m_selectPosDestination);

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
        }

        if (Input.GetMouseButton(2))
        {
        }
#endif

#if !(UNITY_STANDALONE && UNITY_WEBPLAYER)
        // 携帯端末のズーム機能
        if (Input.touches.Length > 0)
        {
            Touch touchZero = Input.GetTouch(0);
        }
#endif
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
