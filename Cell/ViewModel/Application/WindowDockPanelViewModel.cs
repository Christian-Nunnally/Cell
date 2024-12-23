using Cell.Core.Common;
using Cell.View.Skin;
using Cell.View.ToolWindow;
using Cell.ViewModel.ToolWindow;
using FontAwesome.Sharp;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Cell.ViewModel.Application
{
    /// <summary>
    /// A view model for a window that contains docked tool windows and a main content area.
    /// </summary>
    public class WindowDockPanelViewModel : PropertyChangedBase
    {
        private readonly CommandViewModel _closeMainContentCommand;
        private readonly CommandViewModel _dockMainContentCommand;
        private readonly CommandViewModel _undockMainContentCommand;
        private double _desiredHeight = 100;
        private double _desiredWidth = 100;
        private WindowDockType _dockType;
        private ToolWindowViewModel? _mainContent;
        private ToolWindowViewModel? _topFloatingWindow;
        private ToolWindowViewModel? _windowToDock;
        /// <summary>
        /// Creates a new instance of a <see cref="WindowDockPanelViewModel"/>.
        /// </summary>
        /// <param name="parentWindowDockPanel">The optional parent if this is hosted inside another dock panel.</param>
        public WindowDockPanelViewModel(WindowDockPanelViewModel? parentWindowDockPanel = null)
        {
            ParentPanel = parentWindowDockPanel;
            _closeMainContentCommand = new CommandViewModel("", CloseMainContent) { Icon = IconChar.Xmark, ToolTip = "Close this tool window" };
            _undockMainContentCommand = new CommandViewModel("", UndockWindow) { Icon = IconChar.LockOpen, ToolTip = "Undocks this tool window" };
            _dockMainContentCommand = new CommandViewModel("", StartWindowDocking) { Icon = IconChar.Lock, ToolTip = "Docks this tool window" };
        }

        /// <summary>
        /// Gets or sets the assumed size fo the canvas that floating windows are on so that they can be positioned somewhat intellegently.
        /// </summary>
        public Size CanvasSize { get; set; }

        /// <summary>
        /// Gets or sets the height the tool window should give its content.
        /// </summary>
        public double DesiredHeight
        {
            get
            {
                if (ShouldHeightBeAutomatic) return double.NaN;

                var sideMinimumHeight = VisibleContentAreasThatAreDockedOnLeft.Concat(VisibleContentAreasThatAreDockedOnRight).Select(x => x.MainContent?.MinimumHeight ?? 0).DefaultIfEmpty(0).Max();
                var minimumHeight = MainContent?.MinimumHeight ?? 0;
                minimumHeight += VisibleContentAreasThatAreDockedOnTop.Select(x => x.DesiredHeight).Sum();
                minimumHeight += VisibleContentAreasThatAreDockedOnBottom.Select(x => x.DesiredHeight).Sum();
                minimumHeight += DockedToolWindowContainer.ToolBoxHeaderHeight;
                minimumHeight += IsBottomResizerVisible ? ApplicationConstants.ToolWindowResizerSize : 0;
                minimumHeight += IsTopResizerVisible ? ApplicationConstants.ToolWindowResizerSize : 0;
                var limitingDiminsion = Math.Max(sideMinimumHeight, minimumHeight);
                var currentMin = Math.Max(_desiredHeight, limitingDiminsion);
                return currentMin;
            }

            set
            {
                _desiredHeight = Math.Max(value, MainContent?.MinimumHeight ?? 100);
                NotifyPropertyChanged(nameof(DesiredHeight));
            }
        }

        /// <summary>
        /// Gets or sets the width the tool window should give its content.
        /// </summary>
        public double DesiredWidth
        {
            get
            {
                if (ShouldWidthBeAutomatic) return double.NaN;

                var sideMinimumWidth = VisibleContentAreasThatAreDockedOnTop.Concat(VisibleContentAreasThatAreDockedOnBottom).Select(x => x.MainContent?.MinimumWidth ?? 0).DefaultIfEmpty(0).Max();
                var minimumWidth = MainContent?.MinimumWidth ?? 0;
                minimumWidth += VisibleContentAreasThatAreDockedOnLeft.Select(x => x.DesiredWidth).Sum();
                minimumWidth += VisibleContentAreasThatAreDockedOnRight.Select(x => x.DesiredWidth).Sum();
                minimumWidth += IsLeftResizerVisible ? ApplicationConstants.ToolWindowResizerSize : 0;
                minimumWidth += IsRightResizerVisible ? ApplicationConstants.ToolWindowResizerSize : 0;
                var limitingDiminsion = Math.Max(sideMinimumWidth, minimumWidth);
                var currentMin = Math.Max(_desiredWidth, limitingDiminsion);
                return currentMin;
            }

            set
            {
                _desiredWidth = Math.Max(value, MainContent?.MinimumWidth ?? 100);
                NotifyPropertyChanged(nameof(DesiredWidth));
            }
        }

        /// <summary>
        /// Gets or sets how this window is docked in its parent (if its _parentPanel is set).
        /// </summary>
        public WindowDockType DockType
        {
            get => _dockType; set
            {
                if (_dockType == value) return;
                _dockType = value;
                NotifyPropertyChanged(nameof(DockType));
                NotifyPropertyChanged(nameof(IsTopResizerVisible));
                NotifyPropertyChanged(nameof(IsBottomResizerVisible));
                NotifyPropertyChanged(nameof(IsLeftResizerVisible));
                NotifyPropertyChanged(nameof(IsRightResizerVisible));
                NotifyPropertyChanged(nameof(DesiredHeight));
                NotifyPropertyChanged(nameof(DesiredWidth));
            }
        }

        /// <summary>
        /// Determine whether the bottom resizer should be visible right now.
        /// </summary>
        public bool IsBottomResizerVisible => ParentPanel is not null && DockType != WindowDockType.DockedBottom && DockType != WindowDockType.DockedRight && DockType != WindowDockType.DockedLeft;

        /// <summary>
        /// Determine whether the left resizer should be visible right now.
        /// </summary>
        public bool IsLeftResizerVisible => ParentPanel is not null && DockType != WindowDockType.DockedTop && DockType != WindowDockType.DockedBottom && DockType != WindowDockType.DockedLeft;

        /// <summary>
        /// Determine whether the right resizer should be visible right now.
        /// </summary>
        public bool IsRightResizerVisible => ParentPanel is not null && DockType != WindowDockType.DockedTop && DockType != WindowDockType.DockedBottom && DockType != WindowDockType.DockedRight;

        /// <summary>
        /// Determine whether the top resizer should be visible right now.
        /// </summary>
        public bool IsTopResizerVisible => ParentPanel is not null && DockType != WindowDockType.DockedTop && DockType != WindowDockType.DockedRight && DockType != WindowDockType.DockedLeft;

        /// <summary>
        /// Gets or sets the tool window in the middle of this window dock panel.
        /// </summary>
        public ToolWindowViewModel? MainContent
        {
            get => _mainContent;
            set
            {
                if (_mainContent == value) return;
                if (_mainContent is not null)
                {
                    _mainContent.HostingPanel = null;
                    _mainContent.RequestClose = null;
                }
                _mainContent = value;
                if (_mainContent is not null)
                {
                    _mainContent.RequestClose = () => RequestClose(_mainContent);
                    DesiredWidth = _mainContent.DefaultWidth;
                    DesiredHeight = _mainContent.DefaultHeight + DockedToolWindowContainer.ToolBoxHeaderHeight;
                    var dockCommand = _mainContent.ToolBarCommands.FirstOrDefault(x => x.Icon == _dockMainContentCommand.Icon);
                    if (dockCommand is not null) _mainContent.ToolBarCommands.Remove(dockCommand);
                    var undockCommand = _mainContent.ToolBarCommands.FirstOrDefault(x => x.Icon == _undockMainContentCommand.Icon);
                    if (undockCommand is not null) _mainContent.ToolBarCommands.Remove(undockCommand);
                    var closeCommand = _mainContent.ToolBarCommands.FirstOrDefault(x => x.Icon == _closeMainContentCommand.Icon);
                    if (closeCommand is not null) _mainContent.ToolBarCommands.Remove(closeCommand);
                    if (DockType == WindowDockType.Floating)
                    {
                        _mainContent.ToolBarCommands.Insert(0, _dockMainContentCommand);
                    }
                    else
                    {
                        _mainContent.ToolBarCommands.Insert(0, _undockMainContentCommand);
                    }
                    _mainContent.ToolBarCommands.Insert(0, _closeMainContentCommand);
                    if (_mainContent.HostingPanel != null) throw new InvalidOperationException("The tool window is already hosted by another panel.");
                    _mainContent.HostingPanel = this;
                }
                NotifyPropertyChanged(nameof(MainContent));
            }
        }

        public WindowDockPanelViewModel? ParentPanel { get; }

        /// <summary>
        /// Sets the windows that should be shown on top. The window must be added to the list of floating windows first.
        /// </summary>
        public ToolWindowViewModel? TopFloatingWindow
        {
            get => _topFloatingWindow;
            set
            {
                if (VisibleContentAreasThatAreFloating.FirstOrDefault(x => x.MainContent == value) is null) throw new InvalidOperationException("The window to set as top is not in the list of floating windows. Add it first.");
                if (_topFloatingWindow == value) return;
                _topFloatingWindow = value;
                NotifyPropertyChanged(nameof(TopFloatingWindow));
            }
        }

        /// <summary>
        /// Gets the observable collection of visible docked (bottom) tool windows in the application.
        /// </summary>
        public ObservableCollection<WindowDockPanelViewModel> VisibleContentAreasThatAreDockedOnBottom { get; } = [];

        /// <summary>
        /// Gets the observable collection of visible docked (left) tool windows in the application.
        /// </summary>
        public ObservableCollection<WindowDockPanelViewModel> VisibleContentAreasThatAreDockedOnLeft { get; } = [];

        /// <summary>
        /// Gets the observable collection of visible docked (right) tool windows in the application.
        /// </summary>
        public ObservableCollection<WindowDockPanelViewModel> VisibleContentAreasThatAreDockedOnRight { get; } = [];

        /// <summary>
        /// Gets the observable collection of visible docked (top) tool windows in the application.
        /// </summary>
        public ObservableCollection<WindowDockPanelViewModel> VisibleContentAreasThatAreDockedOnTop { get; } = [];

        /// <summary>
        /// Gets the observable collection of visible floating tool windows in the application.
        /// </summary>
        public ObservableCollection<WindowDockPanelViewModel> VisibleContentAreasThatAreFloating { get; } = [];

        /// <summary>
        /// Gets or sets the window that is currently being docked. After this is set, calling DockWindowThatIsCurrentlyFloating will actually dock the window.
        /// </summary>
        public ToolWindowViewModel? WindowToDock
        {
            get
            {
                if (ParentPanel is not null) return ParentPanel.WindowToDock;
                return _windowToDock;
            }

            set
            {
                if (ParentPanel is not null)
                {
                    ParentPanel.WindowToDock = value;
                    return;
                }
                if (_windowToDock == value) return;
                _windowToDock = value;
                NotifyPropertyChanged(nameof(WindowToDock));
            }
        }

        private bool IsParentPanel => ParentPanel is null;

        private bool ShouldHeightBeAutomatic => DockType == WindowDockType.DockedRight || DockType == WindowDockType.DockedLeft || IsParentPanel;

        private bool ShouldWidthBeAutomatic => DockType == WindowDockType.DockedTop || DockType == WindowDockType.DockedBottom || IsParentPanel;

        /// <summary>
        /// Attempts to gracefully close all tool windows docked in this panel.
        /// </summary>
        public void CloseAllWindows()
        {
            CloseAllInList(VisibleContentAreasThatAreDockedOnBottom);
            CloseAllInList(VisibleContentAreasThatAreDockedOnTop);
            CloseAllInList(VisibleContentAreasThatAreDockedOnRight);
            CloseAllInList(VisibleContentAreasThatAreDockedOnLeft);
            if (ParentPanel is null) CloseAllInList(VisibleContentAreasThatAreFloating);
        }

        private void CloseAllInList(ObservableCollection<WindowDockPanelViewModel> originalList)
        {
            foreach (var dockedWindow in originalList.ToList())
            {
                dockedWindow.MainContent?.HandleBeingClosed();
                originalList.Remove(dockedWindow);
            }
        }

        /// <summary>
        /// Moves a tool window that is currently floating to a docked position in this panel.
        /// </summary>
        /// <param name="toolWindowToDock">The view model of the floating window you wish to dock.</param>
        /// <param name="dockSide">The side to dock to.</param>
        /// <exception cref="InvalidOperationException">If dockSide is Floating.</exception>
        public void DockWindowThatIsCurrentlyFloating(ToolWindowViewModel toolWindowToDock, WindowDockType dockSide)
        {
            if (dockSide == WindowDockType.Floating) throw new InvalidOperationException("It was not anticipated to go from a floating window to another floating window.");
            toolWindowToDock.HostingPanel?.RemoveToolWindowFromThisPanel(toolWindowToDock);
            var hostingPanel = new WindowDockPanelViewModel(this)
            {
                DockType = dockSide,
                MainContent = WindowToDock
            };
            ShowAlreadyOpenedToolWindow(dockSide, hostingPanel);
            WindowToDock = null;
        }

        /// <summary>
        /// Opens a tool window in the specified dock type.
        /// </summary>
        /// <param name="viewModel">The view model of the tool window to open.</param>
        /// <param name="dockType">The dock position of the window.</param>
        /// <param name="allowDuplicates">Whether to allow the window to be opened.</param>
        public void ShowToolWindow(ToolWindowViewModel viewModel, WindowDockType dockType, bool allowDuplicates)
        {
            var existingWindowOfSameType = GetPanelContainingToolWindowMatchingTitleAndType(viewModel.GetType(), viewModel.ToolWindowTitle);
            if (existingWindowOfSameType is not null && !allowDuplicates) return;
            var childWindowDockPanel = new WindowDockPanelViewModel(this)
            {
                DockType = dockType,
                MainContent = viewModel
            };
            viewModel.RequestClose = () => childWindowDockPanel.RequestClose(viewModel);
            viewModel.HandleBeingShown();
            ShowAlreadyOpenedToolWindow(dockType, childWindowDockPanel);
        }

        internal void CloseToolWindow(ToolWindowViewModel toolWindowViewModel)
        {
            RemoveToolWindowFromThisPanel(toolWindowViewModel);
            toolWindowViewModel.HandleBeingClosed();
        }

        internal void RemoveToolWindowFromThisPanel(ToolWindowViewModel toolWindowViewModel)
        {
            if (MainContent == toolWindowViewModel)
            {
                MainContent = null;
                if (ParentPanel is not null)
                {
                    var nextChildPanel = VisibleContentAreasThatAreDockedOnRight
                        .Concat(VisibleContentAreasThatAreDockedOnLeft)
                        .Concat(VisibleContentAreasThatAreDockedOnBottom)
                        .Concat(VisibleContentAreasThatAreDockedOnTop).FirstOrDefault();
                    if (nextChildPanel is not null)
                    {
                        // todo: next child
                        CloseAllWindows();
                    }
                    else
                    {
                        ParentPanel.VisibleContentAreasThatAreFloating.Remove(this);
                        ParentPanel.VisibleContentAreasThatAreDockedOnBottom.Remove(this);
                        ParentPanel.VisibleContentAreasThatAreDockedOnLeft.Remove(this);
                        ParentPanel.VisibleContentAreasThatAreDockedOnRight.Remove(this);
                        ParentPanel.VisibleContentAreasThatAreDockedOnTop.Remove(this);
                    }
                }
            }
            else
            {
                throw new NotImplementedException("Tool window not found in this window dock panel.");
            }
        }

        private void CenterFloatingWindow(WindowDockPanelViewModel childWindowDockPanel)
        {
            if (childWindowDockPanel.MainContent == null) throw new InvalidOperationException("Unable to center a tool window that has no main content.");
            childWindowDockPanel.MainContent.X = (CanvasSize.Width / 2) - (childWindowDockPanel.DesiredWidth / 2);
            childWindowDockPanel.MainContent.Y = (CanvasSize.Height / 2) - (childWindowDockPanel.DesiredHeight / 2);
        }

        private void CloseMainContent()
        {
            if (_mainContent is null) return;
            RequestClose(_mainContent);
        }

        private WindowDockPanelViewModel? GetPanelContainingToolWindowMatchingTitleAndType(Type type, string title)
        {
            var result = VisibleContentAreasThatAreDockedOnRight.FirstOrDefault(x => x.MainContent?.GetType() == type && x.MainContent?.ToolWindowTitle == title);
            if (result is not null) return result;
            result = VisibleContentAreasThatAreDockedOnBottom.FirstOrDefault(x => x.MainContent?.GetType() == type && x.MainContent?.ToolWindowTitle == title);
            if (result is not null) return result;
            result = VisibleContentAreasThatAreDockedOnLeft.FirstOrDefault(x => x.MainContent?.GetType() == type && x.MainContent?.ToolWindowTitle == title);
            if (result is not null) return result;
            result = VisibleContentAreasThatAreDockedOnTop.FirstOrDefault(x => x.MainContent?.GetType() == type && x.MainContent?.ToolWindowTitle == title);
            if (result is not null) return result;
            result = VisibleContentAreasThatAreFloating.FirstOrDefault(x => x.MainContent?.GetType() == type && x.MainContent?.ToolWindowTitle == title);
            return result;
        }

        private void RequestClose(ToolWindowViewModel? viewModel)
        {
            var isAllowingClose = viewModel?.HandleCloseRequested() ?? throw new CellError("View model can not be null");
            if (isAllowingClose)
            {
                CloseToolWindow(viewModel);
            }
        }

        private void ShowAlreadyOpenedToolWindow(WindowDockType dockType, WindowDockPanelViewModel childWindowDockPanel)
        {
            if (childWindowDockPanel.MainContent == null) throw new InvalidOperationException("Unable to show a tool window that has no main content.");
            switch (dockType)
            {
                case WindowDockType.DockedTop:
                    VisibleContentAreasThatAreDockedOnTop.Add(childWindowDockPanel);
                    break;
                case WindowDockType.DockedBottom:
                    VisibleContentAreasThatAreDockedOnBottom.Add(childWindowDockPanel);
                    break;
                case WindowDockType.DockedLeft:
                    VisibleContentAreasThatAreDockedOnLeft.Add(childWindowDockPanel);
                    break;
                case WindowDockType.DockedRight:
                    VisibleContentAreasThatAreDockedOnRight.Add(childWindowDockPanel);
                    break;
                case WindowDockType.Floating:
                    CenterFloatingWindow(childWindowDockPanel);
                    VisibleContentAreasThatAreFloating.Add(childWindowDockPanel);
                    break;
            }
        }

        private void StartWindowDocking()
        {
            WindowToDock = MainContent;
        }

        private void UndockWindow()
        {
            if (MainContent == null) return;
            MainContent.ToolBarCommands.Remove(_undockMainContentCommand);
            MainContent.ToolBarCommands.Remove(_dockMainContentCommand);
            MainContent.ToolBarCommands.Remove(_closeMainContentCommand);
            var newChildWindowDockPanel = new WindowDockPanelViewModel(this)
            {
                DockType = WindowDockType.Floating
            };
            var mainContent = MainContent;
            MainContent = null;
            newChildWindowDockPanel.MainContent = mainContent;
            RemoveToolWindowFromThisPanel(MainContent);
            CenterFloatingWindow(newChildWindowDockPanel);
            VisibleContentAreasThatAreFloating.Add(newChildWindowDockPanel);
        }
    }
}
