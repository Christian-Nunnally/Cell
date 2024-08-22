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
        private readonly Canvas _canvas;
        private IToolWindow? _content;
        private IResizableToolWindow? _resizableContent;
        private bool _resizing;
        private Point _resizingStartPosition;
        public FloatingToolWindow(Canvas canvas)
        {
            InitializeComponent();
            _canvas = canvas;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<CommandViewModel> Commands { get; set; } = [];

        public bool IsToolWindowResizeable => _resizableContent != null;

        public string ToolWindowTitle => _content?.GetTitle() ?? "";

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

        private static Point DifferenceBetweenTwoPoints(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            RequestClose();
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
            if (_resizableContent == null) return;
            _resizing = true;
            _resizingStartPosition = e.GetPosition(this);
            Mouse.Capture(sender as IInputElement);
            e.Handled = true;
        }

        private void ResizerRectangleMouseMove(object sender, MouseEventArgs e)
        {
            if (_resizableContent == null) return;
            if (_resizing)
            {
                var mousePosition = e.GetPosition(this);
                var delta = DifferenceBetweenTwoPoints(_resizingStartPosition, mousePosition);
                _resizableContent.SetWidth(Math.Max(50, _resizableContent.GetWidth() - delta.X));
                _resizableContent.SetHeight(Math.Max(50, _resizableContent.GetHeight() - delta.Y));
                _resizingStartPosition = mousePosition;
                e.Handled = true;
            }
        }

        private void ResizerRectangleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_resizableContent == null) return;
            _resizing = false;
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void Toolbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FloatingToolWindow toolbox)
            {
                var offset = e.GetPosition(_canvas);
                offset.X -= Canvas.GetLeft(toolbox);
                offset.Y -= Canvas.GetTop(toolbox);
                toolbox.Tag = offset;
                toolbox.CaptureMouse();
            }
        }

        private void Toolbox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FloatingToolWindow toolbox)
            {
                toolbox.ReleaseMouseCapture();
            }
        }

        private void Toolbox_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is FloatingToolWindow toolbox && toolbox.IsMouseCaptured)
            {
                var position = e.GetPosition(_canvas);
                var offset = (Point)toolbox.Tag;
                Canvas.SetLeft(toolbox, position.X - offset.X);
                Canvas.SetTop(toolbox, position.Y - offset.Y);
            }
        }
    }
}
