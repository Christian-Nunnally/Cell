using Cell.Model;
using System.ComponentModel;
using System.Windows.Media;

namespace Cell.ViewModel
{
    public class CellViewModel : INotifyPropertyChanged
    {

        public CellViewModel(CellModel model, SheetViewModel sheet)
        {
            cellModel = model;
            sheetViewModel = sheet;
        }

        protected SheetViewModel sheetViewModel;
        protected CellModel cellModel;
        private bool isSelected;

        public virtual double XTransformed
        {
            get => cellModel.X;
        }

        public virtual double YTransformed
        {
            get => cellModel.Y;
        }

        public virtual double X
        {
            get => cellModel.X;
            set
            {
                cellModel.X = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(X)));
            }
        }

        public virtual double Y
        {
            get => cellModel.Y;
            set
            {
                cellModel.Y = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y)));
            }
        }

        public virtual double Width
        {
            get => cellModel.Width;
            set
            {
                cellModel.Width = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Width)));
            }
        }

        public virtual double Height
        {
            get => cellModel.Height;
            set
            {
                cellModel.Height = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Height)));
            }
        }

        public virtual string Text
        {
            get => cellModel.Text;
            set
            {
                cellModel.Text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }

        public virtual bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public virtual string GetTextFunctionName
        {
            get => cellModel.GetTextFunctionName;
            set
            {
                cellModel.GetTextFunctionName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GetTextFunctionName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GetTextFunctionCode)));
            }
        }

        public virtual string GetTextFunctionCode
        {
            get => PluginFunctionLoader.GetTextPluginFunctionCodeIfAvailable(GetTextFunctionName);
            set
            {
                PluginFunctionLoader.SetTextPluginFunctionCodeIfAvailable(GetTextFunctionName, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GetTextFunctionCode)));
            }
        }

        public virtual string OnEditFunctionName
        {
            get => cellModel.OnEditFunctionName;
            set
            {
                cellModel.OnEditFunctionName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OnEditFunctionName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OnEditFunctionCode)));
            }
        }

        public virtual string OnEditFunctionCode
        {
            get => PluginFunctionLoader.GetOnEditPluginFunctionCodeIfAvailable(OnEditFunctionName);
            set
            {
                PluginFunctionLoader.SetOnEditPluginFunctionCodeIfAvailable(OnEditFunctionName, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OnEditFunctionCode)));
            }
        }

        public virtual SolidColorBrush BackgroundColor
        {
            get => new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
        }

        public virtual string BackgroundColorHex
        {
            get => cellModel.BackgroundColorHex;
            set
            {
                try
                {
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
                }
                catch
                {
                    return;
                }
                cellModel.BackgroundColorHex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackgroundColorHex)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackgroundColor)));
            }
        }

        public static CellViewModel CreateViewModelForModel(CellModel cellModel, SheetViewModel sheetViewModel)
        {
            if (cellModel.CellType == "Label")
            {
                return new CellViewModel(cellModel, sheetViewModel);
            }
            else if (cellModel.CellType == "Checkbox")
            {
                return new CheckboxCellViewModel(cellModel, sheetViewModel);
            }
            else if (cellModel.CellType == "Row")
            {
                return new RowCellViewModel(cellModel, sheetViewModel);
            }
            else if (cellModel.CellType == "Column")
            {
                return new ColumnCellViewModel(cellModel, sheetViewModel);
            }
            return new CellViewModel(cellModel, sheetViewModel);
        }

        public static CellModel CreateCellModel(double x, double y, double width, double height, string sheet, string text = "")
        {
            var model = new CellModel
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                CellType = "Label",
                Text = text,
                SheetName = sheet,
            };
            CellLoader.AddCell(model);
            return model;
        }

        public static CellModel CreateCellModelWithoutAddingToLoader(double x, double y, double width, double height, string sheet, string text = "")
        {
            var model = new CellModel
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                CellType = "Label",
                Text = text,
                SheetName = sheet,
            };
            return model;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}