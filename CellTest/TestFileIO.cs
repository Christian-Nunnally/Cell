using Cell.Persistence;

namespace CellTest
{
    internal class TestFileIO : IFileIO
    {
        public void CopyDirectory(string oldFullPath, string newFullPath)
        {
            throw new NotImplementedException();
        }

        public void DeleteDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string versionPath)
        {
            throw new NotImplementedException();
        }

        public void MoveDirectory(string oldFullPath, string newFullPath)
        {
            throw new NotImplementedException();
        }

        public string ReadFile(string path)
        {
            throw new NotImplementedException();
        }

        public void WriteFile(string versionPath, string version)
        {
            throw new NotImplementedException();
        }
    }
}