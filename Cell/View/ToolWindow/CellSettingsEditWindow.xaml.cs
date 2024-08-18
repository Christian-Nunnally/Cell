using Cell.View.ToolWindow;
using Cell.ViewModel.Application;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    /// <summary>
    /// Interaction logic for EditCellPanel.xaml
    /// </summary>
    public partial class CellSettingsEditWindow : UserControl, IToolWindow
    {
        public CellSettingsEditWindow()
        {
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public string GetTitle()
        {
            var currentlySelectedCell = ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModel;
            if (currentlySelectedCell is null) return "Select a cell to edit";
            return $"Cell settings editor - {currentlySelectedCell.GetName()}";
        }

        public List<CommandViewModel> GetToolBarCommands() => [];

        public void HandleBeingClosed() { }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}
