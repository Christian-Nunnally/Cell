using Cell.Common;
using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace Cell.View.ToolWindow
{
    public partial class CellFormatEditWindow : ResizableToolWindow
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CellFormatEditWindow"/> class.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public CellFormatEditWindow(CellFormatEditWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private readonly CellFormatEditWindowViewModel _viewModel;

        private static void AddColorsToColorPicker(ObservableCollection<ColorItem> availableColors, List<string> colors, float brightnessFactor)
        {
            foreach (var color in colors)
            {
                var adjustedColor = ColorAdjuster.AdjustBrightness(color, brightnessFactor);
                availableColors.Add(new ColorItem(ColorAdjuster.ConvertHexStringToColor(adjustedColor), adjustedColor));
            }
        }

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
            var availableColors = new ObservableCollection<ColorItem>();
            colorPicker.AvailableColorsSortingMode = ColorSortingMode.Alphabetical;
            AddColorsToColorPicker(availableColors, colors, 1.9f);
            AddColorsToColorPicker(availableColors, colors, 1.4f);
            AddColorsToColorPicker(availableColors, colors, 1.0f);
            AddColorsToColorPicker(availableColors, colors, .7f);
            AddColorsToColorPicker(availableColors, colors, .5f);
            AddColorsToColorPicker(availableColors, colors, .35f);
            AddColorsToColorPicker(availableColors, colors, .27f);
            AddColorsToColorPicker(availableColors, colors, .2f);
            AddColorsToColorPicker(availableColors, colors, .15f);
            AddColorsToColorPicker(availableColors, colors, .1f);
            colorPicker.AvailableColors = availableColors;
        }

        private void CreateNewColumnToTheLeftButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.AddColumnToTheLeft();
            ApplicationViewModel.Instance.SheetViewModel!.UpdateLayout();
        }

        private void CreateNewColumnToTheRightButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.AddColumnToTheRight();
            ApplicationViewModel.Instance.SheetViewModel!.UpdateLayout();
        }

        private void CreateNewRowAboveButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.AddRowAbove();
            ApplicationViewModel.Instance.SheetViewModel!.UpdateLayout();
        }

        private void CreateNewRowBelowButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.AddRowBelow();
            ApplicationViewModel.Instance.SheetViewModel!.UpdateLayout();
        }

        private void DeleteColumnButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.DeleteColumns();
            ApplicationViewModel.Instance.SheetViewModel!.UpdateLayout();
        }

        private void DeleteRowButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.DeleteRows();
            ApplicationViewModel.Instance.SheetViewModel!.UpdateLayout();
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

        private void UnmergeButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.UnmergeCells();
        }
    }
}
