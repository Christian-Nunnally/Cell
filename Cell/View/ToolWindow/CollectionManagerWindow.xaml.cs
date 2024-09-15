using Cell.Data;
using Cell.Model;
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

        public List<CommandViewModel> GetToolBarCommands() => [];

        public double GetMinimumWidth() => 600;

        public bool HandleCloseRequested()
        {
            return true;
        }

        private void OpenCreateCollectionWindowButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.OpenCreateCollectionWindow();
        }

        private void DeleteCollectionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is UserCollection collection)
            {
                if (collection.UsageCount != 0)
                {
                    DialogFactory.ShowDialog("Collection in use", $"Cannot delete '{collection.Model.Name}' because it is being used by {collection.UsageCount} functions.");
                    return;
                }

                var conflictingBase = _viewModel.Collections.FirstOrDefault(x => x.Model.BasedOnCollectionName == collection.Name);
                if (conflictingBase != null)
                {
                    DialogFactory.ShowDialog("Collection in use", $"Cannot delete '{collection.Model.Name}' because it acting as the base for '{conflictingBase.Name}'.");
                    return;
                }

                DialogFactory.ShowYesNoConfirmationDialog($"Delete '{collection.Model.Name}'?", "Are you sure you want to delete this collection?", () =>
                {
                    if (collection.Items.Count > 0)
                    {
                        DialogFactory.ShowYesNoConfirmationDialog($"Are you sure?", "Are you sure? This will delete all items in the collection", () =>
                        {
                            ApplicationViewModel.Instance.UserCollectionLoader.DeleteCollection(collection);
                        });
                    }
                    else
                    {
                        ApplicationViewModel.Instance.UserCollectionLoader.DeleteCollection(collection);
                    }
                });
            }
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

        private void GoToCellButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CellModel cell)
            {
                ApplicationViewModel.Instance.GoToCell(cell);
                ApplicationViewModel.Instance.GoToCell(cell);
            }
        }

        private void ItemJsonEditorTextChanged(object sender, EventArgs e)
        {
        }

        private void ItemsInSelectedCollectionListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _itemJsonEditor.Text = _viewModel.SelectedItemSerialized;
        }

        private void RemoveItemFromCollectionClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is PluginModel item)
            {
                var selectedCollection = _viewModel.SelectedCollection;
                selectedCollection?.Remove(item);
            }
        }

        private void SaveSelectedItemJsonButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectedItemSerialized = _itemJsonEditor.Text;
        }

        public void HandleBeingClosed()
        {
        }

        public void HandleBeingShown()
        {
        }
    }
}
