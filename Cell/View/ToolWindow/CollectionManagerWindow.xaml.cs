using Cell.Model;
using Cell.Model.Plugin;
using Cell.Persistence;
using Cell.Plugin;
using Cell.View.Skin;
using Cell.ViewModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class CollectionManagerWindow : UserControl, IResizableToolWindow, INotifyPropertyChanged
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

        public event PropertyChangedEventHandler? PropertyChanged;

        public Action? RequestClose { get; set; }

        public double GetHeight()
        {
            return ApplicationSettings.Instance.FunctionManagerWindowHeight;
        }

        public string GetTitle() => "Collection Manager";

        public List<CommandViewModel> GetToolBarCommands() => [];

        public double GetWidth()
        {
            return ApplicationSettings.Instance.FunctionManagerWindowWidth;
        }

        public void HandleBeingClosed()
        {
        }

        public void SetHeight(double height)
        {
            ApplicationSettings.Instance.FunctionManagerWindowHeight = height;
            _viewModel.UserSetHeight = height;
        }

        public void SetWidth(double width)
        {
            ApplicationSettings.Instance.FunctionManagerWindowWidth = width;
            _viewModel.UserSetWidth = width;
        }

        private void AddCollectionButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.AddCurrentCollection();
        }

        private void DeleteCollectionButtonClicked(object sender, System.Windows.RoutedEventArgs e)
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
                            UserCollectionLoader.DeleteCollection(collection);
                        });
                    }
                    else
                    {
                        UserCollectionLoader.DeleteCollection(collection);
                    }
                });
            }
        }

        private void EditItemButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CellModel cell)
            {
                throw new Exception("Not implemented");
            }
        }

        private void EditSortAndFilterFunctionButtonClick(object sender, RoutedEventArgs e)
        {
            var functionName = _viewModel.SelectedCollection?.Model.SortAndFilterFunctionName;
            if (string.IsNullOrEmpty(functionName)) return;
            var function = PluginFunctionLoader.GetOrCreateFunction("object", functionName);
            var editor = new CodeEditor(function, x =>
            {
                function.SetUserFriendlyCode(x, null);
                _viewModel.SelectedCollection?.RefreshSortAndFilter();
            }, null);
            ApplicationViewModel.Instance.MainWindow.ShowToolWindow(editor);
        }

        private void GoToCellButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CellModel cell)
            {
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

        private void RemoveItemFromCollectionClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is PluginModel item)
            {
                var selectedCollection = _viewModel.SelectedCollection;
                selectedCollection?.Remove(item);
            }
        }

        private void SaveSelectedItemJsonButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.SelectedItemSerialized = _itemJsonEditor.Text;
        }
    }
}
