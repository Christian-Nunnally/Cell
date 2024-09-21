namespace Cell.Persistence
{
    public interface ITextClipboard
    {
        void Clear();

        bool ContainsText();

        string GetText();

        void SetText(string text);
    }
}
