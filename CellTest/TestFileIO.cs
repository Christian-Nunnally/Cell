using Cell.Persistence;

namespace CellTest
{
    internal class TestFileIO : IFileIO
    {
        private readonly Dictionary<string, string> _files = [];

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

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string versionPath)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDirectories(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFiles(string path)
        {
            throw new NotImplementedException();
        }

        public void MoveDirectory(string oldFullPath, string newFullPath)
        {
            throw new NotImplementedException();
        }

        public string ReadFile(string path)
        {
            if (!_files.TryGetValue(path, out string? value)) throw new FileNotFoundException();
            return value;
        }

        public void WriteFile(string versionPath, string version)
        {
            if (!_files.TryAdd(versionPath, version))
            {
                _files[versionPath] = version;
            }
        }

        public void ZipDirectory(string path, string zipPath)
        {
            throw new NotImplementedException();
        }
    }
}