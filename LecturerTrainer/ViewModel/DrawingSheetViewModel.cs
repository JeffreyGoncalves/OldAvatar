using LecturerTrainer.Model;
using LecturerTrainer.View;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LecturerTrainer.ViewModel
{
    public abstract class DrawingSheetViewModel
    {
        // we keep a reference to the view 
        protected DrawingSheetView dsv;
        
        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        protected DrawingGroup drawingGroup;

        // is our sheet active ? 
        protected bool active; 

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        protected DrawingImage imageSource;

        protected DrawingContext dc;

        public static EventHandler<Bitmap> drawEvent;

        public DrawingSheetViewModel()
        {
            // Display the drawing using our image control
            // Create the drawing group we'll use for drawing
            drawingGroup = new DrawingGroup();
            imageSource = new DrawingImage(drawingGroup);

            if (Main.kinect != null)
            {
                Main_isReady(this, EventArgs.Empty);
            }
            else
            {

                Main.isReady += Main_isReady;
            }
            active = true; 
        }

        protected abstract void Main_isReady(object sender, EventArgs args);

        public abstract void draw(object sender, EventArgs e);


        public DrawingImage getImage()
        {
            return imageSource;
        }

        public void disableSheet()
        {
            active = false; 
        }

        /// <summary>
        ///  is useful for the video treatment when we want to know what type of drawingsheet we want to record
        /// </summary>
        /// <returns></returns>
        public SheetMode activeSheet()
        {
            return this.dsv.activeSheet(); 
        }

        
    }
}
