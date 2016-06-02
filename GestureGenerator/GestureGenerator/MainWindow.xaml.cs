using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Kinect;
using Nui = Microsoft.Kinect;
using System.Windows.Forms;
//using System.Windows.Visibility;
//Version 47

namespace GestureGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _Kinect;

        private WriteableBitmap _ColorImageBitmap;
        private Int32Rect _ColorImageBitmapRect;
        private int _ColorImageStride;
        private byte[] _ColorImagePixelData;

        private Skeleton[] _FrameSkeletons;
        private Pose[] _PoseLibrary;
        private Pose _StartPose;

        bool leftArm = false; 
        bool rightArm = false;
        bool leftLeg = false;
        bool rightLeg = false;
        bool torso = false;
        bool head = false;

        int totalCaptures = 0;
        int jointCount = 0;
        int totalJointCount = 0;
        JointStats[] jointStats = new JointStats[20];
        LinkedList<string> results = new LinkedList<string>();
        LinkedList<string> debugFile = new LinkedList<string>();
        int poseTypeCount = 0;

        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
                this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
                StatusElement.Text = "Connected";
                //this.Kinect.ElevationAngle = 10;
                PopulatePoseLibrary();
            };

            results.AddFirst("<rule id='episode1'>");            
        }
        #endregion Constructor

        #region Methods

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Initializing:
                    StatusElement.Text = "Initializing";
                    break;
                case KinectStatus.Connected:
                    StatusElement.Text = "Connected";
                    this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
                    break;
                case KinectStatus.NotPowered:
                    StatusElement.Text = "Not Powered";
                    break;
                case KinectStatus.NotReady:
                    StatusElement.Text = "Not Ready";
                    break;
                case KinectStatus.DeviceNotGenuine:
                    StatusElement.Text = "Fake Kinect";
                    this.Kinect = e.Sensor;
                    break;
                case KinectStatus.Disconnected:
                    StatusElement.Text = "Disonnected";
                    this.Kinect = null;
                    break;
                default:
                    StatusElement.Text = "Unknown Error";
                    break;
            }
        }

        private void Kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            //if debug angles == true 
            debugAngles();
            if (ColorImageEnabled.IsChecked == true)
            {
                using (ColorImageFrame frame = e.OpenColorImageFrame())
                {
                    if (frame != null)
                    {
                        frame.CopyPixelDataTo(this._ColorImagePixelData);

                        if (ColorImageEnabled.IsChecked == true)
                        {
                            this.ColorImageElement.Source = this._ColorImageBitmap;
                            this._ColorImageBitmap.WritePixels(this._ColorImageBitmapRect, this._ColorImagePixelData, this._ColorImageStride, 0);
                        }
                    }
                }
            }
        }

        private void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    frame.CopySkeletonDataTo(this._FrameSkeletons);
                    Skeleton skeleton = GetPrimarySkeleton(this._FrameSkeletons);
                    if (skeleton != null)
                        PrintPose(skeleton);
                }
            }
        }

        private static Skeleton GetPrimarySkeleton(Skeleton[] skeletons)
        {
            Skeleton skeleton = null;

            if (skeletons != null)
            {
                //Find the closest skeleton       
                for (int i = 0; i < skeletons.Length; i++)
                {
                    if (skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        if (skeleton == null)
                        {
                            skeleton = skeletons[i];
                        }
                        else
                        {
                            if (skeleton.Position.Z > skeletons[i].Position.Z)
                            {
                                skeleton = skeletons[i];
                            }
                        }
                    }
                }
            }

            return skeleton;
        }

        //Review poses 5-9 
        private void PopulatePoseLibrary()
        {
            this._PoseLibrary = new Pose[23];

            //Start Pose - Arms Extended
            this._StartPose = new Pose();
            this._StartPose.Title = "Start Pose";
            this._StartPose.Angles = new PoseAngle[4];
            this._StartPose.Angles[0] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 180, 20);
            this._StartPose.Angles[1] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 180, 20);
            this._StartPose.Angles[2] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 0, 20);
            this._StartPose.Angles[3] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 0, 20);

            //Pose 1 - Both Hands Up
            this._PoseLibrary[0] = new Pose();
            this._PoseLibrary[0].Title = "Arms Up";
            this._PoseLibrary[0].Angles = new PoseAngle[4];
            this._PoseLibrary[0].Angles[0] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 180, 20);
            this._PoseLibrary[0].Angles[1] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 90, 20);
            this._PoseLibrary[0].Angles[2] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 0, 20);
            this._PoseLibrary[0].Angles[3] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 90, 20);

            //Pose 2 - Lift Right hand as a Hi Five 
            this._PoseLibrary[1] = new Pose();
            this._PoseLibrary[1].Title = "Hi Five";
            this._PoseLibrary[1].Angles = new PoseAngle[4];
            this._PoseLibrary[1].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 32, 20);
            this._PoseLibrary[1].Angles[1] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 314, 20);
            this._PoseLibrary[1].Angles[2] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 266, 30);
            this._PoseLibrary[1].Angles[3] = new PoseAngle(JointType.WristRight, JointType.HandRight, 268, 30);

            //Pose 3 - Normal Stance (normal_stance.xml)
            this._PoseLibrary[2] = new Pose();
            this._PoseLibrary[2].Title = "Normal Stance";
            this._PoseLibrary[2].Angles = new PoseAngle[6];
            //this._PoseLibrary[2].Angles[0] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 91, 10);
            //this._PoseLibrary[2].Angles[1] = new PoseAngle(JointType.HipCenter, JointType.Spine, 89, 10);
            this._PoseLibrary[2].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 217, 10);
            this._PoseLibrary[2].Angles[1] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 326, 10);
            this._PoseLibrary[2].Angles[2] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 253, 10);
            this._PoseLibrary[2].Angles[3] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 287, 10);
            this._PoseLibrary[2].Angles[4] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 260, 27);
            this._PoseLibrary[2].Angles[5] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 280, 27);
            //this._PoseLibrary[2].Angles[8] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 257, 19);
            //this._PoseLibrary[2].Angles[9] = new PoseAngle(JointType.WristRight, JointType.HandRight, 279, 12);

            //Pose 4 - Hands on hip (hands_hip.xml)
            this._PoseLibrary[3] = new Pose();
            this._PoseLibrary[3].Title = "Hands on Hip";
            this._PoseLibrary[3].Angles = new PoseAngle[4];
            //this._PoseLibrary[3].Angles[0] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 91, 10);
            //this._PoseLibrary[3].Angles[1] = new PoseAngle(JointType.HipCenter, JointType.Spine, 92, 10);
            this._PoseLibrary[3].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 211, 10);
            this._PoseLibrary[3].Angles[1] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 332, 10);
            this._PoseLibrary[3].Angles[2] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 230, 15);
            this._PoseLibrary[3].Angles[3] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 311, 15);
            //this._PoseLibrary[3].Angles[6] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 290, 22); //original 10
            //this._PoseLibrary[3].Angles[7] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 251, 22); //original 11
            //this._PoseLibrary[3].Angles[8] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 288, 13);
            //this._PoseLibrary[3].Angles[9] = new PoseAngle(JointType.WristRight, JointType.HandRight, 254, 11);

            //Pose 5 - Crossed_arms (crossed_arms.xml)
            this._PoseLibrary[4] = new Pose();
            this._PoseLibrary[4].Title = "Crossed Arms";
            this._PoseLibrary[4].Angles = new PoseAngle[6];
            this._PoseLibrary[4].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 217, 15);
            this._PoseLibrary[4].Angles[1] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 326, 10);
            this._PoseLibrary[4].Angles[2] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 244, 50);
            this._PoseLibrary[4].Angles[3] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 287, 50);
            this._PoseLibrary[4].Angles[4] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 39, 50);
            this._PoseLibrary[4].Angles[5] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 158, 50);
            //this._PoseLibrary[4].Angles[6] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 67, 55);
            //this._PoseLibrary[4].Angles[7] = new PoseAngle(JointType.WristRight, JointType.HandRight, 159, 17);

            //Pose 6 - right hand on left sholder (cross_hand_shoulder.xml)
            this._PoseLibrary[5] = new Pose();
            this._PoseLibrary[5].Title = "Right Hand Left Shoulder";
            this._PoseLibrary[5].Angles = new PoseAngle[6];
            this._PoseLibrary[5].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 218, 15); //10
            this._PoseLibrary[5].Angles[1] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 325, 15); //10
            this._PoseLibrary[5].Angles[2] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 255, 15);
            this._PoseLibrary[5].Angles[3] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 271, 55);
            this._PoseLibrary[5].Angles[4] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 262, 10);
            this._PoseLibrary[5].Angles[5] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 152, 20); //19
            //this._PoseLibrary[5].Angles[6] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 265, 10);
            //this._PoseLibrary[5].Angles[7] = new PoseAngle(JointType.WristRight, JointType.HandRight, 164, 22);

            //Pose 7 - left hand on right sholder (cross_hand_shoulder.xml)
            this._PoseLibrary[6] = new Pose();
            this._PoseLibrary[6].Title = "Left Hand Right Shoulder";
            this._PoseLibrary[6].Angles = new PoseAngle[6];
            this._PoseLibrary[6].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 218, 15); //10
            this._PoseLibrary[6].Angles[1] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 328, 15);//10
            this._PoseLibrary[6].Angles[2] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 268, 55);
            this._PoseLibrary[6].Angles[3] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 289, 15);
            this._PoseLibrary[6].Angles[4] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 35, 20); //12
            this._PoseLibrary[6].Angles[5] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 275, 15);//10
            //this._PoseLibrary[6].Angles[6] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 24, 20);
            //this._PoseLibrary[6].Angles[7] = new PoseAngle(JointType.WristRight, JointType.HandRight, 277, 12);

            //Pose 8 - Hands front (hands_front.xml)
            this._PoseLibrary[7] = new Pose();
            this._PoseLibrary[7].Title = "Hands Front";
            this._PoseLibrary[7].Angles = new PoseAngle[6];
            this._PoseLibrary[7].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 212, 10);
            this._PoseLibrary[7].Angles[1] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 329, 10);
            this._PoseLibrary[7].Angles[2] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 258, 15);//10
            this._PoseLibrary[7].Angles[3] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 279, 15);//10
            this._PoseLibrary[7].Angles[4] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 320, 15); //11
            this._PoseLibrary[7].Angles[5] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 222, 15);//10
            //this._PoseLibrary[7].Angles[6] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 303, 26);
            //this._PoseLibrary[7].Angles[7] = new PoseAngle(JointType.WristRight, JointType.HandRight, 232, 34);

            //Pose 9 - hands on face (hands_face.xml)
            this._PoseLibrary[8] = new Pose();
            this._PoseLibrary[8].Title = "Hands on face";
            this._PoseLibrary[8].Angles = new PoseAngle[8];
            this._PoseLibrary[8].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.Head, 89, 10);
            this._PoseLibrary[8].Angles[1] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 92, 20);
            this._PoseLibrary[8].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 214, 15);//10
            this._PoseLibrary[8].Angles[3] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 333, 15);//10
            this._PoseLibrary[8].Angles[4] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 205, 82);
            this._PoseLibrary[8].Angles[5] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 228, 114);
            this._PoseLibrary[8].Angles[6] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 55, 13);
            this._PoseLibrary[8].Angles[7] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 126, 16);
            //this._PoseLibrary[8].Angles[8] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 57, 18);
            //this._PoseLibrary[8].Angles[9] = new PoseAngle(JointType.WristRight, JointType.HandRight, 125, 24);

            //Pose 10 - hands on head, like scratch (hands_head.xml)
            this._PoseLibrary[9] = new Pose();
            this._PoseLibrary[9].Title = "Right Hand Head";
            this._PoseLibrary[9].Angles = new PoseAngle[5];
            this._PoseLibrary[9].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.Head, 88, 15);
            this._PoseLibrary[9].Angles[1] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 91, 10);
            //this._PoseLibrary[9].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 213, 10);
            this._PoseLibrary[9].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 336, 15);//10
            //this._PoseLibrary[9].Angles[4] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 254, 10);
            this._PoseLibrary[9].Angles[3] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 51, 35);//10
            //this._PoseLibrary[9].Angles[6] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 267, 13);
            this._PoseLibrary[9].Angles[4] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 164, 35);//10
            //this._PoseLibrary[9].Angles[8] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 269, 10);
            //this._PoseLibrary[9].Angles[9] = new PoseAngle(JointType.WristRight, JointType.HandRight, 126, 25);

            //Pose 11 - hands on head, like scratch (hands_head.xml) 
            this._PoseLibrary[10] = new Pose();
            this._PoseLibrary[10].Title = "Left Hand Head ";
            this._PoseLibrary[10].Angles = new PoseAngle[5];
            this._PoseLibrary[10].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.Head, 94, 15);
            this._PoseLibrary[10].Angles[1] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 91, 10);
            this._PoseLibrary[10].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 199, 15);
            //this._PoseLibrary[10].Angles[3] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 329, 10);
            this._PoseLibrary[10].Angles[3] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 133, 35);
            ///this._PoseLibrary[10].Angles[5] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 290, 10);
            this._PoseLibrary[10].Angles[4] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 19, 35);
            //this._PoseLibrary[10].Angles[7] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 279, 10);
            //this._PoseLibrary[10].Angles[8] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 147, 136);
            //this._PoseLibrary[10].Angles[9] = new PoseAngle(JointType.WristRight, JointType.HandRight, 271, 10);

            this._PoseLibrary[11] = new Pose();
            this._PoseLibrary[11].Title = "Balancing";
            this._PoseLibrary[11].Angles = new PoseAngle[6];
            this._PoseLibrary[11].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 215, 15); //10
            this._PoseLibrary[11].Angles[1] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 332, 15);//10
            this._PoseLibrary[11].Angles[2] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 238, 35);//12
            this._PoseLibrary[11].Angles[3] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 313, 25);//12
            this._PoseLibrary[11].Angles[4] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 218, 35); 
            this._PoseLibrary[11].Angles[5] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 326, 25);
            //this._PoseLibrary[11].Angles[6] = new PoseAngle(JointType.WristLeft, JointType.HandLeft,203, 15);
            //this._PoseLibrary[11].Angles[7] = new PoseAngle(JointType.WristRight, JointType.HandRight, 330, 15);


            this._PoseLibrary[12] = new Pose();
            this._PoseLibrary[12].Title = "Hi Five Left";
            this._PoseLibrary[12].Angles = new PoseAngle[6];
            this._PoseLibrary[12].Angles[0] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 83, 10);
            this._PoseLibrary[12].Angles[1] = new PoseAngle(JointType.HipCenter, JointType.Spine, 102, 12);
            this._PoseLibrary[12].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 192, 20);//10
            //this._PoseLibrary[12].Angles[3] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 314, 10);
            this._PoseLibrary[12].Angles[3] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 112, 30);//10
            //this._PoseLibrary[12].Angles[5] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 282, 10);
            this._PoseLibrary[12].Angles[4] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 89, 20);//10
            //this._PoseLibrary[12].Angles[7] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 283, 10);
            this._PoseLibrary[12].Angles[5] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 89, 30);//22
            //this._PoseLibrary[12].Angles[9] = new PoseAngle(JointType.WristRight, JointType.HandRight, 276, 10);

            this._PoseLibrary[13] = new Pose();
            this._PoseLibrary[13].Title = "Hi Five Right";
            this._PoseLibrary[13].Angles = new PoseAngle[6];
            this._PoseLibrary[13].Angles[0] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 97, 11);
            this._PoseLibrary[13].Angles[1] = new PoseAngle(JointType.HipCenter, JointType.Spine, 82, 10);
            //this._PoseLibrary[13].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 223, 15);
            this._PoseLibrary[13].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 294, 57);
            //this._PoseLibrary[13].Angles[4] = new PoseAngle(Join tType.ShoulderLeft, JointType.ElbowLeft, 259, 11);
            this._PoseLibrary[13].Angles[3] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 69, 30);//18
            //this._PoseLibrary[13].Angles[6] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 257, 10);
            this._PoseLibrary[13].Angles[4] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 92, 44);
            //this._PoseLibrary[13].Angles[8] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 260, 10);
            this._PoseLibrary[13].Angles[5] = new PoseAngle(JointType.WristRight, JointType.HandRight, 96, 60);

            this._PoseLibrary[14] = new Pose();
            this._PoseLibrary[14].Title = "Hold";
            this._PoseLibrary[14].Angles = new PoseAngle[6];
            //this._PoseLibrary[14].Angles[0] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 89, 10);
            //this._PoseLibrary[14].Angles[1] = new PoseAngle(JointType.HipCenter, JointType.Spine, 87, 10);
            this._PoseLibrary[14].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 217, 20);//10
            this._PoseLibrary[14].Angles[1] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 321, 20);//10
            this._PoseLibrary[14].Angles[2] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 227, 50);//12
            this._PoseLibrary[14].Angles[3] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 290, 50);//15
            this._PoseLibrary[14].Angles[4] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 133, 80);//28
            this._PoseLibrary[14].Angles[5] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 106, 80);//58
            //this._PoseLibrary[14].Angles[8] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 139, 67);
            //this._PoseLibrary[14].Angles[9] = new PoseAngle(JointType.WristRight, JointType.HandRight, 126, 55);

            this._PoseLibrary[15] = new Pose();
            this._PoseLibrary[15].Title = "Ventilate Right Hand";
            this._PoseLibrary[15].Angles = new PoseAngle[4];
            //this._PoseLibrary[15].Angles[0] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 92, 10);
            //this._PoseLibrary[15].Angles[1] = new PoseAngle(JointType.HipCenter, JointType.Spine, 90, 11);
            //this._PoseLibrary[15].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 219, 10);
            this._PoseLibrary[15].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 327, 10);
            //this._PoseLibrary[15].Angles[2] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 252, 10);
            this._PoseLibrary[15].Angles[1] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 246, 103);
            //this._PoseLibrary[15].Angles[4] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 259, 10);
            this._PoseLibrary[15].Angles[2] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 70, 67);
            //this._PoseLibrary[15].Angles[6] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 270, 10);
            this._PoseLibrary[15].Angles[3] = new PoseAngle(JointType.WristRight, JointType.HandRight, 72, 58);

            this._PoseLibrary[16] = new Pose();
            this._PoseLibrary[16].Title = "Ventilate Left Hand";
            this._PoseLibrary[16].Angles = new PoseAngle[4];
            //this._PoseLibrary[16].Angles[0] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 90, 10);
            //this._PoseLibrary[16].Angles[1] = new PoseAngle(JointType.HipCenter, JointType.Spine, 93, 10);
            this._PoseLibrary[16].Angles[0] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 212, 10);
            //this._PoseLibrary[16].Angles[1] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 324, 10);
            this._PoseLibrary[16].Angles[1] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 184, 40);
            //this._PoseLibrary[16].Angles[3] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 292, 10);
            this._PoseLibrary[16].Angles[2] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 145, 130);
            //this._PoseLibrary[16].Angles[5] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 282, 10);
            this._PoseLibrary[16].Angles[3] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 154, 105);
            //this._PoseLibrary[16].Angles[7] = new PoseAngle(JointType.WristRight, JointType.HandRight, 269, 10);
            
            this._PoseLibrary[17] = new Pose();
            this._PoseLibrary[17].Title = "Spear Throw Right";
            this._PoseLibrary[17].Angles = new PoseAngle[6];
            this._PoseLibrary[17].Angles[0] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 97, 11);
            this._PoseLibrary[17].Angles[1] = new PoseAngle(JointType.HipCenter, JointType.Spine, 82, 10);
            //this._PoseLibrary[17].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 223, 15);
            this._PoseLibrary[17].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 294, 57);
            //this._PoseLibrary[17].Angles[4] = new PoseAngle(Join tType.ShoulderLeft, JointType.ElbowLeft, 259, 11);
            this._PoseLibrary[17].Angles[3] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 69, 30);//18
            //this._PoseLibrary[17].Angles[6] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 257, 10);
            this._PoseLibrary[17].Angles[4] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 92, 44);
            //this._PoseLibrary[17].Angles[8] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 260, 10);
            this._PoseLibrary[17].Angles[5] = new PoseAngle(JointType.WristRight, JointType.HandRight, 96, 60);

            this._PoseLibrary[18] = new Pose();
            this._PoseLibrary[18].Title = "Spear Throw Left";
            this._PoseLibrary[18].Angles = new PoseAngle[6];
            this._PoseLibrary[18].Angles[0] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 83, 10);
            this._PoseLibrary[18].Angles[1] = new PoseAngle(JointType.HipCenter, JointType.Spine, 102, 12);
            this._PoseLibrary[18].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 192, 20);//10
            //this._PoseLibrary[18].Angles[3] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 314, 10);
            this._PoseLibrary[18].Angles[3] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 112, 30);//10
            //this._PoseLibrary[18].Angles[5] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 282, 10);
            this._PoseLibrary[18].Angles[4] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 89, 20);//10
            //this._PoseLibrary[18].Angles[7] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 283, 10);
            this._PoseLibrary[18].Angles[5] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 89, 30);//22
            //this._PoseLibrary[18].Angles[9] = new PoseAngle(JointType.WristRight, JointType.HandRight, 276, 10);

            this._PoseLibrary[19] = new Pose();
            this._PoseLibrary[19].Title = "Striking";
            this._PoseLibrary[19].Angles = new PoseAngle[8];
            this._PoseLibrary[19].Angles[0] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 91, 10);
            this._PoseLibrary[19].Angles[1] = new PoseAngle(JointType.HipCenter, JointType.Spine, 83, 14);
            this._PoseLibrary[19].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 214, 10);
            this._PoseLibrary[19].Angles[3] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 330, 10);
            this._PoseLibrary[19].Angles[4] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 246, 13);
            this._PoseLibrary[19].Angles[5] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 287, 23);
            this._PoseLibrary[19].Angles[6] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 204, 153);
            this._PoseLibrary[19].Angles[7] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 198, 53);
            //this._PoseLibrary[19].Angles[8] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 227, 127);
            //this._PoseLibrary[19].Angles[9] = new PoseAngle(JointType.WristRight, JointType.HandRight, 187, 58);
             
            this._PoseLibrary[20] = new Pose();
            this._PoseLibrary[20].Title = "Lift Hand Right";
            this._PoseLibrary[20].Angles = new PoseAngle[6];
            this._PoseLibrary[20].Angles[0] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 93, 10);
            this._PoseLibrary[20].Angles[1] = new PoseAngle(JointType.HipCenter, JointType.Spine, 77, 12);
            //this._PoseLibrary[20].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 224, 10);
            this._PoseLibrary[20].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 321, 10);
            //this._PoseLibrary[20].Angles[4] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 254, 13);
            this._PoseLibrary[20].Angles[3] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 47, 36);
            //this._PoseLibrary[20].Angles[6] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 259, 10);
            this._PoseLibrary[20].Angles[4] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 135, 31);
            //this._PoseLibrary[20].Angles[8] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 266, 10);
            this._PoseLibrary[20].Angles[5] = new PoseAngle(JointType.WristRight, JointType.HandRight, 149, 31);

            this._PoseLibrary[21] = new Pose();
            this._PoseLibrary[21].Title = "Lift Hand Left";
            this._PoseLibrary[21].Angles = new PoseAngle[6];
            this._PoseLibrary[21].Angles[0] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 88, 10);
            this._PoseLibrary[21].Angles[1] = new PoseAngle(JointType.HipCenter, JointType.Spine, 106, 10);
            this._PoseLibrary[21].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 216, 10);
            //this._PoseLibrary[21].Angles[3] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 320, 10);
            this._PoseLibrary[21].Angles[3] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 144, 22);
            //this._PoseLibrary[21].Angles[5] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 285, 10);
            this._PoseLibrary[21].Angles[4] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 72, 28);
            //this._PoseLibrary[21].Angles[7] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 282, 10);
            this._PoseLibrary[21].Angles[5] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 65, 33);
            //this._PoseLibrary[21].Angles[9] = new PoseAngle(JointType.WristRight, JointType.HandRight, 274, 10);

            this._PoseLibrary[22] = new Pose();
            this._PoseLibrary[22].Title = "Lift Hands";
            this._PoseLibrary[22].Angles = new PoseAngle[10];
            this._PoseLibrary[22].Angles[0] = new PoseAngle(JointType.Spine, JointType.ShoulderCenter, 90, 10);
            this._PoseLibrary[22].Angles[1] = new PoseAngle(JointType.HipCenter, JointType.Spine, 91, 10);
            this._PoseLibrary[22].Angles[2] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderLeft, 224, 10);
            this._PoseLibrary[22].Angles[3] = new PoseAngle(JointType.ShoulderCenter, JointType.ShoulderRight, 321, 10);
            this._PoseLibrary[22].Angles[4] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 127, 29);
            this._PoseLibrary[22].Angles[5] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 73, 31);
            this._PoseLibrary[22].Angles[6] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 55, 10);
            this._PoseLibrary[22].Angles[7] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 138, 10);
            this._PoseLibrary[22].Angles[8] = new PoseAngle(JointType.WristLeft, JointType.HandLeft, 59, 40);
            this._PoseLibrary[22].Angles[9] = new PoseAngle(JointType.WristRight, JointType.HandRight, 137, 32);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="pose"></param>
        /// <returns></returns> 
        private bool IsPose(Skeleton skeleton, Pose pose)
        {
            bool isPose = true;
            double angle;
            double poseAngle;
            double poseThreshold;
            double loAngle;
            double hiAngle;

            for (int i = 0; i < pose.Angles.Length && isPose; i++)
            {
                poseAngle = pose.Angles[i].Angle;
                poseThreshold = pose.Angles[i].Threshold;
                angle = SkeletonViewerElement.GetJointAngle(skeleton.Joints[pose.Angles[i].CenterJoint], skeleton.Joints[pose.Angles[i].AngleJoint]);

                hiAngle = poseAngle + poseThreshold;
                loAngle = poseAngle - poseThreshold;

                if (hiAngle >= 360 || loAngle < 0)
                {
                    loAngle = (loAngle < 0) ? 360 + loAngle : loAngle;
                    hiAngle = hiAngle % 360;

                    isPose = !(loAngle > angle && angle > hiAngle);
                }
                else
                {
                    isPose = (loAngle <= angle && hiAngle >= angle);
                }
            }

            return isPose;
        }
         
        private void PrintPose(Skeleton skeleton)
        {
            /*foreach(Pose x in this._PoseLibrary)
            {
                if (IsPose(skeleton, this._StartPose))
                {
                    PoseDetected.Foreground = Brushes.Cyan;
                    PoseDetected.Text = this._StartPose.Title;
                }            
                else if (IsPose(skeleton, x))
                {
                    PoseDetected.Foreground = Brushes.Gold;
                    PoseDetected.Text = x.Title;
                    return;           
                }
                else
                {
                    PoseDetected.Foreground = Brushes.Red;
                    PoseDetected.Text = "No pose detected.";
                }
            }*/

            for (int i = 11; i < this._PoseLibrary.Length; i++)
            {
                if (IsPose(skeleton, this._StartPose))
                {
                    PoseDetected.Foreground = Brushes.Cyan;
                    PoseDetected.Text = this._StartPose.Title;
                }
                else if (IsPose(skeleton, this._PoseLibrary[i]))
                {
                    PoseDetected.Foreground = Brushes.Gold;
                    PoseDetected.Text = this._PoseLibrary[i].Title;
                    return;
                }
                else
                {
                    PoseDetected.Foreground = Brushes.Red;
                    PoseDetected.Text = "No pose detected.";
                }

            }
         }

        private bool isJointsNull(JointObject[] j)
        {
            for (int i = 0; i < j.Length; i++)
            {
                if (i!= 9 && j[i] == null)
                    return true;
            }
            return false;
        }
        
        private void DebugAngles_Checked(object sender, RoutedEventArgs e)
        {
            if (AnglesBorder.Visibility.Equals(Visibility.Collapsed))
                AnglesBorder.Visibility = Visibility.Visible;
            else if (AnglesBorder.Visibility.Equals(Visibility.Visible))
                AnglesBorder.Visibility = Visibility.Collapsed;
        }

        private void debugAngles()
        {
            JointObject[] j = SkeletonViewerElement.joints;
            if (j != null) {
                for (int i = 0; i < j.Length && (j[i] != null); i++)
                {
                    switch (j[i].getObjectJointName())
                    {
                        case "AnkleLeft": AnkleLeft.Text = String.Format("{0:0.00}", j[0].getObjectJointAngle()); break;
                        case "AnkleRight": AnkleRight.Text = String.Format("{0:0.00}", j[1].getObjectJointAngle()); break;
                        case "ElbowLeft": ElbowLeft.Text = String.Format("{0:0.00}", j[2].getObjectJointAngle()); break;
                        case "ElbowRight": ElbowRight.Text = String.Format("{0:0.00}", j[3].getObjectJointAngle()); break;
                        case "FootLeft": FootLeft.Text = String.Format("{0:0.00}", j[4].getObjectJointAngle()); break;
                        case "FootRight": FootRight.Text = String.Format("{0:0.00}", j[5].getObjectJointAngle()); break;
                        case "HandLeft": HandLeft.Text = String.Format("{0:0.00}", j[6].getObjectJointAngle()); break;
                        case "HandRight": HandRight.Text = String.Format("{0:0.00}", j[7].getObjectJointAngle()); break;
                        case "Head": Head.Text = String.Format("{0:0.00}", j[8].getObjectJointAngle()); break;
                        case "HipCenter": HipCenter.Text = String.Format("{0:0.00}", j[9].getObjectJointAngle()); break;
                        case "HipLeft": HipLeft.Text = String.Format("{0:0.00}", j[10].getObjectJointAngle()); break;
                        case "HipRight": HipRight.Text = String.Format("{0:0.00}", j[11].getObjectJointAngle()); break;
                        case "KneeLeft": KneeLeft.Text = String.Format("{0:0.00}", j[12].getObjectJointAngle()); break;
                        case "KneeRight": KneeRight.Text = String.Format("{0:0.00}", j[13].getObjectJointAngle()); break;
                        case "ShoulderCenter": ShoulderCenter.Text = String.Format("{0:0.00}", j[14].getObjectJointAngle()); break;
                        case "ShoulderLeft": ShoulderLeft.Text = String.Format("{0:0.00}", j[15].getObjectJointAngle()); break;
                        case "ShoulderRight": ShoulderRight.Text = String.Format("{0:0.00}", j[16].getObjectJointAngle()); break;
                        case "Spine": Spine.Text = String.Format("{0:0.00}", j[17].getObjectJointAngle()); break;
                        case "WristLeft": WristLeft.Text = String.Format("{0:0.00}", j[18].getObjectJointAngle()); break;
                        case "WristRight": WristRight.Text = String.Format("{0:0.00}", j[19].getObjectJointAngle()); break;
                    }
                }
            }
        }

        private void Head_Click(object sender, RoutedEventArgs e)
        {
            head = head ? false : true;
            LastAction.Text += "Head Selected\n";
        }

        private void LeftArm_Click(object sender, RoutedEventArgs e)
        {
            leftArm = leftArm ? false : true;
            LastAction.Text += "Left Arm Selected\n";
        }

        private void LeftLeg_Click(object sender, RoutedEventArgs e)
        {
            leftLeg = leftLeg ? false : true;
            LastAction.Text += "Left Leg Selected\n";
        }

        private void RightArm_Click(object sender, RoutedEventArgs e)
        {
            rightArm = rightArm ? false : true;
            LastAction.Text += "Right Arm Selected\n";
        }

        private void RightLeg_Click(object sender, RoutedEventArgs e)
        {
            rightLeg = rightLeg ? false : true;
            LastAction.Text += "Right Leg Selected\n";
        }

        private void Torso_Click(object sender, RoutedEventArgs e)
        {
            torso = torso ? false : true;
            LastAction.Text += "Torso Selected\n";
        }

        private void Capture(object sender, RoutedEventArgs e)
        {
            JointObject[] j = SkeletonViewerElement.joints;
            if (torso || rightLeg || rightArm || leftLeg || leftArm || head)
            {
                if (!isJointsNull(j))
                {
                    if (head)
                    {
                        if (jointStats[0] == null)
                            jointStats[0] = new JointStats("Head");
                        jointStats[0].addCapture(totalCaptures, Math.Round(j[8].getObjectJointAngle(), 2));
                        if (jointStats[1] == null)
                            jointStats[1] = new JointStats("Shoulder Center");
                        jointStats[1].addCapture(totalCaptures, Math.Round(j[14].getObjectJointAngle(), 2));

                        jointCount += 2;
                    }

                    if (torso)
                    {
                        if (!head)
                        {
                            if (jointStats[1] == null)
                                jointStats[1] = new JointStats("Shoulder Center");
                            jointStats[1].addCapture(totalCaptures, Math.Round(j[14].getObjectJointAngle(), 2));

                            jointCount++;
                        }
                        if (jointStats[2] == null)
                            jointStats[2] = new JointStats("Spine");
                        jointStats[2].addCapture(totalCaptures, Math.Round(j[17].getObjectJointAngle(), 2));
                        if (jointStats[3] == null)
                            jointStats[3] = new JointStats("Hip Center");
                        jointStats[3].addCapture(totalCaptures, Math.Round(j[9].getObjectJointAngle(), 2));

                        jointCount += 2;
                    }

                    if (leftArm)
                    {
                        if (jointStats[4] == null)
                            jointStats[4] = new JointStats("Shoulder Left");
                        jointStats[4].addCapture(totalCaptures, Math.Round(j[15].getObjectJointAngle(), 2));
                        if (jointStats[6] == null)
                            jointStats[6] = new JointStats("Elbow Left");
                        jointStats[6].addCapture(totalCaptures, Math.Round(j[2].getObjectJointAngle(), 2));
                        if (jointStats[8] == null)
                            jointStats[8] = new JointStats("Wrist Left");
                        jointStats[8].addCapture(totalCaptures, Math.Round(j[18].getObjectJointAngle(), 2));
                        if (jointStats[10] == null)
                            jointStats[10] = new JointStats("Hand Left");
                        jointStats[10].addCapture(totalCaptures, Math.Round(j[6].getObjectJointAngle(), 2));

                        jointCount += 4;
                    }

                    if (rightArm)
                    {
                        {
                            if (jointStats[5] == null)
                                jointStats[5] = new JointStats("Shoulder Right");
                            jointStats[5].addCapture(totalCaptures, Math.Round(j[16].getObjectJointAngle(), 2));
                            if (jointStats[7] == null)
                                jointStats[7] = new JointStats("Elbow Right");
                            jointStats[7].addCapture(totalCaptures, Math.Round(j[3].getObjectJointAngle(), 2));
                            if (jointStats[9] == null)
                                jointStats[9] = new JointStats("Wrist Right");
                            jointStats[9].addCapture(totalCaptures, Math.Round(j[19].getObjectJointAngle(), 2));
                            if (jointStats[11] == null)
                                jointStats[11] = new JointStats("Hand Right");
                            jointStats[11].addCapture(totalCaptures, Math.Round(j[7].getObjectJointAngle(), 2));

                            jointCount += 4;
                        }
                    }

                    if (leftLeg)
                    {
                        if (!torso)
                        {
                            if (jointStats[3] == null)
                                jointStats[3] = new JointStats("Hip Center");
                            jointStats[3].addCapture(totalCaptures, Math.Round(j[9].getObjectJointAngle(), 2));

                            jointCount += 1;
                        }

                        if (jointStats[12] == null)
                            jointStats[12] = new JointStats("Hip Left");
                        jointStats[12].addCapture(totalCaptures, Math.Round(j[10].getObjectJointAngle(), 2));
                        if (jointStats[14] == null)
                            jointStats[14] = new JointStats("Knee Left");
                        jointStats[14].addCapture(totalCaptures, Math.Round(j[12].getObjectJointAngle(), 2));
                        if (jointStats[16] == null)
                            jointStats[16] = new JointStats("Ankle Left");
                        jointStats[16].addCapture(totalCaptures, Math.Round(j[0].getObjectJointAngle(), 2));
                        if (jointStats[18] == null)
                            jointStats[18] = new JointStats("Foot Left");
                        jointStats[18].addCapture(totalCaptures, Math.Round(j[4].getObjectJointAngle(), 2));

                        jointCount += 4;
                    }

                    if (rightLeg)
                    {
                        if (!torso)
                        {
                            if (jointStats[3] == null)
                                jointStats[3] = new JointStats("Hip Center");
                            jointStats[3].addCapture(totalCaptures, Math.Round(j[9].getObjectJointAngle(), 2));

                            jointCount += 1;
                        }
                        if (jointStats[13] == null)
                            jointStats[13] = new JointStats("Hip Right");
                        jointStats[13].addCapture(totalCaptures, Math.Round(j[11].getObjectJointAngle(), 2));
                        if (jointStats[15] == null)
                            jointStats[15] = new JointStats("Knee Right");
                        jointStats[15].addCapture(totalCaptures, Math.Round(j[13].getObjectJointAngle(), 2));
                        if (jointStats[17] == null)
                            jointStats[17] = new JointStats("Ankle Right");
                        jointStats[17].addCapture(totalCaptures, Math.Round(j[1].getObjectJointAngle(), 2));
                        if (jointStats[19] == null)
                            jointStats[19] = new JointStats("Foot Right");
                        jointStats[19].addCapture(totalCaptures, Math.Round(j[5].getObjectJointAngle(), 2));

                        jointCount += 4;
                    }

                    totalJointCount = jointCount;
                    totalCaptures++;
                    PoseCount.Text = String.Format("{0:0}", totalCaptures);
                    JointCount.Text = String.Format("{0:0}", totalJointCount);
                }
                else
                    ShowPopupOffsetClicked(sender, e, "NullJoints");
            }
            else
                ShowPopupOffsetClicked(sender, e, "UnselectedJoints");
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            if(totalCaptures>0)
                totalCaptures--;
            PoseCount.Text = String.Format("{0:0}", totalCaptures);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            results.AddLast("</rule>");
            System.IO.File.WriteAllLines(@generateFileName(), results);
            System.IO.File.WriteAllLines(@generateFileName(), debugFile);
            ShowPopupOffsetClicked(sender, e, "SavedPose");
        }

        private double generateErrorMargin(double max, double min, double avg)
        {
            //take the smallest difference between the max and the min of the values per joint
            double error = Math.Min(Math.Abs(max - avg), Math.Abs(min - avg));

            if (error < 10)
                return 10;
            return error;
        }

        private String generateFileName()
        {
            String filename = "C:\\Users\\Caro\\Desktop\\FileName.txt";
            // Create an instance of the open file dialog box.
            SaveFileDialog saveFile = new SaveFileDialog();

            // Set filter options and filter index.
            saveFile.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*s";
            saveFile.FilterIndex = 2;

            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = Convert.ToBoolean(saveFile.ShowDialog());

            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                return saveFile.FileName; 
            }
            return filename;
        }

        private void NewPoseType(object sender, RoutedEventArgs e)
        {
            PoseType.Text = "";
            LastAction.Text += "New Pose Type\n";
        }

        private void NewPose(object sender, RoutedEventArgs e)
        {
            PoseName.Text = "";
            poseTypeCount = 0;
            LastAction.Text += "New Pose\n";
        }

        private void SavePose(object sender, RoutedEventArgs e)
        {
            results.AddLast("   </name>");
            LastAction.Text += "Pose " + PoseName.Text + "saved \n";
        }

        private void SavePoseType(object sender, RoutedEventArgs e)
        {
            //if (firstPoseType of the poseName) then print:
            if (poseTypeCount == 0)
                results.AddLast("   <name '" + PoseName.Text + "'>");
            results.AddLast("       <pose '" + PoseType.Text + "'>");
            debugFile.AddLast("---------------------" + PoseName.Text + "---------------------");
            debugFile.AddLast("---------------------" + PoseType.Text + "---------------------");
            for (int i = 0; i < jointStats.Length; i++)
            {
                if (jointStats[i] != null)
                {
                    jointStats[i].Max = jointStats[i].PerCapture.Max();
                    jointStats[i].Min = jointStats[i].PerCapture.Min();
                    jointStats[i].Avg = jointStats[i].PerCapture.Average();
                    jointStats[i].Range = Math.Round(Math.Abs(jointStats[i].Max - jointStats[i].Min), 1);
                    results.AddLast("           <joint id='" + jointStats[i].JointName
                        + "' angle='" + jointStats[i].Avg.ToString() + 
                        "' error='" + generateErrorMargin(jointStats[i].Max, jointStats[i].Min, jointStats[i].Avg) + "'/>");
                    debugFile.AddLast("...*" + jointStats[i].JointName + "*...");
                    debugFile.AddLast("Max: " + jointStats[i].Max.ToString());
                    debugFile.AddLast("Min: " + jointStats[i].Min.ToString());
                    debugFile.AddLast("Avg: " + jointStats[i].Avg.ToString());
                    debugFile.AddLast("Range: " + jointStats[i].Range.ToString());
                    debugFile.AddLast("Values: ");
                    for (int j = 0; j < totalCaptures; j++)
                    {
                        debugFile.AddLast("         " + jointStats[i].PerCapture.ElementAt(j).ToString());
                    }
                    debugFile.AddLast(" ");
                    jointStats[i] = null;
                }
            }
            results.AddLast("       </pose>");
            totalCaptures = 0;
            poseTypeCount++;
            LastAction.Text += "Pose Type " +PoseType.Text+ "saved \n";
        }

        // Handles the Click event on the Button inside the Popup control and 
        // closes the Popup. 
        private void ClosePopupClicked(object sender, RoutedEventArgs e)
        {
            // if the Popup is open, then close it 
            if (SavedPose.IsOpen) { SavedPose.IsOpen = false; }
            if (NullJoints.IsOpen) { NullJoints.IsOpen = false; }
            if (HelpPop.IsOpen) { HelpPop.IsOpen = false; }
            if (UnselectedJoints.IsOpen) { UnselectedJoints.IsOpen = false; }
        }

        // Handles the Click event on the Button on the page and opens the Popup. 
        private void ShowPopupOffsetClicked(object sender, RoutedEventArgs e, String type)
        {
            // open the Popup if it isn't open already 
            if (type == "SavedPose" && !SavedPose.IsOpen) { SavedPose.IsOpen = true; }
            else if (type == "NullJoints" && !NullJoints.IsOpen) { NullJoints.IsOpen = true; }
            else if (type == "UnselectedJoints" && !UnselectedJoints.IsOpen) { UnselectedJoints.IsOpen = true; } 
        }

        private void Help(object sender, RoutedEventArgs e)
        {
            if (!HelpPop.IsOpen) { HelpPop.IsOpen = true; }
        }

       #endregion Methods

        #region Properties
        public KinectSensor Kinect
        {
            get { return this._Kinect; }
            set
            {
                if (this._Kinect != value)
                {
                    //Uninitialize
                    if (this._Kinect != null)
                    {
                        this._Kinect.Stop();
                        _Kinect.ColorFrameReady -= Kinect_ColorFrameReady;
                        this._Kinect.ColorStream.Disable();
                        this._Kinect.SkeletonFrameReady -= Kinect_SkeletonFrameReady;
                        this._Kinect.SkeletonStream.Disable();
                        SkeletonViewerElement.KinectDevice = null;
                        this._FrameSkeletons = null;
                    }

                    this._Kinect = value;

                    //Initialize
                    if (this._Kinect != null)
                    {
                        if (this._Kinect.Status == KinectStatus.Connected)
                        {
                            ColorImageStream colorStream = _Kinect.ColorStream;

                            colorStream.Enable();

                            this._ColorImageBitmap = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                            this._ColorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth, colorStream.FrameHeight);
                            this._ColorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                            this.ColorImageElement.Source = this._ColorImageBitmap;
                            this._ColorImagePixelData = new byte[colorStream.FramePixelDataLength];
                            _Kinect.ColorFrameReady += Kinect_ColorFrameReady;

                            // Skeleton Stream
                            this._Kinect.SkeletonStream.Enable(new TransformSmoothParameters()
                            {
                                Smoothing = 0.7f,//.5
                                Correction = 0.5f,
                                Prediction = 0.5f,
                                JitterRadius = 0.1f,//.05
                                MaxDeviationRadius = 0.1f//.04
                            });

                            //this._Kinect.SkeletonStream.Enable();
                            this._FrameSkeletons = new Skeleton[this._Kinect.SkeletonStream.FrameSkeletonArrayLength];

                          

                            this._Kinect.Start();

                            SkeletonViewerElement.KinectDevice = this.Kinect;
                            this.Kinect.SkeletonFrameReady += Kinect_SkeletonFrameReady;
                        }
                    }
                }
            }
        }
        #endregion Properties
    }
}