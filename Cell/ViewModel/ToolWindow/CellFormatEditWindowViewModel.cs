using Cell.Common;
using Cell.Data;
using Cell.Execution.SyntaxWalkers.CellReferences;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace Cell.ViewModel.ToolWindow
{
    public class CellFormatEditWindowViewModel : ToolWindowViewModel
    {
        private readonly ObservableCollection<CellModel> _cellsToEdit;
        private readonly CellTracker _cellTracker;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private CellStyleModel? _cellStyleToDisplay = null;
        private CellModel _cellToDisplay = CellModel.Null;
        private bool isDetailedBorderEditingEnabled = false;
        public CellFormatEditWindowViewModel(ObservableCollection<CellModel> cellsToEdit, CellTracker cellTracker, PluginFunctionLoader pluginFunctionLoader)
        {
            _pluginFunctionLoader = pluginFunctionLoader;
            _cellsToEdit = cellsToEdit;
            _cellTracker = cellTracker;
        }

        /// <summary>
        /// Provides a list of commands to display in the title bar of the tool window.
        /// </summary>
        public override List<CommandViewModel> ToolBarCommands => 
        [
            new CommandViewModel("╾╼", () => IsDetailedBorderEditingEnabled = !IsDetailedBorderEditingEnabled) { ToolTip = "Show/Hide the text boxes that allow editing the border and margins left/right/top/bottom sides individually." }
        ];

        public override double MinimumHeight => 220;

        public override double MinimumWidth => 260;

        public string BackgroundColor
        {
            get => CellStyleToDisplay.BackgroundColor;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.BackgroundColor = value;
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string BorderColor
        {
            get => CellStyleToDisplay.BorderColor;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.BorderColor = value;
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string BorderThicknessBottom
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.Border).Bottom.ToString();
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Border = $"{currentThickness.Left},{currentThickness.Top},{currentThickness.Right},{value}";
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string BorderThicknessLeft
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.Border).Left.ToString();
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Border = $"{value},{currentThickness.Top},{currentThickness.Right},{currentThickness.Bottom}";
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string BorderThicknessRight
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.Border).Right.ToString();
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Border = $"{currentThickness.Left},{currentThickness.Top},{value},{currentThickness.Bottom}";
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string BorderThicknessTop
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.Border).Top.ToString();
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Border = IsDetailedBorderEditingEnabled
                        ? $"{currentThickness.Left},{value},{currentThickness.Right},{currentThickness.Bottom}"
                        : $"{value},{value},{value},{value}";
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public CellType CellType
        {
            get => CellToDisplay.CellType;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                foreach (var cell in _cellsToEdit)
                {
                    if (cell.CellType.IsSpecial()) return;
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.CellType = value;
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string ContentBackgroundColor
        {
            get => CellStyleToDisplay.ContentBackgroundColor;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentBackgroundColor = value;
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string ContentBorderColor
        {
            get => CellStyleToDisplay.ContentBorderColor;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentBorderColor = value;
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string ContentBorderThicknessBottom
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder).Bottom.ToString();
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentBorder = $"{currentThickness.Left},{currentThickness.Top},{currentThickness.Right},{value}";
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string ContentBorderThicknessLeft
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder).Left.ToString();
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentBorder = $"{value},{currentThickness.Top},{currentThickness.Right},{currentThickness.Bottom}";
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string ContentBorderThicknessRight
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder).Right.ToString();
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentBorder = $"{currentThickness.Left},{currentThickness.Top},{value},{currentThickness.Bottom}";
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string ContentBorderThicknessTop
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder).Top.ToString();
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentBorder = IsDetailedBorderEditingEnabled
                         ? $"{currentThickness.Left},{value},{currentThickness.Right},{currentThickness.Bottom}"
                         : $"{value},{value},{value},{value}";
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string ContentHighlightColor
        {
            get => CellStyleToDisplay.HighlightColor;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.HighlightColor = value;
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string Font
        {
            get => CellStyleToDisplay.Font;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Font = value;
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public double FontSize
        {
            get => CellStyleToDisplay.FontSize;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.FontSize = value;
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public FontStyle FontStyleForView => IsFontItalic ? FontStyles.Italic : FontStyles.Normal;

        public FontWeight FontWeightForView => IsFontBold ? FontWeights.Bold : FontWeights.Normal;

        public string ForegroundColor
        {
            get => CellStyleToDisplay.ForegroundColor;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ForegroundColor = value;
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public double Height
        {
            get => CellToDisplay.Height;
            set
            {
                if (value < 5) return;
                if (value > 500) return;
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                foreach (var cell in _cellsToEdit)
                {
                    var rowCell = _cellTracker.GetCell(cell.SheetName, cell.Row, 0);
                    if (rowCell is null) continue;
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(rowCell);
                    rowCell.Height = value;
                    ApplicationViewModel.SafeInstance?.SheetViewModel?.UpdateLayout();
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get => CellStyleToDisplay.HorizontalAlignment;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.HorizontalAlignment = value;
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public bool IsDetailedBorderEditingEnabled
        {
            get => isDetailedBorderEditingEnabled; set
            {
                if (isDetailedBorderEditingEnabled == value) return;
                isDetailedBorderEditingEnabled = value;
                NotifyPropertyChanged(nameof(IsDetailedBorderEditingEnabled));
            }
        }

        public bool IsFontBold
        {
            get => CellStyleToDisplay.Bold;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Bold = value;
                }
                NotifyPropertyChanged(nameof(IsFontBold));
                NotifyPropertyChanged(nameof(FontWeightForView));
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public bool IsFontItalic
        {
            get => CellStyleToDisplay.Italic;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Italic = value;
                }
                NotifyPropertyChanged(nameof(IsFontItalic));
                NotifyPropertyChanged(nameof(FontStyleForView));
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public bool IsFontStrikethrough
        {
            get => CellStyleToDisplay.Strikethrough;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Strikethrough = value;
                }
                NotifyPropertyChanged(nameof(IsFontStrikethrough));
                NotifyPropertyChanged(nameof(TextDecorationsForView));
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string MarginBottom
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin).Bottom.ToString();
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentMargin = $"{currentThickness.Left},{currentThickness.Top},{currentThickness.Right},{value}";
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string MarginLeft
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin).Left.ToString();
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentMargin = $"{value},{currentThickness.Top},{currentThickness.Right},{currentThickness.Bottom}";
                }
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
            }
        }

        public string MarginRight
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin).Right.ToString();
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentMargin = $"{currentThickness.Left},{currentThickness.Top},{value},{currentThickness.Bottom}";
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public string MarginTop
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin).Top.ToString();
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentMargin = IsDetailedBorderEditingEnabled
                     ? $"{currentThickness.Left},{value},{currentThickness.Right},{currentThickness.Bottom}"
                     : $"{value},{value},{value},{value}";
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public TextAlignment TextAlignment
        {
            get => CellStyleToDisplay.TextAlignment;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.TextAlignment = value;
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public TextDecorationCollection? TextDecorationsForView => IsFontStrikethrough ? TextDecorations.Strikethrough : null;

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle
        {
            get
            {
                var currentlySelectedCell = _cellsToEdit.FirstOrDefault();
                if (currentlySelectedCell is null) return "Select a cell to edit";
                if (currentlySelectedCell == ApplicationViewModel.Instance.ApplicationSettings.DefaultCellStyleCellModel) return "Edit default cell format";
                if (currentlySelectedCell == ApplicationViewModel.Instance.ApplicationSettings.DefaultSpecialCellStyleCellModel) return "Edit default row.column cell format";
                return $"Format editor - {currentlySelectedCell.GetName()}";
            }
        }

        public VerticalAlignment VerticalAlignment
        {
            get => CellStyleToDisplay.VerticalAlignment;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.VerticalAlignment = value;
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        public double Width
        {
            get => CellToDisplay.Width;
            set
            {
                if (value < 5) return;
                if (value > 500) return;
                ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
                foreach (var cell in _cellsToEdit)
                {
                    if (cell.Width == value) continue;
                    var columnCell = _cellTracker.GetCell(cell.SheetName, 0, cell.Column);
                    if (columnCell is null) continue;
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(columnCell);
                    columnCell.Width = value;
                    ApplicationViewModel.SafeInstance?.SheetViewModel?.UpdateLayout();
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        private CellStyleModel CellStyleToDisplay
        {
            get => _cellStyleToDisplay ?? new CellStyleModel();
            set
            {
                if (_cellStyleToDisplay is not null) _cellStyleToDisplay.PropertyChanged -= CellToDisplayStylePropertyChanged;
                _cellStyleToDisplay = value;
                if (_cellStyleToDisplay is not null) _cellStyleToDisplay.PropertyChanged += CellToDisplayStylePropertyChanged;
                NotifyPropertyChanged(nameof(BackgroundColor));
                NotifyPropertyChanged(nameof(BorderColor));
                NotifyPropertyChanged(nameof(ContentHighlightColor));
                NotifyPropertyChanged(nameof(ContentBorderColor));
                NotifyPropertyChanged(nameof(ContentBackgroundColor));
                NotifyPropertyChanged(nameof(ForegroundColor));
                NotifyPropertyChanged(nameof(Font));
            }
        }

        private CellModel CellToDisplay
        {
            get => _cellToDisplay;
            set
            {
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged -= CellToDisplayPropertyChanged;
                _cellToDisplay = value;
                CellStyleToDisplay = _cellToDisplay.Style;
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged += CellToDisplayPropertyChanged;
                NotifyPropertyChanged(nameof(CellToDisplay));
                NotifyPropertyChanged(nameof(Width));
                NotifyPropertyChanged(nameof(Height));
                NotifyPropertyChanged(nameof(CellType));
                NotifyPropertyChanged(nameof(ToolWindowTitle));
            }
        }

        public void AddColumnToTheLeft()
        {
            var leftmostColumnCell = _cellsToEdit
                .Select(x => _cellTracker.GetCell(x.SheetName, 0, x.Column))
                .Where(x => x is not null)
                .Distinct()
                .OrderBy(x => x?.Column ?? 0)
                .FirstOrDefault();
            if (leftmostColumnCell == null) return;
            InsertColumnAtIndex(leftmostColumnCell.SheetName, leftmostColumnCell.Column);
        }

        public void AddColumnToTheRight()
        {
            var rightmostColumnCell = _cellsToEdit
                .Select(x => _cellTracker.GetCell(x.SheetName, 0, x.Column))
                .Where(x => x is not null)
                .Distinct()
                .OrderByDescending(x => x?.Column ?? 0)
                .FirstOrDefault();
            if (rightmostColumnCell == null) return;
            InsertColumnAtIndex(rightmostColumnCell.SheetName, rightmostColumnCell.Column + 1);
        }

        public void AddRowAbove()
        {
            var topmostRowCell = _cellsToEdit
                .Select(x => _cellTracker.GetCell(x.SheetName, x.Row, 0))
                .Where(x => x is not null)
                .Distinct()
                .OrderBy(x => x?.Row ?? 0)
                .FirstOrDefault();
            if (topmostRowCell == null) return;
            InsertRowAtIndex(topmostRowCell.SheetName, topmostRowCell.Row);
        }

        public void AddRowBelow()
        {
            var bottomMostRowCell = _cellsToEdit
                .Select(x => _cellTracker.GetCell(x.SheetName, x.Row, 0))
                .Where(x => x is not null)
                .Distinct()
                .OrderByDescending(x => x?.Row ?? 0)
                .FirstOrDefault();
            if (bottomMostRowCell == null) return;
            InsertRowAtIndex(bottomMostRowCell.SheetName, bottomMostRowCell.Row + 1);
        }

        public void DeleteColumns()
        {
            foreach (var cell in _cellsToEdit.ToList())
            {
                var columnCell = _cellTracker.GetCell(cell.SheetName, 0, cell.Column);
                if (columnCell is not null) DeleteColumn(columnCell);
            }
        }

        public void DeleteRow(CellModel rowCell)
        {
            if (rowCell.CellType != CellType.Row) return;
            var cellsToDelete = _cellTracker.GetCellModelsForSheet(rowCell.SheetName).Where(x => x.Row == rowCell.Row).ToList();
            foreach (var cell in cellsToDelete)
            {
                var nextCell = _cellTracker.GetCell(rowCell.SheetName, cell.Row + 1, cell.Column);
                cell.EnsureIndexStaysCumulativeWhenRemoving(nextCell, _cellTracker);
                _cellTracker.RemoveCell(cell);
            }
            IncrementRowOfAllAtOrBelow(rowCell.SheetName, rowCell.Row, -1);

            foreach (var function in _pluginFunctionLoader.ObservableFunctions)
            {
                IncrementRowReferenceOfAbsoluteReferencesForInsertedRow(rowCell.SheetName, rowCell.Row, function, -1);
            }
        }

        public void DeleteRows()
        {
            foreach (var cell in _cellsToEdit.ToList())
            {
                var rowCell = _cellTracker.GetCell(cell.SheetName, cell.Row, 0);
                if (rowCell is not null) DeleteRow(rowCell);
            }
        }

        public override void HandleBeingClosed()
        {
            _cellsToEdit.CollectionChanged -= CellsToEditCollectionChanged;
            CellToDisplay = CellModel.Null;
        }

        public override void HandleBeingShown()
        {
            _cellsToEdit.CollectionChanged += CellsToEditCollectionChanged;
            PickDisplayedCell();
        }

        public void MergeCells()
        {
            ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
            MergeCells(_cellsToEdit);
            ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
        }

        public void MergeCellsAcross()
        {
            ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
            var selectedCells = _cellsToEdit.ToList();
            var rows = selectedCells?.Select(x => x.Row).Distinct().ToList() ?? [];
            foreach (var row in rows)
            {
                var cellsToMerge = selectedCells?.Where(x => x.Row == row).ToList() ?? [];
                MergeCells(cellsToMerge);
            }
            ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
        }

        public void MergeCellsDown()
        {
            ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
            var selectedCells = _cellsToEdit.ToList();
            var columns = selectedCells?.Select(x => x.Column).Distinct().ToList() ?? [];
            foreach (var column in columns)
            {
                var cellsToMerge = selectedCells?.Where(x => x.Column == column).ToList();
                MergeCells(cellsToMerge ?? []);
            }
            ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
        }

        public void UnmergeCells()
        {
            ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
            UnmergeCells(_cellsToEdit.Where(x => x.IsMergedParent()));
            ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
        }

        private static void SetMergedWithToCellsId(List<CellModel> cellsToMerge, CellModel topLeftCell)
        {
            foreach (var cell in cellsToMerge)
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                cell.MergedWith = topLeftCell.ID;
            }
        }

        private void CellsToEditCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => PickDisplayedCell();

        private void CellToDisplayPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.CellType)) NotifyPropertyChanged(nameof(CellType));
            else if (e.PropertyName == nameof(CellModel.Style)) _cellStyleToDisplay = _cellToDisplay.Style;
        }

        private void CellToDisplayStylePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is null) return;
            if (e.PropertyName == nameof(CellStyleModel.ContentMargin))
            {
                NotifyPropertyChanged(nameof(MarginTop));
                NotifyPropertyChanged(nameof(MarginBottom));
                NotifyPropertyChanged(nameof(MarginRight));
                NotifyPropertyChanged(nameof(MarginLeft));
            }
            else if (e.PropertyName == nameof(CellStyleModel.ContentBorder))
            {
                NotifyPropertyChanged(nameof(ContentBorderThicknessTop));
                NotifyPropertyChanged(nameof(ContentBorderThicknessBottom));
                NotifyPropertyChanged(nameof(ContentBorderThicknessRight));
                NotifyPropertyChanged(nameof(ContentBorderThicknessLeft));
            }
            else if (e.PropertyName == nameof(CellStyleModel.Border))
            {
                NotifyPropertyChanged(nameof(BorderThicknessTop));
                NotifyPropertyChanged(nameof(BorderThicknessBottom));
                NotifyPropertyChanged(nameof(BorderThicknessRight));
                NotifyPropertyChanged(nameof(BorderThicknessLeft));
            }
            else NotifyPropertyChanged(e.PropertyName);
        }

        private void DeleteColumn(CellModel columnCell)
        {
            if (columnCell.CellType != CellType.Column) return;
            var cellsToDelete = _cellTracker.GetCellModelsForSheet(columnCell.SheetName).Where(x => x.Column == columnCell.Column).ToList();
            foreach (var cell in cellsToDelete)
            {
                var nextCell = _cellTracker.GetCell(columnCell.SheetName, cell.Row, cell.Column + 1);
                cell.EnsureIndexStaysCumulativeWhenRemoving(nextCell, _cellTracker);
                _cellTracker.RemoveCell(cell);
            }
            IncrementColumnOfAllAtOrToTheRightOf(columnCell.SheetName, columnCell.Column, -1);

            foreach (var function in _pluginFunctionLoader.ObservableFunctions)
            {
                IncrementColumnReferenceOfAbsoluteReferencesForInsertedColumn(columnCell.SheetName, columnCell.Column, function, -1);
            }
        }

        private List<CellModel> GetAllCellsAtOrBelow(string sheetName, int row) => _cellTracker.GetCellModelsForSheet(sheetName).Where(x => x.Row >= row).ToList();

        private List<CellModel> GetAllCellsAtOrToTheRightOf(string sheetName, int column) => _cellTracker.GetCellModelsForSheet(sheetName).Where(x => x.Column >= column).ToList();

        private List<CellModel> GetCellsInRectangle(int startRow, int startColumn, int endRow, int endColumn, string sheetName)
        {
            var cells = new List<CellModel>();
            for (var row = startRow; row <= endRow; row++)
            {
                for (var column = startColumn; column <= endColumn; column++)
                {
                    var cell = _cellTracker.GetCell(sheetName, row, column);
                    if (cell is not null) cells.Add(cell);
                }
            }
            return cells;
        }

        private void IncrementColumnOfAllAtOrToTheRightOf(string sheetName, int column, int amount = 1)
        {
            var cells = GetAllCellsAtOrToTheRightOf(sheetName, column);
            foreach (var cell in cells) cell.Column += amount;
        }

        private void IncrementColumnReferenceOfAbsoluteReferencesForInsertedColumn(string sheetName, int columnIndex, CellFunction function, int incrementAmount)
        {
            var refactorer = new CellReferenceRefactorRewriter(x =>
            {
                if (x.SheetName != sheetName) return x;
                if (x.IsColumnRelative) return x;
                if (x.Column >= columnIndex) x.Column += 1;
                if (x.IsRange && x.ColumnRangeEnd + 1 >= columnIndex) x.ColumnRangeEnd += incrementAmount;
                return x;
            });
            function.Model.Code = refactorer.Visit(CSharpSyntaxTree.ParseText(function.Model.Code).GetRoot())?.ToFullString() ?? "";
        }

        private void IncrementRowOfAllAtOrBelow(string sheetName, int row, int amount = 1)
        {
            var cells = GetAllCellsAtOrBelow(sheetName, row);
            foreach (var cell in cells) cell.Row += amount;
        }

        private void IncrementRowReferenceOfAbsoluteReferencesForInsertedRow(string sheetName, int rowIndex, CellFunction function, int incrementAmount)
        {
            // TODO: record this in undo stack.
            var refactorer = new CellReferenceRefactorRewriter(x =>
            {
                if (x.SheetName != sheetName) return x;
                if (x.IsRowRelative) return x;
                if (x.Row >= rowIndex) x.Row += 1;
                if (x.IsRange && x.RowRangeEnd + 1 >= rowIndex) x.RowRangeEnd += incrementAmount;
                return x;
            });
            function.Model.Code = refactorer.Visit(CSharpSyntaxTree.ParseText(function.Model.Code).GetRoot())?.ToFullString() ?? "";
        }

        private void InsertColumnAtIndex(string sheetName, int index)
        {
            IncrementColumnOfAllAtOrToTheRightOf(sheetName, index);

            CellModelFactory.Create(0, index, CellType.Column, sheetName, _cellTracker);

            var rowIndexs = _cellTracker.GetCellModelsForSheet(sheetName).Where(x => x.CellType == CellType.Row).Select(x => x.Row).ToList();
            foreach (var rowIndex in rowIndexs)
            {
                var cellModel = CellModelFactory.Create(rowIndex, index, CellType.Label, sheetName, _cellTracker);
                var firstNeighbor = _cellTracker.GetCell(sheetName, rowIndex, index - 1);
                var secondNeighbor = _cellTracker.GetCell(sheetName, rowIndex, index + 1);
                cellModel.MergeCellIntoCellsIfTheyWereMerged(firstNeighbor, secondNeighbor);
                cellModel.EnsureIndexStaysCumulativeBetweenNeighborsWhenAdding(firstNeighbor, secondNeighbor, _cellTracker);
            }

            foreach (var function in _pluginFunctionLoader.ObservableFunctions)
            {
                IncrementColumnReferenceOfAbsoluteReferencesForInsertedColumn(sheetName, index, function, 1);
            }
        }

        private void InsertRowAtIndex(string sheetName, int newRowIndex)
        {
            IncrementRowOfAllAtOrBelow(sheetName, newRowIndex);

            var rowModel = CellModelFactory.Create(newRowIndex, 0, CellType.Row, sheetName, _cellTracker);

            var columnIndexs = _cellTracker.GetCellModelsForSheet(sheetName).Where(x => x.CellType == CellType.Column).Select(x => x.Column).ToList();
            foreach (var columnIndex in columnIndexs)
            {
                var cellModel = CellModelFactory.Create(newRowIndex, columnIndex, CellType.Label, sheetName, _cellTracker);
                var firstNeighbor = _cellTracker.GetCell(sheetName, newRowIndex - 1, columnIndex);
                var secondNeighbor = _cellTracker.GetCell(sheetName, newRowIndex + 1, columnIndex);
                cellModel.MergeCellIntoCellsIfTheyWereMerged(firstNeighbor, secondNeighbor);
                cellModel.EnsureIndexStaysCumulativeBetweenNeighborsWhenAdding(firstNeighbor, secondNeighbor, _cellTracker);
            }

            foreach (var function in _pluginFunctionLoader.ObservableFunctions)
            {
                IncrementRowReferenceOfAbsoluteReferencesForInsertedRow(rowModel.SheetName, rowModel.Row, function, 1);
            }
        }

        private void MergeCells(IEnumerable<CellModel> cells)
        {
            if (cells.Count() < 2) return;
            var leftmost = cells.Select(x => x.Column).Min();
            var topmost = cells.Select(x => x.Row).Min();
            var rightmost = cells.Select(x => x.Column).Max();
            var bottommost = cells.Select(x => x.Row).Max();

            var topLeftCell = cells.FirstOrDefault(x => x.Row == topmost && x.Column == leftmost);
            if (topLeftCell is null) return;
            var bottomRightCell = cells.FirstOrDefault(x => x.Row == bottommost && x.Column == rightmost);
            if (bottomRightCell is null) return;

            var sheetName = topLeftCell.SheetName;
            var cellsToMerge = GetCellsInRectangle(topmost, leftmost, bottommost, rightmost, sheetName);
            if (cellsToMerge.Count(cell => cell.IsMerged()) <= 1)
            {
                UnmergeCells(cellsToMerge);
            }
            else return;
            SetMergedWithToCellsId(cellsToMerge, topLeftCell);
        }

        private void PickDisplayedCell()
        {
            CellToDisplay = _cellsToEdit.Count > 0 ? _cellsToEdit[0] : CellModel.Null;
        }

        private void UnmergeCell(CellModel mergeParent)
        {
            var mergedCells = _cellTracker.GetCellModelsForSheet(mergeParent.SheetName).Where(x => x.IsMergedWith(mergeParent));
            foreach (var cell in mergedCells)
            {
                if (mergeParent == cell) continue;
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                cell.MergedWith = string.Empty;
            }
            ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(mergeParent);
            mergeParent.MergedWith = string.Empty;
        }

        private void UnmergeCells(IEnumerable<CellModel> cells)
        {
            foreach (var cell in cells)
            {
                UnmergeCell(cell);
            }
        }
    }
}
