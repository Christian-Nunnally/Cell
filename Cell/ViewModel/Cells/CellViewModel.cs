using Cell.Common;
using Cell.Model;
using Cell.ViewModel.Application;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Cell.ViewModel.Cells
{
    /// <summary>
    /// The base class for all cell view models.
    /// </summary>
    public abstract class CellViewModel : PropertyChangedBase
    {
        private readonly CellModel _model;
        /// <summary>
        /// The sheet view model that this cell view model belongs to.
        /// </summary>
        protected SheetViewModel _sheetViewModel;
        private SolidColorBrush _backgroundColor = new();
        private SolidColorBrush _borderColor = new();
        private Thickness _borderThickness;
        private SolidColorBrush _contentBackgroundColor = new();
        private SolidColorBrush _contentBorderColor = new();
        private Thickness _contentBorderThickness;
        private SolidColorBrush _contentHighlightColor = new();
        private SolidColorBrush _foregroundColor = new();
        private bool _isHighlighted;
        private bool _isSelected;
        private Thickness _margin;
        private SolidColorBrush _selectionBorderColor = ColorAdjuster.ConvertHexStringToBrush("#ffff0000");
        private SolidColorBrush _selectionColor = ColorAdjuster.ConvertHexStringToBrush("#ffff0000");
        private double _x;
        private double _y;
        /// <summary>
        /// Creates a new instance of <see cref="CellViewModel"/>.
        /// </summary>
        /// <param name="model">The cells model.</param>
        /// <param name="sheet">The sheet this cell is owned by.</param>
        public CellViewModel(CellModel model, SheetViewModel sheet)
        {
            _sheetViewModel = sheet;
            _model = model;

            BackgroundColor = ColorAdjuster.ConvertHexStringToBrush(BackgroundColorHex);
            ContentBackgroundColor = ColorAdjuster.ConvertHexStringToBrush(ContentBackgroundColorHex);
            ForegroundColor = ColorAdjuster.ConvertHexStringToBrush(ForegroundColorHex);
            BorderColor = ColorAdjuster.ConvertHexStringToBrush(BorderColorHex);
            ContentBorderColor = ColorAdjuster.ConvertHexStringToBrush(ContentBorderColorHex);
            ContentHighlightColor = ColorAdjuster.ConvertHexStringToBrush(ContentHighlightColorHex);
            SelectionColor = new(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(BackgroundColorHex), 50));
            SelectionBorderColor = new(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(BackgroundColorHex), 150));
            UpdateBorderThickness(model.Style.Border);
            UpdateContentBorderThickness(model.Style.ContentBorder);
            UpdateMargin(model.Style.ContentMargin);
            model.PropertyChanged += ModelPropertyChanged;
            model.Style.PropertyChanged += ModelStylePropertyChanged;
        }

        /// <summary>
        /// Gets or sets the background color of the cell.
        /// </summary>
        public virtual SolidColorBrush BackgroundColor
        {
            get => _backgroundColor;
            private set
            {
                _backgroundColor = value;
                NotifyPropertyChanged(nameof(BackgroundColor));
            }
        }

        /// <summary>
        /// Gets or sets the background color of the cell as a hexidecimal string and records the state if undo redo is recording.
        /// </summary>
        public virtual string BackgroundColorHex
        {
            get => _model.Style.BackgroundColor;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Style.BackgroundColor = value;
                var color = (Color)ColorConverter.ConvertFromString(value);
                BackgroundColor = new SolidColorBrush(color);
                SelectionColor = new(ColorAdjuster.GetHighlightColor(color, 100));
                SelectionBorderColor = new(ColorAdjuster.GetHighlightColor(color, 175));
            }
        }

        /// <summary>
        /// Gets or sets the border color of the cell.
        /// </summary>
        public virtual SolidColorBrush BorderColor
        {
            get => _borderColor;
            private set
            {
                _borderColor = value;
                NotifyPropertyChanged(nameof(BorderColor));
            }
        }

        /// <summary>
        /// Gets or sets the border color of the cell as a hexidecimal string and records the state if undo redo is recording.
        /// </summary>
        public virtual string BorderColorHex
        {
            get => _model.Style.BorderColor;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Style.BorderColor = value;
                var color = (Color)ColorConverter.ConvertFromString(BorderColorHex);
                BorderColor = new SolidColorBrush(color);
            }
        }

        /// <summary>
        /// Gets or sets the border thickness of the cell.
        /// </summary>
        public virtual Thickness BorderThickness
        {
            get => _borderThickness;
            private set
            {
                _borderThickness = value;
                NotifyPropertyChanged(nameof(BorderThickness));
            }
        }

        /// <summary>
        /// Gets or sets the cell type of the cell and records the state if undo redo is recording.
        /// </summary>
        public virtual CellType CellType
        {
            get => _model.CellType;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.CellType = value;
                NotifyPropertyChanged(nameof(CellType));
            }
        }

        /// <summary>
        /// Gets or sets the column of the cell.
        /// </summary>
        public virtual int Column
        {
            get => _model.Location.Column;
            set { _model.Location.Column = value; }
        }

        /// <summary>
        /// Gets or sets the content background color of the cell.
        /// </summary>
        public virtual SolidColorBrush ContentBackgroundColor
        {
            get => _contentBackgroundColor;
            private set
            {
                _contentBackgroundColor = value;
                NotifyPropertyChanged(nameof(ContentBackgroundColor));
            }
        }

        /// <summary>
        /// Gets or sets the content background color of the cell as a hexidecimal string and records the state if undo redo is recording.
        /// </summary>
        public virtual string ContentBackgroundColorHex
        {
            get => _model.Style.ContentBackgroundColor;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Style.ContentBackgroundColor = value;
                ContentBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentBackgroundColorHex));
                NotifyPropertyChanged(nameof(ContentBackgroundColor));
            }
        }

        /// <summary>
        /// Gets or sets the content border color of the cell.
        /// </summary>
        public virtual SolidColorBrush ContentBorderColor
        {
            get => _contentBorderColor;
            private set
            {
                if (_contentBorderColor == value) return;
                _contentBorderColor = value;
                NotifyPropertyChanged(nameof(ContentBorderColor));
            }
        }

        /// <summary>
        /// Gets or sets the content border color of the cell as a hexidecimal string and records the state if undo redo is recording.
        /// </summary>
        public virtual string ContentBorderColorHex
        {
            get => _model.Style.ContentBorderColor;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Style.ContentBorderColor = value;
                ContentBorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentBorderColorHex));
                NotifyPropertyChanged(nameof(ContentBorderColor));
            }
        }

        /// <summary>
        /// Gets or sets the content border thickness of the cell used by the view binding.
        /// </summary>
        public virtual Thickness ContentBorderThickness
        {
            get => _contentBorderThickness; private set
            {
                _contentBorderThickness = value;
                NotifyPropertyChanged(nameof(ContentBorderThickness));
            }
        }

        /// <summary>
        /// Gets or sets the content highlight color of the cell used by the view binding.
        /// </summary>
        public virtual SolidColorBrush ContentHighlightColor
        {
            get => _contentHighlightColor;
            private set
            {
                _contentHighlightColor = value;
                NotifyPropertyChanged(nameof(ContentHighlightColor));
            }
        }

        /// <summary>
        /// Gets or sets the content highlight color of the cell as a hexidecimal string and records the state if undo redo is recording.
        /// </summary>
        public virtual string ContentHighlightColorHex
        {
            get => _model.Style.HighlightColor;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Style.HighlightColor = value;
                ContentHighlightColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentHighlightColorHex));
            }
        }

        /// <summary>
        /// Gets or sets the font of the cell and records the state if undo redo is recording.
        /// </summary>
        public virtual string Font
        {
            get => _model.Style.Font;
            set
            {
                ApplicationViewModel.Instance.UndoRedoManager.RecordStateIfRecording(_model);
                _model.Style.Font = value;
                NotifyPropertyChanged(nameof(Font));
            }
        }

        /// <summary>
        /// Gets or sets the font size of the cell and records the state if undo redo is recording.
        /// </summary>
        public virtual double FontSize
        {
            get => _model.Style.FontSize;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Style.FontSize = value;
                NotifyPropertyChanged(nameof(FontSize));
            }
        }

        /// <summary>
        /// Gets the font style for the view used by the view binding.
        /// </summary>
        public FontStyle FontStyleForView => _model.Style.Italic ? FontStyles.Italic : FontStyles.Normal;

        /// <summary>
        /// Gets the font weight for the view used by the view binding.
        /// </summary>
        public FontWeight FontWeightForView => _model.Style.Bold ? FontWeights.Bold : FontWeights.Normal;

        /// <summary>
        /// Gets or sets the foreground color brush of the cell used by the view binding.
        /// </summary>
        public virtual SolidColorBrush ForegroundColor
        {
            get => _foregroundColor;
            private set
            {
                _foregroundColor = value;
                NotifyPropertyChanged(nameof(ForegroundColor));
            }
        }

        /// <summary>
        /// Gets or sets the foreground color of the cell as a hexidecimal string and records the state if undo redo is recording.
        /// </summary>
        public virtual string ForegroundColorHex
        {
            get => _model.Style.ForegroundColor;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Style.ForegroundColor = value;
                ForegroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ForegroundColorHex));
            }
        }

        /// <summary>
        /// Gets or sets the height of the cell.
        /// </summary>
        public virtual double Height
        {
            get => _model.Height;
            set => _model.Height = value;
        }

        /// <summary>
        /// Gets or sets the horizontal alignment of the cell and records the state if undo redo is recording.
        /// </summary>
        public virtual HorizontalAlignment HorizontalAlignmentForView
        {
            get => _model.Style.HorizontalAlignment;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Style.HorizontalAlignment = value;
                NotifyPropertyChanged(nameof(HorizontalAlignmentForView), nameof(HorizontalAlignmentForViewCenter));
            }
        }

        /// <summary>
        /// Gets the horizontal alignment for the view center used by the view binding.
        /// </summary>
        public virtual HorizontalAlignment HorizontalAlignmentForViewCenter => HorizontalAlignmentForView == HorizontalAlignment.Stretch ? HorizontalAlignment.Center : HorizontalAlignmentForView;

        /// <summary>
        /// Gets the ID of the cell.
        /// </summary>
        public virtual string ID { get => _model.ID; }

        /// <summary>
        /// Gets or sets the index of the cell and records the state if undo redo is recording.
        /// </summary>
        public virtual int Index
        {
            get => _model.Index;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Index = value;
                NotifyPropertyChanged(nameof(Index));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cell is highlighted and records the state if undo redo is recording.
        /// </summary>
        public virtual bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (_isHighlighted == value) return;
                _isHighlighted = value;
                NotifyPropertyChanged(nameof(IsHighlighted), nameof(ShouldShowSelectionBorder), nameof(ShouldShowSelectionFill));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cell is selected.
        /// </summary>
        public virtual bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                NotifyPropertyChanged(nameof(IsSelected), nameof(ShouldShowSelectionBorder), nameof(ShouldShowSelectionFill));
            }
        }

        /// <summary>
        /// Gets the thickness margin of the cell for the view.
        /// </summary>
        public virtual Thickness Margin
        {
            get => _margin; private set
            {
                _margin = value;
                NotifyPropertyChanged(nameof(Margin));
            }
        }

        /// <summary>
        /// Gets the model for the cell.
        /// </summary>
        public CellModel Model => _model;

        /// <summary>
        /// Gets or sets the row of the cell.
        /// </summary>
        public virtual int Row
        {
            get => _model.Location.Row;
            set { _model.Location.Row = value; }
        }

        /// <summary>
        /// Gets or sets the color brush of the cell selection border used by the view binding.
        /// </summary>
        public virtual SolidColorBrush SelectionBorderColor
        {
            get => _selectionBorderColor;
            set
            {
                if (_selectionBorderColor == value) return;
                _selectionBorderColor = value;
                NotifyPropertyChanged(nameof(SelectionBorderColor));
            }
        }

        /// <summary>
        /// Gets or sets the color brush of the cell selection fill used by the view binding.
        /// </summary>
        public virtual SolidColorBrush SelectionColor
        {
            get => _selectionColor;
            set
            {
                if (_selectionColor == value) return;
                _selectionColor = value;
                NotifyPropertyChanged(nameof(SelectionColor));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the selection border should be shown.
        /// </summary>
        public virtual bool ShouldShowSelectionBorder => IsSelected || IsHighlighted;

        /// <summary>
        /// Gets a value indicating whether the selection fill should be shown.
        /// </summary>
        public virtual bool ShouldShowSelectionFill => IsSelected || IsHighlighted;

        /// <summary>
        /// Gets or sets the text of the cell and records the state if undo redo is recording.
        /// </summary>
        public virtual string Text
        {
            get => _model.Text;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Text = value?.Replace("\\n", "\n") ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the text alignment of the cell and records the state if undo redo is recording.
        /// </summary>
        public virtual TextAlignment TextAlignmentForView
        {
            get => _model.Style.TextAlignment;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Style.TextAlignment = value;
                NotifyPropertyChanged(nameof(TextAlignmentForView));
            }
        }

        /// <summary>
        /// Gets the text decorations for the view used by the view binding.
        /// </summary>
        public TextDecorationCollection? TextDecorationsForView => _model.Style.Strikethrough ? TextDecorations.Strikethrough : null;

        /// <summary>
        /// Gets or sets the vertical alignment of the cell and records the state if undo redo is recording.
        /// </summary>
        public virtual VerticalAlignment VerticalAlignmentForView
        {
            get => _model.Style.VerticalAlignment;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Style.VerticalAlignment = value;
                NotifyPropertyChanged(nameof(VerticalAlignmentForView));
                NotifyPropertyChanged(nameof(VerticalAlignmentForViewCenter));
            }
        }

        /// <summary>
        /// Gets the vertical alignment for the view center used by the view binding.
        /// </summary>
        public virtual VerticalAlignment VerticalAlignmentForViewCenter => VerticalAlignmentForView == VerticalAlignment.Stretch ? VerticalAlignment.Center : VerticalAlignmentForView;

        /// <summary>
        /// Gets or sets the width of the cell.
        /// </summary>
        public virtual double Width
        {
            get => _model.Width;
            set => _model.Width = value;
        }

        /// <summary>
        /// Gets or sets the x position of the cell.
        /// </summary>
        public virtual double X
        {
            get => _x;
            set { _x = value; NotifyPropertyChanged(nameof(X)); }
        }

        /// <summary>
        /// Gets or sets the y position of the cell.
        /// </summary>
        public virtual double Y
        {
            get => _y;
            set { _y = value; NotifyPropertyChanged(nameof(Y)); }
        }

        /// <summary>
        /// Sets the background color of the cell to the given color and the is highlighted property to true.
        /// </summary>
        /// <param name="color">The color to highlight with.</param>
        public void HighlightCell(string color)
        {
            SelectionColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            SelectionBorderColor = new(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(BackgroundColorHex), 150));
            IsHighlighted = true;
            NotifyPropertyChanged(nameof(BackgroundColor));
        }

        /// <summary>
        /// Removes the highlight from the cell.
        /// </summary>
        public void UnhighlightCell()
        {
            SelectionColor = new(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(BackgroundColorHex), 50));
            SelectionBorderColor = new(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(BackgroundColorHex), 150));
            IsHighlighted = false;
        }

        /// <summary>
        /// Updates the border thickness of the cell with the given string border thickness.
        /// </summary>
        /// <param name="stringBorderThickness">The string thickness, like '1,2,3,4' or '2,3' or '1'</param>
        /// <returns>True if the given string was a valid thickness.</returns>
        public bool UpdateBorderThickness(string stringBorderThickness)
        {
            if (Utilities.TryParseStringIntoThickness(stringBorderThickness, out var thickness))
            {
                BorderThickness = thickness;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the content border thickness of the cell with the given string content border thickness.
        /// </summary>
        /// <param name="stringContentBorderThickness">The string thickness, like '1,2,3,4' or '2,3' or '1'</param>
        /// <returns>True if the given string was a valid thickness.</returns>
        public bool UpdateContentBorderThickness(string stringContentBorderThickness)
        {
            if (Utilities.TryParseStringIntoThickness(stringContentBorderThickness, out var thickness))
            {
                ContentBorderThickness = thickness;
                NotifyPropertyChanged(nameof(ContentBorderThickness));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to update the margin of the cell with the given string margin.
        /// </summary>
        /// <param name="stringMargin">The string margin, like '1,2,3,4' or '2,3' or '1'</param>
        /// <returns>True if the given string was a valid margin.</returns>
        public bool UpdateMargin(string stringMargin)
        {
            if (Utilities.TryParseStringIntoThickness(stringMargin, out var thickness))
            {
                Margin = thickness;
                NotifyPropertyChanged(nameof(Margin));
                return true;
            }
            return false;
        }

        private void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(e.PropertyName!);
        }

        private void ModelStylePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellStyleModel.BackgroundColor))
            {
                BackgroundColor = ColorAdjuster.ConvertHexStringToBrush(BackgroundColorHex);
                NotifyPropertyChanged(nameof(BackgroundColorHex));
            }
            else if (e.PropertyName == nameof(CellStyleModel.ContentBackgroundColor))
            {
                ContentBackgroundColor = ColorAdjuster.ConvertHexStringToBrush(ContentBackgroundColorHex);
                NotifyPropertyChanged(nameof(ContentBackgroundColorHex));
            }
            else if (e.PropertyName == nameof(CellStyleModel.ForegroundColor))
            {
                ForegroundColor = ColorAdjuster.ConvertHexStringToBrush(ForegroundColorHex);
                NotifyPropertyChanged(nameof(ForegroundColorHex));
            }
            else if (e.PropertyName == nameof(CellStyleModel.BorderColor))
            {
                BorderColor = ColorAdjuster.ConvertHexStringToBrush(BorderColorHex);
                NotifyPropertyChanged(nameof(BorderColorHex));
            }
            else if (e.PropertyName == nameof(CellStyleModel.ContentBorderColor))
            {
                ContentBorderColor = ColorAdjuster.ConvertHexStringToBrush(ContentBorderColorHex);
                NotifyPropertyChanged(nameof(ContentBorderColorHex));
            }
            else if (e.PropertyName == nameof(CellStyleModel.HighlightColor))
            {
                ContentHighlightColor = ColorAdjuster.ConvertHexStringToBrush(ContentHighlightColorHex);
                NotifyPropertyChanged(nameof(ContentHighlightColorHex));
            }
            else if (e.PropertyName == nameof(CellStyleModel.ContentMargin))
            {
                UpdateMargin(Model.Style.ContentMargin);
            }
            else if (e.PropertyName == nameof(CellStyleModel.Border))
            {
                UpdateBorderThickness(Model.Style.Border);
            }
            else if (e.PropertyName == nameof(CellStyleModel.ContentBorder))
            {
                UpdateContentBorderThickness(Model.Style.ContentBorder);
            }
            else if (e.PropertyName == nameof(CellStyleModel.Bold))
            {
                NotifyPropertyChanged(nameof(FontWeightForView));
            }
            else if (e.PropertyName == nameof(CellStyleModel.Italic))
            {
                NotifyPropertyChanged(nameof(FontStyleForView));
            }
            else if (e.PropertyName == nameof(CellStyleModel.Strikethrough))
            {
                NotifyPropertyChanged(nameof(TextDecorationsForView));
            }
            else if (e.PropertyName != null)
            {
                NotifyPropertyChanged(e.PropertyName);
            }
        }
    }
}
