using LecturerTrainer.Model;
using LecturerTrainer.ViewModel;
using LecturerTrainer.Model;
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
	/// Logique d'interaction pour GeneralSideTool.xaml
	/// </summary>
	public partial class GeneralSideTool : UserControl
	{
        private static GeneralSideTool instance = null;

		private bool twoDimensions = true;

		public GeneralSideTool()
		{
            GeneralSideToolViewModel gstvm = new GeneralSideToolViewModel();
			this.InitializeComponent();
            this.DataContext = gstvm;
            SideToolsViewModel.Get().setGeneralV(this);
            SideToolsViewModel.Get().setGeneralVM(gstvm); 
			instance = this;
		}
        
        public static GeneralSideTool Get()
        {
            if(instance == null)
                instance = new GeneralSideTool();
            return instance;
        }

		private void AudienceControlCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			twoD.Opacity = 1f;
			twoD.IsEnabled = true;
			threeD.Opacity = 1f;
			threeD.IsEnabled = true;
			if(twoDimensions)
				twoD.IsChecked = true;
			else
				threeD.IsChecked = true;
            GridNumberRows.Opacity = 1f;
            GridNumberRows.IsEnabled = true;
		}

		private void AudienceControlCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			if(twoD.IsChecked.Value)
				twoD.IsChecked = false;
			else
				threeD.IsChecked = false;
			twoD.IsEnabled = false;
			threeD.IsEnabled = false;
            GridNumberRows.IsEnabled = false;
			twoD.Opacity = 0.5f;
			threeD.Opacity = 0.5f;
            GridNumberRows.Opacity = 0.5f;
		}

		private void twoD_Checked(object sender, RoutedEventArgs e)
		{
			twoDimensions = true;
		}

		private void threeD_Checked(object sender, RoutedEventArgs e)
		{
			twoDimensions = false;
		}
	}
}