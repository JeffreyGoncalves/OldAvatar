using System.Collections.ObjectModel;

namespace LecturerTrainer.Model
{
    /// <summary>
    /// Class that allows to assemble a collection of IGraph ( in the window, this corresponds to an expander)
    /// </summary>
    public class ResultParts
    {
        public string Title { get; set; }
        public ObservableCollection<IGraph> Items { get; set; }
        
        public ResultParts(string _ti)
        {
            Title = _ti;
            Items = new ObservableCollection<IGraph>();
        }
    }
}
