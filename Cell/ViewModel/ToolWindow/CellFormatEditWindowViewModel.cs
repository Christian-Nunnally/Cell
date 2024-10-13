using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Core.Execution.SyntaxWalkers.CellReferences;
using Cell.Model;
using Cell.Core.Persistence;
using Cell.ViewModel.Application;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using Cell.Core.Execution.Functions;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// Tool window view model for editing the format of a cell. Allows setting the background color, font, font size, font color, and more.
    /// </summary>
    public class CellFormatEditWindowViewModel : ToolWindowViewModel
    {
        private readonly ObservableCollection<CellModel> _cellsToEdit;
        private readonly CellTracker _cellTracker;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private CellStyleModel? _cellStyleToDisplay = null;
        private CellModel _cellToDisplay = CellModel.Null;
        private bool isDetailedBorderEditingEnabled = false;
        /// <summary>
        /// Creates a new instance of <see cref="CellFormatEditWindowViewModel"/>
        /// </summary>
        /// <param name="cellsToEdit">List of cells to edit, which can change outside of the tool window and the window will adapt.</param>
        /// <param name="cellTracker">The tracker to source cells from.</param>
        /// <param name="pluginFunctionLoader">The function loader used to update functions when cells are deleted.</param>
        public CellFormatEditWindowViewModel(ObservableCollection<CellModel> cellsToEdit, CellTracker cellTracker, PluginFunctionLoader pluginFunctionLoader)
        {
            _pluginFunctionLoader = pluginFunctionLoader;
            _cellsToEdit = cellsToEdit;
            _cellTracker = cellTracker;
        }

        /// <summary>
        /// Gets or sets the cell background color of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the border color of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        private static void EnsureIndexStaysCumulativeBetweenNeighborsWhenAdding(CellModel cellModel, CellModel? firstNeighbor, CellModel? secondNeighbor, CellTracker cellTracker)
        {
            var isThereAFirstNeighbor = firstNeighbor is not null && !firstNeighbor.CellType.IsSpecial();
            var isThereASecondNeighbor = secondNeighbor is not null && !secondNeighbor.CellType.IsSpecial();

            if (!isThereAFirstNeighbor && !isThereASecondNeighbor) return;
            if (isThereAFirstNeighbor && !isThereASecondNeighbor) cellModel.Index = firstNeighbor!.Index + 1;
            else if (isThereASecondNeighbor) FixIndexOfCellsAfterAddedCell(cellModel, secondNeighbor!, cellTracker);
        }

        private static void EnsureIndexStaysCumulativeWhenRemoving(CellModel removingCell, CellModel? nextCell, CellTracker cellTracker)
        {
            if (nextCell is null) return;
            if (nextCell.CellType.IsSpecial()) return;
            else FixIndexOfCellsAfterRemovingCell(removingCell, nextCell, cellTracker);
        }

        private static void FixIndexOfCellsAfterAddedCell(CellModel addedCell, CellModel nextCell, CellTracker cellTracker)
        {
            var xDifference = nextCell.Location.Column - addedCell.Location.Column;
            var yDifference = nextCell.Location.Row - addedCell.Location.Row;
            if (xDifference > 1 || yDifference > 1 || xDifference < 0 || yDifference < 0) throw new CellError("FixIndexOfCellsAfterAddedCell must be called with cells that are next to each other, and added cell must above or to the right of next cell");

            addedCell.Index = nextCell.Index;
            var searchingCell = nextCell;
            int i = nextCell.Index;
            while (searchingCell != null && searchingCell.Index == i)
            {
                searchingCell.Index++;
                i++;
                searchingCell = cellTracker.GetCell(nextCell.Location.SheetName, searchingCell.Location.Row + yDifference, searchingCell.Location.Column + xDifference);
            }
        }

        private static void FixIndexOfCellsAfterRemovingCell(CellModel removedCell, CellModel nextCell, CellTracker cellTracker)
        {
            var xDifference = nextCell.Location.Column - removedCell.Location.Column;
            var yDifference = nextCell.Location.Row - removedCell.Location.Row;
            if (xDifference > 1 || yDifference > 1 || xDifference < 0 || yDifference < 0) throw new CellError("FixIndexOfCellsAfterRemovingCell must be called with cells that are next to each other, and added cell must above or to the right of next cell");

            var searchingCell = nextCell;
            int i = removedCell.Index + 1;
            while (searchingCell != null && searchingCell.Index == i)
            {
                searchingCell.Index--;
                i++;
                searchingCell = cellTracker.GetCell(nextCell.Location.SheetName, searchingCell.Location.Row + yDifference, searchingCell.Location.Column + xDifference);
            }
        }

        /// <summary>
        /// Gets or sets the bottom border thickness of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the left border thickness of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the right border thickness of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the top border thickness of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the cell type of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the content background color of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the content border color of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the bottom border thickness of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the left border thickness of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the right border thickness of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the top border thickness of all selected cells, recording changes to the undo stack.
        /// 
        /// If <see cref="IsDetailedBorderEditingEnabled"/> is false, this will set the entire border.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the content highlight color of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the font of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the font size of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets the font style to display the IsFontItalic of the cell on the format window.
        /// </summary>
        public FontStyle FontStyleForView => IsFontItalic ? FontStyles.Italic : FontStyles.Normal;

        /// <summary>
        /// Gets the font weight to display the IsFontBold of the cell on the format window.
        /// </summary>
        public FontWeight FontWeightForView => IsFontBold ? FontWeights.Bold : FontWeights.Normal;

        /// <summary>
        /// Gets or sets the foreground color of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the height of the rows all selected cells are in, recording changes to the undo stack.
        /// </summary>
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
                    var rowCell = _cellTracker.GetCell(cell.Location.SheetName, cell.Location.Row, 0);
                    if (rowCell is null) continue;
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(rowCell);
                    rowCell.Height = value;
                    ApplicationViewModel.SafeInstance?.SheetViewModel?.UpdateLayout();
                }
                ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
            }
        }

        /// <summary>
        /// Gets or sets the horizontal alignment of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets whether additional buttons should be displayed for editing each side of the margin and border individually.
        /// </summary>
        public bool IsDetailedBorderEditingEnabled
        {
            get => isDetailedBorderEditingEnabled; set
            {
                if (isDetailedBorderEditingEnabled == value) return;
                isDetailedBorderEditingEnabled = value;
                NotifyPropertyChanged(nameof(IsDetailedBorderEditingEnabled));
            }
        }

        /// <summary>
        /// Gets or sets the font bold of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the font italic of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the font strikethrough of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the buttom margin of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the left margin of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the right margin of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the top margin of all selected cells, recording changes to the undo stack.
        /// 
        /// If <see cref="IsDetailedBorderEditingEnabled"/> is false, this will set the entire margin."/>
        /// </summary>
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

        /// <summary>
        /// Gets the minimum height this tool window is allowed to be reized to.
        /// </summary>
        public override double MinimumHeight => 220;

        /// <summary>
        /// Gets the minimum width this tool window is allowed to be reized to.
        /// </summary>
        public override double MinimumWidth => 260;

        /// <summary>
        /// Gets or sets the text alignment of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets the text decorations to display the IsFontStrikethrough of the cell on the format window.
        /// </summary>
        public TextDecorationCollection? TextDecorationsForView => IsFontStrikethrough ? TextDecorations.Strikethrough : null;

        /// <summary>
        /// Provides a list of commands to display in the title bar of the tool window.
        /// </summary>
        public override List<CommandViewModel> ToolBarCommands =>
        [
            new CommandViewModel("╾╼", () => IsDetailedBorderEditingEnabled = !IsDetailedBorderEditingEnabled) { ToolTip = "Show/Hide the text boxes that allow editing the border and margins left/right/top/bottom sides individually." }
        ];

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
                return $"Format editor - {currentlySelectedCell.Location.UserFriendlyLocationString}";
            }
        }

        /// <summary>
        /// Gets or sets the vertical alignment of all selected cells, recording changes to the undo stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the width of all selected cells, recording changes to the undo stack.
        /// </summary>
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
                    var columnCell = _cellTracker.GetCell(cell.Location.SheetName, 0, cell.Location.Column);
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

        /// <summary>
        /// Inserts a column to the left of the leftmost column of the selected cells.
        /// </summary>
        public void AddColumnToTheLeft()
        {
            // TODO: add ability to undo/redo column/row adds/removes.
            var leftmostColumnCell = _cellsToEdit
                .Select(x => _cellTracker.GetCell(x.Location.SheetName, 0, x.Location.Column))
                .Where(x => x is not null)
                .Distinct()
                .OrderBy(x => x?.Location.Column ?? 0)
                .FirstOrDefault();
            if (leftmostColumnCell == null) return;
            InsertColumnAtIndex(leftmostColumnCell.Location.SheetName, leftmostColumnCell.Location.Column);
        }

        /// <summary>
        /// Inserts a column to the right of the rightmost column of the selected cells.
        /// </summary>
        public void AddColumnToTheRight()
        {
            var rightmostColumnCell = _cellsToEdit
                .Select(x => _cellTracker.GetCell(x.Location.SheetName, 0, x.Location.Column))
                .Where(x => x is not null)
                .Distinct()
                .OrderByDescending(x => x?.Location.Column ?? 0)
                .FirstOrDefault();
            if (rightmostColumnCell == null) return;
            InsertColumnAtIndex(rightmostColumnCell.Location.SheetName, rightmostColumnCell.Location.Column + 1);
        }

        /// <summary>
        /// Insert a row above the topmost row of the selected cells.
        /// </summary>
        public void AddRowAbove()
        {
            var topmostRowCell = _cellsToEdit
                .Select(x => _cellTracker.GetCell(x.Location.SheetName, x.Location.Row, 0))
                .Where(x => x is not null)
                .Distinct()
                .OrderBy(x => x?.Location.Row ?? 0)
                .FirstOrDefault();
            if (topmostRowCell == null) return;
            InsertRowAtIndex(topmostRowCell.Location.SheetName, topmostRowCell.Location.Row);
        }

        /// <summary>
        /// Inserts a row below the bottommost row of the selected cells.
        /// </summary>
        public void AddRowBelow()
        {
            var bottomMostRowCell = _cellsToEdit
                .Select(x => _cellTracker.GetCell(x.Location.SheetName, x.Location.Row, 0))
                .Where(x => x is not null)
                .Distinct()
                .OrderByDescending(x => x?.Location.Row ?? 0)
                .FirstOrDefault();
            if (bottomMostRowCell == null) return;
            InsertRowAtIndex(bottomMostRowCell.Location.SheetName, bottomMostRowCell.Location.Row + 1);
        }

        /// <summary>
        /// Deletes the columns that the currently selected cells are in.
        /// </summary>
        public void DeleteColumns()
        {
            foreach (var cell in _cellsToEdit.ToList())
            {
                var columnCell = _cellTracker.GetCell(cell.Location.SheetName, 0, cell.Location.Column);
                if (columnCell is not null) DeleteColumn(columnCell);
            }
        }

        /// <summary>
        /// Deletes the rows that the currently selected cells are in.
        /// </summary>
        public void DeleteRows()
        {
            foreach (var cell in _cellsToEdit.ToList())
            {
                var rowCell = _cellTracker.GetCell(cell.Location.SheetName, cell.Location.Row, 0);
                if (rowCell is not null) DeleteRow(rowCell);
            }
        }

        /// <summary>
        /// Occurs when the tool window is really being closed.
        /// </summary>
        public override void HandleBeingClosed()
        {
            _cellsToEdit.CollectionChanged -= CellsToEditCollectionChanged;
            CellToDisplay = CellModel.Null;
        }

        /// <summary>
        /// Occurs when the tool window is being shown.
        /// </summary>
        public override void HandleBeingShown()
        {
            _cellsToEdit.CollectionChanged += CellsToEditCollectionChanged;
            PickDisplayedCell();
        }

        /// <summary>
        /// Merges the selected cells into one cell, recording changes to the undo stack.
        /// </summary>
        public void MergeCells()
        {
            ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
            MergeCells(_cellsToEdit);
            ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
        }

        /// <summary>
        /// Merges the selected cells across the same row into one cell per row, recording changes to the undo stack.
        /// </summary>
        public void MergeCellsAcross()
        {
            ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
            var selectedCells = _cellsToEdit.ToList();
            var rows = selectedCells?.Select(x => x.Location.Row).Distinct().ToList() ?? [];
            foreach (var row in rows)
            {
                var cellsToMerge = selectedCells?.Where(x => x.Location.Row == row).ToList() ?? [];
                MergeCells(cellsToMerge);
            }
            ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
        }

        /// <summary>
        /// Merges the selected cells down into one cell per column, recording changes to the undo stack.
        /// </summary>
        public void MergeCellsDown()
        {
            ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
            var selectedCells = _cellsToEdit.ToList();
            var columns = selectedCells?.Select(x => x.Location.Column).Distinct().ToList() ?? [];
            foreach (var column in columns)
            {
                var cellsToMerge = selectedCells?.Where(x => x.Location.Column == column).ToList();
                MergeCells(cellsToMerge ?? []);
            }
            ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
        }

        /// <summary>
        /// Unmerges the selected cells, recording changes to the undo stack.
        /// </summary>
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
            var location = columnCell.Location;
            var cellsToDelete = _cellTracker.GetCellModelsForSheet(columnCell.Location.SheetName).Where(x => x.Location.Column == location.Column).ToList();
            foreach (var cell in cellsToDelete)
            {
                var nextCell = _cellTracker.GetCell(columnCell.Location.SheetName, cell.Location.Row, cell.Location.Column + 1);
                EnsureIndexStaysCumulativeWhenRemoving(cell, nextCell, _cellTracker);
                _cellTracker.RemoveCell(cell);
            }
            IncrementColumnOfAllAtOrToTheRightOf(columnCell.Location.SheetName, location.Column, -1);

            foreach (var function in _pluginFunctionLoader.CellFunctions)
            {
                IncrementColumnReferenceOfAbsoluteReferencesForInsertedColumn(location.SheetName, location.Column, function, -1);
            }
        }

        private void DeleteRow(CellModel rowCell)
        {
            if (rowCell.CellType != CellType.Row) return;
            var location = rowCell.Location;
            var cellsToDelete = _cellTracker.GetCellModelsForSheet(rowCell.Location.SheetName).Where(x => x.Location.Row == location.Row).ToList();
            foreach (var cell in cellsToDelete)
            {
                var nextCell = _cellTracker.GetCell(rowCell.Location.SheetName, cell.Location.Row + 1, cell.Location.Column);
                EnsureIndexStaysCumulativeWhenRemoving(cell, nextCell, _cellTracker);
                _cellTracker.RemoveCell(cell);
            }
            IncrementRowOfAllAtOrBelow(rowCell.Location.SheetName, location.Row, -1);

            foreach (var function in _pluginFunctionLoader.CellFunctions)
            {
                IncrementRowReferenceOfAbsoluteReferencesForInsertedRow(location.SheetName, location.Row, function, -1);
            }
        }

        private List<CellModel> GetAllCellsAtOrBelow(string sheetName, int row) => _cellTracker.GetCellModelsForSheet(sheetName).Where(x => x.Location.Row >= row).ToList();

        private List<CellModel> GetAllCellsAtOrToTheRightOf(string sheetName, int column) => _cellTracker.GetCellModelsForSheet(sheetName).Where(x => x.Location.Column >= column).ToList();

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
            foreach (var cell in cells) cell.Location.Column += amount;
        }

        private void IncrementColumnReferenceOfAbsoluteReferencesForInsertedColumn(string sheetName, int columnIndex, CellFunction function, int incrementAmount)
        {
            var refactorer = new CellReferenceRefactorRewriter(x =>
            {
                if (x.SheetReference.Value != sheetName) return x;
                if (x.ColumnReference.IsRelative) return x;
                if (x.ColumnReference.Value >= columnIndex) x.ColumnReference.Value += 1;
                if (x.IsRange && x.ColumnRangeEndReference.Value + 1 >= columnIndex) x.ColumnRangeEndReference.Value += incrementAmount;
                return x;
            });
            function.Model.Code = refactorer.Visit(CSharpSyntaxTree.ParseText(function.Model.Code).GetRoot())?.ToFullString() ?? "";
        }

        private void IncrementRowOfAllAtOrBelow(string sheetName, int row, int amount = 1)
        {
            var cells = GetAllCellsAtOrBelow(sheetName, row);
            foreach (var cell in cells) cell.Location.Row += amount;
        }

        private void IncrementRowReferenceOfAbsoluteReferencesForInsertedRow(string sheetName, int rowIndex, CellFunction function, int incrementAmount)
        {
            // TODO: record this in undo stack.
            var refactorer = new CellReferenceRefactorRewriter(x =>
            {
                if (x.SheetReference.Value != sheetName) return x;
                if (x.RowReference.IsRelative) return x;
                if (x.RowReference.Value >= rowIndex) x.RowReference.Value += 1;
                if (x.IsRange && x.RowRangeEndReference.Value + 1 >= rowIndex) x.RowRangeEndReference.Value+= incrementAmount;
                return x;
            });
            function.Model.Code = refactorer.Visit(CSharpSyntaxTree.ParseText(function.Model.Code).GetRoot())?.ToFullString() ?? "";
        }

        private void InsertColumnAtIndex(string sheetName, int index)
        {
            IncrementColumnOfAllAtOrToTheRightOf(sheetName, index);

            CellModelFactory.Create(0, index, CellType.Column, sheetName, _cellTracker);

            var rowIndexs = _cellTracker.GetCellModelsForSheet(sheetName).Where(x => x.CellType == CellType.Row).Select(x => x.Location.Row).ToList();
            foreach (var rowIndex in rowIndexs)
            {
                var cell = CellModelFactory.Create(rowIndex, index, CellType.Label, sheetName, _cellTracker);
                var firstNeighbor = _cellTracker.GetCell(sheetName, rowIndex, index - 1);
                var secondNeighbor = _cellTracker.GetCell(sheetName, rowIndex, index + 1);
                MergeCellIntoCellsIfTheyWereMerged(cell, firstNeighbor, secondNeighbor);
                EnsureIndexStaysCumulativeBetweenNeighborsWhenAdding(cell, firstNeighbor, secondNeighbor, _cellTracker);
            }

            foreach (var function in _pluginFunctionLoader.CellFunctions)
            {
                IncrementColumnReferenceOfAbsoluteReferencesForInsertedColumn(sheetName, index, function, 1);
            }
        }

        private void InsertRowAtIndex(string sheetName, int newRowIndex)
        {
            IncrementRowOfAllAtOrBelow(sheetName, newRowIndex);

            var rowModel = CellModelFactory.Create(newRowIndex, 0, CellType.Row, sheetName, _cellTracker);

            var columnIndexs = _cellTracker.GetCellModelsForSheet(sheetName).Where(x => x.CellType == CellType.Column).Select(x => x.Location.Column).ToList();
            foreach (var columnIndex in columnIndexs)
            {
                var cell = CellModelFactory.Create(newRowIndex, columnIndex, CellType.Label, sheetName, _cellTracker);
                var firstNeighbor = _cellTracker.GetCell(sheetName, newRowIndex - 1, columnIndex);
                var secondNeighbor = _cellTracker.GetCell(sheetName, newRowIndex + 1, columnIndex);
                MergeCellIntoCellsIfTheyWereMerged(cell, firstNeighbor, secondNeighbor);
                EnsureIndexStaysCumulativeBetweenNeighborsWhenAdding(cell, firstNeighbor, secondNeighbor, _cellTracker);
            }

            foreach (var function in _pluginFunctionLoader.CellFunctions)
            {
                IncrementRowReferenceOfAbsoluteReferencesForInsertedRow(rowModel.Location.SheetName, rowModel.Location.Row, function, 1);
            }
        }

        private static void MergeCellIntoCellsIfTheyWereMerged(CellModel cellToMerge, CellModel? firstCell, CellModel? secondCell)
        {
            if (firstCell == null || secondCell == null) return;
            if (firstCell.IsMergedWith(secondCell)) cellToMerge.MergedWith = firstCell.MergedWith;
        }

        private void MergeCells(IEnumerable<CellModel> cells)
        {
            if (cells.Count() < 2) return;
            var leftmost = cells.Select(x => x.Location.Column).Min();
            var topmost = cells.Select(x => x.Location.Row).Min();
            var rightmost = cells.Select(x => x.Location.Column).Max();
            var bottommost = cells.Select(x => x.Location.Row).Max();

            var topLeftCell = cells.FirstOrDefault(x => x.Location.Row == topmost && x.Location.Column == leftmost);
            if (topLeftCell is null) return;
            var bottomRightCell = cells.FirstOrDefault(x => x.Location.Row == bottommost && x.Location.Column == rightmost);
            if (bottomRightCell is null) return;

            var sheetName = topLeftCell.Location.SheetName;
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
            var mergedCells = _cellTracker.GetCellModelsForSheet(mergeParent.Location.SheetName).Where(x => x.IsMergedWith(mergeParent));
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
