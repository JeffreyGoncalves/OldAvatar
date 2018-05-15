using LecturerTrainer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Logique d'interaction pour ToolBarView.xaml
    /// </summary>
    public partial class ToolBarView : UserControl
    {
        private static ToolBarView tbv = null;

        public ToolBarView()
        {
            InitializeComponent();
            this.DataContext = new ToolBarViewModel();
            MainWindow.lastInstance.Initialized += lastInstance_Initialized;
            tbv = this;

        }

        void lastInstance_Initialized(object sender, EventArgs e)
        {
            this.importSpeechButton.DataContext = MainWindow.lastInstance.SideToolsView.DataContext;
        }

        public static ToolBarView Get()
        {
            if (tbv == null)
            {
                tbv = new ToolBarView();
            }
            return tbv;
        }
    }
}
