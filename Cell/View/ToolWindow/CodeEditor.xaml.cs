using Cell.Model;
using Cell.Model.Plugin;
using Cell.Persistence;
using Cell.Plugin;
using Cell.View.Skin;
using Cell.View.ToolWindow;
using Cell.ViewModel;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cell.View
{
    /// <summary>
    /// Interaction logic for CodeEditor.xaml
    /// </summary>
    public partial class CodeEditor : UserControl, INotifyPropertyChanged, IResizableToolWindow
    {
        private readonly Action<string> onCloseCallback = x => { };
        private readonly CellModel? _currentCell;
        private readonly bool _doesFunctionReturnValue;
        private readonly PluginFunctionViewModel _function;

        public event PropertyChangedEventHandler? PropertyChanged;

        public double UserSetHeight { get; set; }

        public double UserSetWidth { get; set; }

        public string ResultString { get; set; } = string.Empty;

        public bool IsTransformedSyntaxTreeViewerVisible { get; set; }

        private CompileResult _lastCompileResult;

        public SolidColorBrush ResultStringColor => _lastCompileResult.Success ? Brushes.Black : Brushes.Red;

        public Action? RequestClose { get; set; }

        private static bool _haveAssembliesBeenRegistered;

        public CodeEditor(PluginFunctionViewModel function, Action<string> callback, CellModel? currentCell)
        {
            DataContext = this;
            InitializeComponent();
            SyntaxHighlightingColors.ApplySyntaxHighlightingToEditor(textEditor);
            SyntaxHighlightingColors.ApplySyntaxHighlightingToEditor(syntaxTreePreviewViewer);

            Visibility = Visibility.Collapsed;
            UserSetWidth = ApplicationSettings.Instance.CodeEditorWidth;
            UserSetHeight = ApplicationSettings.Instance.CodeEditorHeight;
            textEditor.TextArea.TextEntering += OnTextEntering;
            textEditor.TextArea.TextEntered += OnTextEntered;
            _function = function;
            _currentCell = currentCell;
            _doesFunctionReturnValue = function.Model.ReturnType != "void";
            textEditor.Text = function.GetUserFriendlyCode(currentCell); ;
            onCloseCallback = callback;
            Visibility = Visibility.Visible;
            NotifyDockPropertiesChanged();

            if (!_haveAssembliesBeenRegistered)
            {
                CodeCompletionWindowFactory.RegisterTypesInAssembly(typeof(TodoItem).Assembly);
                _haveAssembliesBeenRegistered = true;
            }
        }

        private CompletionWindow? completionWindow;

        private void OnTextEntered(object sender, TextCompositionEventArgs e)
        {
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

        private static string GetVariableTypePriorToCarot(TextArea textArea)
        {
            var offset = textArea.Caret.Offset - 1;
            var text = textArea.Document.Text;
            while (offset > 0 && char.IsLetterOrDigit(text[offset - 1])) offset--;
            return text[offset..(textArea.Caret.Offset - 1)];
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
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        private void NotifyDockPropertiesChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserSetHeight)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserSetWidth)));
        }

        private void TestCode()
        {
            if (_currentCell is null) return;
            var model = new PluginFunctionModel("testtesttest", string.Empty, !_doesFunctionReturnValue ? "void" : "object");
            var function = new PluginFunctionViewModel(model);
            function.SetUserFriendlyCode(textEditor.Text, _currentCell);
            var compiled = function.CompiledMethod;
            var result = function.CompileResult;
            if (result.Success)
            {
                if (_doesFunctionReturnValue)
                {
                    var resultObject = compiled?.Invoke(null, [new PluginContext(ApplicationViewModel.Instance, _currentCell.Index), _currentCell]);
                    result = new CompileResult { Success = true, Result = resultObject?.ToString() ?? "" };
                }
                else
                {
                    compiled?.Invoke(null, [new PluginContext(ApplicationViewModel.Instance, _currentCell.Index), _currentCell]);
                    result = new CompileResult { Success = true, Result = "Success" };
                }
            }

            DisplayResult(result);
        }

        private void DisplayResult(CompileResult result)
        {
            _lastCompileResult = result;
            ResultString = result.Result ?? "null";
            ResultString = ResultString.Replace("Compilation failed, first error is", "Error");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultString)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultStringColor)));
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
            var function = new PluginFunctionViewModel(model);
            if (_currentCell is null) return;
            function.SetUserFriendlyCode(textEditor.Text, _currentCell);
            var syntaxTree = function.SyntaxTree;
            syntaxTreePreviewViewer.Text = syntaxTree.ToString();
            IsTransformedSyntaxTreeViewerVisible = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTransformedSyntaxTreeViewerVisible)));
        }

        public void HandleBeingClosed()
        {
            onCloseCallback?.Invoke(textEditor.Text);
        }

        public double GetWidth()
        {
            return ApplicationSettings.Instance.CodeEditorWidth;
        }

        public double GetHeight()
        {
            return ApplicationSettings.Instance.CodeEditorHeight;
        }

        public void SetWidth(double width)
        {
            ApplicationSettings.Instance.CodeEditorWidth = width;
            UserSetWidth = width;
            NotifyDockPropertiesChanged();
        }

        public void SetHeight(double height)
        {
            ApplicationSettings.Instance.CodeEditorHeight = height;
            UserSetHeight = height;
            NotifyDockPropertiesChanged();
        }

        public string GetTitle()
        {
            return _currentCell == null ? "" : $"{_function.Model.Name} - {ColumnCellViewModel.GetColumnName(_currentCell.Column)}{_currentCell.Row}";
        }

        public List<CommandViewModel> GetToolBarCommands() => [
            new CommandViewModel("Test Code", new RelayCommand(x => true, x => TestCode())),
            new CommandViewModel("Syntax", new RelayCommand(x => true, x => ToggleSyntaxTreePreview()))
            ];
    }
}
