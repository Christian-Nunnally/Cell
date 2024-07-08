using Cell.Controls;
using Cell.Model;
using Cell.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace Cell
{
    public partial class MainWindow : Window
    {
        private PanAndZoomCanvas panAndZoomCanvas;

        public SheetViewModel SheetViewModel { get; set; } = new SheetViewModel("Default");

        public MainWindow()
        {
            DataContext = SheetViewModel;
            SheetViewModel.LoadCellViewModels();
            InitializeComponent();
        }

        private void Rectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                if (e.ClickCount == 2)
                {
                    AdjustWindowSize();
                }
                else
                {
                    Application.Current.MainWindow.DragMove();
                }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void AdjustWindowSize()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.BorderThickness = new System.Windows.Thickness(8);
            }
            else
            {
                this.BorderThickness = new System.Windows.Thickness(0);
            }
        }

        private void CellMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Temp();
                return;
            }
            if (sender is FrameworkElement element)
            {
                if (element.DataContext is CellViewModel cell)
                {
                    if (e.ChangedButton == MouseButton.Left)
                    {
                        SheetViewModel.UnselectAllCells();
                        SheetViewModel.SelectCell(cell);
                    }
                }
            }
        }

        private void PanZoomCanvasLoaded(object sender, RoutedEventArgs e)
        {
            panAndZoomCanvas = sender as PanAndZoomCanvas;
        }

        private void Temp()
        {
            if (SheetViewModel.AreEditingPanelsOpen)
            {
                SheetViewModel.CloseEditingPanels(panAndZoomCanvas);
            }
            else
            {
                SheetViewModel.OpenEditingPanels(panAndZoomCanvas);
            }
        }

        private void RowResizeTopThumbDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {

        }
        double currentPosition = 0.0;

        private void RowResizeBottomThumbDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                if (element.DataContext is RowCellViewModel row)
                {
                    currentPosition += e.VerticalChange;
                    //SheetViewModel.ResizeRow(row.Text, currentPosition);
                }
            }
        }

        private void RowResizeBottomThumbDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                if (element.DataContext is RowCellViewModel row)
                {
                    currentPosition = 0.0;
                }
            }
        }

        private void RowResizeBottomThumbDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
        }

        private void PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.R)
            {
                SheetViewModel.AddRow();
            }
            else if (e.Key == Key.C)
            {
                SheetViewModel.AddColumn();
            }
            else if (e.Key == Key.L)
            {
                CellLoader.LoadCells(CellLoader.DefaultSaveLocation);
                PluginFunctionLoader.LoadPlugins();
            }
            else if (e.Key == Key.S)
            {
                CellLoader.SaveCells(CellLoader.DefaultSaveLocation);
                PluginFunctionLoader.SavePlugins();
            }
            else if (e.Key == Key.Escape)
            {
                SheetViewModel.UnselectAllCells();
                SheetViewModel.CloseEditingPanels(panAndZoomCanvas);
            }
            else if (e.Key == Key.T)
            {
            }
        }

        private void CreateGetTextPluginFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            PluginFunctionLoader.CreateGetTextPluginFunction(SheetViewModel.SelectedCellViewModel.GetTextFunctionName);
        }

        private void CreateOnEditPluginFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            PluginFunctionLoader.CreateOnEditPluginFunction(SheetViewModel.SelectedCellViewModel.OnEditFunctionName);
        }
    }
}