using Cell.Core.Persistence;
using CellTest.TestUtilities;

namespace CellTest.Core.Execution
{
    public class PersistanceManagerTests
    {
        [Fact]
        public void BasicLaunchTest()
        {
            var _ = new PersistedDirectory("", new DictionaryFileIO());
        }
    }
}


