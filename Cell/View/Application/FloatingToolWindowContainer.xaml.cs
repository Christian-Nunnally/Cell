using Cell.View.Application;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cell.View.ToolWindow
{
    public partial class FloatingToolWindowContainer : UserControl, INotifyPropertyChanged
    {
        private readonly WindowDockPanelViewModel _windowDockPanelViewModel;

        /// <summary>
        /// Creates a new instance of the <see cref="FloatingToolWindowContainer"/>.
        /// </summary>
        /// <param name="windowDockPanelViewModel"></param>
        public FloatingToolWindowContainer(WindowDockPanelViewModel windowDockPanelViewModel)
        {
            InitializeComponent();
            _windowDockPanelViewModel = windowDockPanelViewModel;
        }

        /// <summary>
        /// The action that is invoked when this tool window wants to show the dock options.
        /// </summary>
        public Action<ToolWindowViewModel?>? ShowDockOptions;

        /// <summary>
        /// Gets or sets the main content of this floating window.
        /// </summary>
        public WindowDockPanel? WindowDockPanel
        {
            get => _windowDockPanel; 
            set
            {
                if (_windowDockPanel != null && ToolWindowViewModel is not null)
                {
                    ToolWindowViewModel.PropertyChanged -= ToolViewModelPropertyChanged;
                }
                _windowDockPanel = value;
                if (_windowDockPanel != null && ToolWindowViewModel is not null)
                {
                    ContentHost.Content = _windowDockPanel;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowDockPanel)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolWindowTitle)));
                    SetCanvasLeft();
                    SetCanvasTop();
                    DataContext = this;
                    ToolWindowViewModel.PropertyChanged += ToolViewModelPropertyChanged;
                }
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        private WindowDockPanel? _windowDockPanel;

        private ToolWindowViewModel? ToolWindowViewModel => _windowDockPanel?.ViewModel.MainContent;

        /// <summary>
        /// Gets the title string for the tool window.
        /// </summary>
        public string ToolWindowTitle => ToolWindowViewModel?.ToolWindowTitle ?? "";

        private void SetPositionRespectingBounds(double canvasWidth, double canvasHeight, double x, double y)
        {
            if (ToolWindowViewModel is null) return;
            ToolWindowViewModel.X = Math.Max(0, Math.Min(canvasWidth - ActualWidth, x));
            ToolWindowViewModel.Y = Math.Max(0, Math.Min(canvasHeight - ActualHeight, y));
        }

        /// <summary>
        /// Ensures the tool window is sized to fit entirely within the canvas it is displayed on by moving it. Will resize the tool window as a last resort.
        /// </summary>
        public void HandleOwningCanvasSizeChanged(double canvasWidth, double canvasHeight)
        {
            if (ToolWindowViewModel is null) return;
            SetPositionRespectingBounds(canvasWidth, canvasHeight, ToolWindowViewModel.X, ToolWindowViewModel.Y);
        }

        private void MoveToolboxToFrontOfCanvas(FloatingToolWindowContainer toolbox)
        {
            if (toolbox.ToolWindowViewModel is not null)
            {
                _windowDockPanelViewModel.TopFloatingWindow = toolbox.ToolWindowViewModel;
            }
        }

        private void ToolboxMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FloatingToolWindowContainer toolbox)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(toolbox);
                if (parent is not Canvas canvas) return;
                MoveToolboxToFrontOfCanvas(toolbox);
                var offset = e.GetPosition(canvas);
                offset.X -= Canvas.GetLeft(toolbox);
                offset.Y -= Canvas.GetTop(toolbox);
                toolbox.Tag = offset;
                toolbox.CaptureMouse();
            }
        }

        private void ToolboxMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FloatingToolWindowContainer toolbox)
            {
                toolbox.ReleaseMouseCapture();
            }
        }

        private void ToolboxMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is FloatingToolWindowContainer toolbox && toolbox.IsMouseCaptured)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(toolbox);
                if (parent is not Canvas canvas) return;
                var position = e.GetPosition(canvas);
                var offset = (Point)toolbox.Tag;

                var desiredX = position.X - offset.X;
                var desiredY = position.Y - offset.Y;
                SetPositionRespectingBounds(canvas.ActualWidth, canvas.ActualHeight, desiredX, desiredY);
            }
        }

        private void ToolViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.ToolWindow.ToolWindowViewModel.ToolWindowTitle):
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolWindowTitle)));
                    break;
                case nameof(ViewModel.ToolWindow.ToolWindowViewModel.X):
                    SetCanvasLeft();
                    break;
                case nameof(ViewModel.ToolWindow.ToolWindowViewModel.Y):
                    SetCanvasTop();
                    break;
            }
        }

        private void SetCanvasTop() => Canvas.SetTop(this, ToolWindowViewModel?.Y ?? 0);

        private void SetCanvasLeft() => Canvas.SetLeft(this, ToolWindowViewModel?.X ?? 0);
    }
}
