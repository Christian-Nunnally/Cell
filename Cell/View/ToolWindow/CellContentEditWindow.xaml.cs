using Cell.Core.Execution.CodeCompletion;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    /// <summary>
    /// The interaction handling for cell content editing.
    /// </summary>
    public partial class CellContentEditWindow : ResizableToolWindow
    {
        private readonly CellContentEditWindowViewModel _viewModel;
        private CompletionWindow? _completionWindow;
        /// <summary>
        /// Creates a new instance of the <see cref="CellContentEditWindow"/> class.
        /// </summary>
        /// <param name="viewModel">The view model containing the data for this view.</param>
        public CellContentEditWindow(CellContentEditWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();

            _multiUseUserInputTextBox.Text = _viewModel.MultiUseUserInputText;
            _viewModel.PropertyChanged += CellContentEditWindowViewModelPropertyChanged;
            _multiUseUserInputTextBox.TextArea.TextEntering += OnTextEntering;
            _multiUseUserInputTextBox.TextArea.TextEntered += OnTextEntered;
            _multiUseUserInputTextBox.TextArea.TextView.Document.TextChanged += OnTextChanged;
            _multiUseUserInputTextBox.TextArea.PreviewKeyDown += TextEditorPreviewKeyDown;
        }

        private void CellContentEditWindowViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.MultiUseUserInputText))
            {
                if (_multiUseUserInputTextBox.Text == _viewModel.MultiUseUserInputText) return;
                _multiUseUserInputTextBox.Text = _viewModel.MultiUseUserInputText;
            }
        }

        private void EditPopulateFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.EditPopulateFunction();
        }

        private void EditTriggerFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.EditTriggerFunction();
        }

        private void MultiUseUserInputTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            _viewModel.SubmitMultiUseUserInputText();
        }

        private void AutoIndexButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.AutoIndexSelectedCells();
        }

        private void InsertCurrentAutoCompleteResult(TextCompositionEventArgs e)
        {
            _completionWindow?.CompletionList.RequestInsertion(e);
        }

        private void OnTextChanged(object? sender, EventArgs e)
        {
            _viewModel.MultiUseUserInputText = _multiUseUserInputTextBox.Text;
        }

        private void OnTextEntered(object sender, TextCompositionEventArgs e)
        {
            static bool ShouldOpenAutoCompleteWindow(TextCompositionEventArgs e) => e.Text.Length == 1 && e.Text == "=";
            if (ShouldOpenAutoCompleteWindow(e)) OpenAutoCompleteWindow();
        }

        private void OnTextEntering(object sender, TextCompositionEventArgs e)
        {
            bool ShouldSubmitAutoCompleteResult(TextCompositionEventArgs e)
            {
                var anyText = e.Text.Length != 0;
                var isAutoCompleteOpen = _completionWindow is not null;
                var isFirstChangedCharacterALetterOrDigit = char.IsLetterOrDigit(e.Text[0]);
                return anyText && isAutoCompleteOpen && !isFirstChangedCharacterALetterOrDigit;
            }
            if (ShouldSubmitAutoCompleteResult(e)) InsertCurrentAutoCompleteResult(e);
        }

        private void TextEditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                OpenAutoCompleteWindow();
                e.Handled = true;
            }
            else if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                if (e.Key == Key.Enter || e.Key == Key.Tab)
                {
                    if (sender is not TextArea textbox) return;
                    textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    e.Handled = true;
                }
            }
        }

        private void OpenAutoCompleteWindow()
        {
            var textArea = _multiUseUserInputTextBox.TextArea;
            var suggestions = _viewModel.GetPopulateFunctionSuggestions();

            _completionWindow = CodeCompletionWindowFactory.Create(textArea, suggestions);
            if (_completionWindow is null) return;
            _completionWindow.Show();
            _completionWindow.Closed += delegate { _completionWindow = null; };
        }
    }
}
