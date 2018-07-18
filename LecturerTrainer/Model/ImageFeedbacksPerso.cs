using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace LecturerTrainer.Model
{
    /// <summary>
    /// Class allowing to completly manage the load and the use of pictures feedbacks
    /// In OpenGl and on video stream
    /// </summary>
    /// <remarks>Author: Florian BECHU: Summer 2016</remarks>
    class ImageFeedbacksPerso
    {
        #region members
        private Bitmap _bitmapOpenGL;
        /// <summary>
        /// the bitmap load from the ressource file
        /// </summary>
        public Bitmap bitmapOpenGL
        {
            get { return _bitmapOpenGL; }
            set { _bitmapOpenGL = value; }
        }

        private int _idTextureOpenGL;
        /// <summary>
        /// the id used to bind texture
        /// </summary>
        public int idTextureOpenGL
        {
            get { return _idTextureOpenGL; }
            set { _idTextureOpenGL = value; }
        }

        private string _name;
        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        private System.Windows.Controls.Image _image;
        /// <summary>
        /// image used for the video stream
        /// </summary>
        public System.Windows.Controls.Image image
        {
            get { return _image; }
            set { _image = value; }
        }
        #endregion

        #region constructor
        public ImageFeedbacksPerso()
        {
            image = new System.Windows.Controls.Image();
        }
        #endregion

        #region methods
        /// <summary>
        /// Method that allows to rotate a bitmap and flip on the X axis
        /// </summary>
        /// <param name="b">the bitmap</param>
        /// <returns>the transformed bitmap</returns>
        public Bitmap transform(Bitmap b)
        {
            b.RotateFlip(RotateFlipType.Rotate180FlipX);
            return b;
        }

        /// <summary>
        /// Methods that allows to load a bitmap and create an OpenGL texture.
        /// </summary>
        public void initializeOpenGL()
        {
            _bitmapOpenGL = transform(_bitmapOpenGL);
            BitmapData _bitmapdataOpenGL = _bitmapOpenGL.LockBits(new Rectangle(0, 0, _bitmapOpenGL.Width, _bitmapOpenGL.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            _idTextureOpenGL = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _idTextureOpenGL);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _bitmapOpenGL.Width, _bitmapOpenGL.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, _bitmapdataOpenGL.Scan0);
            _bitmapOpenGL.UnlockBits(_bitmapdataOpenGL);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            _bitmapOpenGL = transform(_bitmapOpenGL);

        }

        /// <summary>
        /// Method that allows to change the color of the bitmap gives in parameters with the colour gives in parameters
        /// </summary>
        /// <param name="newcolor">the new colour of the picture</param>
        /// <param name="OpenGL">to know if we must reload the texture in OpenGl</param>
        /// <param name="bitmap">the bitmap</param>
        public void changeColor(Color newcolor, bool OpenGL,Bitmap bitmap)
        {
            bool quitFor = false;
            for (int y = 0; y < bitmap.Height; y++)
            {
                if (quitFor) break;
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y).A != 0)
                    {
                        if (bitmap.GetPixel(x, y).ToArgb() != Color.White.ToArgb() &&
                        bitmap.GetPixel(x, y).ToArgb() != Color.Black.ToArgb())
                        {
                            //If the color is already good, we don't swap all the pixel, we switch to another feedback
                            if (bitmap.GetPixel(x, y).ToArgb() == newcolor.ToArgb())
                            {
                                quitFor = true; break;
                            }
                            bitmap.SetPixel(x, y, newcolor);
                        }
                    }
                    else
                    {
                        bitmap.SetPixel(x, y, Color.FromArgb(0, 255, 255, 255));
                    }

                }
            }
            if (OpenGL)
                reloadInOpenGL();
        }

        /// <summary>
        /// Method that allows to reload the bitmap after a modification and recreate a new OpenGL texture
        /// </summary>
        public void reloadInOpenGL()
        {
            _bitmapOpenGL = transform(_bitmapOpenGL);
            BitmapData _bitmapdataOpenGL = _bitmapOpenGL.LockBits(new Rectangle(0, 0, _bitmapOpenGL.Width, _bitmapOpenGL.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            _idTextureOpenGL = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _idTextureOpenGL);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _bitmapOpenGL.Width, _bitmapOpenGL.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, _bitmapdataOpenGL.Scan0);
            _bitmapOpenGL.UnlockBits(_bitmapdataOpenGL);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            _bitmapOpenGL = transform(_bitmapOpenGL);
        }

        /// <summary>
        /// Method that allows to standardize the name of the picture in function of the name of the file
        /// </summary>
        /// <param name="name">the name of the file</param>
        /// <author>Charles Bidaut</author>
        public void chooseCorrectName(string name)
        {
            if (name.ToLower().Contains("hand_joined"))
            {
                this.name = "Hand_Joined";
            }
            else if (name.ToLower().Contains("crossed_arms"))
            {
                this.name = "Arms_Crossed";
            }
            else if (name.ToLower().Contains("agitation"))
            {
                this.name = "Agitation";
            }
            else if (name.ToLower().Contains("left_arrow"))
            {
                this.name = "Left_Arrow";
            }
            else if (name.ToLower().Contains("right_arrow"))
            {
                this.name = "Right_Arrow";
            }
            else if (name.ToLower().Contains("center_arrow"))
            {
                this.name = "Center_Arrow";
            }
            else if (name.ToLower().Contains("happy"))
            {
                this.name = "Happy";
            }
            else if (name.ToLower().Contains("surprised"))
            {
                this.name = "Surprised";
            }
            else if (name.ToLower().Contains("sound"))
            {
                this.name = name;
            }
            else if (name.ToLower().Contains("signal"))
            {
                this.name = "Signal_Lost";
            }
            else if (name.ToLower().Contains("good_job"))
            {
                this.name = "GoodJob";
            }
            else if (name.ToLower().Contains("elbows"))
            {
                this.name = "Elbows";
            }
            else if (name.ToLower().Contains("slow"))
            {
                this.name = "Slow";
            }
            else if (name.ToLower().Contains("your_turn"))
            {
                this.name = "YourTurn";
            }
            else if (name.ToLower().Contains("like_this"))
            {
                this.name = "LikeThis";
            }
            else
            {
                this.name = name;
            }
        }
        #endregion
    }
}
