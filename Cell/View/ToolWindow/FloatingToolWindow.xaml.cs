using Cell.ViewModel.Application;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    /// <summary>
    /// Interaction logic for FloatingToolWindow.xaml
    /// </summary>
    public partial class FloatingToolWindow : UserControl, INotifyPropertyChanged
    {
        private const int AdditionalHeightFromToolWindowBorder = 52;
        private const int AdditionalWidthFromToolWindowBorder = 24;
        private const int MinimumToolWindowSize = 120;
        private readonly Canvas _canvas;
        private IToolWindow? _content;
        private IResizableToolWindow? _resizableContent;
        private bool _resizing;
        private Point _resizingStartPosition;
        private bool _isDocked;
        public FloatingToolWindow(Canvas canvas)
        {
            InitializeComponent();
            _canvas = canvas;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<CommandViewModel> Commands { get; set; } = [];

        public bool IsToolWindowResizeable => _resizableContent != null;

        public bool IsUndocked => !_isDocked;

        public bool IsDocked => _isDocked;

        public string ToolWindowTitle => _content?.GetTitle() ?? "";

        public double ContentWidth => _resizableContent?.GetWidth() ?? MinimumToolWindowSize;

        public double ContentHeight => _resizableContent?.GetHeight() ?? MinimumToolWindowSize;

        public void SetContent(UserControl content)
        {
            ContentHost.Content = content;
            content.DataContextChanged += ContentDataContextChanged;
            _content = content as IToolWindow;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolWindowTitle)));
            _resizableContent = content as IResizableToolWindow;
            DataContext = this;

            if (_content != null)
            {
                _content.GetToolBarCommands().ForEach(Commands.Add);
                _content.RequestClose = RequestClose;
            }
        }

        public void SetPositionRespectingBounds(double x, double y)
        {
            var boundedX = Math.Max(0, Math.Min(_canvas.ActualWidth - ActualWidth, x));
            var boundedY = Math.Max(0, Math.Min(_canvas.ActualHeight - ActualHeight, y));
            if (_resizableContent != null)
            {
                boundedX = Math.Max(0, Math.Min(_canvas.ActualWidth - _resizableContent.GetWidth() - AdditionalWidthFromToolWindowBorder, x));
                boundedY = Math.Max(0, Math.Min(_canvas.ActualHeight - _resizableContent.GetHeight() - AdditionalHeightFromToolWindowBorder, y));
            }
            Canvas.SetLeft(this, boundedX);
            Canvas.SetTop(this, boundedY);
        }

        internal void UpdateSizeAndPositionRespectingBounds()
        {
            if (_resizableContent != null)
            {
                var width = _resizableContent.GetWidth();
                var height = _resizableContent.GetHeight();
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

        private void UndockButtonClicked(object sender, RoutedEventArgs e)
        {
            _isDocked = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUndocked)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDocked)));
        }

        private void ContentDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolWindowTitle)));
        }

        private void RequestClose()
        {
            var isAllowingClose = _content?.HandleBeingClosed() ?? true;
            if (isAllowingClose) _canvas.Children.Remove(this);
        }

        private void ResizerRectangleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDocked) return;
            if (_resizableContent == null) return;
            _resizing = true;
            _resizingStartPosition = e.GetPosition(this);
            Mouse.Capture(sender as IInputElement);
            e.Handled = true;
        }

        private void ResizerRectangleMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDocked) return;
            if (_resizableContent == null) return;
            if (_resizing)
            {
                var mousePosition = e.GetPosition(this);
                var delta = DifferenceBetweenTwoPoints(_resizingStartPosition, mousePosition);
                var desiredWidth = _resizableContent.GetWidth() - delta.X;
                var desiredHeight = _resizableContent.GetHeight() - delta.Y;
                SetSizeWhileRespectingBounds(desiredWidth, desiredHeight);
                _resizingStartPosition = mousePosition;
                e.Handled = true;
            }
        }

        private void ResizerRectangleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDocked) return;
            if (_resizableContent == null) return;
            _resizing = false;
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void SetSizeWhileRespectingBounds(double desiredWidth, double desiredHeight)
        {
            if (_resizableContent == null) return;
            var boundedWidth = Math.Max(MinimumToolWindowSize, Math.Min(_canvas.ActualWidth - Canvas.GetLeft(this) - AdditionalWidthFromToolWindowBorder, desiredWidth));
            var boundedHeight = Math.Max(MinimumToolWindowSize, Math.Min(_canvas.ActualHeight - Canvas.GetTop(this) - AdditionalHeightFromToolWindowBorder, desiredHeight));
            _resizableContent.SetWidth(boundedWidth);
            _resizableContent.SetHeight(boundedHeight);
        }

        private void ToolboxMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDocked) return;
            if (sender is FloatingToolWindow toolbox)
            {
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
    }
}
