﻿using Cell.Common;
using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells.Types;
using Cell.ViewModel.Cells.Types.Special;
using Cell.ViewModel.Execution;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;

namespace Cell.ViewModel.Cells
{
    public class SheetViewModel(string sheetName) : PropertyChangedBase
    {
        public static readonly SheetViewModel NullSheet = new("null");
        private bool _enableMultiEditSelectedCells = true;
        private string lastKeyPressed = string.Empty;
        private CellModel? oldSelectedCellState;
        private CellViewModel? selectedCellViewModel;
        public ObservableCollection<CellViewModel> CellViewModels { get; set; } = [];

        public List<CellViewModel> HighlightedCellViewModels { get; } = [];

        public string LastKeyPressed
        {
            get => lastKeyPressed;
            set
            {
                lastKeyPressed = value;
                NotifyPropertyChanged(nameof(LastKeyPressed));
            }
        }

        public CellViewModel? SelectedCellViewModel
        {
            get => selectedCellViewModel;
            set
            {
                _enableMultiEditSelectedCells = false;
                if (selectedCellViewModel is not null)
                {
                    selectedCellViewModel.PropertyChanged -= PropertyChangedOnSelectedCell;
                    selectedCellViewModel.SelectionColor = new SolidColorBrush(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(selectedCellViewModel.BackgroundColorHex), 100));
                    selectedCellViewModel.SelectionBorderColor = new SolidColorBrush(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(selectedCellViewModel.BackgroundColorHex), 175));
                }
                selectedCellViewModel = value;
                oldSelectedCellState = selectedCellViewModel?.Model.Copy();
                if (selectedCellViewModel is not null)
                {
                    selectedCellViewModel.PropertyChanged += PropertyChangedOnSelectedCell;
                    selectedCellViewModel.SelectionColor = new SolidColorBrush(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(selectedCellViewModel.BackgroundColorHex), 100));
                    selectedCellViewModel.SelectionBorderColor = new SolidColorBrush(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(selectedCellViewModel.BackgroundColorHex), 175));
                }
                NotifyPropertyChanged(nameof(SelectedCellViewModel));
                _enableMultiEditSelectedCells = true;
            }
        }

        public List<CellViewModel> SelectedCellViewModels { get; } = [];

        public string SheetName { get; private set; } = sheetName;

        public void AddCell(CellModel newModel)
        {
            var newViewModel = CellViewModelFactory.Create(newModel, this);
            AddCell(newViewModel);
        }

        public void AddCell(CellViewModel cell)
        {
            CellViewModels.Add(cell);
            cell.Model.PropertyChanged += CellModelPropertyChanged;
        }

        public void DeleteCell(CellModel cell)
        {
            var viewModel = CellViewModels.First(x => x.Model == cell);
            if (viewModel.Model.MergedWith == viewModel.Model.ID) UnmergeCell(viewModel);
            DeleteCell(viewModel);
        }

        public void DeleteCell(CellViewModel cell)
        {
            cell.Model.PropertyChanged -= CellModelPropertyChanged;
            CellViewModels.Remove(cell);
            SelectedCellViewModels.Remove(cell);
            ApplicationViewModel.Instance.CellTracker.RemoveCell(cell.Model);
        }

        public void HighlightCell(CellViewModel cellToHighlight, string color)
        {
            _enableMultiEditSelectedCells = false;
            cellToHighlight.HighlightCell(color);
            HighlightedCellViewModels.Add(cellToHighlight);
            _enableMultiEditSelectedCells = true;
        }

        public void InitializeSheet()
        {
            if (CellViewModels.Count == 0) AddDefaultCells();
            UpdateLayout();
        }

        public void LoadCellViewModels()
        {
            foreach (var cell in CellViewModelFactory.CreateCellViewModelsForSheet(this))
            {
                AddCell(cell);
            }
            InitializeSheet();
        }

        public void MoveSelection(int columnOffset, int rowOffset)
        {
            if (SelectedCellViewModel is null) return;
            if (columnOffset > 0) columnOffset += SelectedCellViewModel.Model.CellsMergedToRight();
            if (rowOffset > 0) rowOffset += SelectedCellViewModel.Model.CellsMergedBelow();
            var cellToSelect = CellViewModels.FirstOrDefault(x => x.Column == SelectedCellViewModel.Column + columnOffset && x.Row == SelectedCellViewModel.Row + rowOffset);
            if (cellToSelect is null) return;
            UnselectAllCells();
            SelectCell(cellToSelect);
        }

        public void MoveSelectionDown() => MoveSelection(0, 1);

        public void MoveSelectionLeft() => MoveSelection(-1, 0);

        public void MoveSelectionRight() => MoveSelection(1, 0);

        public void MoveSelectionUp() => MoveSelection(0, -1);

        public void ReinstantiateCellsViewModel(CellViewModel? cellViewModel)
        {
            if (cellViewModel is null) return;
            SelectedCellViewModels.Remove(cellViewModel);
            HighlightedCellViewModels.Remove(cellViewModel);
            CellViewModels.Remove(cellViewModel);
            var newViewModel = CellViewModelFactory.Create(cellViewModel.Model, this);
            CellViewModels.Add(newViewModel);
        }

        public void SelectCell(CellModel cell) => SelectCell(CellViewModels.First(x => x.Model == cell));

        public void SelectCell(CellViewModel cell)
        {
            UnhighlightAllCells();
            if (!string.IsNullOrWhiteSpace(cell.Model.MergedWith))
            {
                cell = CellViewModels.First(x => x.Model.ID == cell.Model.MergedWith);
            }
            SelectedCellViewModel = cell;
            if (cell.IsSelected) return;
            cell.IsSelected = true;
            SelectedCellViewModels.Add(cell);
            if (SelectedCellViewModels.Count == 1)
            {
                if (ApplicationViewModel.Instance.PluginFunctionLoader.TryGetFunction("object", SelectedCellViewModel.Model.PopulateFunctionName, out var populate))
                {
                    if (ApplicationViewModel.Instance.ApplicationSettings.HighlightPopulateCellDependencies)
                    {
                        HighlightCellDependenciesOfFunction(populate);
                    }

                    if (ApplicationViewModel.Instance.ApplicationSettings.HighlightPopulateCollectionDependencies)
                    {
                        HighlightCollectionDependenciesForFunction(populate);
                    }
                }
                if (ApplicationViewModel.Instance.PluginFunctionLoader.TryGetFunction("void", SelectedCellViewModel.Model.TriggerFunctionName, out var trigger))
                {
                    if (ApplicationViewModel.Instance.ApplicationSettings.HighlightTriggerCellDependencies)
                    {
                        HighlightCellDependenciesOfFunction(trigger);
                    }

                    if (ApplicationViewModel.Instance.ApplicationSettings.HighlightTriggerCollectionDependencies)
                    {
                        HighlightCollectionDependenciesForFunction(trigger);
                    }
                }
            }
        }

        public void UnhighlightAllCells()
        {
            foreach (var cellToUnHighlight in HighlightedCellViewModels)
            {
                cellToUnHighlight.UnhighlightCell();
            }
            HighlightedCellViewModels.Clear();
        }

        public void UnmergeCell(CellViewModel mergedCell)
        {
            var cells = CellViewModels.Where(x => x.Model.MergedWith == mergedCell.ID);
            foreach (var cell in cells)
            {
                if (mergedCell == cell) continue;
                cell.Model.MergedWith = string.Empty;
            }
            mergedCell.Model.MergedWith = string.Empty;
        }

        public void UnselectAllCells()
        {
            foreach (var cell in SelectedCellViewModels.ToList())
            {
                UnselectCell(cell);
            }
        }

        internal void UnselectCell(CellViewModel cell)
        {
            SelectedCellViewModels.Remove(cell);
            cell.IsSelected = false;
            if (SelectedCellViewModel == cell)
            {
                SelectedCellViewModel = null;
            }
        }

        internal void UpdateLayout()
        {
            _enableMultiEditSelectedCells = false;
            var layout = new CellLayout(CellViewModels);
            layout.UpdateLayout();
            _enableMultiEditSelectedCells = true;
        }

        private void AddDefaultCells()
        {
            var corner = CellModelFactory.Create(0, 0, CellType.Corner, SheetName);
            AddCell(corner);
            var row = CellModelFactory.Create(1, 0, CellType.Row, SheetName);
            AddCell(row);
            var column = CellModelFactory.Create(0, 1, CellType.Column, SheetName);
            AddCell(column);
            var cell = CellModelFactory.Create(1, 1, CellType.Label, SheetName);
            AddCell(cell);

            var columnViewModel = CellViewModels.OfType<ColumnCellViewModel>().First();
            columnViewModel.AddColumnToTheRight();
            columnViewModel.AddColumnToTheRight();

            var rowViewModel = CellViewModels.OfType<RowCellViewModel>().First();
            rowViewModel.AddRowBelow();
            rowViewModel.AddRowBelow();
            rowViewModel.AddRowBelow();
        }

        private void CellModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var model = sender as CellModel ?? throw new NullReferenceException("This handler is only for use with non null CellModel objects.");
            var viewModel = CellViewModels.FirstOrDefault(x => x.Model == model);
            if (viewModel == null) return;
            if (e.PropertyName == nameof(CellModel.CellType))
            {
                var newType = model.CellType;
                ReinstantiateCellsViewModel(viewModel);
            }
        }

        private void HighlightCellDependenciesOfFunction(FunctionViewModel function)
        {
            foreach (var locationDependencies in function.LocationDependencies)
            {
                if (SelectedCellViewModel == null) return;
                if (locationDependencies.IsRange)
                {
                    var sheet = locationDependencies.SheetName;
                    if (sheet != SheetName) continue;
                    var row = locationDependencies.Row;
                    var column = locationDependencies.Column;
                    if (locationDependencies.IsColumnRelative) column += SelectedCellViewModel.Column;
                    if (locationDependencies.IsRowRelative) row += SelectedCellViewModel.Row;

                    var rowRangeEnd = locationDependencies.RowRangeEnd;
                    var columnRangeEnd = locationDependencies.ColumnRangeEnd;
                    if (locationDependencies.IsColumnRelativeRangeEnd) columnRangeEnd += SelectedCellViewModel.Column;
                    if (locationDependencies.IsRowRelativeRangeEnd) rowRangeEnd += SelectedCellViewModel.Row;

                    for (var r = row; r <= rowRangeEnd; r++)
                    {
                        for (var c = column; c <= columnRangeEnd; c++)
                        {
                            var cellToHighlight = CellViewModels.FirstOrDefault(x => x.Row == r && x.Column == c);
                            if (cellToHighlight == null) continue;
                            HighlightCell(cellToHighlight, "#0438ff44");
                        }
                    }
                }
                else
                {
                    var sheet = locationDependencies.SheetName;
                    if (sheet != SheetName) continue;
                    var row = locationDependencies.Row;
                    var column = locationDependencies.Column;
                    if (locationDependencies.IsColumnRelative) column += SelectedCellViewModel.Column;
                    if (locationDependencies.IsRowRelative) row += SelectedCellViewModel.Row;
                    var cellToHighlight = CellViewModels.FirstOrDefault(x => x.Row == row && x.Column == column);
                    if (cellToHighlight == null) continue;
                    HighlightCell(cellToHighlight, "#0438ff44");
                }
            }
        }

        private void HighlightCollectionDependenciesForFunction(FunctionViewModel function)
        {
            foreach (var collectionReference in function.CollectionDependencies)
            {
                var cellsToHighlight = CellViewModels.OfType<ListCellViewModel>().Where(x => x.CollectionName == collectionReference);
                foreach (var cellToHighlight in cellsToHighlight)
                {
                    HighlightCell(cellToHighlight, "#0438ff44");
                }
            }
        }

        private void PropertyChangedOnSelectedCell(object? sender, PropertyChangedEventArgs e)
        {
            if (!_enableMultiEditSelectedCells) return;
            if (sender is null) return;
            if (string.IsNullOrEmpty(e.PropertyName)) return;
            if (oldSelectedCellState == null) return;
            if (e.PropertyName == nameof(CellViewModel.IsSelected)) return;
            if (e.PropertyName == nameof(CellViewModel.IsHighlighted)) return;
            if (e.PropertyName == nameof(CellViewModel.ShouldShowSelectionBorder)) return;
            if (e.PropertyName == nameof(CellViewModel.ShouldShowSelectionFill)) return;
            if (e.PropertyName == nameof(CellViewModel.SelectionColor)) return;
            if (e.PropertyName == nameof(CellViewModel.SelectionBorderColor)) return;

            ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
            ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(oldSelectedCellState);
            foreach (var cell in CellViewModels.Where(x => x.IsSelected).ToList())
            {
                if (cell == sender) continue;
                var cellType = cell.GetType();
                var selectedType = sender.GetType();
                PropertyInfo? cellProperty = cellType.GetProperty(e.PropertyName, BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo? selectedProperty = selectedType.GetProperty(e.PropertyName, BindingFlags.Public | BindingFlags.Instance);
                if (null != cellProperty && cellProperty.CanWrite && selectedProperty != null && selectedProperty.CanRead)
                {
                    cellProperty.SetValue(cell, selectedProperty.GetValue(sender), null);
                }
            }
            ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
        }
    }
}
