using UnityEngine;
using System.Collections;

public class RandomGazeAversion : MonoBehaviour {
	public SimpleDialogManager manager;
	public Transform player;
	public Transform[] targets;
	public LookAt LeftEye;
	public LookAt RightEye;
	bool lookingAtPlayer = true;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if(manager.isSpeaking && lookingAtPlayer){
			int lookindex = Random.Range(0,3);
			LeftEye.target = targets[lookindex];
			RightEye.target = targets[lookindex];
			Invoke ("LookBackAtPlayer", 2f);
			lookingAtPlayer = false;
		}

		if(!manager.isSpeaking && lookingAtPlayer){
			LookBackAtPlayer();
		}
	}

	void LookBackAtPlayer(){
		LeftEye.target = player;
		RightEye.target = player;
		lookingAtPlayer = true;
	}
}
