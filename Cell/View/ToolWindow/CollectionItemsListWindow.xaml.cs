using Cell.Model;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class CollectionItemsListWindow : ResizableToolWindow
    {
        private readonly CollectionItemsListWindowViewModel _viewModel;
        /// <summary>
        /// Creates a new instance of the <see cref="CollectionItemsListWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public CollectionItemsListWindow(CollectionItemsListWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            _viewModel.PropertyChanged += CollectionManagerWindowPropertyChanged;
            InitializeComponent();
            SyntaxHighlightingColors.ApplySyntaxHighlightingToEditor(_itemJsonEditor);
        }

        private void CollectionManagerWindowPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.SelectedItemSerialized))
            {
                _itemJsonEditor.Text = _viewModel.SelectedItemSerialized;
                _viewModel.IsSaveItemJsonButtonVisible = false;
            }
        }

        private void EditSortAndFilterFunctionButtonClick(object sender, RoutedEventArgs e)
        {
            if (ApplicationViewModel.Instance.UserCollectionTracker is null) return;
            if (ApplicationViewModel.Instance.CellTracker is null) return;
            if (ApplicationViewModel.Instance.FunctionTracker is null) return;

            _viewModel.EditSortAndFilterFunctionForCollection();
        }

        private void ItemJsonEditorTextChanged(object sender, EventArgs e)
        {
            _viewModel.IsSaveItemJsonButtonVisible = true;
        }

        private void RemoveItemFromCollectionClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not UserItem item) return;
            _viewModel.RemoveItemFromSelectedCollection(item);
        }

        private void SaveSelectedItemJsonButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectedItemSerialized = _itemJsonEditor.Text;
        }
    }
}
