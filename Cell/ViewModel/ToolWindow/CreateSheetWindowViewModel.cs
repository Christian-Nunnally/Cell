using Cell.Data;
using Cell.Model;
using Cell.ViewModel.Application;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// A tool window that allows the user to create a new sheet.
    /// </summary>
    public class CreateSheetWindowViewModel : ToolWindowViewModel
    {
        private readonly SheetTracker _sheetTracker;
        private int _initialColumns = 5;
        private int _initialRows = 5;
        private string _newSheetName = "NewSheet";
        /// <summary>
        /// Creates a new instance of the <see cref="CreateSheetWindowViewModel"/>.
        /// </summary>
        /// <param name="sheetTracker">The sheet tracker to add new sheets to.</param>
        public CreateSheetWindowViewModel(SheetTracker sheetTracker)
        {
            _sheetTracker = sheetTracker;
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 150;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 300;

        /// <summary>
        /// The number of columns to create in the new sheet.
        /// </summary>
        public string InitialColumns
        {
            get => _initialColumns.ToString();
            set
            {
                _initialColumns = int.TryParse(value, out var result) ? result : 1;
                if (_initialColumns < 1) _initialColumns = 1;
                if (_initialColumns > 50) _initialColumns = 50;
                NotifyPropertyChanged(nameof(InitialColumns));
            }
        }

        /// <summary>
        /// The number of rows to create in the new sheet.
        /// </summary>
        public string InitialRows
        {
            get => _initialRows.ToString();
            set
            {
                _initialRows = int.TryParse(value, out var result) ? result : 1;
                if (_initialRows < 1) _initialRows = 1;
                if (_initialRows > 50) _initialRows = 50;
                NotifyPropertyChanged(nameof(InitialRows));
            }
        }

        /// <summary>
        /// The name to give the new sheet that will be created.
        /// </summary>
        public string NewSheetName
        {
            get => _newSheetName;
            set
            {
                _newSheetName = value;
                NotifyPropertyChanged(nameof(NewSheetName));
            }
        }

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "New sheet";

        /// <summary>
        /// Creates a new sheet with the current settings.
        /// </summary>
        public void AddNewSheet()
        {
            if (!CanAddSheet()) return;
            SheetFactory.CreateSheet(NewSheetName, _initialRows, _initialColumns, ApplicationViewModel.Instance.CellTracker);
            ApplicationViewModel.Instance.GoToSheet(NewSheetName);
            NewSheetName = string.Empty;
            RequestClose?.Invoke();
        }

        private bool CanAddSheet()
        {
            if (_sheetTracker.Sheets.Any(x => x.Name == NewSheetName)) DialogFactory.ShowDialog("Sheet already exists", $"Cannot create a sheet named {NewSheetName} because one already exists with that name.");
            else if (string.IsNullOrEmpty(NewSheetName)) DialogFactory.ShowDialog("Sheet name can not be empty", $"New sheet name can not be empty.");
            else return true;
            return false;
        }
    }
}
