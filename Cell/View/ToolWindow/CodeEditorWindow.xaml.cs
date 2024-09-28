using Cell.Core.Execution.CodeCompletion;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.ComponentModel;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    public partial class CodeEditorWindow : ResizableToolWindow
    {
        private bool _isAllowingCloseWhileDirty = false;
        private bool _isDirty = false;
        private CompletionWindow? completionWindow;
        public CodeEditorWindow(CodeEditorWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
            SyntaxHighlightingColors.ApplySyntaxHighlightingToEditor(textEditor);
            SyntaxHighlightingColors.ApplySyntaxHighlightingToEditor(syntaxTreePreviewViewer);
        }

        public override List<CommandViewModel> ToolBarCommands =>
        [
            new CommandViewModel("Test Code", () => CodeEditorWindowViewModel.TestCode(textEditor.Text)),
            new CommandViewModel("Syntax", () => CodeEditorWindowViewModel.ToggleSyntaxTreePreview(textEditor.Text)),
            new CommandViewModel("Save and Close", SaveAndClose)
        ];

        private CodeEditorWindowViewModel CodeEditorWindowViewModel => (CodeEditorWindowViewModel)ToolViewModel;

        public override void HandleBeingClosed()
        {
            base.HandleBeingClosed();
            CodeEditorWindowViewModel.PropertyChanged += CodeEditorWindowViewModelPropertyChanged;
            textEditor.TextArea.TextEntering -= OnTextEntering;
            textEditor.TextArea.TextEntered -= OnTextEntered;
            textEditor.TextArea.TextView.Document.TextChanged -= OnTextChanged;
            textEditor.TextArea.PreviewKeyDown -= TextEditorPreviewKeyDown;
        }

        public override void HandleBeingShown()
        {
            base.HandleBeingClosed();
            textEditor.Text = CodeEditorWindowViewModel.UserFriendlyCodeString;
            _isDirty = false;
            CodeEditorWindowViewModel.PropertyChanged += CodeEditorWindowViewModelPropertyChanged;
            textEditor.TextArea.TextEntering += OnTextEntering;
            textEditor.TextArea.TextEntered += OnTextEntered;
            textEditor.TextArea.TextView.Document.TextChanged += OnTextChanged;
            textEditor.TextArea.PreviewKeyDown += TextEditorPreviewKeyDown;
        }

        private void TextEditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
            {
                OpenAutoCompleteWindow();
                e.Handled = true;
            } 
        }

        public override bool HandleCloseRequested()
        {
            if (!_isDirty || _isAllowingCloseWhileDirty) return true;
            DialogFactory.ShowYesNoConfirmationDialog("Save Changes", "Do you want to save your changes?", SaveAndClose, CloseWithoutSaving);
            return false;
        }

        private void CloseWithoutSaving()
        {
            _isAllowingCloseWhileDirty = true;
            RequestClose?.Invoke();
        }

        private void CodeEditorWindowViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CodeEditorWindowViewModel.SyntaxTreePreviewText))
            {
                syntaxTreePreviewViewer.Text = CodeEditorWindowViewModel.SyntaxTreePreviewText;
            }
            else if (e.PropertyName == nameof(CodeEditorWindowViewModel.UserFriendlyCodeString))
            {
                textEditor.Text = CodeEditorWindowViewModel.UserFriendlyCodeString;
                _isDirty = false;
            }
        }

        private void InsertCurrentAutoCompleteResult(TextCompositionEventArgs e)
        {
            completionWindow?.CompletionList.RequestInsertion(e);
        }

        private void OnTextChanged(object? sender, EventArgs e) => _isDirty = true;

        private void OnTextEntered(object sender, TextCompositionEventArgs e)
        {
            _isDirty = true;
            if (ShouldOpenAutoCompleteWindow(e)) OpenAutoCompleteWindow();

            static bool ShouldOpenAutoCompleteWindow(TextCompositionEventArgs e)
            {
                return e.Text == ".";
            }
        }

        private void OnTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (ShouldSubmitAutoCompleteResult(e)) InsertCurrentAutoCompleteResult(e);

            bool ShouldSubmitAutoCompleteResult(TextCompositionEventArgs e)
            {
                var anyText = e.Text.Length != 0;
                var isAutoCompleteOpen = completionWindow is not null;
                var isFirstChangedCharacterALetterOrDigit = char.IsLetterOrDigit(e.Text[0]);
                return anyText && isAutoCompleteOpen && !isFirstChangedCharacterALetterOrDigit;
            }
        }

        private void OpenAutoCompleteWindow()
        {
            TextArea textArea = textEditor.TextArea;
            var returnType = CodeEditorWindowViewModel.FunctionBeingEdited.Model.ReturnType;
            completionWindow = CodeCompletionWindowFactory.Create(textArea, CodeEditorWindowViewModel.FunctionBeingEdited);
            if (completionWindow is null) return;
            completionWindow.Show();
            completionWindow.Closed += delegate { completionWindow = null; };
        }

        private void SaveAndClose()
        {
            CodeEditorWindowViewModel.UserFriendlyCodeString = textEditor.Text;
            _isDirty = false;
            RequestClose?.Invoke();
        }
    }
}
