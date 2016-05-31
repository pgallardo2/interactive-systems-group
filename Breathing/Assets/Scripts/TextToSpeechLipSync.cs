using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TextToSpeechLipSync : MonoBehaviour {
	public string inputDialog;
	public List<string> phonemesDialog = new List<string>();
	public float syllableLenghtInSec = 0.07f;
	private float syllableTimer = 0f;
	public bool DialogQueued = false;
	public SkinnedMeshRenderer mesh;
	public SimpleDialogManager dialogManager;

	// Use this for initialization
	void Start () {
		mesh = GetComponent<SkinnedMeshRenderer> ();
		//syllableTimer = syllableLenghtInSec/2;
		syllableTimer = 0.05f;
		Debug.Log(syllableTimer);
	}
	
	// Update is called once per frame
	void Update () {
		if(DialogQueued){
			if(syllableTimer>0){
				syllableTimer -= Time.deltaTime;
			}
			else{
				syllableTimer = 0.05f;
				Speak();
			}
		}
	}

	public void NewDialog(string dialog){
		mesh = GetComponent<SkinnedMeshRenderer> ();
		syllableTimer = 0.05f;
		inputDialog = dialog;
		Debug.Log(inputDialog);
		phonemesDialog = toPhonemes(dialog);
		DialogQueued = true;
		Speak();
	}

	private void Speak(){
		if(phonemesDialog!=null && phonemesDialog.Count>0){
			//Play phoneme anim of phonemesDialog[0];
			setShape(phonemesDialog[0]);
			//Debug.Log(phonemesDialog[0]);
			phonemesDialog.RemoveAt(0);
		}
		else if(inputDialog!=null && inputDialog.Length>0){
			phonemesDialog = toPhonemes(inputDialog);
			//Play phoneme anim of phonemesDialog[0];
			setShape(phonemesDialog[0]);
			//Debug.Log(phonemesDialog[0]);
			phonemesDialog.RemoveAt(0);
		}
		else{
			DialogQueued=false;
			//GameObject.Find ("Face").GetComponent<Animator>().SetInteger("UpperBodyAnim", -1);
			dialogManager.isSpeaking = false;
		}
		if(phonemesDialog.Count<=0){
			setShape("-");
			DialogQueued=false;
			//GameObject.Find("Face").GetComponent<Animator>().SetInteger("UpperBodyAnim",-1);
			dialogManager.isSpeaking = false;
		}
	}

	public static List<string> toPhonemes(string s){
		List<string> output = new List<string>();
		s = s.ToLower();
		for (int i = 0; i < s.Length; i++)
		{
			switch (s[i]) { 
			case 'm': 
			case 'b': 
			case 'p':
				output.Add("M");
				break;
			case 'a':
			case 'i':
				output.Add("A");
				break;
			case 'o':
				output.Add("O");
				break;
			case 'e':
				output.Add("E");
				break;
			case 'u':
				output.Add("U");
				break;
			case 'l':
				output.Add("L");
				break;
			case 'w':
			case 'q':
				output.Add("W");
				break;
			case 'f':
			case 'v':
				output.Add("F");
				break;
			case '\'':
			case 'h':
			case ' ':
				break;
			case ',':
				output.Add("-");
				break;
			case '.':
			case ':':
			case ';':
			case '?':
			case '!':
				output.Add("-");
				output.Add("-");
				break;
			default:
				output.Add("D");
				break;
			}
		}
		return output;
	}

	void setShape(string phoneme){
		mesh.SetBlendShapeWeight(0, 0);//AI
		mesh.SetBlendShapeWeight(1, 0);//FV
		mesh.SetBlendShapeWeight(2, 0);//Rest
		mesh.SetBlendShapeWeight(3, 0);//O
		mesh.SetBlendShapeWeight(4, 0);//U
		mesh.SetBlendShapeWeight(5, 0);//L
		mesh.SetBlendShapeWeight(6, 0);//MBP
		mesh.SetBlendShapeWeight(7, 0);//WQ
		mesh.SetBlendShapeWeight(8, 0);//E
		mesh.SetBlendShapeWeight(9, 0);//OTHERS

		switch (phoneme) { 
		case "M": 
			mesh.SetBlendShapeWeight(6, 60);
			break;
		case "A":
			mesh.SetBlendShapeWeight(0, 60);
			break;
		case "O":
			mesh.SetBlendShapeWeight(3, 60);
			break;
		case "E":
			mesh.SetBlendShapeWeight(8, 60);
			break;
		case "U":
			mesh.SetBlendShapeWeight(4, 60);
			break;
		case "L":
			mesh.SetBlendShapeWeight(5, 60);
			break;
		case "W":
			mesh.SetBlendShapeWeight(7, 60);
			break;
		case "F":
			mesh.SetBlendShapeWeight(1, 60);
			break;
		case "D":
			mesh.SetBlendShapeWeight(9, 60);
			break;
		}

	}
}
