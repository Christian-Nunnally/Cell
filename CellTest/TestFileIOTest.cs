
using CellTest.TestUtilities;

namespace CellTest
{
    public class TestFileIOTest
    {
        [Fact]
        public void BasicLaunchTest()
        {
            var _ = new TestFileIO();
        }

        [Fact]
        public void WriteFile_Runs()
        {
            var testing = new TestFileIO();
            testing.WriteFile("test", "test");
        }

        [Fact]
        public void ReadFileBeforeWriting_ThrowsFileNotFoundException()
        {
            var testing = new TestFileIO();
            var path = "test";

            Assert.Throws<FileNotFoundException>(() => testing.ReadFile(path));
        }

        [Fact]
        public void ReadFileAfterWriting_Runs()
        {
            var testing = new TestFileIO();
            var path = "test";
            testing.WriteFile(path, string.Empty);
            var _ = testing.ReadFile(path);
        }

        [Fact]
        public void FileWritten_ReadFile_ContentsReturned()
        {
            var testing = new TestFileIO();
            var path = "test";
            var content = "content";
            testing.WriteFile(path, content);
            var result = testing.ReadFile(path);
            Assert.Equal(content, result);
        }
    }
}
