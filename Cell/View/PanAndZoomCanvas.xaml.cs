﻿using Cell.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cell.View
{
    public partial class PanAndZoomCanvas : Canvas
    {
        private readonly Dictionary<CellViewModel, FrameworkElement> _viewModelToViewMap = [];

        private double _xPan;
        private double _yPan;
        public double XPan
        {
            get
            {
                return _xPan;
            }
            private set
            {
                _xPan = value;
            }
        }
        public double YPan
        {
            get
            {
                return _yPan;
            }
            private set
            {
                _yPan = value;
            }
        }

        private MatrixTransform _transform = new();
        private Point _initialMousePosition;
        private bool _dragging;
        private UIElement? _selectedElement;
        private Vector _draggingDelta;
        private Color _backgroundColor = Color.FromArgb(0xFF, 0x33, 0x33, 0x33);

        public PanAndZoomCanvas()
        {
            InitializeComponent();

            PreviewMouseDown += PanAndZoomCanvas_MouseDown;
            PreviewMouseUp += PanAndZoomCanvas_MouseUp;
            PreviewMouseMove += PanAndZoomCanvas_MouseMove;
            MouseWheel += PanAndZoomCanvas_MouseWheel;

            BackgroundColor = _backgroundColor;
        }

        public double Zoomfactor { get; set; } = 1.1;
        public double CurrentZoom { get; set; } = 1.0;

        public Color BackgroundColor
        {
            get { return _backgroundColor; }

            set
            {
                _backgroundColor = value;
                Background = new SolidColorBrush(_backgroundColor);
            }
        }

        private void PanAndZoomCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                _initialMousePosition = _transform.Inverse.Transform(e.GetPosition(this));
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                if (this.Children.Contains((UIElement)e.Source))
                {
                    _selectedElement = (UIElement)e.Source;
                    Point mousePosition = Mouse.GetPosition(this);
                    double x = Canvas.GetLeft(_selectedElement);
                    double y = Canvas.GetTop(_selectedElement);
                    Point elementPosition = new(x, y);
                    _draggingDelta = elementPosition - mousePosition;
                }
                _dragging = true;
            }
        }

        private void PanAndZoomCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragging = false;
            _selectedElement = null;
        }

        private void PanAndZoomCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Point mousePosition = _transform.Inverse.Transform(e.GetPosition(this));
                Vector delta = Point.Subtract(mousePosition, _initialMousePosition);
                var translate = new TranslateTransform(delta.X, delta.Y);
                _transform.Matrix = translate.Value * _transform.Matrix;

                foreach (UIElement child in this.Children)
                {
                    child.RenderTransform = _transform;
                }
            }

            if (_dragging && e.LeftButton == MouseButtonState.Pressed)
            {
                double x = Mouse.GetPosition(this).X;
                double y = Mouse.GetPosition(this).Y;

                if (_selectedElement != null)
                {
                    Canvas.SetLeft(_selectedElement, x + _draggingDelta.X);
                    Canvas.SetTop(_selectedElement,  y + _draggingDelta.Y);
                }
            }
        }

        public void PanCanvasTo(double x, double y)
        {
            XPan = -x;
            YPan = -y;
            ArrangeItemsForPanAndZoom();
        }

        private void ArrangeItemsForPanAndZoom()
        {
            _transform = new MatrixTransform();
            var translate = new TranslateTransform(XPan, YPan);
            _transform.Matrix = translate.Value * _transform.Matrix;
            Matrix scaleMatrix = _transform.Matrix;
            scaleMatrix.ScaleAt(CurrentZoom, CurrentZoom, 0, 0);
            _transform.Matrix = scaleMatrix;
            foreach (UIElement child in Children)
            {
                if (child is FrameworkElement element)
                {
                    if (element.DataContext is CellViewModel cellViewModel)
                    {
                        double sx = cellViewModel.X * CurrentZoom;
                        double sy = cellViewModel.Y * CurrentZoom;
                        SetLeft(child, sx);
                        SetTop(child, sy);
                        element.RenderTransform = _transform;
                    }
                }
            }
        }

        private void PanAndZoomCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scaleFactor = Zoomfactor;
            if (e.Delta < 0)
            {
                scaleFactor = 1.0f / scaleFactor;
                CurrentZoom *= scaleFactor;
            }
            else
            {
                CurrentZoom *= scaleFactor;
            }

            Point mousePostion = e.GetPosition(this);

            Matrix scaleMatrix = _transform.Matrix;
            scaleMatrix.ScaleAt(scaleFactor, scaleFactor, mousePostion.X, mousePostion.Y);
            _transform.Matrix = scaleMatrix;

            foreach (UIElement child in this.Children)
            {
                if (child is FrameworkElement element)
                {
                    if (element.DataContext is CellViewModel cellViewModel)
                    {
                        double x = cellViewModel.X;
                        double y = cellViewModel.Y;

                        double sx = x * CurrentZoom;
                        double sy = y * CurrentZoom;

                        Canvas.SetLeft(child, sx);
                        Canvas.SetTop(child, sy);
                    }
                }
                child.RenderTransform = _transform;
            }
        }

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

                        double sx = x * CurrentZoom;
                        double sy = y * CurrentZoom;

                        Canvas.SetLeft(element, sx);
                        Canvas.SetTop(element, sy);
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
                            double sx = x * CurrentZoom;
                            Canvas.SetLeft(element, sx);
                        }
                        else if (e.PropertyName == nameof(CellViewModel.Y))
                        {
                            element.RenderTransform = _transform;
                            double y = cellViewModel.Y;
                            double sy = y * CurrentZoom;
                            Canvas.SetTop(element, sy);
                        }
                    }
                }
            }
        }
    }
}
