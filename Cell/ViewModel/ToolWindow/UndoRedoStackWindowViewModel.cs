using Cell.Common;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.ToolWindow
{
    public class UndoRedoStackWindowViewModel : ToolWindowViewModel
    {
        public UndoRedoStackWindowViewModel()
        {
            var undoRedoManager = ApplicationViewModel.GetUndoRedoManager();
            if (undoRedoManager != null)
            {
                undoRedoManager.UndoStackChanged += () => UpdateUndoStackForViewModel(undoRedoManager);
                UpdateUndoStackForViewModel(undoRedoManager);
            }
        }

        public override double DefaultHeight => 180;

        public override double DefaultWidth => 180;

        public override List<CommandViewModel> ToolBarCommands =>
        [
            new("Undo", new RelayCommand(x => ApplicationViewModel.GetUndoRedoManager()?.Undo())),
            new("Redo", new RelayCommand(x => ApplicationViewModel.GetUndoRedoManager()?.Redo()))
        ];

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "Undo/Redo Stack";

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
