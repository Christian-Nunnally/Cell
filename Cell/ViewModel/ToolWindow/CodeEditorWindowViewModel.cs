using Cell.Core.Common;
using Cell.Core.Execution;
using Cell.Core.Execution.CodeCompletion;
using Cell.Core.Execution.Functions;
using Cell.Model;
using Cell.ViewModel.Application;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.ComponentModel;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// Tool window view model for editing the code of a function in a cell.
    /// </summary>
    public class CodeEditorWindowViewModel : ToolWindowViewModel
    {
        private readonly IReadOnlyDictionary<string, string> _collectionNameToDataTypeMap;
        private bool _isAllowingCloseWhileDirty = false;
        private bool _isDirty = false;
        private CompileResult _lastCompileResult;
        private string _syntaxTreePreviewText = string.Empty;
        private string _currentTextInEditor;
        private readonly CellFunctionCodeSuggestionGenerator _cellFunctionCodeSuggestionGenerator;

        /// <summary>
        /// Creates a new instance of <see cref="CodeEditorWindowViewModel"/>.
        /// </summary>
        /// <param name="functionToBeEdited">The function to be edited.</param>
        /// <param name="cellContextFromWhichTheFunctionIsBeingEdited">The cell context from which the function is ceing edited.</param>
        /// <param name="collectionNameToDataTypeMap">A map from collection names to thier data types for code completion stuff.</param>
        /// <param name="contextToTestWith">The context used when testing the code.</param>
        public CodeEditorWindowViewModel(CellFunction functionToBeEdited, CellModel? cellContextFromWhichTheFunctionIsBeingEdited, IReadOnlyDictionary<string, string> collectionNameToDataTypeMap, IContext contextToTestWith)
        {
            _cellFunctionCodeSuggestionGenerator = new CellFunctionCodeSuggestionGenerator(CellFunction.UsingNamespaces, cellContextFromWhichTheFunctionIsBeingEdited);
            FunctionBeingEdited = functionToBeEdited;
            _currentTextInEditor = functionToBeEdited.GetUserFriendlyCode(cellContextFromWhichTheFunctionIsBeingEdited, collectionNameToDataTypeMap);
            _cellFunctionCodeSuggestionGenerator.UpdateCode(_currentTextInEditor, collectionNameToDataTypeMap);
            _collectionNameToDataTypeMap = collectionNameToDataTypeMap;
            CellContext = cellContextFromWhichTheFunctionIsBeingEdited;
            _contextToTestWith = contextToTestWith;
        }

        /// <summary>
        /// The cell context the function is currently being edited from.
        /// </summary>
        public CellModel? CellContext { get; private set; }

        private readonly IContext _contextToTestWith;

        /// <summary>
        /// Gets the current text in the editor.
        /// </summary>
        public string CurrentTextInEditor
        {
            get => _currentTextInEditor;
            set
            {
                _currentTextInEditor = value;
                _isDirty = true;
                _cellFunctionCodeSuggestionGenerator.UpdateCode(_currentTextInEditor, _collectionNameToDataTypeMap);
                NotifyPropertyChanged(nameof(CurrentTextInEditor));
                NotifyPropertyChanged(nameof(ToolWindowTitle));
            }
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 400;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 750;

        /// <summary>
        /// Gets the function being edited.
        /// </summary>
        public CellFunction FunctionBeingEdited { get; private set; }

        /// <summary>
        /// Gets whether the syntax tree viewer textbox is visible.
        /// </summary>
        public bool IsTransformedSyntaxTreeViewerVisible => !string.IsNullOrWhiteSpace(SyntaxTreePreviewText);

        /// <summary>
        /// Gets the minimum height this tool window is allowed to be reized to.
        /// </summary>
        public override double MinimumHeight => 200;

        /// <summary>
        /// Gets the minimum width this tool window is allowed to be reized to.
        /// </summary>
        public override double MinimumWidth => 200;

        /// <summary>
        /// Gets the syntax tree preview text to be displayed in the UI when the user requests to see the syntax tree.
        /// </summary>
        public string SyntaxTreePreviewText
        {
            get => _syntaxTreePreviewText; 
            set
            {
                if (_syntaxTreePreviewText == value) return;
                _syntaxTreePreviewText = value;
                NotifyPropertyChanged(nameof(SyntaxTreePreviewText));
                NotifyPropertyChanged(nameof(IsTransformedSyntaxTreeViewerVisible));
            }
        }

        /// <summary>
        /// Provides a list of commands to display in the title bar of the tool window.
        /// </summary>
        public override List<CommandViewModel> ToolBarCommands =>
        [
            new CommandViewModel("Test", TestCodeAndOpenLogWindow) { ToolTip = "Runs the current code and displays the result, or just 'success' if the function isn't supposed to return a value." },
            new CommandViewModel("Syntax", () => ToggleSyntaxTreePreview(CurrentTextInEditor)) { ToolTip = "Shows what the code looks like after references have been converted to 'real' code." },
            new CommandViewModel("Save", Save) { ToolTip = "Saves the current code in the editor into the function being edited." },
            new CommandViewModel("Save and Close", SaveAndClose) { ToolTip = "Saves the edited code to the function and closes this tool window." }
        ];

        private void TestCodeAndOpenLogWindow()
        {
            Logger.Instance.Clear();
            TestCode();
            var logWindowViewModel = new LogWindowViewModel();
            ApplicationViewModel.Instance.ShowToolWindow(logWindowViewModel);
        }

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle
        {
            get
            {
                var dirtyDot = _isDirty ? "*" : string.Empty;
                var functionBeingEdited = FunctionBeingEdited.Model;
                var cellContext = CellContext;
                var title = $"Editing `{functionBeingEdited.Name}`{dirtyDot}";
                if (cellContext is not null) title += $" : {cellContext.Location.UserFriendlyLocationString}";
                return title;
            }
        }

        /// <summary>
        /// Occurs when the tool window is really being closed.
        /// </summary>
        public override void HandleBeingClosed()
        {
            base.HandleBeingClosed();
            FunctionBeingEdited.Model.PropertyChanged -= ReloadFunctionCodeWhenItChanges;
        }

        /// <summary>
        /// Occurs when the tool window is being shown.
        /// </summary>
        public override void HandleBeingShown()
        {
            base.HandleBeingShown();
            FunctionBeingEdited.Model.PropertyChanged -= ReloadFunctionCodeWhenItChanges;
        }

        /// <summary>
        /// Called when the tool window requested to be closed, either from the window itself or the host showing it, and gives the tool window a change to disallow the close.
        /// </summary>
        /// <returns>True if the tool window is allowing itself to be closed. If false, the caller should respect it and not call HandleBeingClosed or close the window.</returns>
        public override bool HandleCloseRequested()
        {
            if (!_isDirty || _isAllowingCloseWhileDirty) return true;
            ApplicationViewModel.Instance.DialogFactory.ShowYesNo("Save Changes", "Do you want to save your changes?", SaveAndClose, CloseWithoutSaving);
            return false;
        }

        /// <summary>
        /// Runs the code using copied user lists or cells so that thew actual project is not affected. Displays the result in the status box.
        /// </summary>
        public void TestCode()
        {
            if (CellContext is null)
            {
                Logger.Instance.Log($"Unable to test sort/filter functions for now :(");
                return;
            }

            var model = new CellFunctionModel("test", string.Empty, FunctionBeingEdited.Model.ReturnType);
            var function = new CellFunction(model);
            function.SetUserFriendlyCode(CurrentTextInEditor, CellContext, _collectionNameToDataTypeMap);
            var result = function.Run(_contextToTestWith);
            if (!result.WasSuccess)
            {
                Logger.Instance.Log($"Error occured in function during test: {result.ExecutionResult}");
                return;
            }
            _lastCompileResult = result;
            Logger.Instance.Log($"Test run complete!");
            Logger.Instance.Log($"Result: '{result.ReturnedObject?.ToString() ?? "<null>"}'");
        }

        private void ToggleSyntaxTreePreview(string code)
        {
            if (IsTransformedSyntaxTreeViewerVisible) HideSyntaxTreePreview();
            else ShowSyntaxTreePreview(code);
        }

        private void CloseWithoutSaving()
        {
            _isAllowingCloseWhileDirty = true;
            RequestClose?.Invoke();
        }

        private void HideSyntaxTreePreview()
        {
            SyntaxTreePreviewText = string.Empty;
        }

        private void ReloadFunctionCodeWhenItChanges(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellFunctionModel.Code)) CurrentTextInEditor = FunctionBeingEdited.GetUserFriendlyCode(CellContext, _collectionNameToDataTypeMap);
        }

        private void SaveAndClose()
        {
            Save();
            RequestClose?.Invoke();
        }

        private void Save()
        {
            FunctionBeingEdited.SetUserFriendlyCode(CurrentTextInEditor, CellContext, _collectionNameToDataTypeMap);
            _isDirty = false;
            NotifyPropertyChanged(nameof(ToolWindowTitle));
        }

        private void ShowSyntaxTreePreview(string code)
        {
            var model = new CellFunctionModel("test", "", FunctionBeingEdited.Model.ReturnType);
            var function = new CellFunction(model);
            if (CellContext is null) return;
            function.SetUserFriendlyCode(code, CellContext, _collectionNameToDataTypeMap);
            var syntaxTree = function.SyntaxTree;
            SyntaxTreePreviewText = syntaxTree.ToString();
        }

        internal void ShowContextHelpAtCarot(int offset)
        {
            string contextHelp;
            if (_cellFunctionCodeSuggestionGenerator.TryGetTypeAtTextPositionUsingSemanticAnalyzer(offset, out var type))
            {
                contextHelp = type?.Name ?? "Unknown type";
            }
            else if (offset > 0 && _cellFunctionCodeSuggestionGenerator.TryGetTypeAtTextPositionUsingSemanticAnalyzer(offset - 1, out type))
            {
                contextHelp = type?.Name ?? "Unknown type";
            }
            else if (!_lastCompileResult.WasSuccess)
            {
                contextHelp = _lastCompileResult.ExecutionResult ?? "";
            }
            else
            {
                contextHelp = string.Empty;
            }
            Logger.Instance.Log($"Info at carot positon {offset}: '{contextHelp}'");
        }

        internal IList<ICompletionData> CreateAutoCompleteSuggestions(int offset)
        {
            return _cellFunctionCodeSuggestionGenerator.CreateCompletionData(offset);
        }
    }
}
