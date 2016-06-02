using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureGenerator
{
    public class JointStats
    {
        public String JointName { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
        public double Avg { get; set; }
        public double Range { get; set; }
        public LinkedList<double> PerCapture { get; set; }
        public JointStats(String jn)
        {
            JointName = jn;
            Max = 0;
            Min = 360;
            Avg = 0;
            Range = 0;
            PerCapture = new LinkedList<double>();
        }
        public void addCapture(int index, double capture)
        {
            //PerCapture.a = capture;
            PerCapture.AddLast(capture);
        }
    }
}
