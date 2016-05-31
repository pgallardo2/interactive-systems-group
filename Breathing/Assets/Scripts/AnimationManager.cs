using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Animator))]
public class AnimationManager : MonoBehaviour {
	public Animator mixer;
	public AnimationClip[] Animations;
	// Use this for initialization
	void Start () {
		mixer = gameObject.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void Play(string stateName)
	{
		StartCoroutine("PlayAnim",stateName);
	}

	IEnumerator PlayAnim( string StateName){
		Debug.Log("Playing Animation: " + StateName);
		if(!mixer.GetCurrentAnimatorStateInfo(0).IsName("Sitting")){
			mixer.SetInteger("pathIndex", -1);

			while(!mixer.GetCurrentAnimatorStateInfo(0).IsName("Sitting")){//||mixer.IsInTransition(0)){
				yield return null;
			}
		}

		int index = 0;
		foreach(AnimationClip c in Animations){
			if(c.name.Equals(StateName)){
				break;
			}
			index++;
		}

		if(!Animations[index].isLooping){
			float endTime = Animations[index].length;
			Invoke("StopAnimation", endTime);
		}
		mixer.SetInteger("pathIndex", index);
	}

	void StopAnimation(){
		mixer.SetInteger("pathIndex", -1);
	}
}
