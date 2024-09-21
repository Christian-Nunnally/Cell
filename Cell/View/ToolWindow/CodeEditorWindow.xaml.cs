using Cell.Execution;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells.Types;
using Cell.ViewModel.ToolWindow;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    public partial class CodeEditorWindow : UserControl, INotifyPropertyChanged, IResizableToolWindow
    {
        private readonly CodeEditorWindowViewModel _viewModel;
        private bool _isAllowingCloseWhileDirty = false;
        private CompletionWindow? completionWindow;
        private bool _isDirty = false;

        public CodeEditorWindow(CodeEditorWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            viewModel.PropertyChanged += CodeEditorWindowViewModelPropertyChanged;
            DataContext = viewModel;
            
            InitializeComponent();
            SyntaxHighlightingColors.ApplySyntaxHighlightingToEditor(textEditor);
            SyntaxHighlightingColors.ApplySyntaxHighlightingToEditor(syntaxTreePreviewViewer);
        }

        private void CodeEditorWindowViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CodeEditorWindowViewModel.SyntaxTreePreviewText))
            {
                syntaxTreePreviewViewer.Text = _viewModel.SyntaxTreePreviewText;
            }
            else if (e.PropertyName == nameof(CodeEditorWindowViewModel.UserFriendlyCodeString))
            {
                textEditor.Text = _viewModel.UserFriendlyCodeString;
                _isDirty = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Action? RequestClose { get; set; }

        public double GetMinimumHeight() => 400;

        public double GetMinimumWidth() => 400;

        public string GetTitle()
        {
            var dirtyDot = _isDirty ? "*" : string.Empty;
            var functionBeingEdited = _viewModel.FunctionBeingEdited.Model;
            var cellContext = _viewModel.CellContext;
            return cellContext == null ? $"Code Editor - {functionBeingEdited.Name}{dirtyDot}" : $"Code Editor - {functionBeingEdited.Name} - {ColumnCellViewModel.GetColumnName(cellContext.Column)}{cellContext.Row}";
        }

        public List<CommandViewModel> GetToolBarCommands() => [
            new CommandViewModel("Test Code", () => _viewModel.TestCode(textEditor.Text)),
            new CommandViewModel("Syntax", () => _viewModel.ToggleSyntaxTreePreview(textEditor.Text)),
            new CommandViewModel("Save and Close", SaveAndClose)
            ];

        public void HandleBeingClosed()
        {
            _viewModel.HandleBeingClosed();
            textEditor.TextArea.TextEntering -= OnTextEntering;
            textEditor.TextArea.TextEntered -= OnTextEntered;
            textEditor.TextArea.TextView.Document.TextChanged -= OnTextChanged;
        }

        public void HandleBeingShown()
        {
            _viewModel.HandleBeingShown();
            textEditor.TextArea.TextEntering += OnTextEntering;
            textEditor.TextArea.TextEntered += OnTextEntered;
            textEditor.TextArea.TextView.Document.TextChanged += OnTextChanged;
        }

        public bool HandleCloseRequested()
        {
            if (!_isDirty || _isAllowingCloseWhileDirty) return true;
            DialogFactory.ShowYesNoConfirmationDialog("Save Changes", "Do you want to save your changes?", SaveAndClose, CloseWithoutSaving);
            return false;
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

        private void OpenAutoCompleteWindow()
        {
            TextArea textArea = textEditor.TextArea;
            var returnType = _viewModel.FunctionBeingEdited.Model.ReturnType;
            completionWindow = CodeCompletionWindowFactory.Create(textArea, returnType);
            if (completionWindow is null) return;
            completionWindow.Show();
            completionWindow.Closed += delegate { completionWindow = null; };
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

        private void InsertCurrentAutoCompleteResult(TextCompositionEventArgs e)
        {
            completionWindow?.CompletionList.RequestInsertion(e);
        }

        private void CloseWithoutSaving()
        {
            _isAllowingCloseWhileDirty = true;
            RequestClose?.Invoke();
        }

        private void SaveAndClose()
        {
            _viewModel.UserFriendlyCodeString = textEditor.Text;
            RequestClose?.Invoke();
        }
    }
}
