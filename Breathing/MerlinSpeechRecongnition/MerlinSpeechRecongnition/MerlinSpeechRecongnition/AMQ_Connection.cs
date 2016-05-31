using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Util;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;

namespace MerlinSpeechRecongnition
{
    class AMQ_Connection
    {
        //Fields
        private IConnection connection;
        private ISession session;
        private IMessageProducer producer;
        private static AMQ_Connection instance = null;
        IMessageConsumer consumer;

        public const String QUEUE_ADVISORY_DESTINATION = "ActiveMQ.Advisory.MerlinQueue";
        public const String QUEUE_ADVISORY_INBOX = "ActiveMQ.Advisory.SpeechQueue";

        Thread receiver;

        //Constructor (Singleton)
        private AMQ_Connection()
        {
            // Create new connection
            IConnectionFactory factory = new ConnectionFactory();
            // Default username and password
            connection = factory.CreateConnection("admin", "admin");
            connection.ClientId = "Speech Connection";
            connection.Start();
            session = connection.CreateSession(AcknowledgementMode.DupsOkAcknowledge);
            IDestination dest = session.GetQueue(QUEUE_ADVISORY_DESTINATION);
            producer = session.CreateProducer(dest);
            // Creating queue consumer
            IDestination desta = session.GetQueue(QUEUE_ADVISORY_INBOX);
            consumer = session.CreateConsumer(desta);
            IMessage advisory;
            while ((advisory = consumer.Receive(TimeSpan.FromSeconds(2))) != null)
            { }
            receiver = new Thread(this.StartReceiver);
            receiver.Start();
        }

        /// <summary>
        /// Singleton method to get the only instance of this class or create one
        /// if it does not exist.
        /// </summary>
        /// <returns>Connection instance</returns>
        public static AMQ_Connection GetConnectionInstance()
        {
            if(instance == null)
                instance = new AMQ_Connection();
            return instance;
        }

        /// <summary>
        /// Receives an object to send to the queue.
        /// </summary>
        /// <param name="e"></param>
        public void SendObject(Object e) {
            IObjectMessage msg = producer.CreateObjectMessage(e);
            producer.Send(msg);
        }

        /// <summary>
        /// Receives a string to send  to the queue.
        /// </summary>
        /// <param name="str"></param>
        public void SendMessage(string str) {
            ITextMessage msg = producer.CreateTextMessage(str);
            producer.Send(msg);
        }

        /** Consumer connection not needed for this project, but it can be used for testing.*/
        void StartReceiver()
        {
            IMessage advisory;

            while ((advisory = consumer.Receive()) != null)
            {
                ActiveMQMessage amqMsg = advisory as ActiveMQMessage;
                String msg = System.Text.Encoding.Default.GetString(amqMsg.Content).Substring(4);
                //Console.WriteLine("\tQueue message: " + System.Text.Encoding.Default.GetString(amqMsg.Content).Substring(4));

                string[] grammarArray = msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (!grammarArray[0].Equals("l1p$ync"))
                {
                    Program.inProcRecognition(grammarArray);
                }
                else
                {
                    Console.WriteLine("\tQueue message: " + msg);
                    Program.SS.SpeakAsync(grammarArray[1]);
                    
                }
            }
            //Receival Complete
        }


        void ShutDown()
        {
            try
            {
                session.Close();
                connection.Close();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
            }
        }
    }
}
