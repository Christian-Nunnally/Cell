

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

        public string Notes
        {
            get => _notes;
            set { if (value != _notes) { _notes = value; OnPropertyChanged(nameof(Notes)); } }
        }
        private string _notes = string.Empty;

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

        public DateTime CompletionDate
        {
            get => _completionDate;
            set { if (value != _completionDate) { _completionDate = value; OnPropertyChanged(nameof(CompletionDate)); } }
        }
        private DateTime _completionDate;

        public int Priority
        {
            get => _priority;
            set { if (value != _priority) { _priority = value; OnPropertyChanged(nameof(Priority)); } }
        }
        private int _priority = 0;

        public int Status
        {
            get => _status;
            set { if (value != _status) { _status = value; OnPropertyChanged(nameof(Status)); } }
        }
        private int _status = 0;

        public int Category
        {
            get => _category;
            set { if (value != _category) { _category = value; OnPropertyChanged(nameof(Category)); } }
        }
        private int _category = 0;

        override public string ToString()
        {
            return $"{(IsComplete ? "✓" : "☐")} {Title} - {Notes}";
        }
    }
}
