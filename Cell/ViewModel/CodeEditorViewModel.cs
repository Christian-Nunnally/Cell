using Cell.View;

namespace Cell.ViewModel
{
    internal static class CodeEditorViewModel
    {
        private static CodeEditor? _codeEditor;

        public static void SetCodeEditorView(CodeEditor codeEditor)
        {
            _codeEditor = codeEditor;
        }

        public static void Show(string code, Action<string> callback, bool doesFunctionReturnValue, CellViewModel currentCell)
        {
            _codeEditor?.Show(code, callback, doesFunctionReturnValue, currentCell);
        }
    }
}
