using Cell.Data;
using Cell.Model.Plugin;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class CollectionManagerWindow : ResizableToolWindow
    {
        private CollectionManagerWindowViewModel _viewModel;
        public CollectionManagerWindow(CollectionManagerWindowViewModel viewModel) : base(viewModel)
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

        private void DeleteCollectionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not UserCollection collection) return;
            if (!_viewModel.CanDeleteCollection(collection, out var reason))
            {
                DialogFactory.ShowDialog("Unable to delete collection", $"Cannot delete '{collection.Model.Name}' because {reason}");
                return;
            }
            DialogFactory.ShowYesNoConfirmationDialog($"Delete '{collection.Model.Name}'?", "Are you sure you want to delete this collection?", () =>
            {
                if (collection.Items.Count == 0) _viewModel.DeleteCollection(collection);
                else DialogFactory.ShowYesNoConfirmationDialog($"Are you sure?", "Are you sure? This will delete all items in the collection", () => _viewModel.DeleteCollection(collection));
            });
        }

        private void EditSortAndFilterFunctionButtonClick(object sender, RoutedEventArgs e)
        {
            var functionName = _viewModel.SelectedCollection?.Model.SortAndFilterFunctionName;
            if (string.IsNullOrEmpty(functionName)) return;
            var function = ApplicationViewModel.Instance.PluginFunctionLoader.GetOrCreateFunction("object", functionName);

            var collectionNameToDataTypeMap = ApplicationViewModel.Instance.UserCollectionLoader.GenerateDataTypeForCollectionMap();
            var codeEditorWindowViewModel = new CodeEditorWindowViewModel(function, null, collectionNameToDataTypeMap);
            ApplicationViewModel.Instance.ShowToolWindow(codeEditorWindowViewModel, true);
        }

        private void ItemJsonEditorTextChanged(object sender, EventArgs e)
        {
            _viewModel.IsSaveItemJsonButtonVisible = true;
        }

        private void RemoveItemFromCollectionClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not PluginModel item) return;
            _viewModel.RemoveItemFromSelectedCollection(item);
        }

        private void SaveSelectedItemJsonButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectedItemSerialized = _itemJsonEditor.Text;
        }
    }
}
