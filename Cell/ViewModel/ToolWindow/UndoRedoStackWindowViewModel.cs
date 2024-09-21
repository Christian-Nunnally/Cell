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
