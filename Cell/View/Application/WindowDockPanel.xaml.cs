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
    public enum WindowDockType
    {
        DockedRight,
        DockedLeft,
        DockedTop,
        DockedBottom,
        Floating,
    }

    /// <summary>
    /// Interaction logic for WindowDockPanel.xaml
    /// </summary>
    public partial class WindowDockPanel : UserControl
    {
        private readonly WindowDockPanel? _parentWindowDockPanel;
        private readonly Canvas _toolWindowCanvas;
        private bool _resizing;
        private Point _resizingStartPosition;

        public WindowDockPanel(WindowDockPanelViewModel viewModel, Canvas toolWindowCanvas, WindowDockPanel? parentWindowDockPanel = null)
        {
            _parentWindowDockPanel = parentWindowDockPanel;
            _toolWindowCanvas = toolWindowCanvas;
            ViewModel = viewModel;
            viewModel.VisibleContentAreasThatAreFloating.CollectionChanged += FloatingToolWindowsCollectionChanged;
            DataContext = viewModel;
            ViewModel.PropertyChanged += WindowDockPanelViewModelPropertyChanged;
            InitializeComponent();
        }

        public WindowDockPanelViewModel ViewModel { get; }

        public void MoveToolWindowToTop(ToolWindowViewModel toolWindow)
        {
            throw new NotImplementedException();
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

            var hostingPanel = new WindowDockPanelViewModel(ViewModel);
            hostingPanel.MainContent = toolWindowToDock;

            if (dockSide == Dock.Bottom)
            {
                ViewModel.VisibleContentAreasThatAreDockedOnBottom.Add(hostingPanel);
                return;
            }
            if (dockSide == Dock.Top)
            {
                ViewModel.VisibleContentAreasThatAreDockedOnTop.Add(hostingPanel);
                return;
            }
            if (dockSide == Dock.Left)
            {
                ViewModel.VisibleContentAreasThatAreDockedOnLeft.Add(hostingPanel);
                return;
            }
            if (dockSide == Dock.Right)
            {
                ViewModel.VisibleContentAreasThatAreDockedOnRight.Add(hostingPanel);
                return;
            }

            ViewModel.WindowToDock = null;
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

        private void RemoveFloatingToolWindowView(ToolWindowViewModel toolWindowViewModel)
        {
            throw new NotImplementedException();
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

        private void ShowMainContentFromViewModel()
        {
            if (ViewModel.MainContent is not null)
            {
                var toolWindowContentView = ToolWindowViewFactory.Create(ViewModel.MainContent);
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

        private void ResizerRectangleMouseDown(object sender, MouseButtonEventArgs e)
        {
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
                var desiredWidth = ViewModel.DesiredWidth - delta.X;
                var desiredHeight = ViewModel.DesiredHeight - delta.Y;
                ViewModel.DesiredWidth = desiredWidth;
                ViewModel.DesiredHeight = desiredHeight;
                //ViewModel.MainContent.SetSizeWhileRespectingBounds(desiredWidth, desiredHeight);
                _resizingStartPosition = mousePosition;
                e.Handled = true;
            }
        }

        private static Point DifferenceBetweenTwoPoints(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);

        private void ResizerRectangleMouseUp(object sender, MouseButtonEventArgs e)
        {
            _resizing = false;
            Mouse.Capture(null);
            e.Handled = true;
        }
    }
}
