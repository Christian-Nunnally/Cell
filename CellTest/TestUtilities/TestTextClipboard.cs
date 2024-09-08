using Cell.Persistence;

namespace CellTest.TestUtilities
{
    internal class TestTextClipboard : ITextClipboard
    {
        private string? _textInClipboard;

        public void Clear() => _textInClipboard = null;

        public bool ContainsText() => _textInClipboard != null;

        public string GetText() => _textInClipboard ?? string.Empty;

        public void SetText(string text) => _textInClipboard = text;
    }
}