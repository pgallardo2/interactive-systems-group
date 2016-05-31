using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class EmotionController : MonoBehaviour {
	SkinnedMeshRenderer skin;
	Mesh mesh;
	bool morphing = false;

	public string nextGrammar;
	// Use this for initialization
	void Start () {
		skin = gameObject.GetComponent<SkinnedMeshRenderer>();
		mesh = skin.sharedMesh;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.E)){
			SetEmotion(nextGrammar);
		}
	}

	public void SetEmotion(string emotionList){
		string[] nextEmotions = emotionList.Split(new char[]{','}, System.StringSplitOptions.RemoveEmptyEntries);
		if(morphing){
			StopCoroutine("Interpolate");
		}
		else{
			morphing = true;
		}
		StartCoroutine ("Interpolate",nextEmotions);
	}

	IEnumerator Interpolate(string[] nextEmotions){
		List<string> nextEm = new List<string>(nextEmotions);
		float targetWeight;
		if(nextEmotions.Length>1){
			targetWeight = 100f/nextEmotions.Length;
		}
		else{
			targetWeight = 70f;
		}
		int shapesReady = 0;

		if(nextEmotions[0].Equals("normal")){
			for(int i = 0; i<mesh.blendShapeCount; i++){
				skin.SetBlendShapeWeight(i,0f);
			}
			shapesReady = nextEmotions.Length;
		}

		while(shapesReady<nextEmotions.Length){
			shapesReady = 0;
			for(int i = 11; i < mesh.blendShapeCount; i++){
				string shapeName = mesh.GetBlendShapeName(i).ToLower();
				if(nextEm.Contains(shapeName)){
					float curWeight = skin.GetBlendShapeWeight(i);
					if(curWeight<targetWeight){
						skin.SetBlendShapeWeight(i,curWeight+(20/nextEmotions.Length));
					}
					else{
						shapesReady++;
					}
				}
				else{
					float curWeight = skin.GetBlendShapeWeight(i);
					if(curWeight>0f){
						skin.SetBlendShapeWeight(i,curWeight-15f);
					}
					else{
						skin.SetBlendShapeWeight(i,0f);
					}
				}
			}
			yield return null;
		}
		morphing = false;
	}
}
