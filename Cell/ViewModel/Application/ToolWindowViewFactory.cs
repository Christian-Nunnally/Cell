using Cell.Core.Common;
using Cell.View.ToolWindow;
using Cell.ViewModel.ToolWindow;

namespace Cell.ViewModel.Application
{
    /// <summary>
    /// Factory for creating tool window views for thier view models.
    /// </summary>
    public class ToolWindowViewFactory
    {
        /// <summary>
        /// Creates a new instance of a tool window view for the given view model.
        /// </summary>
        /// <param name="viewModel">The view model to create the view for.</param>
        /// <returns>The newly constructed view.</returns>
        /// <exception cref="CellError">If the view doesn't exist for the given type.</exception>
        public static ResizableToolWindow Create(PropertyChangedBase viewModel)
        {
            return viewModel switch
            {
                CreateCollectionWindowViewModel convertedViewModel => new CreateCollectionWindow(convertedViewModel),
                CreateSheetWindowViewModel convertedViewModel => new CreateSheetWindow(convertedViewModel),
                CellContentEditWindowViewModel convertedViewModel => new CellContentEditWindow(convertedViewModel),
                CellFormatEditWindowViewModel convertedViewModel => new CellFormatEditWindow(convertedViewModel),
                CollectionManagerWindowViewModel convertedViewModel => new CollectionManagerWindow(convertedViewModel),
                ExportWindowViewModel convertedViewModel => new ExportWindow(convertedViewModel),
                FunctionManagerWindowViewModel convertedViewModel => new FunctionManagerWindow(convertedViewModel),
                ImportWindowViewModel convertedViewModel => new ImportWindow(convertedViewModel),
                LogWindowViewModel convertedViewModel => new LogWindow(convertedViewModel),
                SettingsWindowViewModel convertedViewModel => new SettingsWindow(convertedViewModel),
                SheetManagerWindowViewModel convertedViewModel => new SheetManagerWindow(convertedViewModel),
                UndoRedoStackWindowViewModel convertedViewModel => new UndoRedoStackWindow(convertedViewModel),
                CodeEditorWindowViewModel convertedViewModel => new CodeEditorWindow(convertedViewModel),
                DialogWindowViewModel convertedViewModel => new DialogWindow(convertedViewModel),
                _ => throw new CellError($"Unable to create window view for view model {viewModel.GetType().FullName}. Add a type mapping to {nameof(ToolWindowViewFactory)} for {viewModel.GetType().FullName}."),
            };
        }
    }
}
