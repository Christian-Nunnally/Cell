namespace Cell.View.ToolWindow
{
    internal interface IResizableToolWindow : IToolWindow
    {
        public double GetHeight();

        public double GetWidth();

        public void SetHeight(double height);

        public void SetWidth(double width);
    }
}
