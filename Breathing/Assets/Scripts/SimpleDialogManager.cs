using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

public class SimpleDialogManager : MonoBehaviour {
	public string sceneName;
	public scene currentScene;
	public int eventIndex = 0;
	string response;
	public bool isSpeaking = false;
	public bool isListening = false;
	public bool isWaiting = false;
    public bool synthesized = false;

	public AnimationManager Agent;
	public SpeechController Face;
	public AMQ_Connection Connection;
    public TextToSpeechLipSync Synth;
	// Use this for initialization
	void Start () {
		// Construct an instance of the XmlSerializer with the type
		// of object that is being deserialized.
		XmlSerializer seriealizer = 
			new XmlSerializer(typeof(scene));
		// To read the file, create a FileStream.
		FileStream stream = 
			new FileStream(sceneName+".xml", FileMode.Open);
		// Call the Deserialize method and cast to the object type.
		currentScene = (scene) seriealizer.Deserialize(stream);
	}

	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.S))
			StartCoroutine("RunGame");
	}
	
	void sendGrammar(string[] choices){
		string msgAcum = "";
		foreach(string s in choices){
			msgAcum += s + ";";
		}

		Connection.SendMessage(msgAcum);
	}

	public void setResponse(string str){
		response = str;
		isListening = false;
	}

	IEnumerator RunGame(){
		while(eventIndex<currentScene.@event.Length){
			Debug.Log (currentScene.@event[eventIndex].GetType().ToString());

			//Play dialogs (no synthesized voice)
			if(currentScene.@event[eventIndex].dialog!=null && !synthesized){
				while(isSpeaking){
					yield return null;
				}
				sceneEventDialog dialog = currentScene.@event[eventIndex].dialog;
				isSpeaking = true;
				Face.Speak(currentScene.@event[eventIndex].id);
				SetNextDialogIndex(currentScene.@event[eventIndex].jump_id);
				while(isSpeaking){
					yield return null;
				}
				continue;
			}

            //Play dialogs (synthesized voice)
            if (currentScene.@event[eventIndex].dialog != null && synthesized)
            {
                while (isSpeaking)
                {
                    yield return null;
                }
                sceneEventDialog dialog = currentScene.@event[eventIndex].dialog;
                isSpeaking = true;
				string dialogMessage = "l1p$ync;"+dialog.Value;
				Connection.SendMessage(dialogMessage);
                Synth.NewDialog(dialog.Value);
                SetNextDialogIndex(currentScene.@event[eventIndex].jump_id);
                while (isSpeaking)
                {
                    yield return null;
                }
                continue;
            }

			//Play animations
			if(currentScene.@event[eventIndex].animation!=null){
				while(isWaiting){
					yield return null;
				}
				Agent.Play(currentScene.@event[eventIndex].animation);
				//isWaiting = true;
				SetNextDialogIndex(currentScene.@event[eventIndex].jump_id);
				continue;
			}

			//Recognize responses
			if(currentScene.@event[eventIndex].response!=null){
				while(isSpeaking){
					yield return null;
				}

				List<string> grammars = new List<string>();
				Dictionary<string,List<string>> grammarDictionary = new Dictionary<string, List<string>>();
				foreach(sceneEventGrammar g in currentScene.@event[eventIndex].response){
					grammarDictionary.Add(g.jump_id,new List<string>());
					foreach(string s in g.item){
						grammarDictionary[g.jump_id].Add(s);
						grammars.Add(s);
					}
				}
				sendGrammar(grammars.ToArray());
				isListening=true;
				while(isListening){
					yield return null;
				}

				string jump = null;
				foreach(string key in grammarDictionary.Keys){
					if(grammarDictionary[key].Contains(response)){
						jump = key;
					}
				}

				int i = 0;
				foreach(sceneEvent e in currentScene.@event){
					if(e.id.Equals(jump)){
						eventIndex = i;
						break;
					}
					i++;
				}
				continue;
			}

			//Activate trigger
			if(currentScene.@event[eventIndex].trigger!=null){
				sceneEventTrigger trigger = currentScene.@event[eventIndex].trigger;
				if(trigger.Value!=null){
					GameObject.Find(trigger.@object).SendMessage(trigger.method, trigger.Value);
				}
				else{
					GameObject.Find(trigger.@object).SendMessage(trigger.method);
				}
				SetNextDialogIndex(currentScene.@event[eventIndex].jump_id);
				continue;
			}

			//Wait
			if(currentScene.@event[eventIndex].wait!=null){
				isWaiting = true;
				while(isWaiting){
					yield return null;
				}
				SetNextDialogIndex(currentScene.@event[eventIndex].jump_id);
				continue;
			}
			yield return null;
		}

		if(currentScene.nextScene.Equals("none")){
			Application.Quit();
		}
		else{
			Connection.KillConnection();
			Application.LoadLevel(currentScene.nextScene);
		}
	}

	public void StopWaiting(){
		isWaiting = false;
	}

	void SetNextDialogIndex(string jump_id){
		if(jump_id!=null && !jump_id.Equals("none")){
			int i = 0;
			foreach(sceneEvent e in currentScene.@event){
				if(e.id.Equals(jump_id)){
					eventIndex = i;
					break;
				}
				i++;
			}
		}
		else{
			eventIndex++;
		}
	}
}
