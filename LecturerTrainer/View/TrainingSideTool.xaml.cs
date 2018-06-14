using LecturerTrainer.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LecturerTrainer.View
{
	/// <summary>
	/// Logique d'interaction pour TrainingSideTool.xaml
	/// </summary>
	public partial class TrainingSideTool : UserControl
    {
        private static TrainingSideTool instance = null;

        #region CONSTRUCTOR
        public TrainingSideTool()
		{
            TrainingSideToolViewModel tstvm = TrainingSideToolViewModel.Get();
			this.InitializeComponent();
            this.DataContext = tstvm;
            SideToolsViewModel.Get().setTrainingV(this);
            SideToolsViewModel.Get().setTrainingVM(tstvm);
            instance = this;
        }

        public static TrainingSideTool Get()
        {
            if (instance == null)
                instance = new TrainingSideTool();
            return instance;
        }
        #endregion

        /// <summary>
        /// Function setting the display checkboxes's enabling
        /// </summary>
        /// <param name="value"></param>
        public void toggleAllCheckboxes(bool value)
        {
            /*Modified by Baptiste Germond 
             * -- Ensure that the actions are synchronised with the current thread to avoid conflict*/
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                StreamRecordingCheckbox.IsEnabled = value;
                //AvatarOpenGLRecordingCheckbox.IsEnabled = value;
                AvatarVideoRecordingCheckbox.IsEnabled = value;
                AudioRecordingCheckbox.IsEnabled = value;
            });
        }

    }
}