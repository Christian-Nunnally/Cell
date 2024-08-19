namespace Cell.Model.Plugin
{
    [Serializable]
    public class TodoItem : PluginModel
    {
        private string _category = string.Empty;
        private DateTime _completionDate;
        private DateTime _creationDate = DateTime.Now;
        private DateTime _dueDate = DateTime.Now;
        private bool _isComplete = false;
        private string _notes = string.Empty;
        private int _priority = 0;
        private int _status = 0;
        private string _title = string.Empty;
        public string Category
        {
            get => _category;
            set { if (value != _category) { _category = value; OnPropertyChanged(nameof(Category)); } }
        }

        public DateTime CompletionDate
        {
            get => _completionDate;
            set { if (value != _completionDate) { _completionDate = value; OnPropertyChanged(nameof(CompletionDate)); } }
        }

        public DateTime CreationDate
        {
            get => _creationDate;
            set { if (value != _creationDate) { _creationDate = value; OnPropertyChanged(nameof(CreationDate)); } }
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set { if (value != _dueDate) { _dueDate = value; OnPropertyChanged(nameof(DueDate)); } }
        }

        public bool IsComplete
        {
            get => _isComplete;
            set { if (value != _isComplete) { _isComplete = value; OnPropertyChanged(nameof(IsComplete)); } }
        }

        public string Notes
        {
            get => _notes;
            set { if (value != _notes) { _notes = value; OnPropertyChanged(nameof(Notes)); } }
        }

        public int Priority
        {
            get => _priority;
            set { if (value != _priority) { _priority = value; OnPropertyChanged(nameof(Priority)); } }
        }

        public int Status
        {
            get => _status;
            set { if (value != _status) { _status = value; OnPropertyChanged(nameof(Status)); } }
        }

        public string Title
        {
            get => _title;
            set { if (value != _title) { _title = value; OnPropertyChanged(nameof(Title)); } }
        }

        override public string ToString()
        {
            return $"{Priority} - {(IsComplete ? "✅" : "❎")} {Title} - {Notes}";
        }
    }
}
