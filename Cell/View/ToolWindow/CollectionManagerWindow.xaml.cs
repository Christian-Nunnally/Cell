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
            _viewModel.UserSetWidth = GetWidth();
            _viewModel.UserSetHeight = GetHeight();
            InitializeComponent();
            SyntaxHighlightingColors.ApplySyntaxHighlightingToEditor(_itemJsonEditor);
        }

        public Action? RequestClose { get; set; }

        public double GetHeight()
        {
            return ApplicationViewModel.Instance.ApplicationSettings.FunctionManagerWindowHeight;
        }

        public string GetTitle() => "Collection Manager";

        public List<CommandViewModel> GetToolBarCommands() => [];

        public double GetWidth()
        {
            return ApplicationViewModel.Instance.ApplicationSettings.FunctionManagerWindowWidth;
        }

        public bool HandleBeingClosed()
        {
            return true;
        }

        public void SetHeight(double height)
        {
            ApplicationViewModel.Instance.ApplicationSettings.FunctionManagerWindowHeight = height;
            _viewModel.UserSetHeight = height;
        }

        public void SetWidth(double width)
        {
            ApplicationViewModel.Instance.ApplicationSettings.FunctionManagerWindowWidth = width;
            _viewModel.UserSetWidth = width;
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
                    DialogWindow.ShowDialog("Collection in use", $"Cannot delete '{collection.Model.Name}' because it is being used by {collection.UsageCount} functions.");
                    return;
                }

                var conflictingBase = _viewModel.Collections.FirstOrDefault(x => x.Model.BasedOnCollectionName == collection.Name);
                if (conflictingBase != null)
                {
                    DialogWindow.ShowDialog("Collection in use", $"Cannot delete '{collection.Model.Name}' because it acting as the base for '{conflictingBase.Name}'.");
                    return;
                }

                DialogWindow.ShowYesNoConfirmationDialog($"Delete '{collection.Model.Name}'?", "Are you sure you want to delete this collection?", () =>
                {
                    if (collection.Items.Count > 0)
                    {
                        DialogWindow.ShowYesNoConfirmationDialog($"Are you sure?", "Are you sure? This will delete all items in the collection", () =>
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
            var editor = new CodeEditorWindow(function, x =>
            {
                function.SetUserFriendlyCode(x, null, ApplicationViewModel.Instance.UserCollectionLoader.GetDataTypeStringForCollection, ApplicationViewModel.Instance.UserCollectionLoader.CollectionNames);
                _viewModel.SelectedCollection?.RefreshSortAndFilter();
            }, null);
            ApplicationViewModel.Instance.ApplicationView.ShowToolWindow(editor, true);
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
    }
}
