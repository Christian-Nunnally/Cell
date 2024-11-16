using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cell.View.ToolWindow
{
    public partial class FloatingToolWindowContainer : UserControl, INotifyPropertyChanged
    {
        private readonly ApplicationViewModel _applicationViewModel;
        private double _contentHeight;
        private double _contentWidth;
        private bool _resizing;
        private Point _resizingStartPosition;

        /// <summary>
        /// Creates a new instance of the <see cref="FloatingToolWindowContainer"/>.
        /// </summary>
        /// <param name="applicationViewModel">The application view model that owns this <see cref="FloatingToolWindowContainer"/>.</param>
        public FloatingToolWindowContainer(ApplicationViewModel applicationViewModel)
        {
            InitializeComponent();
            _applicationViewModel = applicationViewModel;
        }

        public Action? ShowDockOptions;

        /// <summary>
        /// Gets or sets the content displated within this tool window container.
        /// </summary>
        public ResizableToolWindow? ToolWindowContent
        {
            get => _resizableToolWindow; set
            {
                if (_resizableToolWindow != null && ToolWindowViewModel is not null)
                {
                    Commands.Clear();
                    ToolWindowViewModel.PropertyChanged -= ToolViewModelPropertyChanged;
                }
                _resizableToolWindow = value;
                if (_resizableToolWindow != null && ToolWindowViewModel is not null)
                {
                    ContentHost.Content = _resizableToolWindow;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolWindowContent)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolWindowTitle)));
                    ContentWidth = ToolWindowViewModel.DefaultWidth;
                    ContentHeight = ToolWindowViewModel.DefaultHeight;
                    SetCanvasLeft();
                    SetCanvasTop();
                    DataContext = this;
                    ToolWindowViewModel.ToolBarCommands.ForEach(Commands.Add);
                    ToolWindowViewModel.PropertyChanged += ToolViewModelPropertyChanged;
                }
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the commands that are available to the user in the title bar of the tool window.
        /// </summary>
        public ObservableCollection<CommandViewModel> Commands { get; set; } = [];

        /// <summary>
        /// Gets or sets the height the tool window should give its content.
        /// </summary>
        public double ContentHeight
        {
            get => _contentHeight;
            set
            {
                _contentHeight = Math.Max(value, _resizableToolWindow?.ToolViewModel.MinimumHeight ?? 100);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContentHeight)));
            }
        }

        /// <summary>
        /// Gets or sets the width the tool window should give its content.
        /// </summary>
        public double ContentWidth
        {
            get => _contentWidth;
            set
            {
                _contentWidth = Math.Max(value, _resizableToolWindow?.ToolViewModel.MinimumWidth ?? 100);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContentWidth)));
            }
        }

        /// <summary>
        /// Gets whether the tool window is currently docked (can not be moved or resized if docked).
        /// </summary>
        public bool IsDocked => _resizableToolWindow?.ToolViewModel.IsDocked ?? false;

        private ResizableToolWindow? _resizableToolWindow;

        private ToolWindowViewModel? ToolWindowViewModel => _resizableToolWindow?.ToolViewModel;

        /// <summary>
        /// Gets the title string for the tool window.
        /// </summary>
        public string ToolWindowTitle => _resizableToolWindow?.ToolViewModel.ToolWindowTitle ?? "";

        private void SetPositionRespectingBounds(double x, double y)
        {
            if (ToolWindowViewModel is null) return;
            ToolWindowViewModel.X = Math.Max(0, Math.Min(_applicationViewModel.ApplicationWindowWidth - ActualWidth, x));
            ToolWindowViewModel.Y = Math.Max(0, Math.Min(_applicationViewModel.ApplicationWindowHeight - ActualHeight, y));
        }

        /// <summary>
        /// Ensures the tool window is sized to fit entirely within the canvas it is displayed on by moving it. Will resize the tool window as a last resort.
        /// </summary>
        public void HandleOwningCanvasSizeChanged()
        {
            if (ToolWindowViewModel is null) return;
            SetPositionRespectingBounds(ToolWindowViewModel.X, ToolWindowViewModel.Y);
        }

        private static Point DifferenceBetweenTwoPoints(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            _resizableToolWindow?.ToolViewModel?.RequestClose?.Invoke();
        }

        private void DockButtonClicked(object sender, RoutedEventArgs e)
        {
            ShowDockOptions?.Invoke();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDocked)));
        }

        private void MoveToolboxToFrontOfCanvas(FloatingToolWindowContainer toolbox)
        {
            if (toolbox.ToolWindowViewModel is not null)
            {
                _applicationViewModel.MoveWindowToTop(toolbox.ToolWindowViewModel);
            }
        }

        private void ResizerRectangleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_resizableToolWindow is null) return;
            _resizing = true;
            _resizingStartPosition = e.GetPosition(this);
            Mouse.Capture(sender as IInputElement);
            e.Handled = true;
        }

        private void ResizerRectangleMouseMove(object sender, MouseEventArgs e)
        {
            if (_resizing)
            {
                var mousePosition = e.GetPosition(this);
                var delta = DifferenceBetweenTwoPoints(_resizingStartPosition, mousePosition);
                var desiredWidth = ContentWidth - delta.X;
                var desiredHeight = ContentHeight - delta.Y;
                SetSizeWhileRespectingBounds(desiredWidth, desiredHeight);
                _resizingStartPosition = mousePosition;
                e.Handled = true;
            }
        }

        private void ResizerRectangleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_resizableToolWindow is null) return;
            _resizing = false;
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void SetSizeWhileRespectingBounds(double desiredWidth, double desiredHeight)
        {
            var boundedWidth = Math.Max(50, Math.Min(_applicationViewModel.ApplicationWindowWidth- Canvas.GetLeft(this), desiredWidth));
            var boundedHeight = Math.Max(50, Math.Min(_applicationViewModel.ApplicationWindowHeight - Canvas.GetTop(this), desiredHeight));
            ContentWidth = boundedWidth;
            ContentHeight = boundedHeight;
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
                SetPositionRespectingBounds(desiredX, desiredY);
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

        private void SetCanvasTop() => Canvas.SetTop(this, _resizableToolWindow?.ToolViewModel.Y ?? 0);

        private void SetCanvasLeft() => Canvas.SetLeft(this, _resizableToolWindow?.ToolViewModel.X ?? 0);
    }
}
