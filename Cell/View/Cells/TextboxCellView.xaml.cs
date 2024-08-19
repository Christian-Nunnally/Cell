using Cell.ViewModel.Application;
using Cell.ViewModel.Cells.Types;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.Cells
{
    public partial class TextboxCellView : ResourceDictionary
    {
        public TextboxCellView()
        {
            InitializeComponent();
        }

        private void CellTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.DataContext is TextboxCellViewModel cell)
                {
                    cell.SetTextBox(textBox);
                }
            }
        }

        private void TextBoxKeyDownForCell(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textbox) return;
            if (e.Key == Key.Enter)
            {
                textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                if (Keyboard.Modifiers == ModifierKeys.Shift) ApplicationViewModel.Instance.SheetViewModel?.MoveSelectionUp();
                else ApplicationViewModel.Instance.SheetViewModel?.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                if (Keyboard.Modifiers == ModifierKeys.Shift) ApplicationViewModel.Instance.SheetViewModel?.MoveSelectionLeft();
                else ApplicationViewModel.Instance.SheetViewModel?.MoveSelectionRight();
                e.Handled = true;
            }
        }
    }
}
