using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View
{
    /// <summary>
    /// Interaction logic for EditCellPanel.xaml
    /// </summary>
    public partial class TypeSpecificEditCellPanel : UserControl
    {
        public TypeSpecificEditCellPanel()
        {
            InitializeComponent();
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}
