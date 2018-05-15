using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.BodyAnalysis
{
    public class PowerTraining
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
//            _distance = Geometry.distanceSquare(new Point3D(sk.Joints[JointType.HandLeft].Position), new Point3D(sk.Joints[JointType.HandRight].Position));

            Point3D leftpos = new Point3D(sk.Joints[JointType.HipLeft].Position);
            Point3D rightpos = new Point3D(sk.Joints[JointType.HipRight].Position);
            Point3D hipcentre = new Point3D(sk.Joints[JointType.HipCenter].Position);
            Point3D shoulder = new Point3D(sk.Joints[JointType.ShoulderCenter].Position);

            _distance = shoulder.Z - hipcentre.Z;

            leftpos.X = leftpos.X + (leftpos.X - rightpos.X);
            leftpos.Y = leftpos.Y + (leftpos.Y - rightpos.Y);
            leftpos.Z = leftpos.Z + (leftpos.Z - rightpos.Z);
            rightpos.X = rightpos.X + (rightpos.X - leftpos.X);
            rightpos.Y = rightpos.Y + (rightpos.Y - leftpos.Y);
            rightpos.Z = rightpos.Z + (rightpos.Z - leftpos.Z);

            if (Geometry.distanceSquare(new Point3D(sk.Joints[JointType.HandLeft].Position), leftpos) < 0.03 &
               Geometry.distanceSquare(new Point3D(sk.Joints[JointType.HandRight].Position), rightpos) < 0.03)
            {
                if (GestureRecognized != null)
                {
                    GestureRecognized(this, new EventArgs());
                }
            }
        }
    }

}

