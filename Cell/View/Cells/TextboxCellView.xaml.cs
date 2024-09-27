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

        private void PreviewTextBoxKeyDownForCell(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textbox) return;
            if (e.Key == Key.Enter)
            {
                textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                if (Keyboard.Modifiers == ModifierKeys.Shift) ApplicationViewModel.Instance.CellSelector.MoveSelectionUp();
                else ApplicationViewModel.Instance.CellSelector.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                if (Keyboard.Modifiers == ModifierKeys.Shift) ApplicationViewModel.Instance.CellSelector.MoveSelectionLeft();
                else ApplicationViewModel.Instance.CellSelector.MoveSelectionRight();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                ApplicationViewModel.Instance.CellSelector.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                ApplicationViewModel.Instance.CellSelector.MoveSelectionUp();
                e.Handled = true;
            }
        }

        private void CellTextBoxGotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void CellTextBoxLostFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}
