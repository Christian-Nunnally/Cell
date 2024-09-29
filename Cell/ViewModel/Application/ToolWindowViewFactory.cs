using Cell.Common;
using Cell.View.ToolWindow;
using Cell.ViewModel.ToolWindow;

namespace Cell.ViewModel.Application
{
    public class ToolWindowViewFactory
    {
        public static ResizableToolWindow Create(PropertyChangedBase viewModel)
        {
            return viewModel switch
            {
                CreateCollectionWindowViewModel convertedViewModel => new CreateCollectionWindow(convertedViewModel),
                CreateSheetWindowViewModel convertedViewModel => new CreateSheetWindow(convertedViewModel),
                CellContentEditWindowViewModel convertedViewModel => new CellContentEditWindow(convertedViewModel),
                CellFormatEditWindowViewModel convertedViewModel => new CellFormatEditWindow(convertedViewModel),
                CellSettingsEditWindowViewModel convertedViewModel => new CellSettingsEditWindow(convertedViewModel),
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
