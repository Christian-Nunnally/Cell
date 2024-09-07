
namespace Cell.ViewModel.Application
{
    public interface ITextClipboard
    {
        void Clear();

        bool ContainsText();

        string GetText();

        void SetText(string text);
    }
}
