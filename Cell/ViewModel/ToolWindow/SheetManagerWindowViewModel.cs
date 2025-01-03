﻿using Cell.Core.Common;
using Cell.Core.Data.Tracker;
using Cell.Model;
using Cell.ViewModel.Application;
using System.Collections.Specialized;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// Tool window view model for managing all sheets in a project.
    /// </summary>
    public class SheetManagerWindowViewModel : ToolWindowViewModel
    {
        private readonly SheetTracker _sheetTracker;
        private readonly DialogFactoryBase _dialogFactory;
        private string _sheetsListBoxFilterText = string.Empty;
        /// <summary>
        /// Creates a new instance of the <see cref="SheetManagerWindowViewModel"/>.
        /// </summary>
        /// <param name="sheetTracker">The tracker to get cells from.</param>
        /// <param name="dialogFactory">A factory for showing dialogs.</param>
        public SheetManagerWindowViewModel(SheetTracker sheetTracker, DialogFactoryBase dialogFactory)
        {
            _sheetTracker = sheetTracker;
            _dialogFactory = dialogFactory;
            ToolBarCommands.Add(new CommandViewModel("Export", OpenExportWindow) { ToolTip = "Opens the export tool window" });
            ToolBarCommands.Add(new CommandViewModel("Import", OpenImportWindow) { ToolTip = "Opens the import tool window" });
            ToolBarCommands.Add(new CommandViewModel("New Sheet", OpenAddNewSheetWindow) { ToolTip = "Opens the create new sheet tool window" });
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 300;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 400;

        /// <summary>
        /// The sheets that are currently being displayed in the list box to the user, filtered based on the users filter criteria.
        /// </summary>
        public IEnumerable<SheetModel> FilteredSheets => _sheetTracker.OrderedSheets.Where(x => x.Name.Contains(SheetsListBoxFilterText)).OrderBy(x => x.Order);

        /// <summary>
        /// Gets the minimum height this tool window is allowed to be resized to.
        /// </summary>
        public override double MinimumHeight => 100;

        /// <summary>
        /// Gets the minimum width this tool window is allowed to be resized to.
        /// </summary>
        public override double MinimumWidth => 200;

        /// <summary>
        /// Gets or sets the name of the sheet that is currently selected in the list box.
        /// </summary>
        public string SelectedSheetName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text that the user has entered into the filter box.
        /// </summary>
        public string SheetsListBoxFilterText
        {
            get => _sheetsListBoxFilterText; set
            {
                if (_sheetsListBoxFilterText == value) return;
                _sheetsListBoxFilterText = value;
                NotifyPropertyChanged(nameof(SheetsListBoxFilterText));
                RefreshSheetsList();
            }
        }

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "Sheet Manager";

        /// <summary>
        /// Creates a copy of the sheet with the given name.
        /// </summary>
        /// <param name="sheetName">The name of the sheet to copy.</param>
        public void CopySheet(string sheetName)
        {
            var copiedSheetName = sheetName + "Copy";
            while (_sheetTracker.Sheets.Any(x => x.Name == copiedSheetName)) copiedSheetName += "Copy";

            var copiedCells = _sheetTracker.CreateUntrackedCopiesOfCellsInSheet(sheetName);
            _sheetTracker.UpdateIdentitiesOfCellsForNewSheet(copiedSheetName, copiedCells);
            _sheetTracker.AddAndSaveCells(copiedCells);
        }

        /// <summary>
        /// Occurs when the tool window is really being closed.
        /// </summary>
        public override void HandleBeingClosed()
        {
            _sheetTracker.OrderedSheets.CollectionChanged -= SheetsCollectionChanged;
        }

        /// <summary>
        /// Occurs when the tool window is being shown.
        /// </summary>
        public override void HandleBeingShown()
        {
            _sheetTracker.OrderedSheets.CollectionChanged += SheetsCollectionChanged;
        }

        /// <summary>
        /// Causes the list of sheet visible to the user to be refreshed.
        /// </summary>
        public void RefreshSheetsList()
        {
            NotifyPropertyChanged(nameof(FilteredSheets));
        }

        /// <summary>
        /// Moves the given sheet down in the order of the sheets.
        /// </summary>
        /// <param name="sheetModel">The sheet.</param>
        /// <exception cref="CellError">Thrown when the sheet is not tracked and thus has no relative ordering.</exception>
        public void MoveSheetDownInOrder(SheetModel sheetModel)
        {
            if (!_sheetTracker.Sheets.Contains(sheetModel)) throw new CellError($"Sheet {sheetModel.Name} must be tracked in order to change the ordering.");
            MakeSureSheetOrderingIsConsecutive();
            sheetModel.Order += 3;
            RefreshSheetsList();
            MakeSureSheetOrderingIsConsecutive();
        }

        /// <summary>
        /// Moves the given sheet up in the order of the sheets.
        /// </summary>
        /// <param name="sheetModel">The sheet.</param>
        /// <exception cref="CellError">Thrown when the sheet is not tracked and thus has no relative ordering.</exception>
        public void MoveSheetUpInOrder(SheetModel sheetModel)
        {
            if (!_sheetTracker.Sheets.Contains(sheetModel)) throw new CellError($"Sheet {sheetModel.Name} must be tracked in order to change the ordering.");
            MakeSureSheetOrderingIsConsecutive();
            sheetModel.Order -= 3;
            RefreshSheetsList();
            MakeSureSheetOrderingIsConsecutive();
        }

        private void MakeSureSheetOrderingIsConsecutive()
        {
            var i = 1;
            foreach (var sheet in _sheetTracker.OrderedSheets.ToList())
            {
                sheet.Order = i;
                i += 2;
            }
        }

        private void OpenAddNewSheetWindow()
        {
            var createSheetWindowViewModel = new CreateSheetWindowViewModel(_sheetTracker, _dialogFactory);
            ApplicationViewModel.Instance.ShowToolWindow(createSheetWindowViewModel);
        }

        private void OpenExportWindow()
        {
            var exportWindow = new ExportWindowViewModel
            {
                SheetNames = _sheetTracker.OrderedSheets.Select(x => x.Name),
                SheetNameToExport = _sheetTracker.OrderedSheets.FirstOrDefault()?.Name ?? ""
            };
            ApplicationViewModel.Instance.ShowToolWindow(exportWindow);
        }

        private void OpenImportWindow()
        {
            var importWindow = new ImportWindowViewModel();
            ApplicationViewModel.Instance.ShowToolWindow(importWindow);
        }

        private void SheetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshSheetsList();
    }
}
