using UnityEngine;
using System.Collections;

public class myCam : MonoBehaviour {

    private const float FIELD_OF_VIEW_MIN = 20.0f;
    private const float FIELD_OF_VIEW_MAX = 55.0f;

    private Vector3 m_defaultCameraPos;
    private bool m_isDragToMove = false;
    
    private Vector3 m_movePosOrigin;
    private Vector3 m_movePosDifference;

#if !(UNITY_STANDALONE && UNITY_WEBPLAYER)
    private Vector3 m_pinchZoomPos;
    private Vector3 m_pinchZoomCameraStart;
    private Vector3 m_pinchZoomCameraDest;
#endif

	// Use this for initialization
	void Start () {
        m_defaultCameraPos = Camera.main.transform.position;
	}
	
	// Update is called once per frame
	void Update ()
    {
		//transform.Rotate (Vector3.up * Time.deltaTime * 5f);

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER)

        // ズーム機能（マウス）
        if (Input.mouseScrollDelta.y != 0)
        {
            Camera.main.fieldOfView -= Input.mouseScrollDelta.y;
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, FIELD_OF_VIEW_MIN, FIELD_OF_VIEW_MAX);
        }

        // 移動機能（マウス）
        if (Input.GetMouseButton(0))
        {
            if (m_isDragToMove)
            {
                m_movePosDifference = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10) - Camera.main.transform.position;
                Vector3 newCameraPos = m_movePosOrigin - m_movePosDifference;
                Camera.main.transform.position = newCameraPos;
            }
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            m_isDragToMove = true;
            m_movePosOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            m_isDragToMove = false;
        }

        if (Input.GetMouseButton(2))
        {
            Camera.main.transform.position = m_defaultCameraPos;
            Camera.main.fieldOfView = FIELD_OF_VIEW_MAX;
        }
#endif

#if !(UNITY_STANDALONE && UNITY_WEBPLAYER)
        // 携帯端末のズーム機能
        if (Input.touches.Length > 0)
        {
            Touch touchZero = Input.GetTouch(0);

            if (Input.touchCount == 1 && Camera.main.fieldOfView < FIELD_OF_VIEW_MAX)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    if (m_isDragToMove)
                    {
                        m_movePosDifference = Camera.main.ScreenToWorldPoint((Vector3)touchZero.position + Vector3.forward * 10) - Camera.main.transform.position;
                        Camera.main.transform.position = m_movePosOrigin - m_movePosDifference;
                    }
                }

                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    m_isDragToMove = true;
                    m_movePosOrigin = Camera.main.ScreenToWorldPoint((Vector3)touchZero.position + Vector3.forward * 10);
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    m_isDragToMove = false;
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch touchOne = Input.GetTouch(1);

                // カメラ移動
                if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began)
                {
                    Ray touchPosRay = Camera.main.ScreenPointToRay((Vector3)(touchZero.position + touchOne.position) / 2);
                    m_pinchZoomPos = touchPosRay.origin + (touchPosRay.direction * 10);

                    m_pinchZoomCameraStart = Camera.main.transform.position;
                    m_pinchZoomCameraDest = m_pinchZoomPos - Camera.main.transform.forward * 10;
                }
                // カメラズーム
                if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
                {
                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                    float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                    if (Mathf.Abs(deltaMagnitudeDiff) > 1)
                    {
                        Camera.main.fieldOfView += deltaMagnitudeDiff * 0.25f;
                        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, FIELD_OF_VIEW_MIN, FIELD_OF_VIEW_MAX);

                        if (Camera.main.fieldOfView > FIELD_OF_VIEW_MIN)
                        {
                            // カメラはズームポイントに移動する
                            if (deltaMagnitudeDiff < 0)
                            {
                                float zoomMoveCameraValue = 1 - ((Camera.main.fieldOfView - FIELD_OF_VIEW_MIN) / (FIELD_OF_VIEW_MAX - FIELD_OF_VIEW_MIN));
                                Camera.main.transform.position = Vector3.Lerp(m_pinchZoomCameraStart, m_pinchZoomCameraDest, zoomMoveCameraValue);
                            }
                            else
                            {
                                float zoomMoveCameraValue = (Camera.main.fieldOfView - FIELD_OF_VIEW_MIN) / (FIELD_OF_VIEW_MAX - FIELD_OF_VIEW_MIN);
                                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, m_defaultCameraPos, zoomMoveCameraValue);
                            }
                        }
                    }
                }
            }
        }
#endif
    }
}
