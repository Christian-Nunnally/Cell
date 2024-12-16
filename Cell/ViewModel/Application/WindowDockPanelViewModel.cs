using Cell.Core.Common;
using Cell.View.Application;
using Cell.View.ToolWindow;
using Cell.ViewModel.ToolWindow;
using FontAwesome.Sharp;
using System.Collections.ObjectModel;
using System.Windows.Controls;

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
        private WindowDockPanelViewModel? _parentPanel;
        private CommandViewModel _closeMainContentCommand;
        private CommandViewModel _undockMainContentCommand;
        private CommandViewModel _dockMainContentCommand;
        private double _desiredHeight = 100;
        private double _desiredWidth = 100;

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
                    _mainContent.RequestClose = null;
                }
                _mainContent = value;
                if (_mainContent is not null)
                {
                    _mainContent.RequestClose = () => RequestClose(_mainContent);
                    _mainContent.ToolBarCommands.Add(_closeMainContentCommand);
                    _mainContent.ToolBarCommands.Add(_undockMainContentCommand);
                }
                NotifyPropertyChanged(nameof(MainContent));
            }
        }

        private void UndockWindow()
        {
            if (MainContent == null) return;
            MainContent.ToolBarCommands.Remove(_undockMainContentCommand);
            MainContent.ToolBarCommands.Remove(_dockMainContentCommand);
            MainContent.ToolBarCommands.Remove(_closeMainContentCommand);
            var newChildWindowDockPanel = new WindowDockPanelViewModel(this);
            newChildWindowDockPanel.MainContent = MainContent;
            newChildWindowDockPanel.MainContent.ToolBarCommands.Clear();
            newChildWindowDockPanel.MainContent.ToolBarCommands.Add(newChildWindowDockPanel._closeMainContentCommand);
            newChildWindowDockPanel.MainContent.ToolBarCommands.Add(newChildWindowDockPanel._dockMainContentCommand);
            VisibleContentAreasThatAreFloating.Add(newChildWindowDockPanel);
            MainContent = null;
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
                    if (!AreAnyWindowsOpen())
                    {
                        _parentPanel.VisibleContentAreasThatAreFloating.Remove(this);
                        _parentPanel.VisibleContentAreasThatAreDockedOnBottom.Remove(this);
                        _parentPanel.VisibleContentAreasThatAreDockedOnLeft.Remove(this);
                        _parentPanel.VisibleContentAreasThatAreDockedOnRight.Remove(this);
                        _parentPanel.VisibleContentAreasThatAreDockedOnTop.Remove(this);
                    }
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

        public void ShowToolWindow(ToolWindowViewModel viewModel, WindowDockType dockType, bool allowDuplicates)
        {
            var existingWindowOfSameType = VisibleContentAreasThatAreFloating.FirstOrDefault(x => viewModel.GetType() == x.GetType());
            if (existingWindowOfSameType is not null)
            {
                VisibleContentAreasThatAreFloating.Remove(existingWindowOfSameType);
                VisibleContentAreasThatAreFloating.Add(existingWindowOfSameType);
                if (!allowDuplicates) return;
            }
            viewModel.RequestClose = () => RequestClose(viewModel);
            viewModel.HandleBeingShown();
            //VisibleContentAreasThatAreFloating.Add(viewModel);
        }

        public void CloseAllWindows()
        {
            // TODO: close all windows.
        }
    }
}
