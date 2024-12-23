using Cell.View.Cells;
using Cell.ViewModel.Cells;
using Cell.ViewModel.ToolWindow;
using System.ComponentModel;

namespace Cell.View.ToolWindow
{
    public partial class SheetToolWindow : ResizableToolWindow
    {
        private readonly SheetToolWindowViewModel _viewModel;

        /// <summary>
        /// Creates a new instance of the <see cref="SheetToolWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public SheetToolWindow(SheetToolWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
            viewModel.PropertyChanged += SheetToolWindowViewModelPropertyChanged;
            ShowSheetView(_viewModel.SheetViewModel);
            
        }

        private void ShowSheetView(SheetViewModel? sheetViewModel)
        {
            if (sheetViewModel is null) return;
            if (!_viewModel.SheetViewCache.TryGetValue(sheetViewModel, out var sheetView))
            {
                sheetView = new SheetView(sheetViewModel);
                sheetView.IsPanningEnabled = false;
                _viewModel.SheetViewCache.Add(sheetViewModel, sheetView);
            }
            _sheetViewContentControl.Content = sheetView;
        }

        private void SheetToolWindowViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SheetToolWindowViewModel.SheetViewModel))
            {
                ShowSheetView(_viewModel.SheetViewModel);
            }
        }
    }
}
