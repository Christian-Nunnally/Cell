using Cell.Model;
using Cell.Persistence;
using Cell.Plugin;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace Cell.ViewModel
{
    public abstract partial class CellViewModel : PropertyChangedBase
    {
        protected SheetViewModel _sheetViewModel;
        private readonly CellModel _model;
        private bool isSelected;

        public CellViewModel(CellModel model, SheetViewModel sheet)
        {
            _sheetViewModel = sheet;
            _model = model;
            BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
            ForegroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ForegroundColorHex));
            BorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BorderColorHex));
            UpdateBorderThickness(BorderThicknessString);
            _model.PropertyChanged += ModelPropertyChanged;
        }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.X) ||
                e.PropertyName == nameof(CellModel.Y) ||
                e.PropertyName == nameof(CellModel.Width) ||
                e.PropertyName == nameof(CellModel.Height) ||
                e.PropertyName == nameof(CellModel.Text) ||
                e.PropertyName == nameof(CellModel.Value) ||
                e.PropertyName == nameof(CellModel.Row) ||
                e.PropertyName == nameof(CellModel.Column))
            {
                OnPropertyChanged(e.PropertyName);
                return;
            }
        }

        public CellModel Model => _model;

        public double CanvasLeft => _model.X;

        public double CanvasTop => _model.Y;

        public virtual double X
        {
            get => _model.X;
            set { _model.X = value; OnPropertyChanged(nameof(X)); }
        }

        public virtual double Y
        {
            get => _model.Y;
            set { _model.Y = value; OnPropertyChanged(nameof(Y)); }
        }

        public virtual double Width
        {
            get => _model.Width;
            set { _model.Width = value; OnPropertyChanged(nameof(Width)); }
        }

        public virtual double Height
        {
            get => _model.Height;
            set { _model.Height = value; OnPropertyChanged(nameof(Height)); }
        }

        public virtual int Row
        {
            get => _model.Row;
            set { _model.Row = value; OnPropertyChanged(nameof(Row)); }
        }

        public virtual int Column
        {
            get => _model.Column;
            set { _model.Column = value; OnPropertyChanged(nameof(Column)); }
        }

        public virtual int Index
        {
            get => _model.Index;
            set { _model.Index = value; OnPropertyChanged(nameof(Index)); }
        }

        public virtual string Text
        {
            get => _model.Text;
            set
            {
                _model.Text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        public virtual string Value
        {
            get => _model.Value;
            set
            {
                _model.Value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public virtual string ID
        {
            get => _model.ID;
        }

        public virtual bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public virtual string GetTextFunctionName
        {
            get => _model.PopulateFunctionName;
            set
            {
                _model.PopulateFunctionName = value;
                OnPropertyChanged(nameof(GetTextFunctionName));
                OnPropertyChanged(nameof(GetTextFunctionCode));
            }
        }

        public virtual string GetTextFunctionCode
        {
            get => PluginFunctionLoader.TryGetPopulateFunction(GetTextFunctionName, out var code) ? code.Code : string.Empty;
            set
            {
                PluginFunctionLoader.SetPopulateFunction(GetTextFunctionName, value, !string.IsNullOrWhiteSpace(value));
                UpdateTextFromGetTextFunction();
                OnPropertyChanged(nameof(GetTextFunctionCode));
            }
        }

        public virtual string OnEditFunctionName
        {
            get => _model.TriggerFunctionName;
            set
            {
                _model.TriggerFunctionName = value;
                OnPropertyChanged(nameof(OnEditFunctionName));
                OnPropertyChanged(nameof(OnEditFunctionCode));
            }
        }

        public virtual string OnEditFunctionCode
        {
            get => PluginFunctionLoader.TryGetTriggerFunction(GetTextFunctionName, out var code) ? code.Code : string.Empty;
            set
            {
                PluginFunctionLoader.TrySetTriggerFunction(OnEditFunctionName, value, !string.IsNullOrWhiteSpace(value));
                OnPropertyChanged(nameof(OnEditFunctionCode));
            }
        }

        public void UpdateTextFromGetTextFunction()
        {
            if (GetTextFunctionName != string.Empty)
            {
                var result = DynamicCellPluginExecutor.CompileAndRunPopulate(new PluginContext(ApplicationViewModel.Instance), _model);
                if (result.Success)
                {
                    Text = result.Result;
                }
                else
                {
                    Text = result.Result;
                }
            }
        }

        #region Style Properties

        public virtual double FontSize
        {
            get => _model.FontSize;
            set { _model.FontSize = value; OnPropertyChanged(nameof(FontSize)); }
        }

        public virtual string FontFamily
        {
            get => _model.FontFamily;
            set { _model.FontFamily = value; OnPropertyChanged(nameof(FontFamily)); }
        }

        public virtual string BorderThicknessString
        { 
            get => Model.BorderThicknessString;
            set
            {
                if (UpdateBorderThickness(value))
                {
                    Model.BorderThicknessString = value;
                    OnPropertyChanged(nameof(BorderThicknessString));
                }
            }
        }

        public bool UpdateBorderThickness(string stringBorderThickness)
        {
            var split = stringBorderThickness.Split(',');
            if (split.Length != 4) return false;
            if (!double.TryParse(split[0], out var left)) return false;
            if (!double.TryParse(split[1], out var top)) return false;
            if (!double.TryParse(split[2], out var right)) return false;
            if (!double.TryParse(split[3], out var bottom)) return false;
            BorderThickness = new Thickness(left, top, right, bottom);
            OnPropertyChanged(nameof(BorderThickness));
            return true;
        }

        public virtual Thickness BorderThickness { get; private set; }

        public virtual SolidColorBrush BackgroundColor { get; private set; }

        public void HighlightCell(string color)
        {
            BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            OnPropertyChanged(nameof(BackgroundColor));
        }

        public void UnhighlightCell()
        {
            BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
            OnPropertyChanged(nameof(BackgroundColor));
        }

        public virtual string BackgroundColorHex
        {
            get => _model.BackgroundColorHex;
            set
            {
                if (!IsHexidecimalColorCode().IsMatch(value)) return;
                _model.BackgroundColorHex = value;
                BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
                OnPropertyChanged(nameof(BackgroundColorHex));
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        public virtual SolidColorBrush ForegroundColor { get; private set; }

        public virtual string ForegroundColorHex
        {
            get => _model.ForegroundColorHex;
            set
            {
                if (!IsHexidecimalColorCode().IsMatch(value)) return;
                _model.ForegroundColorHex = value;
                ForegroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ForegroundColorHex));
                OnPropertyChanged(nameof(ForegroundColorHex));
                OnPropertyChanged(nameof(ForegroundColor));
            }
        }

        public virtual SolidColorBrush BorderColor { get; private set; }

        public virtual string BorderColorHex
        {
            get => _model.BorderColorHex;
            set
            {
                if (!IsHexidecimalColorCode().IsMatch(value)) return;
                _model.BorderColorHex = value;
                BorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BorderColorHex));
                OnPropertyChanged(nameof(BorderColorHex));
                OnPropertyChanged(nameof(BorderColor));
            }
        }

        [GeneratedRegex(@"[#][0-9A-Fa-f]{6}\b")]
        private static partial Regex IsHexidecimalColorCode();

        #endregion
    }
}