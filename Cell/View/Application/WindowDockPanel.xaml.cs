using Cell.View.Skin;
using Cell.View.ToolWindow;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
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
        private readonly Canvas _toolWindowCanvas;
        private readonly WindowDockPanelViewModel _viewModel;
        private readonly WindowDockPanel? _parentWindowDockPanel;
        public WindowDockPanel(WindowDockPanelViewModel viewModel, Canvas toolWindowCanvas, WindowDockPanel? parentWindowDockPanel = null)
        {
            _parentWindowDockPanel = parentWindowDockPanel;
            _toolWindowCanvas = toolWindowCanvas;
            _viewModel = viewModel;
            viewModel.VisibleContentAreasThatAreFloating.CollectionChanged += FloatingToolWindowsCollectionChanged;
            DataContext = viewModel;
            _viewModel.PropertyChanged += WindowDockPanelViewModelPropertyChanged;
            InitializeComponent();
        }

        public void MoveToolWindowToTop(ToolWindowViewModel toolWindow)
        {
            foreach (var floatingContainer in _toolWindowCanvas.Children.OfType<FloatingToolWindowContainer>())
            {
                if (floatingContainer.ToolWindowContent?.ToolViewModel == toolWindow)
                {
                    _toolWindowCanvas.Children.Remove(floatingContainer);
                    _toolWindowCanvas.Children.Add(floatingContainer);
                    return;
                }
            }
        }

        /// <summary>
        /// Opens a tool window with the specified view model.
        /// </summary>
        /// <param name="viewModel">The tool window view model to open.</param>
        /// <param name="dock">The side to dock to.</param>
        public void ShowToolWindowInDockedContainer(ToolWindowViewModel viewModel, Dock dock)
        {
            var window = ToolWindowViewFactory.Create(viewModel);
            if (window is null) return;
            var childDockWindowViewModel = new WindowDockPanelViewModel(_viewModel);
            var childDockWindow = new WindowDockPanel(childDockWindowViewModel, _toolWindowCanvas, this);
            childDockWindow._viewModel.MainContent = viewModel;
            _toolWindowDockPanel.Children.Insert(_toolWindowDockPanel.Children.Count - 1, childDockWindow);
        }

        public void UpdateToolWindowLocation(double canvasWidth, double canvasHeight)
        {
            foreach (var toolWindow in _toolWindowCanvas?.Children.Cast<FloatingToolWindowContainer>() ?? [])
            {
                toolWindow.HandleOwningCanvasSizeChanged(canvasWidth, canvasHeight);
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
            toolWindowToDock.HostingPanel?.RemoveToolWindowWithoutClosingIt(toolWindowToDock);

            var hostingPanel = new WindowDockPanelViewModel(_viewModel);
            hostingPanel.MainContent = toolWindowToDock;

            if (dockSide == Dock.Bottom)
            {
                _viewModel.VisibleContentAreasThatAreDockedOnBottom.Add(hostingPanel);
                return;
            }
            if (dockSide == Dock.Top)
            {
                _viewModel.VisibleContentAreasThatAreDockedOnTop.Add(hostingPanel);
                return;
            }
            if (dockSide == Dock.Left)
            {
                _viewModel.VisibleContentAreasThatAreDockedOnLeft.Add(hostingPanel);
                return;
            }
            if (dockSide == Dock.Right)
            {
                _viewModel.VisibleContentAreasThatAreDockedOnRight.Add(hostingPanel);
                return;
            }

            _viewModel.WindowToDock = null;
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

        private IEnumerable<WindowDockPanel> GetChildPanels()
        {
            return _toolWindowDockPanel.Children.OfType<WindowDockPanel>();
        }

        private void OpenToolWindowInFloatingContainer(ResizableToolWindow resizableToolWindow)
        {
            var toolbox = new FloatingToolWindowContainer(_viewModel)
            {
                ShowDockOptions = window => ApplicationViewModel.Instance.ShowDockSites(window!)
            };
            if (resizableToolWindow.ToolViewModel.X < 0) resizableToolWindow.ToolViewModel.X = (_toolWindowCanvas.ActualWidth / 2) - (resizableToolWindow.ToolViewModel.DefaultWidth / 2);
            if (resizableToolWindow.ToolViewModel.Y < 0) resizableToolWindow.ToolViewModel.Y = (_toolWindowCanvas.ActualHeight / 2) - (resizableToolWindow.ToolViewModel.DefaultHeight / 2);
            toolbox.ToolWindowContent = resizableToolWindow;
            _toolWindowCanvas.Children.Add(toolbox);
        }

        private void FloatingToolWindowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
            {
                foreach (var floatingToolWindowContainer in e.NewItems.Cast<WindowDockPanelViewModel>())
                {
                    var windowDockPanel = new WindowDockPanel(floatingToolWindowContainer, _toolWindowCanvas, this);
                    _toolWindowCanvas.Children.Add(windowDockPanel);
                }
            }
            //else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is not null)
            //{
            //    foreach (var windowDockPanel in e.OldItems.Cast<WindowDockPanel>())
            //    {
            //        toolWindowViewModel.HostingPanel = null;
            //        RemoveToolWindowFromView(toolWindowViewModel);
            //    }
            //}
        }

        private void RemoveDockedToolWindowView(ToolWindowViewModel toolWindowViewModel)
        {
            if (_viewModel.MainContent == toolWindowViewModel)
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

        private void RemoveFloatingToolWindowView(ToolWindowViewModel toolWindowViewModel)
        {
            foreach (var floatingContainer in _toolWindowCanvas.Children.OfType<FloatingToolWindowContainer>())
            {
                if (floatingContainer.ToolWindowContent?.ToolViewModel == toolWindowViewModel)
                {
                    _toolWindowCanvas.Children.Remove(floatingContainer);
                    return;
                }
            }
        }

        private void RemoveToolWindowFromView(ToolWindowViewModel toolWindowViewModel)
        {
            RemoveDockedToolWindowView(toolWindowViewModel);
            RemoveFloatingToolWindowView(toolWindowViewModel);
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

        private void ShowToolWindowInFloatingContainer(ToolWindowViewModel viewModel)
        {
            var window = ToolWindowViewFactory.Create(viewModel);
            if (window is null) return;
            OpenToolWindowInFloatingContainer(window);
        }

        private void WindowDockPanelViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WindowDockPanelViewModel.WindowToDock))
            {
                if (_viewModel.WindowToDock is null)
                {
                    HideDockSites();
                }
                else
                {
                    ShowDockSites(_viewModel.WindowToDock);
                }
                foreach (var childDockPanel in _toolWindowDockPanel.Children.OfType<WindowDockPanel>())
                {
                    childDockPanel._viewModel.WindowToDock = _viewModel.WindowToDock;
                }
            }
            if (e.PropertyName == nameof(WindowDockPanelViewModel.MainContent))
            {
                ShowMainContentFromViewModel();
            }
        }

        private void ShowMainContentFromViewModel()
        {
            if (_viewModel.MainContent is not null)
            {
                var toolWindowContentView = ToolWindowViewFactory.Create(_viewModel.MainContent);
                var dockedToolWindowContainer = new DockedToolWindowContainer()
                {
                    ToolWindowContent = toolWindowContentView,
                    //Undock = UndockToolWindow,
                };
                _mainContentControl.Content = dockedToolWindowContainer;
            }
            else
            {
                _mainContentControl.Content = null;
            }
        }

        private void OnMainContentControlLoaded(object sender, RoutedEventArgs e)
        {
            ShowMainContentFromViewModel();
        }
    }

    public enum WindowDockType
    {
        DockedRight,
        DockedLeft,
        DockedTop,
        DockedBottom,
        Floating,
    }
}
