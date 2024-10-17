using Cell.ViewModel.Cells;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cell.View.Application
{
    public partial class PanAndZoomCanvas : Canvas
    {
        private const double _zoomfactor = 1.15;
        private readonly Dictionary<CellViewModel, FrameworkElement> _viewModelToViewMap = [];
        private double _currentZoom = 1.0;
        private Point _initialMousePosition;
        private bool _isLockedToCenter = true;
        private MatrixTransform _transform = new();
        private double _xPan;
        private double _yPan;
        /// <summary>
        /// Creates a new instance of the <see cref="PanAndZoomCanvas"/>.
        /// </summary>
        public PanAndZoomCanvas()
        {
            InitializeComponent();

            PreviewMouseDown += OnPanAndZoomCanvasMouseDown;
            PreviewMouseUp += OnPanAndZoomCanvasMouseUp;
            PreviewMouseMove += OnPanAndZoomCanvasMouseMove;
            MouseWheel += OnPanAndZoomCanvasMouseWheel;
            SizeChanged += PanAndZoomCanvasSizeChanged;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the canvas children should automatically pan to be centered in the view.
        /// </summary>
        public bool IsLockedToCenter
        {
            get => _isLockedToCenter; set
            {
                _isLockedToCenter = value;
                PanSheetToCenter();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether panning the canvas is allowed.
        /// </summary>
        public bool IsPanningEnabled { get; set; } = true;

        /// <summary>
        /// Gets the height of the canvas after it has been laid out.
        /// </summary>
        public double LaidOutHeight { get; internal set; }

        /// <summary>
        /// Gets the width of the canvas after it has been laid out.
        /// </summary>
        public double LaidOutWidth { get; internal set; }

        /// <summary>
        /// Pan the canvas to the specified coordinates.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public void PanCanvasTo(double x, double y)
        {
            _xPan = -x;
            _yPan = -y;
            ArrangeItemsForPanAndZoom();
        }

        /// <summary>
        /// Occurs when the visual children of a <see cref="PanAndZoomCanvas"/> change.
        /// </summary>
        /// <param name="visualAdded">The added visual if any.</param>
        /// <param name="visualRemoved">The removed visual if any.</param>
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (visualAdded != null)
            {
                if (visualAdded is FrameworkElement element)
                {
                    element.RenderTransform = _transform;
                    if (element.DataContext is CellViewModel cellViewModel)
                    {
                        if (_viewModelToViewMap.TryAdd(cellViewModel, element))
                        {
                            cellViewModel.PropertyChanged += OnCellViewModelPropertyChanged;
                        }
                        double x = cellViewModel.X;
                        double y = cellViewModel.Y;

                        double sx = x * _currentZoom;
                        double sy = y * _currentZoom;

                        SetLeft(element, sx);
                        SetTop(element, sy);
                    }
                }
            }
            if (visualRemoved != null)
            {
                if (visualRemoved is FrameworkElement element)
                {
                    if (element.DataContext is CellViewModel cellViewModel)
                    {
                        _viewModelToViewMap.Remove(cellViewModel);
                        cellViewModel.PropertyChanged -= OnCellViewModelPropertyChanged;
                    }
                }
            }
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        private void ArrangeItemsForPanAndZoom()
        {
            _transform = new MatrixTransform();
            var translate = new TranslateTransform(_xPan, _yPan);
            _transform.Matrix = translate.Value * _transform.Matrix;
            Matrix scaleMatrix = _transform.Matrix;
            scaleMatrix.ScaleAt(_currentZoom, _currentZoom, 0, 0);
            _transform.Matrix = scaleMatrix;
            foreach (UIElement child in Children)
            {
                if (child is FrameworkElement element)
                {
                    if (element.DataContext is CellViewModel cellViewModel)
                    {
                        double sx = cellViewModel.X * _currentZoom;
                        double sy = cellViewModel.Y * _currentZoom;
                        SetLeft(child, sx);
                        SetTop(child, sy);
                        element.RenderTransform = _transform;
                    }
                }
            }
        }

        private void OnCellViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellViewModel.X) || e.PropertyName == nameof(CellViewModel.Y))
            {
                if (sender is CellViewModel cellViewModel)
                {
                    if (_viewModelToViewMap.TryGetValue(cellViewModel, out var element))
                    {
                        if (e.PropertyName == nameof(CellViewModel.X))
                        {
                            element.RenderTransform = _transform;
                            double x = cellViewModel.X;
                            double sx = x * _currentZoom;
                            SetLeft(element, sx);
                        }
                        else if (e.PropertyName == nameof(CellViewModel.Y))
                        {
                            element.RenderTransform = _transform;
                            double y = cellViewModel.Y;
                            double sy = y * _currentZoom;
                            SetTop(element, sy);
                        }
                    }
                }
            }
        }

        private void OnPanAndZoomCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not PanAndZoomCanvas canvas) return;
            if (e.ChangedButton == MouseButton.Right)
            {
                _initialMousePosition = _transform.Inverse.Transform(e.GetPosition(this));
                Mouse.Capture(canvas);
            }
        }

        private void OnPanAndZoomCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (!IsPanningEnabled) return;
                Point mousePosition = _transform.Inverse.Transform(e.GetPosition(this));
                Vector delta = Point.Subtract(mousePosition, _initialMousePosition);

                var translate = new TranslateTransform(delta.X, delta.Y);
                _transform.Matrix = translate.Value * _transform.Matrix;
                e.Handled = true;
            }
        }

        private void OnPanAndZoomCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                Mouse.Capture(null);
            }
        }

        private void OnPanAndZoomCanvasMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scaleFactor = _zoomfactor;
            if (e.Delta < 0) scaleFactor = 1.0f / scaleFactor;
            if (IsPanningEnabled) ZoomCanvas(e.GetPosition(this), scaleFactor);
            else ZoomCanvas(new Point(ActualWidth / 2, ActualHeight / 2), scaleFactor);
        }

        private void PanAndZoomCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            EnsureCenteredIfLocked();
        }

        public void EnsureCenteredIfLocked()
        {
            if (IsLockedToCenter) PanSheetToCenter();
        }

        private void PanSheetToCenter()
        {
            if (LaidOutWidth == 0 || LaidOutHeight == 0) return;
            if (ActualWidth == 0 || ActualHeight == 0) return;
            var horizontialCenter = LaidOutWidth / 2 - ActualWidth / _currentZoom / 2;
            var verticalCenter = LaidOutHeight / 2 - ActualHeight / _currentZoom / 2;
            PanCanvasTo(horizontialCenter, verticalCenter);
        }

        private void ZoomCanvas(Point centerOfZoom, double scaleFactor)
        {
            _currentZoom *= scaleFactor;
            Matrix scaleMatrix = _transform.Matrix;
            scaleMatrix.ScaleAt(scaleFactor, scaleFactor, centerOfZoom.X, centerOfZoom.Y);
            _transform.Matrix = scaleMatrix;

            foreach (UIElement child in Children)
            {
                if (child is FrameworkElement element)
                {
                    if (element.DataContext is CellViewModel cellViewModel)
                    {
                        double x = cellViewModel.X;
                        double y = cellViewModel.Y;

                        double sx = x * _currentZoom;
                        double sy = y * _currentZoom;

                        SetLeft(child, sx);
                        SetTop(child, sy);
                    }
                }
            }
        }
    }
}
