using Cell.Core.Persistence;
using CellTest.TestUtilities;

namespace CellTest.Core.Persistence
{
    public class PersistedDirectoryTests
    {
        private DictionaryFileIO _testFileIO;
        private PersistedDirectory _testing;

        public PersistedDirectoryTests()
        {
            _testFileIO = new DictionaryFileIO();
            _testing = new PersistedDirectory("", _testFileIO);
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void CreatedWithPath_DirectoryCreatedAtThatPath()
        {
            _testing = new PersistedDirectory("Directory", _testFileIO);

            Assert.True(_testFileIO.DirectoryExists("Directory"));
        }
    }
}
