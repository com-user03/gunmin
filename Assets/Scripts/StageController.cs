using UnityEngine;
using System.Collections;

public class StageController : MonoBehaviour {
	private const string ASTAR_OBJ_NAME = "A*";
	private static StageController mInstance = null;
	public static bool hasInstance{ get { return mInstance!=null; } }
	public static StageController instance{
		get{
			if(mInstance==null){
				GameObject ctrlObj;
				string className = typeof(StageController).Name;
				UnityEngine.Object resObj = Resources.Load(className);
				if(resObj!=null){
					ctrlObj = GameObject.Instantiate(resObj) as GameObject;
					ctrlObj.name = className;
					mInstance = ctrlObj.GetComponent<StageController>();
				}else{ // Instantiate 
					ctrlObj = new GameObject(className);
					mInstance = ctrlObj.AddComponent<StageController>();
				}
				ctrlObj.transform.parent = Camera.main.transform;
				ctrlObj.transform.localPosition = Vector3.zero;
				//				DontDestroyOnLoad(ctrlObj);
			}
			return mInstance;
		}
	}
	private AstarPath mAstarPath;
	public AstarPath astarPath { get{ return mAstarPath; } }

	void Awake () {
		GameObject asGo = GameObject.Find (ASTAR_OBJ_NAME);
		mAstarPath = asGo.GetComponent<AstarPath>();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
