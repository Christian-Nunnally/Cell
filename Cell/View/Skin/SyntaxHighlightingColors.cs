
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit;
using System.Windows.Media;

namespace Cell.View.Skin
{
    internal static class SyntaxHighlightingColors
    {
        public static void ApplySyntaxHighlightingToEditor(TextEditor editor)
        {
            var highlighting = editor.SyntaxHighlighting;
            if (highlighting.GetNamedColor("StringInterpolation") != null) highlighting.GetNamedColor("StringInterpolation").Foreground = new SimpleHighlightingBrush(Color.FromRgb(215, 200, 200));
            if (highlighting.GetNamedColor("Punctuation") != null) highlighting.GetNamedColor("Punctuation").Foreground = new SimpleHighlightingBrush(Color.FromRgb(215, 225, 225));
            if (highlighting.GetNamedColor("NumberLiteral") != null) highlighting.GetNamedColor("NumberLiteral").Foreground = new SimpleHighlightingBrush(Colors.Orange);
            if (highlighting.GetNamedColor("Comment") != null) highlighting.GetNamedColor("Comment").Foreground = new SimpleHighlightingBrush(Colors.Purple);
            if (highlighting.GetNamedColor("MethodCall") != null) highlighting.GetNamedColor("MethodCall").Foreground = new SimpleHighlightingBrush(Colors.LightSeaGreen);
            if (highlighting.GetNamedColor("GetSetAddRemove") != null) highlighting.GetNamedColor("GetSetAddRemove").Foreground = new SimpleHighlightingBrush(Colors.BlueViolet);
            if (highlighting.GetNamedColor("Visibility") != null) highlighting.GetNamedColor("Visibility").Foreground = new SimpleHighlightingBrush(Colors.LightPink);
            if (highlighting.GetNamedColor("ParameterModifiers") != null) highlighting.GetNamedColor("ParameterModifiers").Foreground = new SimpleHighlightingBrush(Colors.Gold);
            if (highlighting.GetNamedColor("Modifiers") != null) highlighting.GetNamedColor("Modifiers").Foreground = new SimpleHighlightingBrush(Colors.Gold);
            if (highlighting.GetNamedColor("String") != null) highlighting.GetNamedColor("String").Foreground = new SimpleHighlightingBrush(Colors.LightSeaGreen);
            if (highlighting.GetNamedColor("Char") != null) highlighting.GetNamedColor("Char").Foreground = new SimpleHighlightingBrush(Colors.Orange);
            if (highlighting.GetNamedColor("Preprocessor") != null) highlighting.GetNamedColor("Preprocessor").Foreground = new SimpleHighlightingBrush(Colors.LightBlue);
            if (highlighting.GetNamedColor("TrueFalse") != null) highlighting.GetNamedColor("TrueFalse").Foreground = new SimpleHighlightingBrush(Colors.Orange);
            if (highlighting.GetNamedColor("Keywords") != null) highlighting.GetNamedColor("Keywords").Foreground = new SimpleHighlightingBrush(Colors.MediumPurple);
            if (highlighting.GetNamedColor("ValueTypeKeywords") != null) highlighting.GetNamedColor("ValueTypeKeywords").Foreground = new SimpleHighlightingBrush(Colors.LightBlue);
            if (highlighting.GetNamedColor("SemanticKeywords") != null) highlighting.GetNamedColor("SemanticKeywords").Foreground = new SimpleHighlightingBrush(Colors.LightBlue);
            if (highlighting.GetNamedColor("NamespaceKeywords") != null) highlighting.GetNamedColor("NamespaceKeywords").Foreground = new SimpleHighlightingBrush(Colors.LightBlue);
            if (highlighting.GetNamedColor("ReferenceTypeKeywords") != null) highlighting.GetNamedColor("ReferenceTypeKeywords").Foreground = new SimpleHighlightingBrush(Colors.LightGreen);
            if (highlighting.GetNamedColor("ThisOrBaseReference") != null) highlighting.GetNamedColor("ThisOrBaseReference").Foreground = new SimpleHighlightingBrush(Colors.LightBlue);
            if (highlighting.GetNamedColor("NullOrValueKeywords") != null) highlighting.GetNamedColor("NullOrValueKeywords").Foreground = new SimpleHighlightingBrush(Colors.LightBlue);
            if (highlighting.GetNamedColor("GotoKeywords") != null) highlighting.GetNamedColor("GotoKeywords").Foreground = new SimpleHighlightingBrush(Colors.LightBlue);
            if (highlighting.GetNamedColor("ContextKeywords") != null) highlighting.GetNamedColor("ContextKeywords").Foreground = new SimpleHighlightingBrush(Colors.LightBlue);
            if (highlighting.GetNamedColor("ExceptionKeywords") != null) highlighting.GetNamedColor("ExceptionKeywords").Foreground = new SimpleHighlightingBrush(Colors.LightBlue);
            if (highlighting.GetNamedColor("CheckedKeyword") != null) highlighting.GetNamedColor("CheckedKeyword").Foreground = new SimpleHighlightingBrush(Colors.LightBlue);
            if (highlighting.GetNamedColor("UnsafeKeywords") != null) highlighting.GetNamedColor("UnsafeKeywords").Foreground = new SimpleHighlightingBrush(Colors.LightBlue);
            if (highlighting.GetNamedColor("OperatorKeywords") != null) highlighting.GetNamedColor("OperatorKeywords").Foreground = new SimpleHighlightingBrush(Colors.LightBlue);
            if (highlighting.GetNamedColor("SemanticKeywords") != null) highlighting.GetNamedColor("SemanticKeywords").Foreground = new SimpleHighlightingBrush(Colors.MediumPurple);

            // Json
            if (highlighting.GetNamedColor("Bool") != null) highlighting.GetNamedColor("Bool").Foreground = new SimpleHighlightingBrush(Colors.LightGreen);
            if (highlighting.GetNamedColor("Number") != null) highlighting.GetNamedColor("Number").Foreground = new SimpleHighlightingBrush(Colors.LightPink);
            if (highlighting.GetNamedColor("Null") != null) highlighting.GetNamedColor("Null").Foreground = new SimpleHighlightingBrush(Colors.MediumPurple);
            if (highlighting.GetNamedColor("FieldName") != null) highlighting.GetNamedColor("FieldName").Foreground = new SimpleHighlightingBrush(Colors.LightGray);

            foreach (var color in highlighting.NamedHighlightingColors)
            {
                color.FontWeight = null;
            }
            editor.SyntaxHighlighting = null;
            editor.SyntaxHighlighting = highlighting;
        }
    }
}
