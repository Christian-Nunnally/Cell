using Cell.ViewModel.ToolWindow;

namespace Cell.View.ToolWindow
{
    public partial class CreateCollectionWindow : ResizableToolWindow
    {
        private CreateCollectionWindowViewModel CreateCollectionWindowViewModel => (CreateCollectionWindowViewModel)ToolViewModel;
        public CreateCollectionWindow(CreateCollectionWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        public override double MinimumHeight => 250;

        public override double MinimumWidth => 350;

        public override string ToolWindowTitle => "Creating new collection";

        private void AddCollectionButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            CreateCollectionWindowViewModel.AddCurrentCollection();
            RequestClose?.Invoke();
        }
    }
}
