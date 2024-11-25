using Cell.ViewModel.Application;
using Cell.ViewModel.Cells.Types;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cell.View.Cells
{
    public partial class TextboxCellView : ResourceDictionary
    {
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
                if (textbox.DataContext is TextboxCellViewModel cell)
                {
                    cell.FocusTextbox = () => textbox.Focus();
                    cell.UnfocusTextbox = () =>
                    {
                        DependencyObject parent = VisualTreeHelper.GetParent(textbox);
                        parent = VisualTreeHelper.GetParent(parent);
                        parent = VisualTreeHelper.GetParent(parent);
                        parent = VisualTreeHelper.GetParent(parent);
                        Keyboard.Focus(parent as IInputElement);
                    };
                    cell.SelectAllText = () => textbox.SelectAll();
                    cell.PropertyChanged += CellViewModelPropertyChanged;
                }
            }
        }

        private void CellViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(TextboxCellViewModel.IsSelected)) return;
            if (sender is not TextboxCellViewModel textboxViewModel) return;
            if (!textboxViewModel.IsSelected)
            {
                textboxViewModel.UnfocusTextbox();
                return;
            }
            if (textboxViewModel.GetNumberOfSelectedCells() < 2)
            {
                textboxViewModel.FocusTextbox();
                textboxViewModel.SelectAllText();
            }
        }

        private void PreviewTextBoxKeyDownForCell(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textbox) return;
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift) ApplicationViewModel.Instance.CellSelector?.MoveSelectionUp();
                else ApplicationViewModel.Instance.CellSelector?.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
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
            else if (e.Key == Key.Left)
            {
                if (textbox.CaretIndex == 0)
                {
                    ApplicationViewModel.Instance.CellSelector?.MoveSelectionLeft();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Right)
            {
                if (textbox.CaretIndex == textbox.Text.Length)
                {
                    ApplicationViewModel.Instance.CellSelector?.MoveSelectionRight();
                    e.Handled = true;
                }
            }
        }
    }
}
