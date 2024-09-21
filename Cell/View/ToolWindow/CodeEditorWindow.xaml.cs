using Cell.Execution;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells.Types;
using Cell.ViewModel.Execution;
using Cell.ViewModel.ToolWindow;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cell.View.ToolWindow
{
    public partial class CodeEditorWindow : UserControl, INotifyPropertyChanged, IResizableToolWindow
    {
        private readonly CellModel? _currentCell;
        private readonly bool _doesFunctionReturnValue;
        private readonly PluginFunction _function;
        private readonly Action<string> _saveCodeCallback = x => { };
        private static bool _haveAssembliesBeenRegistered;
        private bool _isAllowingCloseWhileDirty = false;
        private bool _isDirty = false;
        private CompileResult _lastCompileResult;
        private CompletionWindow? completionWindow;

        private readonly CodeEditorWindowViewModel _viewModel;
        public CodeEditorWindow(CodeEditorWindowViewModel viewModel, PluginFunction function, Action<string> saveCodeCallback, CellModel? currentCell)
        {
            _viewModel = viewModel;
            //DataContext = viewModel;
            DataContext = this;

            InitializeComponent();
            SyntaxHighlightingColors.ApplySyntaxHighlightingToEditor(textEditor);
            SyntaxHighlightingColors.ApplySyntaxHighlightingToEditor(syntaxTreePreviewViewer);

            _currentCell = currentCell;
            _doesFunctionReturnValue = function.Model.ReturnType != "void";
            _function = function;
            _saveCodeCallback = saveCodeCallback;
            DisplayResult(_function.CompileResult);

            if (!_haveAssembliesBeenRegistered)
            {
                CodeCompletionWindowFactory.RegisterTypesInAssembly(typeof(TodoItem).Assembly);
                _haveAssembliesBeenRegistered = true;
            }
        }

        private void ReloadFunctionCodeWhenItChanges(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PluginFunctionModel.Code)) ReloadFunctionCode();
        }

        private void ReloadFunctionCode()
        {
            textEditor.Text = _function.GetUserFriendlyCode(_currentCell, ApplicationViewModel.Instance.UserCollectionLoader.GetDataTypeStringForCollection, ApplicationViewModel.Instance.UserCollectionLoader.CollectionNames);
            _isDirty = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsTransformedSyntaxTreeViewerVisible { get; set; }

        public Action? RequestClose { get; set; }

        public double ResultColumnWidth => ResultString == string.Empty ? 0 : 200;

        public string? ResultString { get; set; } = string.Empty;

        public SolidColorBrush ResultStringColor => _lastCompileResult.WasSuccess ? new SolidColorBrush(ColorConstants.ForegroundColorConstant) : new SolidColorBrush(ColorConstants.ErrorForegroundColorConstant);

        public double GetMinimumHeight() => 400;

        public string GetTitle()
        {
            var dirtyDot = _isDirty ? "*" : string.Empty;
            return _currentCell == null ? $"Code Editor - {_function.Model.Name}{dirtyDot}" : $"Code Editor - {_function.Model.Name} - {ColumnCellViewModel.GetColumnName(_currentCell.Column)}{_currentCell.Row}";
        }

        public List<CommandViewModel> GetToolBarCommands() => [
            new CommandViewModel("Test Code", TestCode),
            new CommandViewModel("Syntax", ToggleSyntaxTreePreview),
            new CommandViewModel("Save and Close", SaveAndClose)
            ];

        public double GetMinimumWidth() => 400;

        public bool HandleCloseRequested()
        {
            if (!_isDirty || _isAllowingCloseWhileDirty) return true;
            DialogFactory.ShowYesNoConfirmationDialog("Save Changes", "Do you want to save your changes?", () =>
            {
                SaveCode();
                RequestClose?.Invoke();
            }, () =>
            {
                _isAllowingCloseWhileDirty = true;
                RequestClose?.Invoke();
            });
            return false;
        }

        private static string GetVariableTypePriorToCarot(TextArea textArea)
        {
            var offset = textArea.Caret.Offset - 1;
            var text = textArea.Document.Text;
            while (offset > 0 && char.IsLetterOrDigit(text[offset - 1])) offset--;
            return text[offset..(textArea.Caret.Offset - 1)];
        }

        private void DisplayResult(CompileResult result)
        {
            _lastCompileResult = result;
            ResultString = result.ExecutionResult ?? "";
            ResultString = ResultString.Replace("Compilation failed, first error is", "Error");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultString)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultStringColor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultColumnWidth)));
        }

        private void OnTextChanged(object? sender, EventArgs e) => _isDirty = true;

        private void OnTextEntered(object sender, TextCompositionEventArgs e)
        {
            _isDirty = true;
            if (e.Text == ".")
            {
                TextArea textArea = textEditor.TextArea;
                var type = GetVariableTypePriorToCarot(textArea);
                completionWindow = CodeCompletionWindowFactory.Create(textArea, type, _doesFunctionReturnValue);
                if (completionWindow is not null)
                {
                    completionWindow.Show();
                    completionWindow.Closed += delegate
                    {
                        completionWindow = null;
                    };
                }
            }
        }

        private void OnTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private void SaveAndClose()
        {
            SaveCode();
            RequestClose?.Invoke();
        }

        private void SaveCode()
        {
            _saveCodeCallback?.Invoke(textEditor.Text);
            _isDirty = false;
        }

        private void TestCode()
        {
            try
            {
                if (_currentCell is null) return;
                var model = new PluginFunctionModel("testtesttest", string.Empty, !_doesFunctionReturnValue ? "void" : "object");
                var function = new PluginFunction(model);
                function.SetUserFriendlyCode(textEditor.Text, _currentCell, ApplicationViewModel.Instance.UserCollectionLoader.GetDataTypeStringForCollection, ApplicationViewModel.Instance.UserCollectionLoader.CollectionNames);
                var compiled = function.CompiledMethod;
                var result = function.CompileResult;
                if (result.WasSuccess)
                {
                    if (_doesFunctionReturnValue)
                    {
                        var resultObject = compiled?.Invoke(null, [new PluginContext(ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.UserCollectionLoader, _currentCell.Index), _currentCell]);
                        result = new CompileResult { WasSuccess = true, ExecutionResult = resultObject?.ToString() ?? "" };
                    }
                    else
                    {
                        compiled?.Invoke(null, [new PluginContext(ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.UserCollectionLoader, _currentCell.Index), _currentCell]);
                        result = new CompileResult { WasSuccess = true, ExecutionResult = "Success" };
                    }
                }
                DisplayResult(result);
            }
            catch (Exception ex)
            {
                var result = new CompileResult { WasSuccess = false, ExecutionResult = ex.Message };
                DisplayResult(result);
            }
        }

        private void ToggleSyntaxTreePreview()
        {
            if (IsTransformedSyntaxTreeViewerVisible)
            {
                IsTransformedSyntaxTreeViewerVisible = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTransformedSyntaxTreeViewerVisible)));
                return;
            }
            var model = new PluginFunctionModel("testtesttest", "", !_doesFunctionReturnValue ? "void" : "object");
            var function = new PluginFunction(model);
            if (_currentCell is null) return;
            function.SetUserFriendlyCode(textEditor.Text, _currentCell, ApplicationViewModel.Instance.UserCollectionLoader.GetDataTypeStringForCollection, ApplicationViewModel.Instance.UserCollectionLoader.CollectionNames);
            var syntaxTree = function.SyntaxTree;
            syntaxTreePreviewViewer.Text = syntaxTree.ToString();
            IsTransformedSyntaxTreeViewerVisible = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTransformedSyntaxTreeViewerVisible)));
        }

        public void HandleBeingClosed()
        {
            _function.Model.PropertyChanged -= ReloadFunctionCodeWhenItChanges;
            textEditor.TextArea.TextEntering -= OnTextEntering;
            textEditor.TextArea.TextEntered -= OnTextEntered;
            textEditor.TextArea.TextView.Document.TextChanged -= OnTextChanged;
        }

        public void HandleBeingShown()
        {
            _function.Model.PropertyChanged += ReloadFunctionCodeWhenItChanges;
            textEditor.TextArea.TextEntering += OnTextEntering;
            textEditor.TextArea.TextEntered += OnTextEntered;
            textEditor.TextArea.TextView.Document.TextChanged += OnTextChanged;
            ReloadFunctionCode();
        }
    }
}
