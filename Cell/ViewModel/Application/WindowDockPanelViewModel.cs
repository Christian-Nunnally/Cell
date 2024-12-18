using Cell.Core.Common;
using Cell.View.Application;
using Cell.View.ToolWindow;
using Cell.ViewModel.ToolWindow;
using FontAwesome.Sharp;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Cell.ViewModel.Application
{
    /// <summary>
    /// A view model for a window that contains docked tool windows and a main content area.
    /// </summary>
    public class WindowDockPanelViewModel : PropertyChangedBase
    {
        private ToolWindowViewModel? _mainContent;
        private bool _showDockSites;
        private ToolWindowViewModel _windowToDock;
        private ToolWindowViewModel _topWindow;
        private readonly WindowDockPanelViewModel? _parentPanel;
        private CommandViewModel _closeMainContentCommand;
        private CommandViewModel _undockMainContentCommand;
        private CommandViewModel _dockMainContentCommand;
        private double _desiredHeight = 100;
        private double _desiredWidth = 100;
        private WindowDockType _dockType;

        /// <summary>
        /// Gets the observable collection of visible floating tool windows in the application.
        /// </summary>
        public ObservableCollection<WindowDockPanelViewModel> VisibleContentAreasThatAreFloating { get; } = [];

        /// <summary>
        /// Gets the observable collection of visible docked (top) tool windows in the application.
        /// </summary>
        public ObservableCollection<WindowDockPanelViewModel> VisibleContentAreasThatAreDockedOnTop { get; } = [];

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
        /// Gets or sets the height the tool window should give its content.
        /// </summary>
        public double DesiredHeight
        {
            get
            {
                if (DockType == WindowDockType.DockedRight || DockType == WindowDockType.DockedLeft) return double.NaN;
                if (_parentPanel is null) return double.NaN;
                return _desiredHeight;
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
                if (DockType == WindowDockType.DockedTop || DockType == WindowDockType.DockedBottom) return double.NaN;
                if (_parentPanel is null) return double.NaN;
                return _desiredWidth;
            }

            set
            {
                _desiredWidth = Math.Max(value, MainContent?.MinimumWidth ?? 100);
                NotifyPropertyChanged(nameof(DesiredWidth));
            }
        }

        public WindowDockPanelViewModel(WindowDockPanelViewModel? parentWindowDockPanel = null)
        {
            _parentPanel = parentWindowDockPanel;
            _closeMainContentCommand = new CommandViewModel("", CloseMainContent) { Icon = IconChar.Xmark };
            _undockMainContentCommand = new CommandViewModel("", UndockWindow) { Icon = IconChar.LockOpen };
            _dockMainContentCommand = new CommandViewModel("", StartWindowDocking) { Icon = IconChar.Lock };
        }

        private bool IsFloating
        {
            get
            {
                var currentPanel = this;
                while (currentPanel._parentPanel is not null)
                {
                    if (VisibleContentAreasThatAreFloating.Contains(this)) return true;
                    currentPanel = currentPanel._parentPanel;
                }
                return false;
            }
        }

        private void StartWindowDocking()
        {
            WindowToDock = MainContent;
        }

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

        private void CloseMainContent()
        {
            if (_mainContent is null) return;
            RequestClose(_mainContent);
        }

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

        public bool IsTopResizerVisible => _parentPanel is not null && DockType != WindowDockType.DockedTop && DockType != WindowDockType.DockedRight && DockType != WindowDockType.DockedLeft;

        public bool IsBottomResizerVisible => _parentPanel is not null && DockType != WindowDockType.DockedBottom && DockType != WindowDockType.DockedRight && DockType != WindowDockType.DockedLeft;

        public bool IsLeftResizerVisible => _parentPanel is not null && DockType != WindowDockType.DockedTop && DockType != WindowDockType.DockedBottom && DockType != WindowDockType.DockedLeft;

        public bool IsRightResizerVisible => _parentPanel is not null && DockType != WindowDockType.DockedTop && DockType != WindowDockType.DockedBottom && DockType != WindowDockType.DockedRight;

        private void UndockWindow()
        {
            if (MainContent == null) return;
            MainContent.ToolBarCommands.Remove(_undockMainContentCommand);
            MainContent.ToolBarCommands.Remove(_dockMainContentCommand);
            MainContent.ToolBarCommands.Remove(_closeMainContentCommand);
            var newChildWindowDockPanel = new WindowDockPanelViewModel(this);
            newChildWindowDockPanel.DockType = WindowDockType.Floating;
            var mainContent = MainContent;
            MainContent = null;
            newChildWindowDockPanel.MainContent = mainContent;
            //newChildWindowDockPanel.MainContent.ToolBarCommands.Clear();
            //newChildWindowDockPanel.MainContent.ToolBarCommands.Insert(0, newChildWindowDockPanel._dockMainContentCommand);
            //newChildWindowDockPanel.MainContent.ToolBarCommands.Insert(0, newChildWindowDockPanel._closeMainContentCommand);
            RemoveToolWindowWithoutClosingIt(MainContent);
            VisibleContentAreasThatAreFloating.Add(newChildWindowDockPanel);
        }

        public ToolWindowViewModel WindowToDock
        {
            get
            {
                if (_parentPanel is not null) return _parentPanel.WindowToDock;
                return _windowToDock;
            }

            set
            {
                if (_parentPanel is not null)
                {
                    _parentPanel.WindowToDock = value;
                    return;
                }
                if (_windowToDock == value) return;
                _windowToDock = value;
                NotifyPropertyChanged(nameof(WindowToDock));
            }
        }

        public void BringWindowToFront(ToolWindowViewModel viewModel)
        {
            var view = VisibleContentAreasThatAreFloating.FirstOrDefault(x => x.MainContent == viewModel);
            if (view is null) return;
            VisibleContentAreasThatAreFloating.Remove(view);
            VisibleContentAreasThatAreFloating.Add(view);
        }

        internal void MoveToolWindowToTop(ToolWindowViewModel toolWindow)
        {
            //TopWindow = toolWindow;
        }

        internal void RemoveToolWindowWithoutClosingIt(ToolWindowViewModel toolWindowViewModel)
        {
            if (MainContent == toolWindowViewModel)
            {
                MainContent = null;
                if (_parentPanel is not null)
                {
                    //if (!AreAnyWindowsOpen())
                    //{
                        _parentPanel.VisibleContentAreasThatAreFloating.Remove(this);
                        _parentPanel.VisibleContentAreasThatAreDockedOnBottom.Remove(this);
                        _parentPanel.VisibleContentAreasThatAreDockedOnLeft.Remove(this);
                        _parentPanel.VisibleContentAreasThatAreDockedOnRight.Remove(this);
                        _parentPanel.VisibleContentAreasThatAreDockedOnTop.Remove(this);
                    //}
                }
            }
            else
            {
                throw new NotImplementedException("Tool window not found in this window dock panel.");
            }
            //VisibleContentAreasThatAreFloating.Remove(toolWindowViewModel);
        }

        private bool AreAnyWindowsOpen()
        {
            return VisibleContentAreasThatAreDockedOnBottom.Any()
                || VisibleContentAreasThatAreDockedOnLeft.Any()
                || VisibleContentAreasThatAreDockedOnRight.Any()
                || VisibleContentAreasThatAreDockedOnTop.Any()
                || VisibleContentAreasThatAreFloating.Any();
        }

        internal void RemoveToolWindow(ToolWindowViewModel toolWindowViewModel)
        {
            RemoveToolWindowWithoutClosingIt(toolWindowViewModel);
            toolWindowViewModel.HandleBeingClosed();
        }

        private void RequestClose(ToolWindowViewModel viewModel)
        {
            var isAllowingClose = viewModel.HandleCloseRequested();
            if (isAllowingClose)
            {
                RemoveToolWindow(viewModel);
            }
        }

        public WindowDockPanelViewModel? GetPanelContainingToolWindowMatchingTitle(string title)
        {
            var result = VisibleContentAreasThatAreDockedOnRight.FirstOrDefault(x => x.MainContent?.ToolWindowTitle == title);
            if (result is not null) return result;
            result = VisibleContentAreasThatAreDockedOnBottom.FirstOrDefault(x => x.MainContent?.ToolWindowTitle == title);
            if (result is not null) return result;
            result = VisibleContentAreasThatAreDockedOnLeft.FirstOrDefault(x => x.MainContent?.ToolWindowTitle == title);
            if (result is not null) return result;
            result = VisibleContentAreasThatAreDockedOnTop.FirstOrDefault(x => x.MainContent?.ToolWindowTitle == title);
            if (result is not null) return result;
            result = VisibleContentAreasThatAreFloating.FirstOrDefault(x => x.MainContent?.ToolWindowTitle == title);
            return result;
        }

        public void ShowToolWindow(ToolWindowViewModel viewModel, WindowDockType dockType, bool allowDuplicates)
        {
            var existingWindowOfSameType = GetPanelContainingToolWindowMatchingTitle(viewModel.ToolWindowTitle);
            if (existingWindowOfSameType is not null)
            {
                VisibleContentAreasThatAreFloating.Remove(existingWindowOfSameType);
                VisibleContentAreasThatAreFloating.Add(existingWindowOfSameType);
                if (!allowDuplicates) return;
            }
            var childWindowDockPanel = new WindowDockPanelViewModel(this);
            childWindowDockPanel.DockType = dockType;
            childWindowDockPanel.MainContent = viewModel;
            viewModel.RequestClose = () => RequestClose(viewModel);
            viewModel.HandleBeingShown();
            ShowAlreadyOpenedToolWindow(dockType, childWindowDockPanel);
        }

        private void ShowAlreadyOpenedToolWindow(WindowDockType dockType, WindowDockPanelViewModel childWindowDockPanel)
        {
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
                    VisibleContentAreasThatAreFloating.Add(childWindowDockPanel);
                    break;
            }
        }

        public void CloseAllWindows()
        {
            // TODO: close all windows.
        }

        public void DockWindowThatIsCurrentlyFloating(ToolWindowViewModel toolWindowToDock, Dock dockSide)
        {
            toolWindowToDock.HostingPanel?.RemoveToolWindowWithoutClosingIt(toolWindowToDock);

            var hostingPanel = new WindowDockPanelViewModel(this);
            if (dockSide == Dock.Bottom)
            {
                hostingPanel.DockType = WindowDockType.DockedBottom;
            }
            else if (dockSide == Dock.Top)
            {
                hostingPanel.DockType = WindowDockType.DockedTop;
            }
            else if (dockSide == Dock.Left)
            {
                hostingPanel.DockType = WindowDockType.DockedLeft;
            }
            else if (dockSide == Dock.Right)
            {
                hostingPanel.DockType = WindowDockType.DockedRight;
            }
            hostingPanel.MainContent = WindowToDock;

            if (dockSide == Dock.Bottom)
            {
                ShowAlreadyOpenedToolWindow(WindowDockType.DockedBottom, hostingPanel);
                return;
            }
            if (dockSide == Dock.Top)
            {
                ShowAlreadyOpenedToolWindow(WindowDockType.DockedTop, hostingPanel);
                return;
            }
            if (dockSide == Dock.Left)
            {
                ShowAlreadyOpenedToolWindow(WindowDockType.DockedLeft, hostingPanel);
                return;
            }
            if (dockSide == Dock.Right)
            {
                ShowAlreadyOpenedToolWindow(WindowDockType.DockedRight, hostingPanel);
                return;
            }

            WindowToDock = null;
        }
    }
}
