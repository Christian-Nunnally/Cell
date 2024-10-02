using Cell.Common;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// Tool window view model for the undo/redo stack window, which displays the current undo and redo stack for the application for visualization and debugging. Also provides undo and redo button.
    /// </summary>
    public class UndoRedoStackWindowViewModel : ToolWindowViewModel
    {
        /// <summary>
        /// Creates a new instance of the <see cref="UndoRedoStackWindowViewModel"/> class.
        /// </summary>
        public UndoRedoStackWindowViewModel()
        {
            var undoRedoManager = ApplicationViewModel.GetUndoRedoManager();
            if (undoRedoManager != null)
            {
                undoRedoManager.UndoStackChanged += () => UpdateUndoStackForViewModel(undoRedoManager);
                UpdateUndoStackForViewModel(undoRedoManager);
            }
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 180;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 180;

        /// <summary>
        /// Provides a list of commands to display in the title bar of the tool window.
        /// </summary>
        public override List<CommandViewModel> ToolBarCommands =>
        [
            new("Undo", new RelayCommand(x => ApplicationViewModel.GetUndoRedoManager()?.Undo())),
            new("Redo", new RelayCommand(x => ApplicationViewModel.GetUndoRedoManager()?.Redo()))
        ];

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "Undo/Redo Stack";

        /// <summary>
        /// Gets the list of string representations of the items in the undo stack for display in the UI.
        /// </summary>
        public ObservableCollection<string> UndoStack { get; set; } = [];

        private void UpdateUndoStackForViewModel(UndoRedoManager undoRedoManager)
        {
            UndoStack.Clear();
            foreach (var item in undoRedoManager.UndoStack)
            {
                UndoStack.Add(item);
            }
        }
    }
}
