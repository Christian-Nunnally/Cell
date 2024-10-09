using Cell.Common;
using Cell.Core.Execution.References;
using Cell.Model;
using Cell.ViewModel.Application;

namespace Cell.ViewModel.Execution
{
    /// <summary>
    /// A view model for displaying a <see cref="CellFunction"/> in a view.
    /// </summary>
    public class CellFunctionViewModel : PropertyChangedBase
    {
        private readonly CellFunction _function;
        private readonly CellFunctionModel _model;
        /// <summary>
        /// Creates a new instance of <see cref="CellFunctionViewModel"/>.
        /// </summary>
        /// <param name="function">The underlying function.</param>
        public CellFunctionViewModel(CellFunction function)
        {
            _model = function.Model;
            _function = function;
        }

        /// <summary>
        /// Gets the cells that use this function.
        /// </summary>
        public List<CellModel> CellsThatUseFunction => ApplicationViewModel.Instance.CellPopulateManager.GetCellsThatUsePopulateFunction(_model);

        /// <summary>
        /// Gets the references this function depends on.
        /// </summary>
        public IEnumerable<IReferenceFromCell> Dependencies => _function.Dependencies;

        /// <summary>
        /// Gets the underlying function.
        /// </summary>
        public CellFunction Function => _function;

        /// <summary>
        /// Gets or renames the name of the function.
        /// </summary>
        public string Name
        {
            get { return _model.Name; }
            set
            {
                if (" !@#$%^&*(){}[]/*+.<>,\\\n\t?`~|=".Any(x => value.Contains(x))) return;
                if (_model.Name == value) return;
                var oldName = _model.Name;
                _model.Name = value;
                NotifyPropertyChanged(nameof(Name));
                DialogFactory.ShowYesNoConfirmationDialog("Refactor?", $"Do you want to update cells that used '{oldName}' to use '{value}' instead?", () => RefactorCellsFunctionUseage(ApplicationViewModel.Instance.CellTracker.AllCells, oldName, value));
            }
        }

        /// <summary>
        /// Gets the return type of this function.
        /// </summary>
        public string ReturnType => _model.ReturnType;

        /// <summary>
        /// Gets the number of cells that use this function.
        /// </summary>
        public int UsageCount => CellsThatUseFunction.Count;

        private static void RefactorCellsFunctionUseage(IEnumerable<CellModel> cells, string oldName, string newName)
        {
            foreach (var cell in cells)
            {
                if (cell.PopulateFunctionName == oldName) cell.PopulateFunctionName = newName;
                if (cell.TriggerFunctionName == oldName) cell.TriggerFunctionName = newName;
            }
        }
    }
}
