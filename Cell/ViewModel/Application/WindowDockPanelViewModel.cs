using Cell.Core.Common;
using Cell.View.ToolWindow;
using Cell.ViewModel.ToolWindow;
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

        public WindowDockPanelViewModel(WindowDockPanelViewModel? parentWindowDockPanel = null)
        {
            _parentPanel = parentWindowDockPanel;
        }

        /// <summary>
        /// Gets the observable collection of open tool windows in the application.
        /// </summary>
        public ToolWindowViewModel? MainContent
        {
            get => _mainContent; set
            {
                if (_mainContent == value) return;
                _mainContent = value;
                NotifyPropertyChanged(nameof(MainContent));
            }
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
            var view = VisibleContentAreasThatAreFloating.FirstOrDefault(x => x.MainContent is FloatingToolWindowContainer toolWindow && toolWindow.ToolWindowContent.ToolViewModel == viewModel);
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
            //VisibleContentAreasThatAreFloating.Remove(toolWindowViewModel);
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
