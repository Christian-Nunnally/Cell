using Cell.Data;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Application;
using CellTest.TestUtilities;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest
{
    public class CellClipboardTests
    {
        private CellTracker _cellTracker;
        private UndoRedoManager _undoRedoManager;
        private TestFileIO _testFileIO;
        private PersistenceManager _persistenceManager;
        private CellLoader _cellLoader;
        private ITextClipboard _textClipboard;

        private CellClipboard CreateInstance()
        {
            _testFileIO = new TestFileIO();
            _persistenceManager = new PersistenceManager("", _testFileIO);
            _cellLoader = new CellLoader(_persistenceManager);
            _cellTracker = new CellTracker(_cellLoader);
            _undoRedoManager = new UndoRedoManager(_cellTracker);
            _textClipboard = new TestTextClipboard();
            return new CellClipboard(_undoRedoManager, _cellTracker, _textClipboard);
        }

        [Fact]
        public void BasicLaunchTest()
        {
            var _ = CreateInstance();
        }

        [Fact]
        public void TwoCells_CellACopiedToCellB_TitleIsCopied()
        {
            var testing = CreateInstance();
            var cellToCopy = new CellModel() { Text = "wololo" };
            var cellToPasteInto = new CellModel();
            testing.CopyCells([cellToCopy], false);
            Assert.NotEqual(cellToCopy.Text, cellToPasteInto.Text);

            testing.PasteIntoCells(cellToPasteInto, [cellToPasteInto]);

            Assert.Equal(cellToCopy.Text, cellToPasteInto.Text);
        }

        [Fact]
        public void TwoCells_CellACopiedToCellB_FontSizeIsCopied()
        {
            var testing = CreateInstance();
            var cellToCopy = new CellModel() { FontSize = 20 };
            var cellToPasteInto = new CellModel();
            testing.CopyCells([cellToCopy], false);
            Assert.NotEqual(cellToCopy.FontSize, cellToPasteInto.FontSize);

            testing.PasteIntoCells(cellToPasteInto, [cellToPasteInto]);

            Assert.Equal(cellToCopy.FontSize, cellToPasteInto.FontSize);
        }

        [Fact]
        public void NormalTextInClipboard_PasteIntoCell_TextOfCellIsSetToTextInClipboard()
        {
            var testing = CreateInstance();
            var expectedText = "wololo";
            _textClipboard.SetText(expectedText);
            var cellToPasteInto = new CellModel();
            Assert.NotEqual(expectedText, cellToPasteInto.Text);

            testing.PasteIntoCells(cellToPasteInto, [cellToPasteInto]);

            Assert.Equal(expectedText, cellToPasteInto.Text);
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.