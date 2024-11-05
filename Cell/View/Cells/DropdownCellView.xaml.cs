using Cell.ViewModel.Cells.Types;
using System.Windows;

namespace Cell.View.Cells
{
    /// <summary>
    /// Event handlers for dropdown cell views.
    /// </summary>
    public partial class DropdownCellView : ResourceDictionary
    {
        private void DropdownOpened(object sender, EventArgs e)
        {
            if (sender is not FrameworkElement element) return;
            if (element.DataContext is not DropdownCellViewModel viewModel) return;
            viewModel.DropdownOpened();
        }
    }
}
