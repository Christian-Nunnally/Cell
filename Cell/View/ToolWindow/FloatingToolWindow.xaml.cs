using Cell.ViewModel.Application;
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
        private IResizableToolWindow? _content;
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
                _contentHeight = Math.Max(value, _content?.MinimumHeight ?? 100);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContentHeight)));
            }
        }

        public double ContentWidth
        {
            get => _contentWidth;
            set
            {
                _contentWidth = Math.Max(value, _content?.MinimumWidth ?? 100);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContentWidth)));
            }
        }

        public bool IsDocked => _isDocked;

        public bool IsUndocked => !_isDocked;

        public string ToolWindowTitle => _content?.ToolWindowTitle ?? "";

        public void SetContent(UserControl content)
        {
            ContentHost.Content = content;
            _content = content as IResizableToolWindow;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolWindowTitle)));
            ContentWidth = _contentWidth;
            ContentHeight = _contentHeight;
            DataContext = this;

            if (_content != null)
            {
                _content.ToolBarCommands.ForEach(Commands.Add);
                _content.RequestClose = RequestClose;
            }
        }

        public void SetPositionRespectingBounds(double x, double y)
        {
            var boundedX = Math.Max(0, Math.Min(_canvas.ActualWidth - ActualWidth, x));
            var boundedY = Math.Max(0, Math.Min(_canvas.ActualHeight - ActualHeight, y));
            if (_content != null)
            {
                boundedX = Math.Max(0, Math.Min(_canvas.ActualWidth - _content.MinimumWidth, x));
                boundedY = Math.Max(0, Math.Min(_canvas.ActualHeight - _content.MinimumHeight, y));
            }
            Canvas.SetLeft(this, boundedX);
            Canvas.SetTop(this, boundedY);
        }

        public void UpdateSizeAndPositionRespectingBounds()
        {
            if (_content != null)
            {
                var width = _content.MinimumWidth;
                var height = _content.MinimumHeight;
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
            var isAllowingClose = _content?.HandleCloseRequested() ?? true;
            if (isAllowingClose)
            {
                _canvas.Children.Remove(this);
                _content?.HandleBeingClosed();
            }
        }

        private void ResizerRectangleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDocked) return;
            if (_content == null) return;
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
            if (_content == null) return;
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

        private void UndockButtonClicked(object sender, RoutedEventArgs e)
        {
            _isDocked = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUndocked)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDocked)));
        }
    }
}
