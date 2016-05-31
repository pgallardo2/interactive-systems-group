using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Animator))]
public class SpeechController : MonoBehaviour {
	Animator mixer;
	public AnimationClip[] Dialogs;
	public SimpleDialogManager dialogManager;
	// Use this for initialization
	void Start () {
		mixer = gameObject.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Speak(string dialogID){
		Debug.Log("Playing dialog: " + dialogID);
		//Play dialog animation.
		int index = 0;
		foreach(AnimationClip c in Dialogs){
			if(c.name.Equals(dialogID+"Sync")){
				break;
			}
			index++;
		}
		
		float endTime = Dialogs[index].length;
		mixer.SetInteger("pathIndex", index);
		Invoke("StopAnimation", endTime);
		//Mark as done when finished.
	}

	void StopAnimation(){
		mixer.SetInteger("pathIndex", -1);
		dialogManager.isSpeaking = false;
	}
}
