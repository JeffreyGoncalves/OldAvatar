using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.BodyAnalysis
{
    public class HandTraining
    {
        public event EventHandler GestureRecognized;

        double _distance;

        public double Distance
        {
            get
            {
                return _distance;
            }
        }

        public void Update(Skeleton sk)
        {

            _distance = Geometry.distanceSquare(new Point3D(sk.Joints[JointType.HandLeft].Position), new Point3D(sk.Joints[JointType.HandRight].Position));
            
                if (_distance < 0.05)
            {

                if (GestureRecognized != null)
                {
                    GestureRecognized(this, new EventArgs());
                }
            }
        }
    }

}
