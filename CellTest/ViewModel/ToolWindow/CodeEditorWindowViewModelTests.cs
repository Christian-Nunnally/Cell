using Cell.Data;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.ToolWindow;
using CellTest.TestUtilities;
using System.Collections.ObjectModel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest.ViewModel.ToolWindow
{
    public class CodeEditorWindowViewModelTests
    {
        private CellTracker _cellTracker;
        private TestFileIO _testFileIO;
        private PersistedDirectory _persistenceManager;
        private CellLoader _cellLoader;
        private ObservableCollection<CellModel> _cellsToEdit;
        private PluginFunctionLoader _pluginFunctionLoader;

        private CellFormatEditWindowViewModel CreateInstance()
        {
            _testFileIO = new TestFileIO();
            _persistenceManager = new PersistedDirectory("", _testFileIO);
            _cellLoader = new CellLoader(_persistenceManager);
            _cellTracker = new CellTracker(_cellLoader);
            _cellsToEdit = [];
            _pluginFunctionLoader = new PluginFunctionLoader(_persistenceManager);
            return new CellFormatEditWindowViewModel(_cellsToEdit, _cellTracker, _pluginFunctionLoader);
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.