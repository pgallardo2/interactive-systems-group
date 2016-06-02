using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;

namespace GestureGenerator
{
    public class JointObject
    {
        //public JointType
        private Joint kinectJoint;
        private String jointName;
        private double jointAngle;

        
        //public void setKinectJoint(Joint j)
        public JointObject(Joint j)
        {
            this.kinectJoint = j;
            this.jointName = j.JointType.ToString();
        }

        public void setObjectJointAngle(double angle)
        {
            this.jointAngle = angle;
        }

        public Joint getKinectJoint()
        {
            return this.kinectJoint;
        }

        public String getObjectJointName()
        {
            return this.jointName;
        }

        public double getObjectJointAngle()
        {
            return this.jointAngle;
        }

    }
}
