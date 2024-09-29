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
        public FloatingToolWindow(Canvas canvas)
        {
            InitializeComponent();
            _canvas = canvas;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<CommandViewModel> Commands { get; set; } = [];

        public double ContentHeight
        {
            get => _contentHeight;
            set
            {
                _contentHeight = Math.Max(value, ResizableToolWindow?.ToolViewModel.MinimumHeight ?? 100);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContentHeight)));
            }
        }

        public double ContentWidth
        {
            get => _contentWidth;
            set
            {
                _contentWidth = Math.Max(value, ResizableToolWindow?.ToolViewModel.MinimumWidth ?? 100);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContentWidth)));
            }
        }

        public bool IsDocked => _isDocked;

        public bool IsUndocked => !_isDocked;

        public ResizableToolWindow? ResizableToolWindow { get; set; }

        public string ToolWindowTitle => ResizableToolWindow?.ToolViewModel.ToolWindowTitle ?? "";

        public void SetContent(ResizableToolWindow resizableToolWindow)
        {
            ContentHost.Content = resizableToolWindow;
            ResizableToolWindow = resizableToolWindow;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResizableToolWindow)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolWindowTitle)));
            ContentWidth = resizableToolWindow.ToolViewModel.DefaultWidth;
            ContentHeight = resizableToolWindow.ToolViewModel.DefaultHeight;
            ContentHeight = _contentHeight;
            DataContext = this;
            ResizableToolWindow.ToolViewModel.ToolBarCommands.ForEach(Commands.Add);
            ResizableToolWindow.ToolViewModel.RequestClose = RequestClose;
            ResizableToolWindow.ToolViewModel.PropertyChanged += ToolViewModelPropertyChanged;
        }

        public void SetPositionRespectingBounds(double x, double y)
        {
            var boundedX = Math.Max(0, Math.Min(_canvas.ActualWidth - ActualWidth, x));
            var boundedY = Math.Max(0, Math.Min(_canvas.ActualHeight - ActualHeight, y));
            if (ResizableToolWindow != null)
            {
                boundedX = Math.Max(0, Math.Min(_canvas.ActualWidth - ResizableToolWindow.ToolViewModel.MinimumWidth, x));
                boundedY = Math.Max(0, Math.Min(_canvas.ActualHeight - ResizableToolWindow.ToolViewModel.MinimumHeight, y));
            }
            Canvas.SetLeft(this, boundedX);
            Canvas.SetTop(this, boundedY);
        }

        public void UpdateSizeAndPositionRespectingBounds()
        {
            if (ResizableToolWindow != null)
            {
                var width = ResizableToolWindow.ToolViewModel.DefaultWidth;
                var height = ResizableToolWindow.ToolViewModel.DefaultHeight;
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
            var isAllowingClose = ResizableToolWindow?.ToolViewModel.HandleCloseRequested() ?? true;
            if (isAllowingClose)
            {
                _canvas.Children.Remove(this);
                ResizableToolWindow?.ToolViewModel.HandleBeingClosed();
            }
        }

        private void ResizerRectangleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDocked) return;
            if (ResizableToolWindow == null) return;
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
            if (ResizableToolWindow == null) return;
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
