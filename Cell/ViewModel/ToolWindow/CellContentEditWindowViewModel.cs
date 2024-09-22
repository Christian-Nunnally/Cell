using Cell.Execution;
using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells.Types;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Cell.ViewModel.ToolWindow
{
    public class CellContentEditWindowViewModel : ToolWindowViewModel
    {
        private readonly CellPopulateManager _cellPopulateManager;
        private readonly ObservableCollection<CellModel> _cellsToEdit;
        private CellModel _cellToDisplay = CellModel.Null;
        public CellContentEditWindowViewModel(ObservableCollection<CellModel> cellsToEdit, CellPopulateManager cellPopulateManager)
        {
            _cellPopulateManager = cellPopulateManager;
            _cellsToEdit = cellsToEdit;
        }

        public override void HandleBeingShown()
        {
            _cellsToEdit.CollectionChanged += CellsToEditCollectionChanged;
            PickDisplayedCell();
        }

        public override void HandleBeingClosed()
        {
            _cellsToEdit.CollectionChanged -= CellsToEditCollectionChanged;
            CellToDisplay = CellModel.Null;
        }

        public IEnumerable<CellModel> CellsBeingEdited => _cellsToEdit;

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

        public IEnumerable<string> PrettyCellLocationDependencyNames => _cellPopulateManager.GetAllLocationSubscriptions(CellToDisplay).Select(x =>
        {
            var split = x.Replace($"{CellToDisplay.SheetName}_", "").Split('_');
            if (split.Length == 2) return $"{ColumnCellViewModel.GetColumnName(int.Parse(split[1]))}{split[0]}";
            return $"{split[0]}_{ColumnCellViewModel.GetColumnName(int.Parse(split[2]))}{split[1]}";
        });

        public List<string> PrettyDependencyNames => [.. _cellPopulateManager.GetAllCollectionSubscriptions(CellToDisplay), .. PrettyCellLocationDependencyNames];

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

        private CellModel CellToDisplay
        {
            get => _cellToDisplay;
            set
            {
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged -= CellToDisplayPropertyChanged;
                _cellToDisplay = value;
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged += CellToDisplayPropertyChanged;
                NotifyPropertyChanged(nameof(CellToDisplay));
                NotifyPropertyChanged(nameof(Text));
                NotifyPropertyChanged(nameof(Index));
                NotifyPropertyChanged(nameof(PopulateFunctionName));
                NotifyPropertyChanged(nameof(TriggerFunctionName));
            }
        }

        public void EditPopulateFunction()
        {
            if (CellToDisplay == null) return;
            if (string.IsNullOrEmpty(CellToDisplay.PopulateFunctionName)) CellToDisplay.PopulateFunctionName = "Untitled";
            var function = ApplicationViewModel.Instance.PluginFunctionLoader.GetOrCreateFunction("object", CellToDisplay.PopulateFunctionName);
            var collectionNameToDataTypeMap = ApplicationViewModel.Instance.UserCollectionLoader.GenerateDataTypeForCollectionMap();
            var codeEditWindowViewModel = new CodeEditorWindowViewModel(function, CellToDisplay, collectionNameToDataTypeMap);
            ApplicationViewModel.Instance.ShowToolWindow(codeEditWindowViewModel, true);
        }

        public void EditTriggerFunction()
        {
            if (CellToDisplay == null) return;
            if (string.IsNullOrEmpty(CellToDisplay.TriggerFunctionName)) CellToDisplay.TriggerFunctionName = "Untitled";
            var function = ApplicationViewModel.Instance.PluginFunctionLoader.GetOrCreateFunction("void", CellToDisplay.TriggerFunctionName);
            var collectionNameToDataTypeMap = ApplicationViewModel.Instance.UserCollectionLoader.GenerateDataTypeForCollectionMap();
            var codeEditWindowViewModel = new CodeEditorWindowViewModel(function, CellToDisplay, collectionNameToDataTypeMap);
            ApplicationViewModel.Instance.ShowToolWindow(codeEditWindowViewModel, true);
        }

        private void CellsToEditCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => PickDisplayedCell();

        private void CellToDisplayPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Text)) NotifyPropertyChanged(nameof(CellModel.Text));
        }

        private void PickDisplayedCell()
        {
            CellToDisplay = _cellsToEdit.Count > 0 ? _cellsToEdit[0] : CellModel.Null;
        }
    }
}
