using Cell.View.Skin;
using Cell.View.ToolWindow;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.Application
{
    /// <summary>
    /// Interaction logic for WindowDockPanel.xaml
    /// </summary>
    public partial class WindowDockPanel : UserControl
    {
        private readonly WindowDockPanel? _parentWindowDockPanel;
        private readonly Canvas _toolWindowCanvas;
        private bool _resizing;
        private Point _resizingStartPosition;
        /// <summary>
        /// A view model for the window dock panel view model.
        /// </summary>
        /// <param name="viewModel">View model for this view.</param>
        /// <param name="toolWindowCanvas">The canvas view to add floating windows to. This is shared between all floating windows in the tree.</param>
        /// <param name="parentWindowDockPanel">Optionally, the parent window dock panel view that created this one and manages its lifecycle.</param>
        public WindowDockPanel(WindowDockPanelViewModel viewModel, Canvas toolWindowCanvas, WindowDockPanel? parentWindowDockPanel = null)
        {
            _parentWindowDockPanel = parentWindowDockPanel;
            _toolWindowCanvas = toolWindowCanvas;
            ViewModel = viewModel;
            viewModel.VisibleContentAreasThatAreFloating.CollectionChanged += FloatingToolWindowsCollectionChanged;
            viewModel.VisibleContentAreasThatAreDockedOnTop.CollectionChanged += TopDockedToolWindowsCollectionChanged;
            viewModel.VisibleContentAreasThatAreDockedOnBottom.CollectionChanged += BottomDockedToolWindowsCollectionChanged;
            viewModel.VisibleContentAreasThatAreDockedOnLeft.CollectionChanged += LeftDockedToolWindowsCollectionChanged;
            viewModel.VisibleContentAreasThatAreDockedOnRight.CollectionChanged += RightDockedToolWindowsCollectionChanged;
            DataContext = viewModel;
            ViewModel.PropertyChanged += WindowDockPanelViewModelPropertyChanged;
            InitializeComponent();
        }

        /// <summary>
        /// The view model for this view.
        /// </summary>
        public WindowDockPanelViewModel ViewModel { get; }

        public void UpdateToolWindowLocation(double canvasWidth, double canvasHeight)
        {
            foreach (var toolWindow in _toolWindowCanvas?.Children.Cast<FloatingToolWindowContainer>() ?? [])
            {
                toolWindow.HandleOwningCanvasSizeChanged(canvasWidth, canvasHeight);
            }
        }

        private static Point DifferenceBetweenTwoPoints(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);

        private void BottomDockedToolWindowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
            {
                foreach (var windowDockPanelViewModel in e.NewItems.Cast<WindowDockPanelViewModel>())
                {
                    var windowDockPanelView = new WindowDockPanel(windowDockPanelViewModel, _toolWindowCanvas, this);
                    DockPanel.SetDock(windowDockPanelView, Dock.Bottom);
                    _toolWindowDockPanel.Children.Insert(_toolWindowDockPanel.Children.Count - 1, windowDockPanelView);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is not null)
            {
                foreach (var windowDockPanelViewModel in e.OldItems.Cast<WindowDockPanelViewModel>())
                {
                    var floatingToolWindowToRemove = _toolWindowDockPanel.Children.OfType<WindowDockPanel>().FirstOrDefault(x => x.ViewModel == windowDockPanelViewModel);
                    _toolWindowDockPanel.Children.Remove(floatingToolWindowToRemove);
                }
            }
        }

        private Border CreateDockSiteBorder(ToolWindowViewModel windowToDock)
        {
            var border = new Border();
            border.MouseEnter += DockSiteMouseEnter;
            border.MouseLeave += DockSiteMouseLeave;
            border.Background = ColorConstants.ForegroundColorConstantBrush;
            border.MouseDown += DockSiteMouseDown;
            border.Tag = windowToDock;
            return border;
        }

        private void DockSiteMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dockSite = (Border)sender!;
            var dockSide = DockPanel.GetDock(dockSite);
            HideDockSitesFromParent();
            if (dockSite.Tag is not ToolWindowViewModel toolWindowToDock) return;

            ViewModel.DockWindowThatIsCurrentlyFloating(toolWindowToDock, dockSide);
        }

        private void DockSiteMouseEnter(object sender, MouseEventArgs e)
        {
            var border = (Border)sender!;
            border.Background = ColorConstants.AccentColorConstantBrush;
        }

        private void DockSiteMouseLeave(object sender, MouseEventArgs e)
        {
            var border = (Border)sender!;
            border.Background = ColorConstants.ForegroundColorConstantBrush;
        }

        private void FloatingToolWindowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
            {
                foreach (var windowDockPanelViewModel in e.NewItems.Cast<WindowDockPanelViewModel>())
                {
                    var floatingToolWindow = new FloatingToolWindowContainer(ViewModel);
                    _toolWindowCanvas.Children.Add(floatingToolWindow);
                    var windowDockPanel = new WindowDockPanel(windowDockPanelViewModel, _toolWindowCanvas, this);
                    floatingToolWindow.WindowDockPanel = windowDockPanel;
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is not null)
            {
                foreach (var windowDockPanelViewModel in e.OldItems.Cast<WindowDockPanelViewModel>())
                {
                    var floatingToolWindowToRemove = _toolWindowCanvas.Children.OfType<FloatingToolWindowContainer>().FirstOrDefault(x => x.WindowDockPanel?.ViewModel == windowDockPanelViewModel);
                    _toolWindowCanvas.Children.Remove(floatingToolWindowToRemove);
                }
            }
        }

        private IEnumerable<WindowDockPanel> GetChildPanels()
        {
            return _toolWindowDockPanel.Children.OfType<WindowDockPanel>();
        }

        private void HideDockSites()
        {
            foreach (var dockSite in _toolWindowDockPanel.Children.OfType<Border>().Where(x => x.Tag is ToolWindowViewModel).ToList())
            {
                _toolWindowDockPanel.Children.Remove(dockSite);
            }

            foreach (var childDockPanel in GetChildPanels())
            {
                childDockPanel.HideDockSites();
            }
        }

        private void HideDockSitesFromParent()
        {
            if (_parentWindowDockPanel is not null)
            {
                _parentWindowDockPanel.HideDockSitesFromParent();
            }
            else
            {
                HideDockSites();
            }
        }

        private void LeftDockedToolWindowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
            {
                foreach (var windowDockPanelViewModel in e.NewItems.Cast<WindowDockPanelViewModel>())
                {
                    var windowDockPanelView = new WindowDockPanel(windowDockPanelViewModel, _toolWindowCanvas, this);
                    DockPanel.SetDock(windowDockPanelView, Dock.Left);
                    _toolWindowDockPanel.Children.Insert(0, windowDockPanelView);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is not null)
            {
                foreach (var windowDockPanelViewModel in e.OldItems.Cast<WindowDockPanelViewModel>())
                {
                    var floatingToolWindowToRemove = _toolWindowDockPanel.Children.OfType<WindowDockPanel>().FirstOrDefault(x => x.ViewModel == windowDockPanelViewModel);
                    _toolWindowDockPanel.Children.Remove(floatingToolWindowToRemove);
                }
            }
        }

        private void OnMainContentControlLoaded(object sender, RoutedEventArgs e)
        {
            ShowMainContentFromViewModel();
        }

        private void RemoveDockedToolWindowView(ToolWindowViewModel toolWindowViewModel)
        {
            if (ViewModel.MainContent == toolWindowViewModel)
            {
                // TODO: make the docked child the new center panel and don't remove it if there is a child.
                _parentWindowDockPanel?._toolWindowDockPanel.Children.Remove(this);
                return;
            }
            foreach (var childPanel in GetChildPanels().ToList())
            {
                childPanel.RemoveDockedToolWindowView(toolWindowViewModel);
            }
        }

        private void ResizerRectangleMouseDown(object sender, MouseButtonEventArgs e)
        {
            _resizing = true;
            _resizingStartPosition = Mouse.GetPosition(App.Current.MainWindow);
            Mouse.Capture(sender as IInputElement);
            e.Handled = true;
        }

        private void ResizerRectangleMouseMove(object sender, MouseEventArgs e)
        {
            if (_resizing)
            {
                var mousePosition = Mouse.GetPosition(App.Current.MainWindow);
                var delta = DifferenceBetweenTwoPoints(_resizingStartPosition, mousePosition);
                if (ViewModel.DockType == WindowDockType.DockedTop || ViewModel.DockType == WindowDockType.DockedLeft)
                {
                    ViewModel.DesiredWidth -= delta.X;
                    ViewModel.DesiredHeight -= delta.Y;
                }
                else
                {
                    ViewModel.DesiredWidth += delta.X;
                    ViewModel.DesiredHeight += delta.Y;
                }
                _resizingStartPosition = mousePosition;
                e.Handled = true;
            }
        }

        private void ResizerRectangleMouseUp(object sender, MouseButtonEventArgs e)
        {
            _resizing = false;
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void RightDockedToolWindowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
            {
                foreach (var windowDockPanelViewModel in e.NewItems.Cast<WindowDockPanelViewModel>())
                {
                    var windowDockPanelView = new WindowDockPanel(windowDockPanelViewModel, _toolWindowCanvas, this);
                    DockPanel.SetDock(windowDockPanelView, Dock.Right);
                    _toolWindowDockPanel.Children.Insert(_toolWindowDockPanel.Children.Count - 1, windowDockPanelView);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is not null)
            {
                foreach (var windowDockPanelViewModel in e.OldItems.Cast<WindowDockPanelViewModel>())
                {
                    var floatingToolWindowToRemove = _toolWindowDockPanel.Children.OfType<WindowDockPanel>().FirstOrDefault(x => x.ViewModel == windowDockPanelViewModel);
                    _toolWindowDockPanel.Children.Remove(floatingToolWindowToRemove);
                }
            }
        }

        private void ShowDockSites(ToolWindowViewModel windowToDock)
        {
            var topDock = CreateDockSiteBorder(windowToDock);
            DockPanel.SetDock(topDock, Dock.Top);
            topDock.Height = 10;
            _toolWindowDockPanel.Children.Insert(_toolWindowDockPanel.Children.Count - 1, topDock);

            var bottomDock = CreateDockSiteBorder(windowToDock);
            DockPanel.SetDock(bottomDock, Dock.Bottom);
            bottomDock.Height = 10;
            _toolWindowDockPanel.Children.Insert(_toolWindowDockPanel.Children.Count - 1, bottomDock);

            var leftDock = CreateDockSiteBorder(windowToDock);
            DockPanel.SetDock(leftDock, Dock.Left);
            leftDock.Width = 10;
            _toolWindowDockPanel.Children.Insert(_toolWindowDockPanel.Children.Count - 1, leftDock);

            var rightDock = CreateDockSiteBorder(windowToDock);
            DockPanel.SetDock(rightDock, Dock.Right);
            rightDock.Width = 10;
            _toolWindowDockPanel.Children.Insert(_toolWindowDockPanel.Children.Count - 1, rightDock);

            foreach (var childDockPanel in _toolWindowDockPanel.Children.OfType<WindowDockPanel>())
            {
                childDockPanel.ShowDockSites(windowToDock);
            }
        }

        private void ShowMainContentFromViewModel()
        {
            if (ViewModel.MainContent is not null)
            {
                var toolWindowContentView = ToolWindowViewFactory.Create(ViewModel.MainContent);
                var dockedToolWindowContainer = new DockedToolWindowContainer()
                {
                    ToolWindowContent = toolWindowContentView,
                };
                _mainContentControl.Content = dockedToolWindowContainer;
            }
            else
            {
                _mainContentControl.Content = null;
            }
        }

        private void TopDockedToolWindowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
            {
                foreach (var windowDockPanelViewModel in e.NewItems.Cast<WindowDockPanelViewModel>())
                {
                    var windowDockPanelView = new WindowDockPanel(windowDockPanelViewModel, _toolWindowCanvas, this);
                    DockPanel.SetDock(windowDockPanelView, Dock.Top);
                    _toolWindowDockPanel.Children.Insert(0, windowDockPanelView);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is not null)
            {
                foreach (var windowDockPanelViewModel in e.OldItems.Cast<WindowDockPanelViewModel>())
                {
                    var floatingToolWindowToRemove = _toolWindowDockPanel.Children.OfType<WindowDockPanel>().FirstOrDefault(x => x.ViewModel == windowDockPanelViewModel);
                    _toolWindowDockPanel.Children.Remove(floatingToolWindowToRemove);
                }
            }
        }

        private void WindowDockPanelViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WindowDockPanelViewModel.WindowToDock))
            {
                if (ViewModel.WindowToDock is null)
                {
                    HideDockSites();
                }
                else
                {
                    ShowDockSites(ViewModel.WindowToDock);
                }
                foreach (var childDockPanel in _toolWindowDockPanel.Children.OfType<WindowDockPanel>())
                {
                    childDockPanel.ViewModel.WindowToDock = ViewModel.WindowToDock;
                }
            }
            if (e.PropertyName == nameof(WindowDockPanelViewModel.MainContent))
            {
                ShowMainContentFromViewModel();
            }
        }
    }
}
