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
	/// Logique d'interaction pour SideToolsView.xaml
	/// </summary>
	public partial class SideToolsView : UserControl
    {
        private static SideToolsView instance = null;

		public SideToolsView()
		{
            this.InitializeComponent();
            this.DataContext = SideToolsViewModel.Get();
            SideToolsViewModel.Get().setToolV(this); 
		}

        public static SideToolsView Get()
        {
            if (instance == null)
                instance = new SideToolsView();
            return instance;
        }
	}
}