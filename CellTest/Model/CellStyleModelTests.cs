using Cell.Model;
using CellTest.TestUtilities;

namespace CellTest.Model
{
    public class CellStyleModelTests
    {
        [Fact]
        public void BasicLaunchTest()
        {
            var _ = new CellStyleModel();
        }

        [Fact]
        public void PropertyChangeTest_MarginPropertySet_Notified()
        {
            var testing = new CellStyleModel();
            var propertyChangedTester = new PropertyChangedTester(testing);

            testing.ContentMargin = "2";

            propertyChangedTester.AssertPropertyChanged(nameof(testing.ContentMargin));
        }
    }
}