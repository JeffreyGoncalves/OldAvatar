using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.BodyAnalysis
{
    public class FaceTraining
    {
        public event EventHandler GestureRecognized;

        bool _complete;

        public bool Complete
        {
            get
            {
                return _complete;
            }
        }

        public void Update(Skeleton sk)
        {
            if(_complete)
            {
                if (GestureRecognized != null)
                {
                    GestureRecognized(this, new EventArgs());
                }
            }
        }
    }
}
