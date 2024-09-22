using Cell.Data;
using Cell.Model.Plugin;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    public partial class CollectionManagerWindow : ResizableToolWindow
    {
        private CollectionManagerWindowViewModel CollectionManagerWindowViewModel => (CollectionManagerWindowViewModel)ToolViewModel;
        public CollectionManagerWindow(CollectionManagerWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
            SyntaxHighlightingColors.ApplySyntaxHighlightingToEditor(_itemJsonEditor);
        }

        public override double MinimumHeight => 300;

        public override double MinimumWidth => 600;

        public override List<CommandViewModel> ToolBarCommands => [
            new CommandViewModel("New Collection", CollectionManagerWindowViewModel.OpenCreateCollectionWindow),
            ];

        public override void HandleBeingClosed()
        {
            base.HandleBeingClosed();
            CollectionManagerWindowViewModel.PropertyChanged -= CollectionManagerWindowPropertyChanged;
        }

        public override void HandleBeingShown()
        {
            base.HandleBeingShown();
            CollectionManagerWindowViewModel.PropertyChanged += CollectionManagerWindowPropertyChanged;
        }

        private void CollectionManagerWindowPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CollectionManagerWindowViewModel.SelectedItemSerialized))
            {
                _itemJsonEditor.Text = CollectionManagerWindowViewModel.SelectedItemSerialized;
                CollectionManagerWindowViewModel.IsSaveItemJsonButtonVisible = false;
            }
        }

        private void DeleteCollectionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not UserCollection collection) return;
            if (!CollectionManagerWindowViewModel.CanDeleteCollection(collection, out var reason))
            {
                DialogFactory.ShowDialog("Unable to delete collection", $"Cannot delete '{collection.Model.Name}' because {reason}");
                return;
            }
            DialogFactory.ShowYesNoConfirmationDialog($"Delete '{collection.Model.Name}'?", "Are you sure you want to delete this collection?", () =>
            {
                if (collection.Items.Count == 0) CollectionManagerWindowViewModel.DeleteCollection(collection);
                else DialogFactory.ShowYesNoConfirmationDialog($"Are you sure?", "Are you sure? This will delete all items in the collection", () => CollectionManagerWindowViewModel.DeleteCollection(collection));
            });
        }

        private void EditSortAndFilterFunctionButtonClick(object sender, RoutedEventArgs e)
        {
            var functionName = CollectionManagerWindowViewModel.SelectedCollection?.Model.SortAndFilterFunctionName;
            if (string.IsNullOrEmpty(functionName)) return;
            var function = ApplicationViewModel.Instance.PluginFunctionLoader.GetOrCreateFunction("object", functionName);

            var collectionNameToDataTypeMap = ApplicationViewModel.Instance.UserCollectionLoader.GenerateDataTypeForCollectionMap();
            var codeEditorWindowViewModel = new CodeEditorWindowViewModel(function, null, collectionNameToDataTypeMap);
            ApplicationViewModel.Instance.ShowToolWindow(codeEditorWindowViewModel, true);
        }

        private void ItemJsonEditorTextChanged(object sender, EventArgs e)
        {
            CollectionManagerWindowViewModel.IsSaveItemJsonButtonVisible = true;
        }

        private void RemoveItemFromCollectionClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not PluginModel item) return;
            CollectionManagerWindowViewModel.RemoveItemFromSelectedCollection(item);
        }

        private void SaveSelectedItemJsonButtonClick(object sender, RoutedEventArgs e)
        {
            CollectionManagerWindowViewModel.SelectedItemSerialized = _itemJsonEditor.Text;
        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }
    }
}
