using Cell.View.Cells;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells;
using Cell.ViewModel.ToolWindow;
using System.ComponentModel;

namespace Cell.View.ToolWindow
{
    public partial class SheetToolWindow : ResizableToolWindow
    {
        private readonly Dictionary<SheetViewModel, SheetView> _sheetViews = [];

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
            if (!_sheetViews.TryGetValue(sheetViewModel, out var sheetView))
            {
                sheetView = new SheetView(sheetViewModel);
                sheetView.IsPanningEnabled = false;
                _sheetViews.Add(sheetViewModel, sheetView);
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
