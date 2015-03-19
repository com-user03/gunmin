using UnityEngine;
using System.Collections;

public class NpcSoldier : NpcBase {

	override public void Awake(){
		base.Awake ();
	}

	override public void Start () {
		base.Start ();
		mCkDistMin = 0f;
		mCkDistMax = 2.0f;
	}

	override public void Update () {
		base.Update ();
	}

	override protected bool updateAI(){
		return base.updateAI ();
	}

	override protected Vector3 updatePosition(Vector3 _nextPos){
		return base.updatePosition (_nextPos);
	}
}
