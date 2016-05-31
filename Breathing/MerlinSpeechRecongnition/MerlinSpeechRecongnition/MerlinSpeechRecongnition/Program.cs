

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;

namespace MerlinSpeechRecongnition
{
    class Program
    {
        // Create a new SpeechRecognitionEngine instance.
        //static SpeechRecognizer recognizer = new SpeechRecognizer();
        public static SpeechSynthesizer SS;
        static SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine
                (new System.Globalization.CultureInfo("en-US"));

        public static Program instance;
        
        static bool wildcardExpected = false;

        static bool listening = false;

        public static Program GetInstance()
        {
            if (instance == null)
            {
                return new Program();
            }
            return instance;
        }

        static void Main(string[] args)
        {
            //Use only once to convert to binary XML files.
            //XMLEncryption encryptXML = new XMLEncryption();
            //encryptXML.EncryptXML();
            
            SS = new SpeechSynthesizer();
            SS.SelectVoice("IVONA 2 Salli");
            SS.SetOutputToDefaultAudioDevice();
            // Subscribe to the SpeakProgress event.       
            SS.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(synth_SpeakProgress);

            // Speak a string asynchronously.
            //SS.SpeakAsync("Far far away, behind the word mountains, far from the countries Vokalia and Consonantia, there live.");
            Console.WriteLine("Close this console after you are done with the game.");
            
            // Configure the input to the speech recognizer.
          
            recognizer.SetInputToDefaultAudioDevice();
            AMQ_Connection.GetConnectionInstance();
        }

        /*
        /// <summary>
        /// Receives a grammar from AMQ_Connection, loads the grammar and connects
        /// to the event handler.
        /// </summary>
        /// <see cref="sre_SpeechRecognized"/>
        /// <param name="choices">List of words in current grammar</param>
        public static void recognizeGrammar(string[] choices) 
        {
            recognizer.UnloadAllGrammars();

            GrammarBuilder gb = null;
            if (choices[0].Equals("wildcard"))
            {
                gb = new GrammarBuilder();
                gb.AppendWildcard();
                wildcardExpected = true;
            }
            else {
                gb = new GrammarBuilder();
                Choices responses = new Choices();
                responses.Add(choices);
                gb.Append(responses);
            }

            Grammar g = new Grammar(gb);
            recognizer.LoadGrammar(g);

            // Register a handler for the SpeechRecognized event.
            recognizer.SpeechRecognized +=
                new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);
        }
        */

        /// <summary>
        /// Updated version of the recognizer. This excludes the Windows commands
        /// from being recognized and executed, and stops C# from opening a recognition
        /// window. 
        /// Info: http://bit.ly/20JKie8
        /// Info: http://bit.ly/1mr7awj
        /// <see cref="recognizeGrammar"/>
        /// <param name="choices">List of words in current grammar</param>
        /// </summary>
        public static void inProcRecognition(string[] choices)
        {
                recognizer.UnloadAllGrammars();
                recognizer.RecognizeAsyncStop();
                recognizer.RequestRecognizerUpdate();
                GrammarBuilder gb = null;
                if (choices[0].Equals("wildcard"))
                {
                    gb = new GrammarBuilder();
                    gb.AppendWildcard();
                    wildcardExpected = true;
                }
                else
                {
                    gb = new GrammarBuilder();
                    Choices responses = new Choices();
                    responses.Add(choices);
                    gb.Append(responses);
                }

                Grammar g = new Grammar(gb);
                recognizer.LoadGrammarAsync(g);
                Thread.Sleep(150);
                recognizer.RequestRecognizerUpdate();

                // Start asynchronous, continuous speech recognition.
                //recognizer.RecognizeAsync(RecognizeMode.Multiple);
                try
                {
                    recognizer.RecognizeAsync(RecognizeMode.Multiple);
                }

                catch (System.InvalidOperationException rec) 
                { 

                }

                // Register a handler for the SpeechRecognized event.
                recognizer.SpeechRecognized +=
                    new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);
                listening = true;
        }

        /// <summary>
        ///  Create a simple handler for the SpeechRecognized event.
        /// </summary>
        /// <seealso cref="recognizeGrammar"/>
        /// <param name="sender"></param>
        /// <param name="e"></param
        static void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (listening)
            {
                if (wildcardExpected)
                {
                    AMQ_Connection.GetConnectionInstance().SendMessage("wildcard");
                    wildcardExpected = false;
                }
                else
                {
                    AMQ_Connection.GetConnectionInstance().SendMessage(e.Result.Text);
                }

                listening = false;
                recognizer.UnloadAllGrammars();
                recognizer.RecognizeAsyncStop();
                recognizer.RequestRecognizerUpdate();
            }
        }
        static void synth_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            //Console.WriteLine(e.Text);
        }

        static void VoiceParams(SpeechSynthesizer synth)
        {
            // Output information about all of the installed voices. 
            Console.WriteLine("Installed voices -");
            foreach (InstalledVoice voice in synth.GetInstalledVoices())
            {
                VoiceInfo info = voice.VoiceInfo;
                // Get information about supported audio formats.
                string AudioFormats = "";
                foreach (SpeechAudioFormatInfo fmt in synth.Voice.SupportedAudioFormats)
                {
                    AudioFormats += String.Format("{0}\n",
                    fmt.EncodingFormat.ToString());
                }

                // Write information about the voice to the console.
                Console.WriteLine(" Name:          " + synth.Voice.Name);
                Console.WriteLine(" Culture:       " + synth.Voice.Culture);
                Console.WriteLine(" Age:           " + synth.Voice.Age);
                Console.WriteLine(" Gender:        " + synth.Voice.Gender);
                Console.WriteLine(" Description:   " + synth.Voice.Description);
                Console.WriteLine(" ID:            " + synth.Voice.Id);

                if (synth.Voice.SupportedAudioFormats.Count != 0)
                {
                    Console.WriteLine(" Audio formats: " + AudioFormats);
                }
                else
                {
                    Console.WriteLine(" No supported audio formats found");
                }

                // Get additional information about the voice.
                string AdditionalInfo = "";
                foreach (string key in synth.Voice.AdditionalInfo.Keys)
                {
                    AdditionalInfo += String.Format("  {0}: {1}\n",
                        key, synth.Voice.AdditionalInfo[key]);
                }

                Console.WriteLine(" Additional Info - " + AdditionalInfo);
                Console.WriteLine();

                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
            }
        }
    }
}
