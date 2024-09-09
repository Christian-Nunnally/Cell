using Cell.Data;
using Cell.Model;
using Cell.ViewModel.Application;

namespace Cell.ViewModel.ToolWindow
{
    public class CreateSheetWindowViewModel : ResizeableToolWindowViewModel
    {
        private string _newSheetName = "NewSheet";
        private int _initialRows = 5;
        private int _initialColumns = 5;
        private readonly SheetTracker _sheetTracker;

        public string NewSheetName
        {
            get => _newSheetName;
            set
            {
                _newSheetName = value;
                NotifyPropertyChanged(nameof(NewSheetName));
            }
        }

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

        public CreateSheetWindowViewModel(SheetTracker sheetTracker)
        {
            _sheetTracker = sheetTracker;
        }

        public void AddNewSheet()
        {
            if (!CanAddSheet()) return;
            SheetFactory.CreateSheet(NewSheetName, _initialRows, _initialColumns, ApplicationViewModel.Instance.CellTracker);
            ApplicationViewModel.Instance.GoToSheet(NewSheetName);
            NewSheetName = string.Empty;
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
