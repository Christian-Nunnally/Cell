using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Cells.Types;
using Cell.ViewModel.Execution;

namespace Cell.ViewModel.Cells
{
    public static class SheetViewModelFactory
    {
        public static SheetViewModel Create(
            SheetModel sheetModel, 
            CellPopulateManager cellPopulateManager, 
            CellTracker cellTracker, 
            SheetTracker sheetTracker,
            CellSelector cellSelector,
            UserCollectionLoader userCollectionLoader, 
            ApplicationSettings applicationSettings, 
            PluginFunctionLoader pluginFunctionLoader)
        {
            var sheetViewModel = new SheetViewModel(sheetModel, cellPopulateManager, cellTracker, sheetTracker, cellSelector, userCollectionLoader, applicationSettings, pluginFunctionLoader);
            sheetViewModel.PropertyChanged += SheetViewModelPropertyChanged;
            return sheetViewModel;
        }

        private static void SheetViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // TODO: Make trigger off CellSelector changes
            if (e.PropertyName == nameof(SheetViewModel.SelectedCellViewModel))
            {
                if (sender is SheetViewModel sheetViewModel)
                {
                    HighlightFunctionDependencies(sheetViewModel);
                }
            }
        }

        private static void HighlightFunctionDependencies(SheetViewModel sheet)
        {
            if (sheet.SelectedCellViewModel == null) return;
            if (sheet.PluginFunctionLoader.TryGetFunction("object", sheet.SelectedCellViewModel.Model.PopulateFunctionName, out var populate))
            {
                if (sheet.ApplicationSettings.HighlightPopulateCellDependencies) HighlightCellDependenciesOfFunction(sheet, populate);
                if (sheet.ApplicationSettings.HighlightPopulateCollectionDependencies) HighlightCollectionDependenciesForFunction(sheet, populate);
            }
            if (sheet.PluginFunctionLoader.TryGetFunction("void", sheet.SelectedCellViewModel.Model.TriggerFunctionName, out var trigger))
            {
                if (sheet.ApplicationSettings.HighlightTriggerCellDependencies) HighlightCellDependenciesOfFunction(sheet, trigger);
                if (sheet.ApplicationSettings.HighlightTriggerCollectionDependencies) HighlightCollectionDependenciesForFunction(sheet, trigger);
            }
        }

        private static void HighlightCellDependenciesOfFunction(SheetViewModel sheet, PluginFunction function)
        {
            foreach (var locationDependencies in function.LocationDependencies)
            {
                if (sheet.SelectedCellViewModel == null) return;
                if (locationDependencies.IsRange)
                {
                    var sheetName = locationDependencies.SheetName;
                    if (sheetName != sheet.SheetName) continue;
                    var row = locationDependencies.Row;
                    var column = locationDependencies.Column;
                    if (locationDependencies.IsColumnRelative) column += sheet.SelectedCellViewModel.Column;
                    if (locationDependencies.IsRowRelative) row += sheet.SelectedCellViewModel.Row;

                    var rowRangeEnd = locationDependencies.RowRangeEnd;
                    var columnRangeEnd = locationDependencies.ColumnRangeEnd;
                    if (locationDependencies.IsColumnRelativeRangeEnd) columnRangeEnd += sheet.SelectedCellViewModel.Column;
                    if (locationDependencies.IsRowRelativeRangeEnd) rowRangeEnd += sheet.SelectedCellViewModel.Row;

                    for (var r = row; r <= rowRangeEnd; r++)
                    {
                        for (var c = column; c <= columnRangeEnd; c++)
                        {
                            var cellToHighlight = sheet.CellViewModels.FirstOrDefault(x => x.Row == r && x.Column == c);
                            if (cellToHighlight == null) continue;
                            sheet.HighlightCell(cellToHighlight, "#0438ff44");
                        }
                    }
                }
                else
                {
                    var sheetName = locationDependencies.SheetName;
                    if (sheetName != sheet.SheetName) continue;
                    var row = locationDependencies.Row;
                    var column = locationDependencies.Column;
                    if (locationDependencies.IsColumnRelative) column += sheet.SelectedCellViewModel.Column;
                    if (locationDependencies.IsRowRelative) row += sheet.SelectedCellViewModel.Row;
                    var cellToHighlight = sheet.CellViewModels.FirstOrDefault(x => x.Row == row && x.Column == column);
                    if (cellToHighlight == null) continue;
                    sheet.HighlightCell(cellToHighlight, "#0438ff44");
                }
            }
        }

        private static void HighlightCollectionDependenciesForFunction(SheetViewModel sheet, PluginFunction function)
        {
            foreach (var collectionReference in function.CollectionDependencies)
            {
                var cellsToHighlight = sheet.CellViewModels.OfType<ListCellViewModel>().Where(x => x.CollectionName == collectionReference);
                foreach (var cellToHighlight in cellsToHighlight)
                {
                    sheet.HighlightCell(cellToHighlight, "#0438ff44");
                }
            }
        }
    }
}
