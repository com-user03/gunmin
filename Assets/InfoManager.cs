using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfoManager : MonoBehaviour {

    private const string NUM_UNITS_TEXT = "グンミン: ";
    private const string FPS_TEXT = "FPS: ";

    private GameObject FPSTextObj;
    private int frameCount = 0;
    private float dt = 0;
    private float fpsCheckInterval = 4;

    private GameObject NumUnitsTextObj;
    int numUnitsOnScreen = 0;

	// Use this for initialization
	void Start () {
        FPSTextObj = GameObject.Find("TextFPS");
        NumUnitsTextObj = GameObject.Find("TextNumUnits");

        Npc.UnitCreatedEvent += UnitAdded;
        Npc.UnitRemovedEvent += UnitRemoved;
	}
	
	// Update is called once per frame
	void Update ()
    {
        // FPS チェック
        frameCount++;
        dt += Time.deltaTime;

        if (dt > 1.0f / fpsCheckInterval)
        {
            float fps = frameCount / dt;
            FPSTextObj.GetComponent<Text>().text = FPS_TEXT + fps.ToString("F2");

            frameCount = 0;
            dt -= 1.0f / fpsCheckInterval;
        }
	}

    private void UnitAdded()
    {
        numUnitsOnScreen++;
        NumUnitsTextObj.GetComponent<Text>().text = NUM_UNITS_TEXT + numUnitsOnScreen;
    }

    private void UnitRemoved()
    {
        numUnitsOnScreen--;
        NumUnitsTextObj.GetComponent<Text>().text = NUM_UNITS_TEXT + numUnitsOnScreen;
    }
}
