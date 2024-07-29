using Cell.Model;
using Cell.View.ToolWindow;
using Cell.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cell.View
{
    /// <summary>
    /// Interaction logic for FloatingToolWindow.xaml
    /// </summary>
    public partial class FloatingToolWindow : UserControl
    {
        private Canvas _canvas;
        private IToolWindow? _content;
        private IResizableToolWindow? _resizableContent;

        public FloatingToolWindow(Canvas canvas)
        {
            InitializeComponent();
            _canvas = canvas;
        }

        public void SetContent(UserControl content)
        {
            ContentHost.Content = content;
            _content = content as IToolWindow;
            _resizableContent = content as IResizableToolWindow;
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            _canvas.Children.Remove(this);
            _content?.Close();
        }

        private void Toolbox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var toolbox = sender as FloatingToolWindow;
            if (toolbox != null)
            {
                var offset = e.GetPosition(_canvas);
                offset.X -= Canvas.GetLeft(toolbox);
                offset.Y -= Canvas.GetTop(toolbox);
                toolbox.Tag = offset;
                toolbox.CaptureMouse();
            }
        }

        private void Toolbox_MouseMove(object sender, MouseEventArgs e)
        {
            var toolbox = sender as FloatingToolWindow;
            if (toolbox != null && toolbox.IsMouseCaptured)
            {
                var position = e.GetPosition(_canvas);
                var offset = (Point)toolbox.Tag;
                Canvas.SetLeft(toolbox, position.X - offset.X);
                Canvas.SetTop(toolbox, position.Y - offset.Y);
            }
        }

        private void Toolbox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var toolbox = sender as FloatingToolWindow;
            if (toolbox != null)
            {
                toolbox.ReleaseMouseCapture();
            }
        }

        private bool _resizing;
        private Point _resizingStartPosition;

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

        private static Point DifferenceBetweenTwoPoints(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);

    }
}
