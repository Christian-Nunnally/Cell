
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.ToolWindow
{
    public class UndoRedoStackWindowViewModel : ResizeableToolWindowViewModel
    {
        public ObservableCollection<string> UndoStack { get; set; } = [];

        public UndoRedoStackWindowViewModel()
        {
            UndoRedoManager.UndoStackChanged += UpdateUndoStackForViewModel;
            UpdateUndoStackForViewModel();
        }

        private void UpdateUndoStackForViewModel()
        {
            UndoStack.Clear();
            foreach (var item in UndoRedoManager.UndoStack)
            {
                UndoStack.Add(item);
            }
        }
    }
}
