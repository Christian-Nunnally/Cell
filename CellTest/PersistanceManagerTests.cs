using Cell.Persistence;
using CellTest.TestUtilities;

namespace CellTest
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


