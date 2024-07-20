using Cell.Model.Plugin;
using Cell.Model;
using Cell.Plugin.SyntaxRewriters;
using Cell.Plugin;
using Cell.View;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;

namespace Cell.ViewModel
{
    internal class CodeCompletionWindowFactory
    {
        public static CompletionWindow? Create(TextArea textArea, string type)
        {
            if (type == "c")
            {
                var completionWindow = new CompletionWindow(textArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                data.Add(new PluginContextCompletionData("GoToSheet"));
                data.Add(new PluginContextCompletionData("GoToCell"));
                data.Add(new PluginContextCompletionData("SheetNames"));
                return completionWindow;
            }
            else if (FindAndReplaceCellLocationsSyntaxRewriter.IsCellLocation(type))
            {
                var completionWindow = new CompletionWindow(textArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                foreach (var property in typeof(CellModel).GetProperties())
                {
                    data.Add(new PluginContextCompletionData(property.Name));
                }
                return completionWindow;
            }
            else if (FindAndReplaceCollectionReferencesSyntaxWalker.IsCollectionName(type))
            {
                var completionWindow = new CompletionWindow(textArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                foreach (var property in typeof(UserList<PluginModel>).GetMethods())
                {
                    data.Add(new PluginContextCompletionData(property.Name));
                }
                return completionWindow;
            }
            return null;
        }
    }
}
