using Cell.Common;
using Cell.Data;
using Cell.Model;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace Cell.ViewModel.ToolWindow
{
    public class CellFormatEditWindowViewModel : PropertyChangedBase
    {
        private readonly ObservableCollection<CellModel> _cellsToEdit;
        private readonly CellTracker _cellTracker;
        private CellModel _cellToDisplay = CellModel.Null;
        private CellModel CellToDisplay
        {
            get => _cellToDisplay;
            set
            {
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged -= CellToDisplayPropertyChanged;
                _cellToDisplay = value;
                CellStyleToDisplay = _cellToDisplay.Style;
                if (CellToDisplay != CellModel.Null) CellToDisplay.PropertyChanged += CellToDisplayPropertyChanged;
            }
        }

        private void CellToDisplayPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.CellType)) NotifyPropertyChanged(nameof(CellType));
            else if (e.PropertyName == nameof(CellModel.Style)) _cellStyleToDisplay = _cellToDisplay.Style;
        }

        private CellStyleModel? _cellStyleToDisplay = null;

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
                NotifyPropertyChanged(nameof(FontFamily));
            }
        }

        public IEnumerable<CellModel> CellsBeingEdited => _cellsToEdit;

        public CellFormatEditWindowViewModel(ObservableCollection<CellModel> cellsToEdit, CellTracker cellTracker)
        {
            _cellsToEdit = cellsToEdit;
            _cellTracker = cellTracker;
            _cellsToEdit.CollectionChanged += CellsToEditCollectionChanged;
            PickDisplayedCell();
        }

        private void PickDisplayedCell()
        {
            CellToDisplay = _cellsToEdit.Count > 0 ? _cellsToEdit[0] : CellModel.Null;

        }

        private void CellsToEditCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => PickDisplayedCell();

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

        public void UnmergeCells(IEnumerable<CellModel> cells)
        {
            foreach (var cell in cells)
            {
                UnmergeCell(cell);
            }
        }

        public void UnmergeCells()
        {
            UnmergeCells(_cellsToEdit.Where(x => x.IsMergedParent()));
        }

        public void UnmergeCell(CellModel mergeParent)
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

        internal void MergeCells()
        {
            MergeCells(_cellsToEdit);
        }

        internal void MergeCells(IEnumerable<CellModel> cells)
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

        private static void SetMergedWithToCellsId(List<CellModel> cellsToMerge, CellModel topLeftCell)
        {
            foreach (var cell in cellsToMerge)
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                cell.MergedWith = topLeftCell.ID;
            }
        }

        public void MergeCellsDown()
        {
            var selectedCells = _cellsToEdit.ToList();
            var columns = selectedCells?.Select(x => x.Column).Distinct().ToList() ?? [];
            foreach (var column in columns)
            {
                var cellsToMerge = selectedCells?.Where(x => x.Column == column).ToList();
                MergeCells(cellsToMerge ?? []);
            }
        }

        internal void MergeCellsAcross()
        {
            var selectedCells = _cellsToEdit.ToList();
            var rows = selectedCells?.Select(x => x.Row).Distinct().ToList() ?? [];
            foreach (var row in rows)
            {
                var cellsToMerge = selectedCells?.Where(x => x.Row == row).ToList() ?? [];
                MergeCells(cellsToMerge);
            }
        }

        public string BackgroundColor
        {
            get => CellStyleToDisplay.BackgroundColor;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.BackgroundColor = value;
                }
            }
        }

        public string BorderColor
        {
            get => CellStyleToDisplay.BorderColor;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.BorderColor = value;
                }
            }
        }

        public string ContentHighlightColor
        {
            get => CellStyleToDisplay.HighlightColor;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.HighlightColor = value;
                }
            }
        }

        public string ContentBorderColor
        {
            get => CellStyleToDisplay.ContentBorderColor;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentBorderColor = value;
                }
            }
        }

        public string ContentBackgroundColor
        {
            get => CellStyleToDisplay.ContentBackgroundColor;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentBackgroundColor = value;
                }
            }
        }

        public string ForegroundColor
        {
            get => CellStyleToDisplay.ForegroundColor;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ForegroundColor = value;
                }
            }
        }

        public string FontFamily
        {
            get => CellStyleToDisplay.Font;
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Font = value;
                }
            }
        }

        public double FontSize
        {
            get => CellStyleToDisplay.FontSize;
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.FontSize = value;
                }
            }
        }

        public FontStyle FontStyleForView => IsFontItalic ? FontStyles.Italic : FontStyles.Normal;

        public FontWeight FontWeightForView => IsFontBold ? FontWeights.Bold : FontWeights.Normal;

        public TextDecorationCollection? TextDecorationsForView => IsFontStrikethrough ? TextDecorations.Strikethrough : null;

        public bool IsFontBold
        {
            get => CellStyleToDisplay.Bold;
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Bold = value;
                }
                NotifyPropertyChanged(nameof(IsFontBold));
                NotifyPropertyChanged(nameof(FontWeightForView));
            }
        }

        public bool IsFontItalic
        {
            get => CellStyleToDisplay.Italic;
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Italic = value;
                }
                NotifyPropertyChanged(nameof(IsFontItalic));
                NotifyPropertyChanged(nameof(FontStyleForView));
            }
        }

        public bool IsFontStrikethrough
        {
            get => CellStyleToDisplay.Strikethrough;
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Strikethrough = value;
                }
                NotifyPropertyChanged(nameof(IsFontStrikethrough));
                NotifyPropertyChanged(nameof(TextDecorationsForView));
            }
        }

        public string ContentBorderThicknessBottom
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder).Bottom.ToString();
            set
            {
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentBorder = $"{currentThickness.Left},{currentThickness.Top},{currentThickness.Right},{value}";
                }
            }
        }

        public string ContentBorderThicknessLeft
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder).Left.ToString();
            set
            {
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentBorder = $"{value},{currentThickness.Top},{value},{currentThickness.Bottom}";
                }
            }
        }

        public string ContentBorderThicknessRight
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder).Right.ToString();
            set
            {
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentBorder = $"{currentThickness.Left},{currentThickness.Top},{value},{currentThickness.Bottom}";
                }
            }
        }

        public string ContentBorderThicknessTop
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder).Top.ToString();
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentBorder = $"{value},{value},{value},{value}";
                }
            }
        }

        public string BorderThicknessTop
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.Border).Top.ToString();
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Border = $"{value},{value},{value},{value}";
                }
            }
        }

        public string BorderThicknessBottom
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.Border).Bottom.ToString();
            set
            {
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Border = $"{currentThickness.Left},{currentThickness.Top},{currentThickness.Right},{value}";
                }
            }
        }

        public string BorderThicknessLeft
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.Border).Left.ToString();
            set
            {
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Border = $"{value},{currentThickness.Top},{value},{currentThickness.Bottom}";
                }
            }
        }

        public string BorderThicknessRight
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.Border).Right.ToString();
            set
            {
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentBorder);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.Border = $"{currentThickness.Left},{currentThickness.Top},{value},{currentThickness.Bottom}";
                }
            }
        }

        public string MarginTop
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin).Top.ToString();
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentMargin = $"{value},{value},{value},{value}";
                }
            }
        }

        public string MarginBottom
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin).Bottom.ToString();
            set
            {
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentMargin = $"{currentThickness.Left},{currentThickness.Top},{currentThickness.Right},{value}";
                }
            }
        }

        public string MarginLeft
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin).Left.ToString();
            set
            {
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentMargin = $"{value},{currentThickness.Top},{value},{currentThickness.Bottom}";
                }
            }
        }

        public string MarginRight
        {
            get => Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin).Right.ToString();
            set
            {
                var currentThickness = Utilities.ParseStringIntoThickness(CellStyleToDisplay.ContentMargin);
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.ContentMargin = $"{currentThickness.Left},{currentThickness.Top},{value},{currentThickness.Bottom}";
                }
            }
        }

        public TextAlignment TextAlignment
        {
            get => CellStyleToDisplay.TextAlignment;
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.TextAlignment = value;
                }
            }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get => CellStyleToDisplay.HorizontalAlignment;
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.HorizontalAlignment = value;
                }
            }
        }

        public VerticalAlignment VerticalAlignment
        {
            get => CellStyleToDisplay.VerticalAlignment;
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.Style.VerticalAlignment = value;
                }
            }
        }

        public CellType CellType
        {
            get => CellToDisplay.CellType;
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    if (cell.CellType.IsSpecial()) return;
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.CellType = value;
                }
            }
        }
    }
}
