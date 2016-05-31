using UnityEngine;
using System.Collections;

public class Breathing : MonoBehaviour {


	public float BreathingState = 0;
	public float Frequency = 0f;
	public float FrequencyRate = 1f;
	public int Amplitude = 50;
	public float RelativeOffset = 0f;
	public int targetBlendShape = 0;

	float ErrorTolerance = .2f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float prevState = BreathingState;
		BreathingState = (float)Amplitude + (float)Amplitude * (Mathf.Sin(Mathf.PI * Frequency * Time.deltaTime));

		float constant = Mathf.Abs(BreathingState-prevState)/BreathingState;

		if(RelativeOffset!=0f && (RelativeOffset-constant)>ErrorTolerance){
			int iterations = 0;
			while((RelativeOffset-constant)>ErrorTolerance && iterations<3){
				BreathingState = (prevState + BreathingState)/2f;
				constant = Mathf.Abs(BreathingState-prevState)/BreathingState;
				iterations++;
			}
		}
		else{
			RelativeOffset = constant;
		}

		float slope = (float)Amplitude * (Mathf.Cos(Mathf.PI * Frequency * Time.deltaTime));
		//Debug.Log (BreathingState-prevState);

		if((slope>=0) && ((BreathingState-prevState)>0)){
			this.gameObject.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(targetBlendShape,BreathingState);
		}
		else if((slope<=0) && ((BreathingState-prevState)<0)){
			this.gameObject.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(targetBlendShape,BreathingState);
		}

		if(Frequency<100){
			Frequency += FrequencyRate;
		}
		else{
			Frequency -= 100;
			Frequency += FrequencyRate;
		}
	}
}
