using System;
using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.Threading;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Util;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class AMQ_Connection : MonoBehaviour {
	private IConnection connection;
	private ISession session;
	public const String QUEUE_ADVISORY_DESTINATION = "ActiveMQ.Advisory.SpeechQueue";
	public const String QUEUE_ADVISORY_INBOX = "ActiveMQ.Advisory.MerlinQueue";
	private IMessageConsumer consumer;
	private IMessageProducer producer;
	public bool running;
	public SimpleDialogManager dialogManager;

	void Start () {
		//dialogManager = GameObject.Find("SimpleDialogManager").GetComponent<SimpleDialogManager>();
		//Create new connection
		IConnectionFactory factory = new ConnectionFactory();
		connection = factory.CreateConnection("admin","admin");
		connection.ClientId = "Unity Connection";
		connection.Start();
		running = true;
		//Use default message queue
		session = connection.CreateSession(AcknowledgementMode.DupsOkAcknowledge);
		
		IDestination inbox = session.GetQueue(QUEUE_ADVISORY_INBOX);
		consumer = session.CreateConsumer(inbox);
		
		IDestination dest = session.GetQueue(QUEUE_ADVISORY_DESTINATION);
		producer = session.CreateProducer(dest);

		StartCoroutine("StartConsumer");
	}
	
	// Update is called once per frame
	void Update () {
	}

	IEnumerator StartConsumer(){
		Debug.Log("Starting message consumer.");

		IMessage advisory;
			
		while (running)
		{
			advisory = consumer.ReceiveNoWait();
			if(advisory != null){
				ActiveMQMessage amqMsg = advisory as ActiveMQMessage;
				String msg = System.Text.Encoding.Default.GetString(amqMsg.Content).Substring(4);
				Debug.Log ("Message received: " + msg);
				dialogManager.setResponse(msg);
			}
			yield return null;
		}
		KillConnection();
	}
	
	public void SendMessage(System.Object obj){
		IObjectMessage msg = session.CreateObjectMessage(obj);
		producer.Send(msg);
	}
	
	public void SendMessage(String str){
		ITextMessage msg = session.CreateTextMessage(str);
		producer.Send(msg);
	}
	
	/*public void purgeQueue(){
		IMessage advisory;
		
		while (connection.IsStarted && ((advisory = consumer.Receive(TimeSpan.FromSeconds(3)))!= null)){
			
		}
	}*/

	public void KillConnection(){
		running = false;
		if(consumer!=null){
			consumer.Close();
		}
		if(producer!=null){
			producer.Close();
		}
		if(session!=null){
			session.Dispose();
		}
		if(connection!=null){
			connection.Dispose();
		}
		Debug.Log("Consumer Closed.");
	}

	void OnApplicationQuit() {
		KillConnection();
		Debug.Log("Application ending after " + Time.time + " seconds");
	}
}
