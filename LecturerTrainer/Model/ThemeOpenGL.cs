using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model
{
    /// <summary>
    /// Define a theme for the OpenGL
    /// Added by Baptiste Germond
    /// </summary>
    class ThemeOpenGL
    {
        private string name;
        private OpenTK.Vector4 iJC; //Colour of innered joints
        private OpenTK.Vector4 tJC; //Colour of tracked joints
        private OpenTK.Vector4 iBC; //Colour of innered bones
        private OpenTK.Vector4 tBC; //Colour of tracked bones
        private System.Drawing.Color pFC; //Colour of feedbacks
        private System.Drawing.Color bC; //Colour background
        private string soundName; //Name of the texture for the sound bar

        public string Name {get{return name;}}
        public OpenTK.Vector4 IJC { get { return iJC; } }
        public OpenTK.Vector4 TJC { get { return tJC; } }
        public OpenTK.Vector4 IBC { get { return iBC; } }
        public OpenTK.Vector4 TBC { get { return tBC; } }
        public System.Drawing.Color PFC { get { return pFC; } }
        public System.Drawing.Color BC { get { return bC; } }
        public string SN { get { return soundName; } }

        public ThemeOpenGL(string n, OpenTK.Vector4 iJ, OpenTK.Vector4 tJ, OpenTK.Vector4 iB, OpenTK.Vector4 tB, System.Drawing.Color pF, System.Drawing.Color b,string sN)
        {
            name = n;
            iJC = iJ;
            tJC = tJ;
            iBC = iB;
            tBC = tB;
            pFC = pF;
            bC = b;
            soundName = sN;
        }
    }
}
