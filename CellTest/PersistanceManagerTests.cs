using Cell.Persistence;

namespace CellTest
{
    public class PersistanceManagerTests
    {
        [Fact]
        public void BasicLaunchTest()
        {
            var _ = new PersistenceManager("", new TestFileIO());
        }
    }
}


