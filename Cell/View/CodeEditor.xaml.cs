using System.Windows;
using System.Windows.Controls;

namespace Cell.View
{
    /// <summary>
    /// Interaction logic for CodeEditor.xaml
    /// </summary>
    public partial class CodeEditor : UserControl
    {
        private Action<string> onCloseCallback = x => { };

        public CodeEditor()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }

        public void Show(string code, Action<string> callback)
        {
            Close();
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
    }
}
