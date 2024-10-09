using Cell.Execution;
using Cell.Model;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using System.ComponentModel;
using System.Windows.Media;

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
        private string? syntaxTreePreviewText = string.Empty;
        private string _currentTextInEditor;

        /// <summary>
        /// Creates a new instance of <see cref="CodeEditorWindowViewModel"/>.
        /// </summary>
        /// <param name="functionToBeEdited">The function to be edited.</param>
        /// <param name="cellContextFromWhichTheFunctionIsBeingEdited">The cell context from which the function is ceing edited.</param>
        /// <param name="collectionNameToDataTypeMap"></param>
        public CodeEditorWindowViewModel(CellFunction functionToBeEdited, CellModel? cellContextFromWhichTheFunctionIsBeingEdited, IReadOnlyDictionary<string, string> collectionNameToDataTypeMap)
        {
            FunctionBeingEdited = functionToBeEdited;
            _currentTextInEditor = functionToBeEdited.GetUserFriendlyCode(cellContextFromWhichTheFunctionIsBeingEdited, collectionNameToDataTypeMap);
            _collectionNameToDataTypeMap = collectionNameToDataTypeMap;
            CellContext = cellContextFromWhichTheFunctionIsBeingEdited;

            DisplayResult(functionToBeEdited.CompileResult);
        }

        /// <summary>
        /// The cell context the function is currently being edited from.
        /// </summary>
        public CellModel? CellContext { get; private set; }

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
                NotifyPropertyChanged(nameof(CurrentTextInEditor));
            }
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 400;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 500;

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
        /// Gets or sets the result of the last compile and run of the function that is displayed in the UI.
        /// </summary>
        public string? ResultString { get; set; } = string.Empty;

        /// <summary>
        /// Gets the color the results string should be displayed in.
        /// </summary>
        public SolidColorBrush ResultStringColor => _lastCompileResult.WasSuccess ? new SolidColorBrush(ColorConstants.ForegroundColorConstant) : new SolidColorBrush(ColorConstants.ErrorForegroundColorConstant);

        /// <summary>
        /// Gets the syntax tree preview text to be displayed in the UI when the user requests to see the syntax tree.
        /// </summary>
        public string? SyntaxTreePreviewText
        {
            get => syntaxTreePreviewText; set
            {
                if (syntaxTreePreviewText == value) return;
                syntaxTreePreviewText = value;
                NotifyPropertyChanged(nameof(SyntaxTreePreviewText));
                NotifyPropertyChanged(nameof(IsTransformedSyntaxTreeViewerVisible));
            }
        }

        /// <summary>
        /// Provides a list of commands to display in the title bar of the tool window.
        /// </summary>
        public override List<CommandViewModel> ToolBarCommands =>
        [
            new CommandViewModel("Test Code", () => TestCode(CurrentTextInEditor)) { ToolTip = "Runs the current code and displays the result, or just 'success' if the function isn't supposed to return a value." },
            new CommandViewModel("Syntax", () => ToggleSyntaxTreePreview(CurrentTextInEditor)) { ToolTip = "Shows what the code looks like after references have been converted to 'real' code." },
            new CommandViewModel("Save and Close", SaveAndClose) { ToolTip = "Saves the edited code to the function and closes this tool window." }
        ];

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
            DialogFactory.ShowYesNoConfirmationDialog("Save Changes", "Do you want to save your changes?", SaveAndClose, CloseWithoutSaving);
            return false;
        }

        private void TestCode(string code)
        {
            if (CellContext is null) return;
            var model = new CellFunctionModel("testtesttest", string.Empty, FunctionBeingEdited.Model.ReturnType);
            var function = new CellFunction(model);
            function.SetUserFriendlyCode(code, CellContext, _collectionNameToDataTypeMap);
            var pluginContext = new Context(ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.UserCollectionLoader, CellContext.Index);
            var result = function.Run(pluginContext, CellContext);
            DisplayResult(result);
        }

        private void ToggleSyntaxTreePreview(string code)
        {
            if (IsTransformedSyntaxTreeViewerVisible) HideSyntaxTreePreview();
            ShowSyntaxTreePreview(code);
        }

        private void CloseWithoutSaving()
        {
            _isAllowingCloseWhileDirty = true;
            RequestClose?.Invoke();
        }

        private void DisplayResult(CompileResult result)
        {
            _lastCompileResult = result;
            ResultString = result.ExecutionResult ?? "";
            ResultString = ResultString.Replace("Compilation failed, first error is", "Error");
            if (result.ReturnedObject is not null) ResultString = result.ReturnedObject.ToString();
            NotifyPropertyChanged(nameof(ResultString));
            NotifyPropertyChanged(nameof(ResultStringColor));
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
            FunctionBeingEdited.SetUserFriendlyCode(CurrentTextInEditor, CellContext, _collectionNameToDataTypeMap);
            _isDirty = false;
            RequestClose?.Invoke();
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
    }
}
