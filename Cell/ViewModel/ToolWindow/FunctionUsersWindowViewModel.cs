using Cell.Model;
using Cell.ViewModel.Application;
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
        /// <summary>
        /// Creates a new instance of the <see cref="FunctionManagerWindowViewModel"/>.
        /// </summary>
        /// <param name="function">The object to get the functions from.</param>
        public FunctionUsersWindowViewModel(CellFunctionViewModel function)
        {
            SelectedFunction = function;
        }

        public CellFunctionViewModel SelectedFunction { get; set; }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 300;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 300;

        private IEnumerable<CellModel> UsersOfFunction => SelectedFunction.CellsThatUseFunction;

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
                return $"Users of function '{name}' ({totalCount})";

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
    }
}
