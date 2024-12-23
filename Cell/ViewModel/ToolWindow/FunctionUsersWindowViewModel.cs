using Cell.Model;
using Cell.ViewModel.Execution;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// View model for a tool window that displays the users of a function.
    /// </summary>
    public class FunctionUsersWindowViewModel : ToolWindowViewModel
    {
        private CellModel? _selectedUser;
        private string _usersListBoxFilterText = string.Empty;
        private CellFunctionViewModel? _selectedFunction;

        /// <summary>
        /// Creates a new instance of the <see cref="FunctionManagerWindowViewModel"/>.
        /// </summary>
        public FunctionUsersWindowViewModel()
        {
        }

        public CellFunctionViewModel? SelectedFunction
        {
            get => _selectedFunction; set
            {
                if (_selectedFunction == value) return;
                _selectedFunction = value;
                SelectedUser = null;
                NotifyPropertyChanged(nameof(SelectedFunction));
                NotifyPropertyChanged(nameof(FilteredUsersOfFunction));
                NotifyPropertyChanged(nameof(ToolWindowTitle));
            }
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 300;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 300;

        private IEnumerable<CellModel> UsersOfFunction => SelectedFunction?.CellsThatUseFunction ?? [];

        /// <summary>
        /// Gets the list of users of the selected function after the filter has been applied from the user.
        /// </summary>
        public IEnumerable<CellModel> FilteredUsersOfFunction => UsersOfFunction.Where(x => x.Location.ToString().Contains(UsersListBoxFilterText)) ?? [];

        /// <summary>
        /// Gets the minimum height this tool window is allowed to be reized to.
        /// </summary>
        public override double MinimumHeight => 200;

        /// <summary>
        /// Gets the minimum width this tool window is allowed to be reized to.
        /// </summary>
        public override double MinimumWidth => 200;

        /// <summary>
        /// Gets or sets the user selected cell from the function users list.
        /// </summary>
        public CellModel? SelectedUser
        {
            get => _selectedUser; set
            {
                if (_selectedUser == value) return;
                _selectedUser = value;
                NotifyPropertyChanged(nameof(SelectedUser));
            }
        }

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle
        {
            get
            {
                if (SelectedFunction is null) return "Select a function to view users";
                var name = SelectedFunction.Name;
                var totalCount = UsersOfFunction.Count();
                return $"{totalCount} Users of {name}";

            }
        }

        /// <summary>
        /// Gets or sets the string that the user has entered to filter the users of the selected function.
        /// </summary>
        public string UsersListBoxFilterText
        {
            get => _usersListBoxFilterText; set
            {
                if (_usersListBoxFilterText == value) return;
                _usersListBoxFilterText = value;
                NotifyPropertyChanged(nameof(UsersListBoxFilterText));
                NotifyPropertyChanged(nameof(FilteredUsersOfFunction));
            }
        }

        /// <summary>
        /// Occurs when the tool window is really being closed.
        /// </summary>
        public override void HandleBeingClosed()
        {
        }

        /// <summary>
        /// Occurs when the tool window is being shown.
        /// </summary>
        public override void HandleBeingShown()
        {
        }

        public void UnassignFunctionFromCell(CellModel cell)
        {
            if (SelectedFunction is null) return;
            if (cell.TriggerFunctionName == SelectedFunction.Name)
            {
                cell.TriggerFunctionName = "";
            }
            else if (cell.PopulateFunctionName == SelectedFunction.Name)
            {
                cell.PopulateFunctionName = "";
            }
            NotifyPropertyChanged(nameof(FilteredUsersOfFunction));
        }
    }
}
