using Cell.ViewModel.Application;
using Cell.ViewModel.Cells.Types;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.Cells
{
    public partial class TextboxCellView : ResourceDictionary
    {
        private static TextBox? _focusedTextBox;
        private TextBox? _textbox;

        /// <summary>
        /// The view for a textbox cell.
        /// </summary>
        public TextboxCellView()
        {
            InitializeComponent();
        }

        private void CellTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void CellTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textbox)
            {
                _textbox = textbox;
                if (textbox.DataContext is TextboxCellViewModel cell)
                {
                    cell.PropertyChanged += CellViewModelPropertyChanged;
                }
            }
        }

        private void CellViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_textbox is null) return;
            if (e.PropertyName != nameof(TextboxCellViewModel.IsSelected)) return;
            if (sender is not TextboxCellViewModel textboxViewModel) return;
            if (!textboxViewModel.IsSelected) return;
            if (textboxViewModel.GetNumberOfSelectedCells() < 2)
            {
                _textbox.Focus();
                _textbox.SelectAll();
                _focusedTextBox = _textbox;
            }
            else
            {
                if (_focusedTextBox != null)
                {
                    _focusedTextBox.SelectionStart = _textbox.Text.Length; // Move caret to end
                    _focusedTextBox.SelectionLength = 0; // Deselect all text
                    Window.GetWindow(_focusedTextBox).Focus();
                    _focusedTextBox = null;
                }
            }
        }

        private void PreviewTextBoxKeyDownForCell(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textbox) return;
            if (e.Key == Key.Enter)
            {
                textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                if (Keyboard.Modifiers == ModifierKeys.Shift) ApplicationViewModel.Instance.CellSelector?.MoveSelectionUp();
                else ApplicationViewModel.Instance.CellSelector?.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                if (Keyboard.Modifiers == ModifierKeys.Shift) ApplicationViewModel.Instance.CellSelector?.MoveSelectionLeft();
                else ApplicationViewModel.Instance.CellSelector?.MoveSelectionRight();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                ApplicationViewModel.Instance.CellSelector?.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                ApplicationViewModel.Instance.CellSelector?.MoveSelectionUp();
                e.Handled = true;
            }
        }
    }
}
