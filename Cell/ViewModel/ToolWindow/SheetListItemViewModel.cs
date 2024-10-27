using Cell.Model;
using Cell.ViewModel.Application;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// A view model for a sheet in a list item.
    /// </summary>
    public class SheetListItemViewModel
    {
        private readonly SheetModel _model;

        /// <summary>
        /// The name of the sheet.
        /// </summary>
        public string Name
        {
            get => _model.Name; 
            set
            {
                var oldName = _model.Name;
                _model.Name = value;
                ApplicationViewModel.Instance.CellLoader.RenameSheet(oldName, _model.Name);
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="SheetListItemViewModel"/>.
        /// </summary>
        /// <param name="model">The sheets model.</param>
        public SheetListItemViewModel(SheetModel model)
        {
            _model = model;
        }
    }
}
