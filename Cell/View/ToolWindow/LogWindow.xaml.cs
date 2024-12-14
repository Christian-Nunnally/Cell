using Cell.View.Application;
using Cell.ViewModel.ToolWindow;
using System.ComponentModel;

namespace Cell.View.ToolWindow
{
    public partial class LogWindow : ResizableToolWindow
    {
        /// <summary>
        /// Creates a new instance of the <see cref="LogWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public LogWindow(LogWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
            viewModel.PropertyChanged += LogWindowViewModelPropertyChanged;
            _logScrollViewer.ScrollToBottom();
        }

        private void LogWindowViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LogWindowViewModel.LogBuffer))
            {
                _logScrollViewer.ScrollToBottom();
            }
        }
    }
}
