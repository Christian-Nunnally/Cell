using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    public partial class DockedToolWindowContainer : UserControl, INotifyPropertyChanged
    {
        private readonly ApplicationViewModel _applicationViewModel;
        private double _contentHeight;
        private double _contentWidth;
        private bool _resizing;
        private Point _resizingStartPosition;
        private ResizableToolWindow? _resizableToolWindow;

        /// <summary>
        /// Creates a new instance of the <see cref="DockedToolWindowContainer"/>.
        /// </summary>
        /// <param name="applicationViewModel">The application view model that owns this <see cref="DockedToolWindowContainer"/>.</param>
        public DockedToolWindowContainer(ApplicationViewModel applicationViewModel)
        {
            InitializeComponent();
            _applicationViewModel = applicationViewModel;
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
                _contentHeight = Math.Max(value, ToolWindowContent?.ToolViewModel.MinimumHeight ?? 100);
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
                _contentWidth = Math.Max(value, ToolWindowContent?.ToolViewModel.MinimumWidth ?? 100);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContentWidth)));
            }
        }

        /// <summary>
        /// Gets whether the tool window is currently docked.
        /// </summary>
        public bool IsDocked => _resizableToolWindow?.ToolViewModel.IsDocked ?? false;

        /// <summary>
        /// Gets whether the resizer should appear in the bottom right (true) or top left (false).
        /// </summary>
        public bool IsBottomRightResizerVisible => ToolWindowContent?.ToolViewModel.Dock == Dock.Top || ToolWindowContent?.ToolViewModel.Dock == Dock.Left;

        /// <summary>
        /// Gets whether the resizer should appear on the left and right of the tool window.
        /// </summary>
        public bool AreSideResizersVisible => ToolWindowContent?.ToolViewModel.Dock == Dock.Left || ToolWindowContent?.ToolViewModel.Dock == Dock.Right;

        /// <summary>
        /// Gets or sets the content displated within this tool window container.
        /// </summary>
        public ResizableToolWindow? ToolWindowContent
        {
            get => _resizableToolWindow; set
            {
                if (_resizableToolWindow != null)
                {
                    Commands.Clear();
                    _resizableToolWindow.ToolViewModel.PropertyChanged -= ToolViewModelPropertyChanged;
                }
                _resizableToolWindow = value;
                if (_resizableToolWindow != null)
                {
                    ContentHost.Content = _resizableToolWindow;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolWindowContent)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolWindowTitle)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBottomRightResizerVisible)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AreSideResizersVisible)));
                    ContentWidth = _resizableToolWindow.ToolViewModel.DefaultWidth;
                    ContentHeight = _resizableToolWindow.ToolViewModel.DefaultHeight;
                    SetSizeWhileRespectingBounds(ContentWidth, ContentHeight);
                    DataContext = this;
                    _resizableToolWindow.ToolViewModel.ToolBarCommands.ForEach(Commands.Add);
                    _resizableToolWindow.ToolViewModel.PropertyChanged += ToolViewModelPropertyChanged;
                }
            }
        }

        /// <summary>
        /// Gets the title string for the tool window.
        /// </summary>
        public string ToolWindowTitle => ToolWindowContent?.ToolViewModel.ToolWindowTitle ?? "";

        /// <summary>
        /// Ensures the tool window is sized to fit entirely within the canvas it is displayed on by moving it. Will resize the tool window as a last resort.
        /// </summary>
        public void HandleOwningCanvasSizeChanged()
        {
            SetSizeWhileRespectingBounds(ContentWidth, ContentHeight);
        }

        private static Point DifferenceBetweenTwoPoints(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            ToolWindowContent?.ToolViewModel?.RequestClose?.Invoke();
        }

        private void UndockButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_resizableToolWindow == null) return;
            _resizableToolWindow.ToolViewModel.IsDocked = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDocked)));
        }

        private void ResizerRectangleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ToolWindowContent is null) return;
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
                if (!IsBottomRightResizerVisible)
                {
                    var desiredWidth = ContentWidth + delta.X;
                    var desiredHeight = ContentHeight + delta.Y;
                    SetSizeWhileRespectingBounds(desiredWidth, desiredHeight);
                }
                else
                {
                    var desiredWidth = ContentWidth - delta.X;
                    var desiredHeight = ContentHeight - delta.Y;
                    _resizingStartPosition = mousePosition;
                    SetSizeWhileRespectingBounds(desiredWidth, desiredHeight);
                }
                e.Handled = true;
            }
        }

        private void ResizerRectangleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ToolWindowContent is null) return;
            _resizing = false;
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void SetSizeWhileRespectingBounds(double desiredWidth, double desiredHeight)
        {
            var dock = ToolWindowContent?.ToolViewModel.Dock;
            if (dock == Dock.Left || dock == Dock.Right)
            {
                var boundedWidth = Math.Max(50, Math.Min(_applicationViewModel.ApplicationWindowWidth-30, desiredWidth));
                ContentWidth = boundedWidth;
                ContentHeight = double.NaN;
            }
            else
            {
                var boundedHeight = Math.Max(50, Math.Min(_applicationViewModel.ApplicationWindowHeight-30, desiredHeight));
                ContentHeight = boundedHeight;
                ContentWidth = double.NaN;
            }
        }

        private void ToolboxMouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void ToolboxMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void ToolboxMouseMove(object sender, MouseEventArgs e)
        {
        }

        private void ToolViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ToolWindowViewModel.ToolWindowTitle)) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolWindowTitle)));
        }
    }
}
