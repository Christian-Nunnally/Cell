using Cell.Common;
using Cell.Model;
using Cell.ViewModel.Application;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Cell.ViewModel.Cells
{
    public abstract class CellViewModel : PropertyChangedBase
    {
        private readonly CellModel _model;
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

        public virtual SolidColorBrush BackgroundColor
        {
            get => _backgroundColor;
            private set
            {
                _backgroundColor = value;
                NotifyPropertyChanged(nameof(BackgroundColor));
            }
        }

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

        public virtual SolidColorBrush BorderColor
        {
            get => _borderColor;
            private set
            {
                _borderColor = value;
                NotifyPropertyChanged(nameof(BorderColor));
            }
        }

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

        public virtual Thickness BorderThickness
        {
            get => _borderThickness;
            private set
            {
                _borderThickness = value;
                NotifyPropertyChanged(nameof(BorderThickness));
            }
        }

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

        public virtual int Column
        {
            get => _model.Location.Column;
            set { _model.Location.Column = value; }
        }

        public virtual SolidColorBrush ContentBackgroundColor
        {
            get => _contentBackgroundColor;
            private set
            {
                _contentBackgroundColor = value;
                NotifyPropertyChanged(nameof(ContentBackgroundColor));
            }
        }

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

        public virtual Thickness ContentBorderThickness
        {
            get => _contentBorderThickness; private set
            {
                _contentBorderThickness = value;
                NotifyPropertyChanged(nameof(ContentBorderThickness));
            }
        }

        public virtual SolidColorBrush ContentHighlightColor
        {
            get => _contentHighlightColor;
            private set
            {
                _contentHighlightColor = value;
                NotifyPropertyChanged(nameof(ContentHighlightColor));
            }
        }

        public virtual string ContentHighlightColorHex
        {
            get => _model.Style.HighlightColor;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _model.Style.HighlightColor = value;
                ContentHighlightColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentHighlightColorHex));
            }
        }

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

        public FontStyle FontStyleForView => _model.Style.Italic ? FontStyles.Italic : FontStyles.Normal;

        public FontWeight FontWeightForView => _model.Style.Bold ? FontWeights.Bold : FontWeights.Normal;

        public virtual SolidColorBrush ForegroundColor
        {
            get => _foregroundColor;
            private set
            {
                _foregroundColor = value;
                NotifyPropertyChanged(nameof(ForegroundColor));
            }
        }

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

        public virtual double Height
        {
            get => _model.Height;
            set
            {
                _model.Height = value;
            }
        }

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

        public virtual HorizontalAlignment HorizontalAlignmentForViewCenter => HorizontalAlignmentForView == HorizontalAlignment.Stretch ? HorizontalAlignment.Center : HorizontalAlignmentForView;

        public virtual string ID { get => _model.ID; }

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

        public virtual Thickness Margin
        {
            get => _margin; private set
            {
                _margin = value;
                NotifyPropertyChanged(nameof(Margin));
            }
        }

        public CellModel Model => _model;

        public virtual int Row
        {
            get => _model.Location.Row;
            set { _model.Location.Row = value; }
        }

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

        public virtual bool ShouldShowSelectionBorder => IsSelected || IsHighlighted;

        public virtual bool ShouldShowSelectionFill => IsSelected || IsHighlighted;

        public virtual string Text
        {
            get => _model.Text;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Text = value?.Replace("\\n", "\n") ?? string.Empty;
            }
        }

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

        public TextDecorationCollection? TextDecorationsForView => _model.Style.Strikethrough ? TextDecorations.Strikethrough : null;

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

        public virtual VerticalAlignment VerticalAlignmentForViewCenter => VerticalAlignmentForView == VerticalAlignment.Stretch ? VerticalAlignment.Center : VerticalAlignmentForView;

        public virtual double Width
        {
            get => _model.Width;
            set { _model.Width = value; }
        }

        public virtual double X
        {
            get => _x;
            set { _x = value; NotifyPropertyChanged(nameof(X)); }
        }

        public virtual double Y
        {
            get => _y;
            set { _y = value; NotifyPropertyChanged(nameof(Y)); }
        }

        public void HighlightCell(string color)
        {
            SelectionColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            SelectionBorderColor = new(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(BackgroundColorHex), 150));
            IsHighlighted = true;
            NotifyPropertyChanged(nameof(BackgroundColor));
        }

        public void UnhighlightCell()
        {
            SelectionColor = new(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(BackgroundColorHex), 50));
            SelectionBorderColor = new(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(BackgroundColorHex), 150));
            IsHighlighted = false;
        }

        public bool UpdateBorderThickness(string stringBorderThickness)
        {
            if (Utilities.TryParseStringIntoThickness(stringBorderThickness, out var thickness))
            {
                BorderThickness = thickness;
                return true;
            }
            return false;
        }

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
