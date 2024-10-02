using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    public partial class FloatingToolWindow : UserControl, INotifyPropertyChanged
    {
        private readonly Canvas _canvas;
        private double _contentHeight;
        private double _contentWidth;
        private bool _isDocked;
        private bool _resizing;
        private Point _resizingStartPosition;

        /// <summary>
        /// Creates a new instance of the <see cref="FloatingToolWindow"/>.
        /// </summary>
        /// <param name="canvas">The canvas the tool window is being displayed on.</param>
        public FloatingToolWindow(Canvas canvas)
        {
            InitializeComponent();
            _canvas = canvas;
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
        public bool IsDocked => _isDocked;

        /// <summary>
        /// Gets whether the tool window is currently undocked (can not be moved or resized if docked).
        /// </summary>
        public bool IsUndocked => !_isDocked;


        private ResizableToolWindow? _resizableToolWindow;

        /// <summary>
        /// Gets the title string for the tool window.
        /// </summary>
        public string ToolWindowTitle => _resizableToolWindow?.ToolViewModel.ToolWindowTitle ?? "";

        /// <summary>
        /// Sets the content of this floating tool window.
        /// </summary>
        /// <param name="resizableToolWindow">The content that goes inside the tool window border.</param>
        public void SetContent(ResizableToolWindow resizableToolWindow)
        {
            ContentHost.Content = resizableToolWindow;
            _resizableToolWindow = resizableToolWindow;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_resizableToolWindow)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolWindowTitle)));
            ContentWidth = resizableToolWindow.ToolViewModel.DefaultWidth;
            ContentHeight = resizableToolWindow.ToolViewModel.DefaultHeight;
            ContentHeight = _contentHeight;
            DataContext = this;
            _resizableToolWindow.ToolViewModel.ToolBarCommands.ForEach(Commands.Add);
            _resizableToolWindow.ToolViewModel.RequestClose = RequestClose;
            _resizableToolWindow.ToolViewModel.PropertyChanged += ToolViewModelPropertyChanged;
        }

        private void SetPositionRespectingBounds(double x, double y)
        {
            var boundedX = Math.Max(0, Math.Min(_canvas.ActualWidth - ActualWidth, x));
            var boundedY = Math.Max(0, Math.Min(_canvas.ActualHeight - ActualHeight, y));
            if (_resizableToolWindow != null)
            {
                boundedX = Math.Max(0, Math.Min(_canvas.ActualWidth - _resizableToolWindow.ToolViewModel.MinimumWidth, x));
                boundedY = Math.Max(0, Math.Min(_canvas.ActualHeight - _resizableToolWindow.ToolViewModel.MinimumHeight, y));
            }
            Canvas.SetLeft(this, boundedX);
            Canvas.SetTop(this, boundedY);
        }

        /// <summary>
        /// Ensures the tool window is sized to fit entirely within the canvas it is displayed on by moving it. Will resize the tool window as a last resort.
        /// </summary>
        public void UpdateSizeAndPositionRespectingBounds()
        {
            if (_resizableToolWindow != null)
            {
                var width = _resizableToolWindow.ToolViewModel.DefaultWidth;
                var height = _resizableToolWindow.ToolViewModel.DefaultHeight;
                SetSizeWhileRespectingBounds(width, height);
            }

            var x = Canvas.GetLeft(this);
            var y = Canvas.GetTop(this);
            SetPositionRespectingBounds(x, y);
        }

        private static Point DifferenceBetweenTwoPoints(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            RequestClose();
        }

        private void DockButtonClicked(object sender, RoutedEventArgs e)
        {
            _isDocked = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUndocked)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDocked)));
        }

        private void MoveToolboxToFrontOfCanvas(FloatingToolWindow toolbox)
        {
            if (_canvas.Children[^1] == toolbox) return;
            _canvas.Children.Remove(toolbox);
            _canvas.Children.Add(toolbox);
        }

        private void RequestClose()
        {
            var isAllowingClose = _resizableToolWindow?.ToolViewModel.HandleCloseRequested() ?? true;
            if (isAllowingClose)
            {
                _canvas.Children.Remove(this);
                _resizableToolWindow?.ToolViewModel.HandleBeingClosed();
            }
        }

        private void ResizerRectangleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDocked) return;
            if (_resizableToolWindow == null) return;
            _resizing = true;
            _resizingStartPosition = e.GetPosition(this);
            Mouse.Capture(sender as IInputElement);
            e.Handled = true;
        }

        private void ResizerRectangleMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDocked) return;
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
            if (_isDocked) return;
            if (_resizableToolWindow == null) return;
            _resizing = false;
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void SetSizeWhileRespectingBounds(double desiredWidth, double desiredHeight)
        {
            var boundedWidth = Math.Max(50, Math.Min(_canvas.ActualWidth - Canvas.GetLeft(this), desiredWidth));
            var boundedHeight = Math.Max(50, Math.Min(_canvas.ActualHeight - Canvas.GetTop(this), desiredHeight));
            ContentWidth = boundedWidth;
            ContentHeight = boundedHeight;
        }

        private void ToolboxMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDocked) return;
            if (sender is FloatingToolWindow toolbox)
            {
                MoveToolboxToFrontOfCanvas(toolbox);
                var offset = e.GetPosition(_canvas);
                offset.X -= Canvas.GetLeft(toolbox);
                offset.Y -= Canvas.GetTop(toolbox);
                toolbox.Tag = offset;
                toolbox.CaptureMouse();
            }
        }

        private void ToolboxMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FloatingToolWindow toolbox)
            {
                toolbox.ReleaseMouseCapture();
            }
        }

        private void ToolboxMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDocked) return;
            if (sender is FloatingToolWindow toolbox && toolbox.IsMouseCaptured)
            {
                var position = e.GetPosition(_canvas);
                var offset = (Point)toolbox.Tag;

                var desiredX = position.X - offset.X;
                var desiredY = position.Y - offset.Y;
                SetPositionRespectingBounds(desiredX, desiredY);
            }
        }

        private void ToolViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ToolWindowViewModel.ToolWindowTitle)) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolWindowTitle)));
        }

        private void UndockButtonClicked(object sender, RoutedEventArgs e)
        {
            _isDocked = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUndocked)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDocked)));
        }
    }
}
