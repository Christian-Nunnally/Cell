using Cell.Common;

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
        private string _parent = string.Empty;
        private int _priority = 0;
        private int _status = 0;
        private string _taskID = Utilities.GenerateUnqiueId(12);
        private string _title = string.Empty;
        public string Category
        {
            get => _category;
            set { if (value != _category) { _category = value; NotifyPropertyChanged(nameof(Category)); } }
        }

        public DateTime CompletionDate
        {
            get => _completionDate;
            set { if (value != _completionDate) { _completionDate = value; NotifyPropertyChanged(nameof(CompletionDate)); } }
        }

        public DateTime CreationDate
        {
            get => _creationDate;
            set { if (value != _creationDate) { _creationDate = value; NotifyPropertyChanged(nameof(CreationDate)); } }
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set { if (value != _dueDate) { _dueDate = value; NotifyPropertyChanged(nameof(DueDate)); } }
        }

        public bool IsComplete
        {
            get => _isComplete;
            set { if (value != _isComplete) { _isComplete = value; NotifyPropertyChanged(nameof(IsComplete)); } }
        }

        public string Notes
        {
            get => _notes;
            set { if (value != _notes) { _notes = value; NotifyPropertyChanged(nameof(Notes)); } }
        }

        public string Parent
        {
            get => _parent;
            set { if (value != _parent) { _parent = value; NotifyPropertyChanged(nameof(Parent)); } }
        }

        public int Priority
        {
            get => _priority;
            set { if (value != _priority) { _priority = value; NotifyPropertyChanged(nameof(Priority)); } }
        }

        public int Status
        {
            get => _status;
            set { if (value != _status) { _status = value; NotifyPropertyChanged(nameof(Status)); } }
        }

        public string TaskID
        {
            get => _taskID;
            set { if (value != _taskID) { _taskID = value; NotifyPropertyChanged(nameof(TaskID)); } }
        }

        public string Title
        {
            get => _title;
            set { if (value != _title) { _title = value; NotifyPropertyChanged(nameof(Title)); } }
        }

        public override object Clone()
        {
            return new TodoItem
            {
                Category = Category,
                CompletionDate = CompletionDate,
                CreationDate = CreationDate,
                DueDate = DueDate,
                IsComplete = IsComplete,
                Notes = Notes,
                Priority = Priority,
                Status = Status,
                Title = Title,
                Parent = Parent,
                TaskID = TaskID
            };
        }

        override public string ToString()
        {
            return $"{(IsComplete ? "✅" : "❎")} {Title}";
        }
    }
}
