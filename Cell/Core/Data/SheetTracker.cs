using Cell.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Cell.Core.Data
{
    /// <summary>
    /// The tracker for all the sheets in a project.
    /// </summary>
    public class SheetTracker
    {
        /// <summary>
        /// Creates a new instance of <see cref="SheetTracker"/>.
        /// </summary>
        /// <param name="cellTracker">The cell tracker used to get the cells in the sheet.</param>
        public SheetTracker(CellTracker cellTracker)
        {
            CellTracker = cellTracker;
            cellTracker.CellAdded += CellAddedToTracker;
            cellTracker.CellRemoved += CellRemovedFromTracker;
            Sheets.CollectionChanged += SheetsCollectionChanged;
        }

        /// <summary>
        /// The cell tracker used to get and create cells in the sheet.
        /// </summary>
        public CellTracker CellTracker { get; }

        /// <summary>
        /// The sheets in the project, ordered by their order.
        /// </summary>
        public ObservableCollection<SheetModel> OrderedSheets { get; } = [];

        /// <summary>
        /// The sheets in the project.
        /// </summary>
        public ObservableCollection<SheetModel> Sheets { get; } = [];

        /// <summary>
        /// Adds the list of cells to the project.
        /// </summary>
        /// <param name="cellsToAdd">The cells to add.</param>
        public void AddAndSaveCells(IEnumerable<CellModel> cellsToAdd)
        {
            foreach (var cell in cellsToAdd)
            {
                CellTracker.AddCell(cell);
            }
        }

        /// <summary>
        /// Copies all cells from one sheet and nullifies thier names to facilitate copying them to a new sheet.
        /// </summary>
        /// <param name="sheetName">The name of the sheet to copy the cells of.</param>
        /// <returns>A List of copied cells.</returns>
        public List<CellModel> CreateUntrackedCopiesOfCellsInSheet(string sheetName)
        {
            var copiedCells = CellTracker.GetCellModelsForSheet(sheetName).Select(c => c.Copy()).ToList();
            foreach (var copiedCell in copiedCells)
            {
                copiedCell.Location.SheetName = "";
            }

            return copiedCells;
        }

        /// <summary>
        /// Gives cells new IDs and updates merged cells with the new IDs to facilitate copying existing cells from a sheet.
        /// </summary>
        /// <param name="sheetName">The name of the sheet to give all the cell.</param>
        /// <param name="cellsToAdd">The cells to give now IDs.</param>
        public void UpdateIdentitiesOfCellsForNewSheet(string sheetName, IEnumerable<CellModel> cellsToAdd)
        {
            var oldIdToNewIdMap = GiveCellsNewUniqueIndentities(sheetName, cellsToAdd);
            FixMergedCellsWithNewIdentities(cellsToAdd, oldIdToNewIdMap);
        }

        private void CellAddedToTracker(CellModel model)
        {
            var sheet = Sheets.FirstOrDefault(x => x.Name == model.Location.SheetName);
            if (sheet == null)
            {
                sheet = new SheetModel(model.Location.SheetName);
                Sheets.Add(sheet);
            }

            sheet.Cells.Add(model);

            if (model.CellType == CellType.Corner)
            {
                sheet.CornerCell = model;
            }
        }

        private void CellRemovedFromTracker(CellModel model)
        {
            var sheet = Sheets.First(x => x.Name == model.Location.SheetName);
            sheet.Cells.Remove(model);
            if (sheet.Cells.Count == 0)
            {
                if (sheet != null)
                {
                    Sheets.Remove(sheet);
                    RefreshOrderedSheetsList();
                }
            }
        }

        private void FixMergedCellsWithNewIdentities(IEnumerable<CellModel> cells, Dictionary<string, string> oldIdToNewIdMap)
        {
            foreach (var cell in cells.Where(cell => cell.MergedWith != string.Empty))
            {
                cell.MergedWith = oldIdToNewIdMap[cell.MergedWith];
            }
        }

        private Dictionary<string, string> GiveCellsNewUniqueIndentities(string sheetName, IEnumerable<CellModel> cells)
        {
            var oldIdToNewIdMap = new Dictionary<string, string>();
            foreach (var cell in cells)
            {
                var newId = Guid.NewGuid().ToString();
                oldIdToNewIdMap[cell.ID] = newId;
                cell.ID = newId;
                cell.Location.SheetName = sheetName;
            }
            return oldIdToNewIdMap;
        }

        private void RefreshOrderedSheetsList()
        {
            OrderedSheets.Clear();
            foreach (var sheet in Sheets.OrderBy(x => x.Order))
            {
                OrderedSheets.Add(sheet);
            }
        }

        private void SheetPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SheetModel.Order))
            {
                RefreshOrderedSheetsList();
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
