using Cell.Common;
using Cell.Model;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Cell.Execution;
using Cell.View.ToolWindow;
using Cell.ViewModel.Cells.Types;

namespace Cell.ViewModel.ToolWindow
{
    public class CellContentEditWindowViewModel : PropertyChangedBase
    {
        private readonly ObservableCollection<CellModel> _cellsToEdit;
        private CellModel _cellToDisplay = CellModel.Null;
        private readonly CellPopulateManager _cellPopulateManager;
        private CellModel CellToDisplay
        {
            get => _cellToDisplay;
            set
            {
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged -= CellToDisplayPropertyChanged;
                _cellToDisplay = value;
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged += CellToDisplayPropertyChanged;
                NotifyPropertyChanged(nameof(Text));
                NotifyPropertyChanged(nameof(Index));
                NotifyPropertyChanged(nameof(PopulateFunctionName));
                NotifyPropertyChanged(nameof(TriggerFunctionName));
            }
        }

        private void CellToDisplayPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Text)) NotifyPropertyChanged(nameof(CellModel.Text));
        }

        public IEnumerable<CellModel> CellsBeingEdited => _cellsToEdit;

        public CellContentEditWindowViewModel(ObservableCollection<CellModel> cellsToEdit, CellPopulateManager cellPopulateManager)
        {
            _cellPopulateManager = cellPopulateManager;
            _cellsToEdit = cellsToEdit;
            _cellsToEdit.CollectionChanged += CellsToEditCollectionChanged;
            PickDisplayedCell();
        }

        private void PickDisplayedCell()
        {
            CellToDisplay = _cellsToEdit.Count > 0 ? _cellsToEdit[0] : CellModel.Null;
        }

        private void CellsToEditCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => PickDisplayedCell();

        public void EditPopulateFunction()
        {
            if (CellToDisplay == null) return;
            if (string.IsNullOrEmpty(CellToDisplay.PopulateFunctionName)) CellToDisplay.PopulateFunctionName = "Untitled";
            var function = ApplicationViewModel.Instance.PluginFunctionLoader.GetOrCreateFunction("object", CellToDisplay.PopulateFunctionName);
            var codeEditWindowViewModel = new CodeEditorWindowViewModel();
            var editor = new CodeEditorWindow(codeEditWindowViewModel, function, x =>
            {
                function.SetUserFriendlyCode(x, CellToDisplay, ApplicationViewModel.Instance.UserCollectionLoader.GetDataTypeStringForCollection, ApplicationViewModel.Instance.UserCollectionLoader.CollectionNames);
                // TODO: update list cells when function code changes they depend on. (cell as ListCellViewModel)?.UpdateList();
            }, CellToDisplay);
            ApplicationViewModel.Instance.ShowToolWindow(editor, true);
        }

        public void EditTriggerFunction()
        {
            if (CellToDisplay == null) return;
            if (string.IsNullOrEmpty(CellToDisplay.TriggerFunctionName)) CellToDisplay.TriggerFunctionName = "Untitled";
            var function = ApplicationViewModel.Instance.PluginFunctionLoader.GetOrCreateFunction("void", CellToDisplay.TriggerFunctionName);
            var codeEditWindowViewModel = new CodeEditorWindowViewModel();
            var editor = new CodeEditorWindow(codeEditWindowViewModel, function, x =>
            {
                function.SetUserFriendlyCode(x, CellToDisplay, ApplicationViewModel.Instance.UserCollectionLoader.GetDataTypeStringForCollection, ApplicationViewModel.Instance.UserCollectionLoader.CollectionNames);
            }, CellToDisplay);
            ApplicationViewModel.Instance.ShowToolWindow(editor, true);
        }

        public string Text
        {
            get => CellToDisplay.Text;
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Text = value;
                }
            }
        }

        public int Index
        {
            get => CellToDisplay.Index;
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    CellToDisplay.Index = value;
                }
            }
        }

        public virtual string PopulateFunctionName
        {
            get => CellToDisplay.PopulateFunctionName;
            set
            {
                if (ApplicationViewModel.Instance.PluginFunctionLoader.GetOrCreateFunction("object", value) is not null)
                {
                    foreach (var cell in _cellsToEdit)
                    {
                        ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                        cell.PopulateFunctionName = value;
                    }
                }
            }
        }

        public virtual string TriggerFunctionName
        {
            get => CellToDisplay.TriggerFunctionName;
            set
            {
                if (ApplicationViewModel.Instance.PluginFunctionLoader.GetOrCreateFunction("void", value) is not null)
                {
                    foreach (var cell in _cellsToEdit)
                    {
                        ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                        cell.TriggerFunctionName = value;
                    }
                };
            }
        }

        public IEnumerable<string> PrettyCellLocationDependencyNames => _cellPopulateManager.GetAllLocationSubscriptions(CellToDisplay).Select(x =>
        {
            var split = x.Replace($"{CellToDisplay.SheetName}_", "").Split('_');
            if (split.Length == 2) return $"{ColumnCellViewModel.GetColumnName(int.Parse(split[1]))}{split[0]}";
            return $"{split[0]}_{ColumnCellViewModel.GetColumnName(int.Parse(split[2]))}{split[1]}";
        });

        public List<string> PrettyDependencyNames => [.. _cellPopulateManager.GetAllCollectionSubscriptions(CellToDisplay), .. PrettyCellLocationDependencyNames];
    }
}
