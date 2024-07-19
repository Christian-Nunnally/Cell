
namespace Cell.Model.Plugin
{
    [Serializable]
    public class TodoItem : PluginModel
    {
        public string Title
        {
            get => _title;
            set { if (value != _title) { _title = value; OnPropertyChanged(nameof(Title)); } }
        }
        private string _title = string.Empty;

        public string Description
        {
            get => _description;
            set { if (value != _description) { _description = value; OnPropertyChanged(nameof(Description)); } }
        }
        private string _description = string.Empty;

        public bool IsComplete
        {
            get => _isComplete;
            set { if (value != _isComplete) { _isComplete = value; OnPropertyChanged(nameof(IsComplete)); } }
        }
        private bool _isComplete = false;

        public DateTime DueDate
        {
            get => _dueDate;
            set { if (value != _dueDate) { _dueDate = value; OnPropertyChanged(nameof(DueDate)); } }
        }
        private DateTime _dueDate = DateTime.Now;

        public DateTime CreationDate
        {
            get => _creationDate;
            set { if (value != _creationDate) { _creationDate = value; OnPropertyChanged(nameof(CreationDate)); } }
        }
        private DateTime _creationDate = DateTime.Now;

        public int Priority
        {
            get => _priority;
            set { if (value != _priority) { _priority = value; OnPropertyChanged(nameof(Priority)); } }
        }
        private int _priority = 0;

        override public string ToString()
        {
            return $"{(IsComplete ? "✓" : "☐")} {Title} - {Description}";
        }
    }
}
