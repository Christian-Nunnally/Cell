using Cell.Execution;
using Cell.Model;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells.Types;
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
        public CodeEditorWindowViewModel(PluginFunction functionToBeEdited, CellModel? cellContextFromWhichTheFunctionIsBeingEdited, IReadOnlyDictionary<string, string> collectionNameToDataTypeMap)
        {
            FunctionBeingEdited = functionToBeEdited;
            _collectionNameToDataTypeMap = collectionNameToDataTypeMap;
            CellContext = cellContextFromWhichTheFunctionIsBeingEdited;

            DisplayResult(functionToBeEdited.CompileResult);
        }

        public CellModel? CellContext { get; private set; }

        public PluginFunction FunctionBeingEdited { get; private set; }

        public string FunctionReturnType => FunctionBeingEdited.Model.ReturnType;

        public bool IsTransformedSyntaxTreeViewerVisible => !string.IsNullOrWhiteSpace(SyntaxTreePreviewText);

        public Action? RequestClose { get; set; }

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

        public override string ToolWindowTitle
        {
            get
            {
                // TODO: fix dirty dot
                //var dirtyDot = _isDirty ? "*" : string.Empty;
                var dirtyDot = false ? "*" : string.Empty;
                var functionBeingEdited = FunctionBeingEdited.Model;
                var cellContext = CellContext;
                return cellContext == null ? $"Code Editor - {functionBeingEdited.Name}{dirtyDot}" : $"Code Editor - {functionBeingEdited.Name} - {ColumnCellViewModel.GetColumnName(cellContext.Column)}{cellContext.Row}";
            }
        }

        public string UserFriendlyCodeString { get => FunctionBeingEdited.GetUserFriendlyCode(CellContext, _collectionNameToDataTypeMap); set => FunctionBeingEdited.SetUserFriendlyCode(value, CellContext, _collectionNameToDataTypeMap); }

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
            var model = new PluginFunctionModel("testtesttest", string.Empty, FunctionReturnType);
            var function = new PluginFunction(model);
            function.SetUserFriendlyCode(code, CellContext, _collectionNameToDataTypeMap);
            var pluginContext = new PluginContext(ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.UserCollectionLoader, CellContext.Index);
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
            if (e.PropertyName == nameof(PluginFunctionModel.Code)) NotifyPropertyChanged(nameof(UserFriendlyCodeString));
        }

        private void ShowSyntaxTreePreview(string code)
        {
            var model = new PluginFunctionModel("testtesttest", "", FunctionReturnType);
            var function = new PluginFunction(model);
            if (CellContext is null) return;
            function.SetUserFriendlyCode(code, CellContext, _collectionNameToDataTypeMap);
            var syntaxTree = function.SyntaxTree;
            SyntaxTreePreviewText = syntaxTree.ToString();
        }
    }
}
