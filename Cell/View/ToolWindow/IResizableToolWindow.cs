
namespace Cell.View.ToolWindow
{
    internal interface IResizableToolWindow : IToolWindow
    {
        public double GetWidth();

        public double GetHeight();

        public void SetWidth(double width);

        public void SetHeight(double height);
    }
}
