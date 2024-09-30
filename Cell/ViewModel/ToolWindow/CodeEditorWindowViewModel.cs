using Cell.Execution;
using Cell.Model;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using System.ComponentModel;
using System.Windows.Media;

namespace Cell.ViewModel.ToolWindow
{
    public class CodeEditorWindowViewModel : ToolWindowViewModel
    {
        private readonly IReadOnlyDictionary<string, string> _collectionNameToDataTypeMap;
        private CompileResult _lastCompileResult;
        private string? syntaxTreePreviewText = string.Empty;
        private bool _isAllowingCloseWhileDirty = false;
        private bool _isDirty = false;
        public CodeEditorWindowViewModel(CellFunction functionToBeEdited, CellModel? cellContextFromWhichTheFunctionIsBeingEdited, IReadOnlyDictionary<string, string> collectionNameToDataTypeMap)
        {
            FunctionBeingEdited = functionToBeEdited;
            _collectionNameToDataTypeMap = collectionNameToDataTypeMap;
            CellContext = cellContextFromWhichTheFunctionIsBeingEdited;

            DisplayResult(functionToBeEdited.CompileResult);
        }

        public override bool HandleCloseRequested()
        {
            if (!_isDirty || _isAllowingCloseWhileDirty) return true;
            DialogFactory.ShowYesNoConfirmationDialog("Save Changes", "Do you want to save your changes?", SaveAndClose, CloseWithoutSaving);
            return false;
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

        public override double MinimumHeight => 200;

        public override double MinimumWidth => 200;

        public override double DefaultHeight => 400;

        public override double DefaultWidth => 500;

        public CellModel? CellContext { get; private set; }

        public CellFunction FunctionBeingEdited { get; private set; }

        public string FunctionReturnType => FunctionBeingEdited.Model.ReturnType;

        public bool IsTransformedSyntaxTreeViewerVisible => !string.IsNullOrWhiteSpace(SyntaxTreePreviewText);

        public double ResultColumnWidth => ResultString == string.Empty ? 0 : 200;

        public string? ResultString { get; set; } = string.Empty;

        public SolidColorBrush ResultStringColor => _lastCompileResult.WasSuccess ? new SolidColorBrush(ColorConstants.ForegroundColorConstant) : new SolidColorBrush(ColorConstants.ErrorForegroundColorConstant);

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
                if (cellContext is not null) title += $" : {cellContext.UserFriendlyCellName}";
                return title;
            }
        }

        public string UserFriendlyCodeString { get => FunctionBeingEdited.GetUserFriendlyCode(CellContext, _collectionNameToDataTypeMap); set => FunctionBeingEdited.SetUserFriendlyCode(value, CellContext, _collectionNameToDataTypeMap); }
        public bool IsDirty { get => _isDirty; internal set => _isDirty = value; }
        public string CurrentTextInEditor { get; internal set; }

        public override void HandleBeingClosed()
        {
            base.HandleBeingClosed();
            FunctionBeingEdited.Model.PropertyChanged -= ReloadFunctionCodeWhenItChanges;
        }

        public override void HandleBeingShown()
        {
            base.HandleBeingShown();
            FunctionBeingEdited.Model.PropertyChanged -= ReloadFunctionCodeWhenItChanges;
            NotifyPropertyChanged(nameof(UserFriendlyCodeString));
        }

        public void TestCode(string code)
        {
            if (CellContext is null) return;
            var model = new CellFunctionModel("testtesttest", string.Empty, FunctionReturnType);
            var function = new CellFunction(model);
            function.SetUserFriendlyCode(code, CellContext, _collectionNameToDataTypeMap);
            var pluginContext = new Context(ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.UserCollectionLoader, CellContext.Index);
            var result = function.Run(pluginContext, CellContext);
            DisplayResult(result);
        }

        public void ToggleSyntaxTreePreview(string code)
        {
            if (IsTransformedSyntaxTreeViewerVisible) HideSyntaxTreePreview();
            ShowSyntaxTreePreview(code);
        }

        private void DisplayResult(CompileResult result)
        {
            _lastCompileResult = result;
            ResultString = result.ExecutionResult ?? "";
            ResultString = ResultString.Replace("Compilation failed, first error is", "Error");
            if (result.ReturnedObject is not null) ResultString = result.ReturnedObject.ToString();
            NotifyPropertyChanged(nameof(ResultString));
            NotifyPropertyChanged(nameof(ResultStringColor));
            NotifyPropertyChanged(nameof(ResultColumnWidth));
        }

        private void HideSyntaxTreePreview()
        {
            SyntaxTreePreviewText = string.Empty;
        }

        private void ReloadFunctionCodeWhenItChanges(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellFunctionModel.Code)) NotifyPropertyChanged(nameof(UserFriendlyCodeString));
        }

        private void ShowSyntaxTreePreview(string code)
        {
            var model = new CellFunctionModel("testtesttest", "", FunctionReturnType);
            var function = new CellFunction(model);
            if (CellContext is null) return;
            function.SetUserFriendlyCode(code, CellContext, _collectionNameToDataTypeMap);
            var syntaxTree = function.SyntaxTree;
            SyntaxTreePreviewText = syntaxTree.ToString();
        }

        private void CloseWithoutSaving()
        {
            _isAllowingCloseWhileDirty = true;
            RequestClose?.Invoke();
        }

        private void SaveAndClose()
        {
            UserFriendlyCodeString = CurrentTextInEditor;
            _isDirty = false;
            RequestClose?.Invoke();
        }
    }
}
