using Cell.Data;
using Cell.Model.Plugin;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class CollectionManagerWindow : UserControl, IResizableToolWindow
    {
        private readonly CollectionManagerWindowViewModel _viewModel;
        public CollectionManagerWindow(CollectionManagerWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
            SyntaxHighlightingColors.ApplySyntaxHighlightingToEditor(_itemJsonEditor);
        }

        public Action? RequestClose { get; set; }

        public double GetMinimumHeight() => 300;

        public string GetTitle() => "Collection Manager";

        public List<CommandViewModel> GetToolBarCommands() => [
            new CommandViewModel("New Collection", _viewModel.OpenCreateCollectionWindow),
            ];

        public double GetMinimumWidth() => 600;

        public bool HandleCloseRequested() => true;

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
            var codeEditorWindowViewModel = new CodeEditorWindowViewModel();
            // TODO move args to vm.
            var editor = new CodeEditorWindow(codeEditorWindowViewModel, function, x =>
            {
                function.SetUserFriendlyCode(x, null, ApplicationViewModel.Instance.UserCollectionLoader.GetDataTypeStringForCollection, ApplicationViewModel.Instance.UserCollectionLoader.CollectionNames);
                _viewModel.SelectedCollection?.RefreshSortAndFilter();
            }, null);
            ApplicationViewModel.Instance.ShowToolWindow(editor, true);
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

        public void HandleBeingClosed()
        {
            _viewModel.StartTrackingCollections();
        }

        public void HandleBeingShown()
        {
            _viewModel.StopTrackingCollections();
        }
    }
}
