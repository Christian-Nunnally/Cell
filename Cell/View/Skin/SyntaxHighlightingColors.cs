using Cell.Plugin.SyntaxWalkers;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows.Media;

namespace Cell.View.Skin
{
    /// <summary>
    /// Provides a way to apply syntax highlighting colors to an AvalonEdit text editor.
    /// </summary>
    public static class SyntaxHighlightingColors
    {
        /// <summary>
        /// Sets the syntax highlighting colors for the given text editor to the cell defaults.
        /// </summary>
        /// <param name="editor">The editor to set.</param>
        public static void ApplySyntaxHighlightingToEditor(TextEditor editor, List<string> collectionNames)
        {
            var highlighting = editor.SyntaxHighlighting;
            if (highlighting.GetNamedColor("StringInterpolation") != null) highlighting.GetNamedColor("StringInterpolation").Foreground = new SimpleHighlightingBrush(Color.FromRgb(215, 200, 200));
            if (highlighting.GetNamedColor("Punctuation") != null) highlighting.GetNamedColor("Punctuation").Foreground = new SimpleHighlightingBrush(Color.FromRgb(215, 225, 225));
            if (highlighting.GetNamedColor("NumberLiteral") != null) highlighting.GetNamedColor("NumberLiteral").Foreground = new SimpleHighlightingBrush(Color.FromRgb(255, 180, 240));
            if (highlighting.GetNamedColor("Comment") != null) highlighting.GetNamedColor("Comment").Foreground = new SimpleHighlightingBrush(Color.FromRgb(128, 128, 128));
            if (highlighting.GetNamedColor("MethodCall") != null) highlighting.GetNamedColor("MethodCall").Foreground = new SimpleHighlightingBrush(Colors.CadetBlue);
            if (highlighting.GetNamedColor("GetSetAddRemove") != null) highlighting.GetNamedColor("GetSetAddRemove").Foreground = new SimpleHighlightingBrush(Colors.BlueViolet);
            if (highlighting.GetNamedColor("Visibility") != null) highlighting.GetNamedColor("Visibility").Foreground = new SimpleHighlightingBrush(Colors.LightPink);
            if (highlighting.GetNamedColor("ParameterModifiers") != null) highlighting.GetNamedColor("ParameterModifiers").Foreground = new SimpleHighlightingBrush(Colors.Gold);
            if (highlighting.GetNamedColor("Modifiers") != null) highlighting.GetNamedColor("Modifiers").Foreground = new SimpleHighlightingBrush(Colors.Gold);
            if (highlighting.GetNamedColor("String") != null) highlighting.GetNamedColor("String").Foreground = new SimpleHighlightingBrush(Color.FromRgb(220, 144, 220));
            if (highlighting.GetNamedColor("Char") != null) highlighting.GetNamedColor("Char").Foreground = new SimpleHighlightingBrush(Color.FromRgb(255, 180, 240));
            if (highlighting.GetNamedColor("Preprocessor") != null) highlighting.GetNamedColor("Preprocessor").Foreground = new SimpleHighlightingBrush(Colors.LightBlue);
            if (highlighting.GetNamedColor("TrueFalse") != null) highlighting.GetNamedColor("TrueFalse").Foreground = new SimpleHighlightingBrush(Color.FromRgb(255, 180, 240));
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
            if (!highlighting.MainRuleSet.Rules.Contains(CellLocationHighlightingRule)) highlighting.MainRuleSet.Rules.Add(CellLocationHighlightingRule);
            if (!highlighting.MainRuleSet.Rules.Contains(UserCollectionHighlightingRule)) highlighting.MainRuleSet.Rules.Remove(UserCollectionHighlightingRule);
            if (collectionNames.Count != 0)
            {
                UserCollectionHighlightingRule = GenerateUserCollectionHighlightRule(collectionNames);
                highlighting.MainRuleSet.Rules.Add(UserCollectionHighlightingRule);
            }
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

        public static HighlightingRule CellLocationHighlightingRule { get; set; } = new HighlightingRule
        {
            Regex = new System.Text.RegularExpressions.Regex(@"\b([a-zA-Z]*_[BCR]_[A-Z]+[0-9]+|[a-zA-Z]*_?[A-Z]+[0-9]+)\b"),
            Color = new HighlightingColor
            {
                Foreground = new SimpleHighlightingBrush(Color.FromRgb(144, 250, 144))
            },
        };

        public static HighlightingRule UserCollectionHighlightingRule { get; set; } = new HighlightingRule
        {
            Regex = new System.Text.RegularExpressions.Regex(@"\b([a-zA-Z]*_[BCR]_[A-Z]+[0-9]+|[a-zA-Z]*_?[A-Z]+[0-9]+)\b"),
            Color = new HighlightingColor
            {
                Foreground = new SimpleHighlightingBrush(Colors.Gold)
            },
        };

        public static HighlightingRule GenerateUserCollectionHighlightRule(IEnumerable<string> collectionNames) => new HighlightingRule
        {
            Regex = new System.Text.RegularExpressions.Regex(@"\b(" + string.Join('|', collectionNames) + @")\b"),
            Color = new HighlightingColor
            {
                Foreground = new SimpleHighlightingBrush(Color.FromRgb(144, 250, 144))
            },
        };
    }
}
