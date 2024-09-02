using Cell.Model;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Cell.Data
{
    public class SheetTracker
    {
        public SheetTracker()
        {
            Sheets.CollectionChanged += SheetsCollectionChanged;
        }

        public ObservableCollection<SheetModel> OrderedSheets { get; } = [];

        public ObservableCollection<SheetModel> Sheets { get; } = [];

        public void RenameSheet(string oldSheetName, string newSheetName)
        {
            ApplicationViewModel.Instance.CellLoader.RenameSheet(oldSheetName, newSheetName);
            ApplicationViewModel.Instance.CellTracker.GetCellModelsForSheet(oldSheetName).ForEach(x => x.SheetName = newSheetName);
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
    }
}
