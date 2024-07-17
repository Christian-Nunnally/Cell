using Cell.Model;
using Cell.Plugin;
using Cell.ViewModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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

        public Dock UserSetDockOrientation { get; set; }

        public double UserSetHeight { get; set; }

        public double UserSetWidth { get; set; }

        public string ResultString { get; set; } = string.Empty;

        public bool IsTransformedSyntaxTreeViewerVisible { get; set; }

        private CompileResult _lastCompileResult;

        public SolidColorBrush ResultStringColor => _lastCompileResult.Success ? Brushes.Black : Brushes.Red;

        public CodeEditor()
        {
            InitializeComponent();
            DataContext = this;
            Visibility = Visibility.Collapsed;
            UserSetDockOrientation = Dock.Left;
            UserSetWidth = 200;
            UserSetHeight = double.NaN;
        }

        public void Show(string code, Action<string> callback, bool doesFunctionReturnValue, CellViewModel currentCell)
        {
            Close();
            _currentCell = currentCell;
            _doesFunctionReturnValue = doesFunctionReturnValue;
            textEditor.Text = code;
            onCloseCallback = callback;
            Visibility = Visibility.Visible;
        }

        public void Close()
        {
            if (Visibility == Visibility.Visible)
            {
                onCloseCallback(textEditor.Text);
            }
            Visibility = Visibility.Collapsed;
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DockLeftButtonClicked(object sender, RoutedEventArgs e)
        {
            UserSetDockOrientation = Dock.Left;
            UserSetHeight = double.NaN;
            UserSetWidth = 400;
            NotifyDockPropertiesChanged();
        }

        private void DockRightButtonClicked(object sender, RoutedEventArgs e)
        {
            UserSetDockOrientation = Dock.Right;
            UserSetHeight = double.NaN;
            UserSetWidth = 400;
            NotifyDockPropertiesChanged();
        }

        private void DockTopButtonClicked(object sender, RoutedEventArgs e)
        {
            UserSetDockOrientation = Dock.Top;
            UserSetHeight = 400;
            UserSetWidth = double.NaN;
            NotifyDockPropertiesChanged();
        }

        private void DockBottomButtonClicked(object sender, RoutedEventArgs e)
        {
            UserSetDockOrientation = Dock.Bottom;
            UserSetHeight = 400;
            UserSetWidth = double.NaN;
            NotifyDockPropertiesChanged();
        }

        private void NotifyDockPropertiesChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserSetDockOrientation)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserSetHeight)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserSetWidth)));
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
    }
}
