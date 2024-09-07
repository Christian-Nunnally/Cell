
using System.Windows;

namespace Cell.ViewModel.Application
{
    public class TextClipboard : ITextClipboard
    {
        public void Clear() => Clipboard.Clear();

        public bool ContainsText() => Clipboard.ContainsText();

        public string GetText() => Clipboard.GetText();

        public void SetText(string text) => Clipboard.SetText(text);
    }
}
