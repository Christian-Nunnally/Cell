using Cell.Model;
using Cell.Plugin;
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
    public partial class CodeEditor : UserControl, INotifyPropertyChanged
    {
        private Action<string> onCloseCallback = x => { };
        private CellViewModel? _currentCell;
        private bool _doesFunctionReturnValue;

        public event PropertyChangedEventHandler? PropertyChanged;

#pragma warning disable CA1822 // Mark members as static. Justification: This property is used in XAML.
        public Dock UserSetDockOrientation => ApplicationSettings.Instance.CodeEditorDockPosition;
#pragma warning restore CA1822 // Mark members as static

        public Cursor DockOrientationCursor => UserSetDockOrientation == Dock.Left || UserSetDockOrientation == Dock.Right ? Cursors.SizeWE : Cursors.SizeNS;

        public double UserSetHeight { get; set; }

        public double UserSetWidth { get; set; }

        public double UserSetHeightOfResizeBar => double.IsNaN(UserSetHeight) ? double.NaN : 5;

        public double UserSetWidthOfResizeBar => double.IsNaN(UserSetWidth) ? double.NaN : 5;

        public string ResultString { get; set; } = string.Empty;

        public bool IsTransformedSyntaxTreeViewerVisible { get; set; }

        private CompileResult _lastCompileResult;

        public SolidColorBrush ResultStringColor => _lastCompileResult.Success ? Brushes.Black : Brushes.Red;

        public CodeEditor()
        {
            InitializeComponent();
            DataContext = this;
            Visibility = Visibility.Collapsed;
            UserSetWidth = ApplicationSettings.Instance.CodeEditorWidth;
            UserSetHeight = double.NaN;
            textEditor.TextArea.TextEntering += OnTextEntering;
            textEditor.TextArea.TextEntered += OnTextEntered;
        }

        private CompletionWindow? completionWindow;

        private void OnTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                TextArea textArea = textEditor.TextArea;
                var type = GetVariableTypePriorToCarot(textArea);
                completionWindow = CodeCompletionWindowFactory.Create(textArea, type);
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

        public void Show(string code, Action<string> callback, bool doesFunctionReturnValue, CellViewModel currentCell)
        {
            Close();
            _currentCell = currentCell;
            _doesFunctionReturnValue = doesFunctionReturnValue;
            textEditor.Text = code;
            onCloseCallback = callback;
            Visibility = Visibility.Visible;
            NotifyDockPropertiesChanged();
        }

        public void Close()
        {
            if (Visibility == Visibility.Visible) onCloseCallback(textEditor.Text);
            Visibility = Visibility.Collapsed;
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DockLeftButtonClicked(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.Instance.CodeEditorDockPosition = Dock.Left;
            NotifyDockPropertiesChanged();
        }

        private void DockRightButtonClicked(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.Instance.CodeEditorDockPosition = Dock.Right;
            NotifyDockPropertiesChanged();
        }

        private void DockTopButtonClicked(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.Instance.CodeEditorDockPosition = Dock.Top;
            NotifyDockPropertiesChanged();
        }

        private void DockBottomButtonClicked(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.Instance.CodeEditorDockPosition = Dock.Bottom;
            NotifyDockPropertiesChanged();
        }

        private void SetWdithAndHeightAccordingToDockPosition()
        {
            if (ApplicationSettings.Instance.CodeEditorDockPosition == Dock.Left || ApplicationSettings.Instance.CodeEditorDockPosition == Dock.Right)
            {
                UserSetHeight = double.NaN;
                UserSetWidth = ApplicationSettings.Instance.CodeEditorWidth;
            }
            else
            {
                UserSetHeight = ApplicationSettings.Instance.CodeEditorHeight;
                UserSetWidth = double.NaN;
            }
        }

        private void NotifyDockPropertiesChanged()
        {
            SetWdithAndHeightAccordingToDockPosition();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserSetDockOrientation)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserSetHeight)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserSetHeightOfResizeBar)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserSetWidth)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserSetWidthOfResizeBar)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DockOrientationCursor)));
        }

        private void TestCodeButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_currentCell is null) return;
            var function = new PluginFunction("testtesttest", textEditor.Text, !_doesFunctionReturnValue);
            var compiled = function.CompiledMethod;
            var result = function.CompileResult;
            if (result.Success)
            {
                if (_doesFunctionReturnValue)
                {
                    var resultObject = compiled?.Invoke(null, [new PluginContext(ApplicationViewModel.Instance), _currentCell.Model]);
                    result = new CompileResult { Success = true, Result = resultObject?.ToString() ?? "" };
                }
                else
                {
                    compiled?.Invoke(null, [new PluginContext(ApplicationViewModel.Instance), _currentCell.Model]);
                    result = new CompileResult { Success = true, Result = "Success" };
                }
            }

            DisplayResult(result);
        }

        private void DisplayResult(CompileResult result)
        {
            _lastCompileResult = result;
            ResultString = result.Result;
            ResultString = ResultString.Replace("Compilation failed, first error is", "Error");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultString)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultStringColor)));
        }

        private void ShowTransformedSyntaxTreeButtonClicked(object sender, RoutedEventArgs e)
        {
            if (IsTransformedSyntaxTreeViewerVisible)
            {
                IsTransformedSyntaxTreeViewerVisible = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTransformedSyntaxTreeViewerVisible)));
                return;
            }
            var function = new PluginFunction("testtesttest", textEditor.Text, !_doesFunctionReturnValue);
            var syntaxTree = function.SyntaxTree;
            syntaxTreePreviewViewer.Text = syntaxTree.ToString();
            IsTransformedSyntaxTreeViewerVisible = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTransformedSyntaxTreeViewerVisible)));
        }

        private bool _resizing;
        private Point _resizingStartPosition;

        private void CodeWindowResizerRectangleMouseDown(object sender, MouseButtonEventArgs e)
        {
            _resizing = true;
            _resizingStartPosition = e.GetPosition(this);
            Mouse.Capture(sender as IInputElement);
        }

        private void CodeWindowResizerRectangleMouseMove(object sender, MouseEventArgs e)
        {
            if (_resizing)
            {
                var mousePosition = e.GetPosition(this);
                if (UserSetDockOrientation == Dock.Left)
                {
                    var delta = DifferenceBetweenTwoPoints(_resizingStartPosition, mousePosition);
                    ApplicationSettings.Instance.CodeEditorWidth = Math.Max(50, ApplicationSettings.Instance.CodeEditorWidth - delta.X);
                }
                else if (UserSetDockOrientation == Dock.Right)
                {
                    var delta = DifferenceBetweenTwoPoints(_resizingStartPosition, mousePosition);
                    ApplicationSettings.Instance.CodeEditorWidth = Math.Max(50, ApplicationSettings.Instance.CodeEditorWidth + delta.X);
                }
                else if (UserSetDockOrientation == Dock.Top)
                {
                    var delta = DifferenceBetweenTwoPoints(_resizingStartPosition, mousePosition);
                    ApplicationSettings.Instance.CodeEditorHeight = Math.Max(50, ApplicationSettings.Instance.CodeEditorHeight - delta.Y);
                }
                else if (UserSetDockOrientation == Dock.Bottom)
                {
                    var delta = DifferenceBetweenTwoPoints(_resizingStartPosition, mousePosition);
                    ApplicationSettings.Instance.CodeEditorHeight = Math.Max(50, ApplicationSettings.Instance.CodeEditorHeight + delta.Y);
                }
                _resizingStartPosition = mousePosition;
                NotifyDockPropertiesChanged();
            }
        }

        private static Point DifferenceBetweenTwoPoints(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);

        private void CodeWindowResizerRectangleMouseUp(object sender, MouseButtonEventArgs e)
        {
            _resizing = false;
            Mouse.Capture(null);
        }
    }
}
