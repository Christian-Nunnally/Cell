﻿using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    public partial class DockedToolWindowContainer : UserControl, INotifyPropertyChanged
    {
        private double _contentHeight;
        private double _contentWidth;
        private bool _resizing;
        private Point _resizingStartPosition;
        private ResizableToolWindow? _resizableToolWindow;
        /// <summary>
        /// The extra height from the tool box header.
        /// </summary>
        public static double ToolBoxHeaderHeight = 20;
        private double _contentHeightMinimum;
        private double _contentWidthMinimum;

        /// <summary>
        /// Creates a new instance of the <see cref="DockedToolWindowContainer"/>.
        /// </summary>
        public DockedToolWindowContainer()
        {
            InitializeComponent();
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
        public double ContentHeightMinimum
        {
            get => _contentHeightMinimum;
            set
            {
                if (_contentHeightMinimum != value)
                _contentHeightMinimum = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContentHeightMinimum)));
            }
        }

        /// <summary>
        /// Gets or sets the width the tool window should give its contentMinimum.
        /// </summary>
        public double ContentWidthMinimum
        {
            get => _contentWidthMinimum;
            set
            {
                if (_contentWidthMinimum != value)
                _contentWidthMinimum = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContentWidthMinimum)));
            }
        }

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
        /// Gets whether the resizer should appear in the bottom right (true) or top left (false).
        /// </summary>
        public bool IsDockedOnTopOrLeft => DockPanel.GetDock(this) == Dock.Top || DockPanel.GetDock(this) == Dock.Left;

        /// <summary>
        /// Gets whether the resizer should appear on the left and right of the tool window.
        /// </summary>
        public bool AreSideResizersVisible => DockPanel.GetDock(this) == Dock.Left || DockPanel.GetDock(this) == Dock.Right;

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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AreSideResizersVisible)));
                    ContentWidth = _resizableToolWindow.ToolViewModel.DefaultWidth;
                    ContentHeight = _resizableToolWindow.ToolViewModel.DefaultHeight;
                    ContentWidthMinimum = _resizableToolWindow.ToolViewModel.MinimumWidth;
                    ContentHeightMinimum = _resizableToolWindow.ToolViewModel.MinimumHeight;

                    SetSizeWhileRespectingBounds(ContentWidth, ContentHeight);
                    DataContext = this;
                    foreach (var command in _resizableToolWindow.ToolViewModel.ToolBarCommands)
                    {
                        Commands.Add(command);
                    }
                    _resizableToolWindow.ToolViewModel.ToolBarCommands.CollectionChanged += ToolBarCommandsCollectionChanged;
                    _resizableToolWindow.ToolViewModel.PropertyChanged += ToolViewModelPropertyChanged;
                }
            }
        }

        private void ToolBarCommandsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Commands.Clear();
            foreach (var command in _resizableToolWindow?.ToolViewModel.ToolBarCommands ?? [])
            {
                Commands.Add(command);
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
                if (!IsDockedOnTopOrLeft)
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
            var minimumWidth = ToolWindowContent?.ToolViewModel?.MinimumWidth ?? 50;
            var minimumHeight = ToolWindowContent?.ToolViewModel?.MinimumHeight ?? 50;
            var dock = DockPanel.GetDock(this);
            if (dock == Dock.Left || dock == Dock.Right)
            {
                var boundedWidth = Math.Max(minimumWidth, Math.Min(1000, desiredWidth));
                ContentWidth = boundedWidth;
                ContentHeight = double.NaN;
            }
            else
            {
                var boundedHeight = Math.Max(minimumHeight, Math.Min(1000, desiredHeight));
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
