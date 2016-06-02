using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;

using Microsoft.Kinect;
using System.Windows.Media.Imaging;


namespace GestureGenerator
{
    /// <summary>
    /// Interaction logic for SkeletonViewer.xaml
    /// </summary>
    public partial class SkeletonViewer : UserControl
    {
        #region Member Variables
        private const float FeetPerMeters = 3.2808399f;
        private readonly Brush[] _SkeletonBrushes = new Brush[] { Brushes.Black, Brushes.Yellow, Brushes.Crimson, Brushes.Indigo, Brushes.DodgerBlue, Brushes.Purple, Brushes.Pink };
        int color = 1; // Used to select drawing brush
        static private Skeleton skeleton;
        public JointObject[] joints = new JointObject[20];

        #region Joint declarations     
        /*private Joint ankleLeft; 
        private Joint ankleRight; 
        private Joint elbowLeft;
        private Joint elbowRight; 
        private Joint footLeft;
        private Joint footRight; 
        private Joint handLeft;
        private Joint handRight;
        private Joint head;
        private Joint hipCenter;
        private Joint hipLeft;
        private Joint hipRight;
        private Joint kneeLeft;
        private Joint kneeRight; 
        private Joint shoulderCenter;
        private Joint shoulderLeft; 
        private Joint shoulderRight;
        private Joint spine;
        private Joint wristLeft;
        private Joint wristRight;*/
        #endregion

        #endregion Member Variables


        #region Constructor
        public SkeletonViewer()
        {
            InitializeComponent();
        }
        #endregion Constructor


        #region Methods
        private void KinectDevice_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonsPanel.Children.Clear();
            JointInfoPanel.Children.Clear();

            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {

                    Skeleton[] skeletonData = new Skeleton[frame.SkeletonArrayLength];

                    frame.CopySkeletonDataTo(skeletonData);
                    skeleton = (from s in skeletonData where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();

                    if (this.IsEnabled && skeleton != null)
                    {
                        DrawSkeleton(skeleton, this._SkeletonBrushes[color]);

                        joints[0] = new JointObject(skeleton.Joints[JointType.AnkleLeft]);
                        joints[1] = new JointObject(skeleton.Joints[JointType.AnkleRight]);
                        joints[2] = new JointObject(skeleton.Joints[JointType.ElbowLeft]);
                        joints[3] = new JointObject(skeleton.Joints[JointType.ElbowRight]);
                        joints[4] = new JointObject(skeleton.Joints[JointType.FootLeft]);
                        joints[5] = new JointObject(skeleton.Joints[JointType.FootRight]);
                        joints[6] = new JointObject(skeleton.Joints[JointType.HandLeft]);
                        joints[7] = new JointObject(skeleton.Joints[JointType.HandRight]);
                        joints[8] = new JointObject(skeleton.Joints[JointType.Head]);
                        joints[9] = new JointObject(skeleton.Joints[JointType.HipCenter]);
                        joints[10] = new JointObject(skeleton.Joints[JointType.HipLeft]);
                        joints[11] = new JointObject(skeleton.Joints[JointType.HipRight]);
                        joints[12] = new JointObject(skeleton.Joints[JointType.KneeLeft]);
                        joints[13] = new JointObject(skeleton.Joints[JointType.KneeRight]);
                        joints[14] = new JointObject(skeleton.Joints[JointType.ShoulderCenter]);
                        joints[15] = new JointObject(skeleton.Joints[JointType.ShoulderLeft]);
                        joints[16] = new JointObject(skeleton.Joints[JointType.ShoulderRight]);
                        joints[17] = new JointObject(skeleton.Joints[JointType.Spine]);
                        joints[18] = new JointObject(skeleton.Joints[JointType.WristLeft]);
                        joints[19] = new JointObject(skeleton.Joints[JointType.WristRight]);

                        TrackJoint(joints[0].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[1].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[2].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[3].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[4].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[5].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[6].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[7].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[8].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[9].getKinectJoint(), this._SkeletonBrushes[5]); //HIP CENTER
                        TrackJoint(joints[10].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[11].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[12].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[13].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[14].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[15].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[16].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[17].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[18].getKinectJoint(), this._SkeletonBrushes[color]);
                        TrackJoint(joints[19].getKinectJoint(), this._SkeletonBrushes[color]);
                    }
                }
            }
        }


        private void DrawSkeleton(Skeleton skeleton, Brush brush)
        {
            //if (MainWindow.CheckBox.SkeleTrackerEnabled)
            if (skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked)
            {
                //Draw head and torso
                Polyline figure = CreateFigure(skeleton, brush, new[] { JointType.Head, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.Spine,
                                                                             JointType.ShoulderRight, JointType.ShoulderCenter, JointType.HipCenter});
                SkeletonsPanel.Children.Add(figure);

                figure = CreateFigure(skeleton, brush, new[] { JointType.HipLeft, JointType.HipRight });
                SkeletonsPanel.Children.Add(figure);

                //Draw left leg
                figure = CreateFigure(skeleton, brush, new[] { JointType.HipCenter, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft });
                SkeletonsPanel.Children.Add(figure);

                //Draw right leg
                figure = CreateFigure(skeleton, brush, new[] { JointType.HipCenter, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight });
                SkeletonsPanel.Children.Add(figure);

                //Draw left arm
                figure = CreateFigure(skeleton, brush, new[] { JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft });
                SkeletonsPanel.Children.Add(figure);

                //Draw right arm
                figure = CreateFigure(skeleton, brush, new[] { JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight });
                SkeletonsPanel.Children.Add(figure);
            }
        }


        private Polyline CreateFigure(Skeleton skeleton, Brush brush, JointType[] joints)
        {
            Polyline figure = new Polyline();

            figure.StrokeThickness = 4;
            figure.Stroke = brush;

            for (int i = 0; i < joints.Length; i++)
            {
                figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]]));
            }

            return figure;
        }


        private Point GetJointPoint(Joint joint)
        {
            DepthImagePoint depthPoint = this.KinectDevice.CoordinateMapper.MapSkeletonPointToDepthPoint(joint.Position, this.KinectDevice.DepthStream.Format);
            depthPoint.X *= (int)this.LayoutRoot.ActualWidth / KinectDevice.DepthStream.FrameWidth;
            depthPoint.Y *= (int)this.LayoutRoot.ActualHeight / KinectDevice.DepthStream.FrameHeight;

            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="centerJoint"></param>
        /// <param name="angleJoint"></param>
        /// <returns></returns>
        public double GetJointAngle(Joint zeroJoint, Joint angleJoint)
        {
            Point primaryPoint = GetJointPoint(zeroJoint);
            Point anglePoint = GetJointPoint(angleJoint);
            Point x = new Point(primaryPoint.X + anglePoint.X, primaryPoint.Y);

            double a;
            double b;
            double c;

            a = Math.Sqrt(Math.Pow(primaryPoint.X - anglePoint.X, 2) + Math.Pow(primaryPoint.Y - anglePoint.Y, 2));
            b = anglePoint.X;
            c = Math.Sqrt(Math.Pow(anglePoint.X - x.X, 2) + Math.Pow(anglePoint.Y - x.Y, 2));

            double angleRad = Math.Acos((a * a + b * b - c * c) / (2 * a * b));
            double angleDeg = angleRad * 180 / Math.PI;

            if (primaryPoint.Y < anglePoint.Y)
            {
                angleDeg = 360 - angleDeg;
            }

            return angleDeg;
        }

        private void TrackJoint(Joint joint, Brush brush)
        {
            if (joint.TrackingState != JointTrackingState.NotTracked)
            {
                Canvas container = new Canvas();
                Point jointPoint = GetJointPoint(joint);

                double z = joint.Position.Z * FeetPerMeters;

                Ellipse element = new Ellipse();
                element.Height = 15;
                element.Width = 15;
                element.Fill = brush;
                Canvas.SetLeft(element, 0 - (element.Width / 2));
                Canvas.SetTop(element, 0 - (element.Height / 2));
                container.Children.Add(element);

                TextBlock positionText = new TextBlock();
                int jointIndex = getJointsIndex(joint);
                joints[jointIndex].setObjectJointAngle(GetJointAngle(getZeroJoint(joint), joint));
                //positionText.Text = string.Format("{0:0.0}", joints[jointIndex].getObjectJointAngle());
                positionText.Text = string.Format("<{0:0.00}, {1:0}>", joints[jointIndex].getObjectJointAngle(), jointIndex);  
                positionText.Foreground = brush;
                positionText.FontSize = 16;
                positionText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(positionText, 35);
                Canvas.SetTop(positionText, 15);
                container.Children.Add(positionText);

                Canvas.SetLeft(container, jointPoint.X);
                Canvas.SetTop(container, jointPoint.Y);

                JointInfoPanel.Children.Add(container);
            }
        }

        private int getJointsIndex(Joint joint)
        {
            switch (joint.JointType.ToString())
            {
                case "AnkleLeft": return 0;
                case "AnkleRight": return 1;
                case "ElbowLeft": return 2;
                case "ElbowRight": return 3;
                case "FootLeft": return 4;
                case "FootRight": return 5;
                case "HandLeft": return 6;
                case "HandRight": return 7;
                case "Head": return 8;
                case "HipCenter": return 9;
                case "HipLeft": return 10;
                case "HipRight": return 11;
                case "KneeLeft": return 12;
                case "KneeRight": return 13;
                case "ShoulderCenter": return 14;
                case "ShoulderLeft": return 15;
                case "ShoulderRight": return 16;
                case "Spine": return 17;
                case "WristLeft": return 18;
                case "WristRight": return 19;
                default: return 0; 
            }
        }

        private Joint getZeroJoint(Joint joint)
        {
            switch (joint.JointType.ToString())
            {
                case "Spine": return joints[9].getKinectJoint(); //HipCenter
                case "ShoulderCenter": return joints[17].getKinectJoint();//spine;
                case "Head": return joints[14].getKinectJoint();//shoulderCenter;
                case "ShoulderLeft": return joints[14].getKinectJoint();//shoulderCenter;
                case "ElbowLeft": return joints[15].getKinectJoint();//shoulderLeft;
                case "WristLeft": return joints[2].getKinectJoint();//elbowLeft;
                case "HandLeft": return joints[18].getKinectJoint();//wristLeft;
                case "ShoulderRight": return joints[14].getKinectJoint();//shoulderCenter;
                case "ElbowRight": return joints[16].getKinectJoint();//shoulderRight;
                case "WristRight": return joints[3].getKinectJoint(); //elbowRight;
                case "HandRight": return joints[19].getKinectJoint();//wristRight;
                case "HipLeft": return joints[9].getKinectJoint();//hipCenter;
                case "KneeLeft": return joints[10].getKinectJoint();//hipLeft;
                case "AnkleLeft": return joints[12].getKinectJoint();//kneeLeft;
                case "FootLeft": return joints[0].getKinectJoint();//ankleLeft;
                case "HipRight": return joints[9].getKinectJoint();//hipCenter;
                case "KneeRight": return joints[11].getKinectJoint();//hipRight;
                case "AnkleRight": return joints[13].getKinectJoint();//kneeRight;
                case "FootRight": return joints[1].getKinectJoint();//ankleRight;
                default: return joint;
            }
        }
        #endregion Methods


        #region Properties
        protected const string KinectDevicePropertyName = "KinectDevice";
        public static readonly DependencyProperty KinectDeviceProperty = DependencyProperty.Register(KinectDevicePropertyName, typeof(KinectSensor), typeof(SkeletonViewer), new PropertyMetadata(null, KinectDeviceChanged));

        private static void KinectDeviceChanged(DependencyObject owner, DependencyPropertyChangedEventArgs e)
        {
            SkeletonViewer viewer = (SkeletonViewer)owner;

            if (e.OldValue != null)
            {
                ((KinectSensor)e.OldValue).SkeletonFrameReady -= viewer.KinectDevice_SkeletonFrameReady;
            }

            if (e.NewValue != null)
            {
                viewer.KinectDevice = (KinectSensor)e.NewValue;
                viewer.KinectDevice.SkeletonFrameReady += viewer.KinectDevice_SkeletonFrameReady;
            }
        }

        public KinectSensor KinectDevice
        {
            get { return (KinectSensor)GetValue(KinectDeviceProperty); }
            set { SetValue(KinectDeviceProperty, value); }
        }
        #endregion Properties
    }
}
