using Cell.Data;
using Cell.Model;
using Cell.View.ToolWindow;
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
            AddDefaultCells(NewSheetName);
            ApplicationViewModel.Instance.GoToSheet(NewSheetName);
            NewSheetName = string.Empty;
        }

        private bool CanAddSheet()
        {
            if (_sheetTracker.Sheets.Any(x => x.Name == NewSheetName)) DialogWindow.ShowDialog("Sheet already exists", $"Cannot create a sheet named {NewSheetName} because one already exists with that name.");
            else if (string.IsNullOrEmpty(NewSheetName)) DialogWindow.ShowDialog("Sheet name can not be empty", $"New sheet name can not be empty.");
            else return true;
            return false;
        }

        private void AddDefaultCells(string sheetName)
        {
            var corner = CellModelFactory.Create(0, 0, CellType.Corner, sheetName);
            ApplicationViewModel.Instance.CellTracker.AddCell(corner);

            for (var i = 0; i < _initialRows; i++)
            {
                var row = CellModelFactory.Create(i + 1, 0, CellType.Row, sheetName);
                ApplicationViewModel.Instance.CellTracker.AddCell(row);
            }

            for (var i = 0; i < _initialColumns; i++)
            {
                var columnCell = CellModelFactory.Create(0, i + 1, CellType.Column, sheetName);
                ApplicationViewModel.Instance.CellTracker.AddCell(columnCell);
            }

            for (var i = 0; i < _initialRows; i++)
            {
                for (var j = 0; j < _initialColumns; j++)
                {
                    var newCell = CellModelFactory.Create(i + 1, j + 1, CellType.Label, sheetName);
                    ApplicationViewModel.Instance.CellTracker.AddCell(newCell);
                }
            }   
        }
    }
}
