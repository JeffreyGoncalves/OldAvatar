using System.Windows;
using System.Windows.Controls;

namespace LecturerTrainer.Model
{
    /// <summary>
    /// class that allows to do the link between the data template in the XAML file and the class for the charts
    /// </summary>
    public class GraphTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PieTemplate { get; set; }
        public DataTemplate CartesianTemplate { get; set; }
        public DataTemplate NumberTemplate { get; set; }
        public DataTemplate GraphEmptyTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
          DependencyObject container)
        {
            if (item is PieGraph)
                return PieTemplate;
            else if (item is CartesianGraph)
                return CartesianTemplate;
            else if (item is NumberGraph)
                return NumberTemplate;
            else if (item is GraphEmpty)
                return GraphEmptyTemplate;
            return null;
        }
    }
}
