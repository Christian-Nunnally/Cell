using Cell.Core.Data;
using Cell.Core.Execution.Functions;
using Cell.Model;
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
        private readonly CollectionManagerWindowViewModel _viewModel;
        /// <summary>
        /// Creates a new instance of the <see cref="CollectionManagerWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
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
                ApplicationViewModel.Instance.DialogFactory.Show("Unable to delete collection", $"Cannot delete '{collection.Model.Name}' because {reason}");
                return;
            }
            ApplicationViewModel.Instance.DialogFactory.ShowYesNo($"Delete '{collection.Model.Name}'?", "Are you sure you want to delete this collection?", () =>
            {
                if (collection.Items.Count == 0) _viewModel.DeleteCollection(collection);
                else ApplicationViewModel.Instance.DialogFactory.ShowYesNo($"Are you sure?", "Are you sure? This will delete all items in the collection", () => _viewModel.DeleteCollection(collection));
            });
        }

        private void EditSortAndFilterFunctionButtonClick(object sender, RoutedEventArgs e)
        {
            var functionName = _viewModel.SelectedCollection?.Collection.Model.SortAndFilterFunctionName;
            if (string.IsNullOrEmpty(functionName)) return;
            var function = ApplicationViewModel.Instance.FunctionTracker.GetOrCreateFunction("object", functionName);

            var collectionNameToDataTypeMap = ApplicationViewModel.Instance.UserCollectionLoader.GenerateDataTypeForCollectionMap();
            var testingContext = new TestingContext(ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.UserCollectionLoader, CellModel.Null, ApplicationViewModel.Instance.FunctionTracker);
            var codeEditorWindowViewModel = new CodeEditorWindowViewModel(function, null, collectionNameToDataTypeMap, testingContext);
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
