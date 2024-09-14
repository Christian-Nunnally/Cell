using Cell.Common;
using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace Cell.View.ToolWindow
{
    public partial class CellFormatEditWindow : UserControl, IToolWindow
    {
        private readonly CellFormatEditWindowViewModel _viewModel;

        public CellFormatEditWindow(CellFormatEditWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public static void AddColorsToColorPicker(ColorPicker colorPicker, List<string> colors, float brightnessFactor)
        {
            foreach (var color in colors)
            {
                var adjustedColor = ColorAdjuster.AdjustBrightness(color, brightnessFactor);
                colorPicker.AvailableColors.Add(new ColorItem(ColorAdjuster.ConvertHexStringToColor(adjustedColor), ""));
            }
        }

        public string GetTitle()
        {
            var currentlySelectedCell = _viewModel.CellsBeingEdited.FirstOrDefault();
            if (currentlySelectedCell is null) return "Select a cell to edit";
            if (currentlySelectedCell == ApplicationViewModel.Instance.ApplicationSettings.DefaultCellStyleCellModel) return "Edit default cell format";
            if (currentlySelectedCell == ApplicationViewModel.Instance.ApplicationSettings.DefaultSpecialCellStyleCellModel) return "Edit default row.column cell format";
            return $"Format editor - {currentlySelectedCell.GetName()}";
        }

        public List<CommandViewModel> GetToolBarCommands() => [];

        public bool HandleBeingClosed() => true;

        private void ChangeCellTypeCellClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            var cellTypeString = button.Content is Label label ? label.Content.ToString() : button.Content.ToString();
            if (Enum.TryParse(cellTypeString, out CellType newType)) _viewModel.CellType = newType;
            ApplicationViewModel.Instance.SheetViewModel?.UpdateLayout();
        }

        private void ColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not ColorPicker colorPicker) return;
            var colors = new List<string> { "#9678b5", "#b272a1", "#c17188", "#c3776f", "#b8825c", "#a48f54", "#8a9b5c", "#6da471", "#50aa8f", "#3dadaf", "#4aadca", "#6fa9dc" };
            colorPicker.AvailableColors.Clear();
            colorPicker.AvailableColorsSortingMode = ColorSortingMode.Alphabetical;
            AddColorsToColorPicker(colorPicker, colors, 1.0f);
            AddColorsToColorPicker(colorPicker, colors, .1f);
            AddColorsToColorPicker(colorPicker, colors, 1.9f);
        }

        private void MergeAcrossButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.MergeCellsAcross();
            ApplicationViewModel.Instance.SheetViewModel?.UpdateLayout();
        }

        private void MergeButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.MergeCells();
            ApplicationViewModel.Instance.SheetViewModel?.UpdateLayout();
        }

        private void MergeDownButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.MergeCellsDown();
            ApplicationViewModel.Instance.SheetViewModel?.UpdateLayout();
        }

        private void SetAlignmentToBottomButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.HorizontalAlignment = HorizontalAlignment.Stretch;
            _viewModel.VerticalAlignment = VerticalAlignment.Bottom;
        }

        private void SetAlignmentToBottomLeftButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.HorizontalAlignment = HorizontalAlignment.Left;
            _viewModel.VerticalAlignment = VerticalAlignment.Bottom;
        }

        private void SetAlignmentToBottomRightButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.HorizontalAlignment = HorizontalAlignment.Right;
            _viewModel.VerticalAlignment = VerticalAlignment.Bottom;
        }

        private void SetAlignmentToCenterButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.HorizontalAlignment = HorizontalAlignment.Center;
            _viewModel.VerticalAlignment = VerticalAlignment.Center;
        }

        private void SetAlignmentToLeftButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.HorizontalAlignment = HorizontalAlignment.Left;
            _viewModel.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private void SetAlignmentToRightButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.HorizontalAlignment = HorizontalAlignment.Right;
            _viewModel.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private void SetAlignmentToTopButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.HorizontalAlignment = HorizontalAlignment.Stretch;
            _viewModel.VerticalAlignment = VerticalAlignment.Top;
        }

        private void SetAlignmentToTopLeftButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.HorizontalAlignment = HorizontalAlignment.Left;
            _viewModel.VerticalAlignment = VerticalAlignment.Top;
        }

        private void SetAlignmentToTopRightButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.HorizontalAlignment = HorizontalAlignment.Right;
            _viewModel.VerticalAlignment = VerticalAlignment.Top;
        }

        private void SetTextAlignmentToCenterButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.TextAlignment = TextAlignment.Center;
        }

        private void SetTextAlignmentToLeftButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.TextAlignment = TextAlignment.Left;
        }

        private void SetTextAlignmentToRightButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.TextAlignment = TextAlignment.Right;
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void UnmergeButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.UnmergeCells();
        }
    }
}
