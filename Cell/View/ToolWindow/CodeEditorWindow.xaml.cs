﻿using Cell.Core.Execution.CodeCompletion;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.ComponentModel;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    public partial class CodeEditorWindow : ResizableToolWindow
    {
        private readonly CodeEditorWindowViewModel _viewModel;
        private CompletionWindow? _completionWindow;
        private int _carotPosition;
        /// <summary>
        /// Creates a new instance of the <see cref="CodeEditorWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public CodeEditorWindow(CodeEditorWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
            var collectionTracker = ApplicationViewModel.Instance.UserCollectionTracker;
            var collectionNames = collectionTracker?.UserCollections.Select(c => c.Model.Name).ToList() ?? [];
            SyntaxHighlightingColors.ApplySyntaxHighlightingToEditor(textEditor, collectionNames);
            SyntaxHighlightingColors.ApplySyntaxHighlightingToEditor(syntaxTreePreviewViewer, collectionNames);

            textEditor.Text = _viewModel.CurrentTextInEditor;
            _viewModel.PropertyChanged += CodeEditorWindowViewModelPropertyChanged;
            textEditor.TextArea.TextEntering += OnTextEntering;
            textEditor.TextArea.TextEntered += OnTextEntered;
            textEditor.TextArea.TextView.Document.TextChanged += OnTextChanged;
            textEditor.TextArea.PreviewKeyDown += TextEditorPreviewKeyDown;
            textEditor.TextArea.Caret.PositionChanged += TextEditorCaretPositionChanged;
        }

        private void TextEditorCaretPositionChanged(object? sender, EventArgs e)
        {
            _carotPosition = textEditor.TextArea.Caret.Offset;
        }

        private void CodeEditorWindowViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.SyntaxTreePreviewText))
            {
                syntaxTreePreviewViewer.Text = _viewModel.SyntaxTreePreviewText;
            }
        }

        private void InsertCurrentAutoCompleteResult(TextCompositionEventArgs e)
        {
            _completionWindow?.CompletionList.RequestInsertion(e);
        }

        private void OnTextChanged(object? sender, EventArgs e)
        {
            _viewModel.CurrentTextInEditor = textEditor.Text;
        }

        private void OnTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (ShouldOpenAutoCompleteWindow(e)) OpenAutoCompleteWindow();
        }

        private static bool ShouldOpenAutoCompleteWindow(TextCompositionEventArgs e)
        {
            return e.Text == "." || e.Text == " ";
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

        private void OpenAutoCompleteWindow()
        {
            var textArea = textEditor.TextArea;
            var suggestions = _viewModel.CreateAutoCompleteSuggestions(textArea.Caret.Offset);

            _completionWindow = CodeCompletionWindowFactory.Create(textArea, suggestions);
            if (_completionWindow is null) return;
            _completionWindow.Show();
            _completionWindow.Closed += delegate { _completionWindow = null; };
        }

        private void TextEditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
            {
                OpenAutoCompleteWindow();
                e.Handled = true;
            }
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                _viewModel.Save();
                e.Handled = true;
            }
            else if (e.Key == Key.OemQuestion)
            {
                _viewModel.ShowContextHelpAtCarot(_carotPosition);
            }
        }
    }
}
