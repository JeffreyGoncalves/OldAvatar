using LecturerTrainer.ViewModel;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LecturerTrainer.View
{
    /// <summary>
    /// Logique d'interaction pour ResultsView.xaml
    /// </summary>
    public partial class ResultsView
    {
        public ResultsView(List<bool> lbool)
        {
            InitializeComponent();
            ResultsViewModel rd = ResultsViewModel.Get();
            rd.checkBoxUpdates(lbool);
            DataContext = rd;
        }

        /// <summary>
        /// Function called when the user changed the color of the first ColorCanvas
        /// </summary>
        private void choiceColorGraphSeries1_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if(DataContext != null)
            {
                ((ResultsViewModel)DataContext).ColorOne = new SolidColorBrush(choiceColorGraphSeries1.SelectedColor.Value);
                ((ResultsViewModel)DataContext).changeColor(0);
            }
            
        }

        /// <summary>
        /// Function called when the user changed the color of the first ColorCanvas
        /// </summary>
        private void choiceColorGraphSeries2_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (DataContext != null)
            {
                ((ResultsViewModel)DataContext).ColorTwo = new SolidColorBrush(choiceColorGraphSeries2.SelectedColor.Value);
                ((ResultsViewModel)DataContext).changeColor(1);
            }
        }

        /// <summary>
        /// Allow to resize the window to continue to show charts
        /// </summary>
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if(ColorExpander.IsExpanded)
            {
                this.Width += 150;
            }
            else
            {
                this.Width -= 150;
            }
        }

        /// <summary>
        /// Allow to save each chart in picture
        /// </summary>
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            string path = "";
            /**Here, we choose the folder to save pictures*/
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
            folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog1.SelectedPath = Directory.GetParent(Model.Main.session.sessionPath).ToString();
            folderBrowserDialog1.ShowDialog();
            path = folderBrowserDialog1.SelectedPath;

            int value = 0;
            for (int i=0;i<ItemControl.Items.Count;i++)
            {
                foreach (Expander ex in FindVisualChildren<Expander>(ItemControl.ItemContainerGenerator.ContainerFromIndex(i)))
                {
                    foreach (StackPanel chart in FindVisualChildren<StackPanel>(ex))
                    {
                        if(chart.Name.Contains("Chart"))
                        {
                            RenderTargetBitmap rtb = new RenderTargetBitmap((int)chart.ActualWidth+10, (int)chart.ActualHeight+10, 96, 96, PixelFormats.Pbgra32);
                            rtb.Render(chart);

                            PngBitmapEncoder png = new PngBitmapEncoder();
                            png.Frames.Add(BitmapFrame.Create(rtb));
                            MemoryStream stream = new MemoryStream();
                            png.Save(stream);
                            System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                            image.Save(path+Path.DirectorySeparatorChar+((Model.IGraph)chart.DataContext).title +".png", System.Drawing.Imaging.ImageFormat.Png);
                            value++;
                        }
                        
                    }
                }
            }
        }

        /// <summary>
        /// This function allows to return a list of visual element belong to the debObj in function of the type T
        /// </summary>
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
