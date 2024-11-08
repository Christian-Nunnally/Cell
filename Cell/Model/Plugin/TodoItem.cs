﻿using Cell.Core.Common;

namespace Cell.Model.Plugin
{
    /// <summary>
    /// A model for a todo item.
    /// </summary>
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
        private string _parentTaskID = Utilities.GenerateUnqiueId(12);
        private string _title = string.Empty;
        /// <summary>
        /// The category of the todo item in a string format.
        /// </summary>
        public string Category
        {
            get => _category;
            set { if (value != _category) { _category = value; NotifyPropertyChanged(nameof(Category)); } }
        }

        /// <summary>
        /// The date the todo item was marked completed.
        /// </summary>
        public DateTime CompletionDate
        {
            get => _completionDate;
            set { if (value != _completionDate) { _completionDate = value; NotifyPropertyChanged(nameof(CompletionDate)); } }
        }

        /// <summary>
        /// The date the todo item was created.
        /// </summary>
        public DateTime CreationDate
        {
            get => _creationDate;
            set { if (value != _creationDate) { _creationDate = value; NotifyPropertyChanged(nameof(CreationDate)); } }
        }

        /// <summary>
        /// The due date of the todo item.
        /// </summary>
        public DateTime DueDate
        {
            get => _dueDate;
            set { if (value != _dueDate) { _dueDate = value; NotifyPropertyChanged(nameof(DueDate)); } }
        }

        /// <summary>
        /// Whether or not the todo item is marked complete.
        /// </summary>
        public bool IsComplete
        {
            get => _isComplete;
            set 
            {
                if (value == _isComplete) return;
                _isComplete = value; NotifyPropertyChanged(nameof(IsComplete));
                CompletionDate = value ? DateTime.Now : DateTime.MinValue;
            }
        }

        /// <summary>
        /// Notes about the todo item.
        /// </summary>
        public string Notes
        {
            get => _notes;
            set { if (value != _notes) { _notes = value; NotifyPropertyChanged(nameof(Notes)); } }
        }

        /// <summary>
        /// The ID of the parent task. This is used to create a hierarchy of tasks.
        /// </summary>
        public string Parent
        {
            get => _parent;
            set { if (value != _parent) { _parent = value; NotifyPropertyChanged(nameof(Parent)); } }
        }

        /// <summary>
        /// A number based priority of the task.
        /// </summary>
        public int Priority
        {
            get => _priority;
            set { if (value != _priority) { _priority = value; NotifyPropertyChanged(nameof(Priority)); } }
        }

        /// <summary>
        /// A number based status code of the task.
        /// </summary>
        public int Status
        {
            get => _status;
            set { if (value != _status) { _status = value; NotifyPropertyChanged(nameof(Status)); } }
        }

        /// <summary>
        /// The ID of the task for tracking purposes. When this task is copied the <see cref="PluginModel.ID"/> will be new every time, but this will remain the same.
        /// </summary>
        public string TaskID
        {
            get => _taskID;
            set { if (value != _taskID) { _taskID = value; NotifyPropertyChanged(nameof(TaskID)); } }
        }

        /// <summary>
        /// The ID of the parent task for tracking purposes. When this task is copied the <see cref="PluginModel.ID"/> will be new every time, but this will remain the same.
        /// </summary>
        public string ParentTaskID
        {
            get => _parentTaskID;
            set { if (value != _parentTaskID) { _parentTaskID = value; NotifyPropertyChanged(nameof(ParentTaskID)); } }
        }

        /// <summary>
        /// The title of the todo item.
        /// </summary>
        public string Title
        {
            get => _title;
            set { if (value != _title) { _title = value; NotifyPropertyChanged(nameof(Title)); } }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TodoItem"/> with the exact same properties.
        /// </summary>
        /// <returns>The new todo item.</returns>
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

        /// <summary>
        /// Converts the todo item to a string.
        /// </summary>
        /// <returns>The string representation of the todo item.</returns>
        override public string ToString()
        {
            return $"{(IsComplete ? "✅" : "❎")} {Title}";
        }
    }
}
