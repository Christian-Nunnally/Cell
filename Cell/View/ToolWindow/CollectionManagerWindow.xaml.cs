using Cell.Core.Data;
using Cell.ViewModel.Application;
using Cell.ViewModel.Data;
using Cell.ViewModel.ToolWindow;
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
            InitializeComponent();
        }

        private void DeleteCollectionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not UserCollection collection) return;
            if (!_viewModel.CanDeleteCollection(collection, out var reason))
            {
                ApplicationViewModel.Instance.DialogFactory?.Show("Unable to delete collection", $"Cannot delete '{collection.Model.Name}' because {reason}");
                return;
            }
            ApplicationViewModel.Instance.DialogFactory?.ShowYesNo($"Delete '{collection.Model.Name}'?", "Are you sure you want to delete this collection?", () =>
            {
                if (collection.Items.Count == 0) _viewModel.DeleteCollection(collection);
                else ApplicationViewModel.Instance.DialogFactory?.ShowYesNo($"Are you sure?", "Are you sure? This will delete all items in the collection", () => _viewModel.DeleteCollection(collection));
            });
        }

        private void ShowCollectionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<UserCollectionListItemViewModel>(sender, out var collectionListItemViewModel)) return;
            _viewModel.ShowCollection(collectionListItemViewModel.Collection);
        }
    }
}
