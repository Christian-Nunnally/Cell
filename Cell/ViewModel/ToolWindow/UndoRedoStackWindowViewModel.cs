using Cell.ViewModel.Application;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.ToolWindow
{
    public class UndoRedoStackWindowViewModel : ResizeableToolWindowViewModel
    {
        public UndoRedoStackWindowViewModel()
        {
            UndoRedoManager.UndoStackChanged += UpdateUndoStackForViewModel;
            UpdateUndoStackForViewModel();
        }

        public ObservableCollection<string> UndoStack { get; set; } = [];

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
