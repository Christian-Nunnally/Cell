using Cell.ViewModel;

namespace Cell.View.ToolWindow
{
    internal interface IToolWindow
    {
        void Close();

        string GetTitle();

        List<CommandViewModel> GetToolBarCommands();
    }
}
