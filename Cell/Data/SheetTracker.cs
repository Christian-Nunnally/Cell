
using Cell.Model;
using Cell.Persistence;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Cell.Data
{
    internal class SheetTracker
    {
        private readonly CellLoader _cellLoader = new(PersistenceManager.CurrentRootPath);
        private static SheetTracker? _instance;
        public static SheetTracker Instance => _instance ??= new SheetTracker();

        private SheetTracker()
        {
            Sheets.CollectionChanged += SheetsCollectionChanged;
        }

        private void SheetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var sheet in e.NewItems.OfType<SheetModel>())
                {
                    sheet.PropertyChanged += SheetPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (var sheet in e.OldItems.OfType<SheetModel>())
                {
                    sheet.PropertyChanged -= SheetPropertyChanged;
                }
            }
        }

        private void SheetPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not SheetModel sheetModel) return;
            if (e.PropertyName == nameof(SheetModel.Order))
            {
                OrderedSheets.Clear();
                foreach (var sheet in Sheets.OrderBy(x => x.Order))
                {
                    OrderedSheets.Add(sheet);
                }
            }
            if (e.PropertyName == nameof(SheetModel.Name))
            {
                RenameSheet(sheetModel.OldName, sheetModel.Name);
                sheetModel.OldName = sheetModel.Name;
            }
        }

        public void RenameSheet(string oldSheetName, string newSheetName)
        {
            _cellLoader.RenameSheet(oldSheetName, newSheetName);
            CellTracker.Instance.GetCellModelsForSheet(oldSheetName).ForEach(x => x.SheetName = newSheetName);
        }

        public ObservableCollection<SheetModel> Sheets { get; } = [];

        public ObservableCollection<SheetModel> OrderedSheets { get; } = [];
    }
}
