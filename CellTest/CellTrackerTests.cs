
using Cell.Persistence;

namespace CellTest
{
    public class PersistanceManagerTests
    {
        [Fact]
        public void BasicLaunchTest()
        {
            var persistenceManager = new PersistenceManager("", new TestFileIO());
        }
    }
}