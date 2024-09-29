using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class CreateCollectionWindow : ResizableToolWindow
    {
        private CreateCollectionWindowViewModel CreateCollectionWindowViewModel => (CreateCollectionWindowViewModel)ToolViewModel;
        public CreateCollectionWindow(CreateCollectionWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        private void AddCollectionButtonClick(object sender, RoutedEventArgs e)
        {
            CreateCollectionWindowViewModel.AddCurrentCollection();
            RequestClose?.Invoke();
        }
    }
}
