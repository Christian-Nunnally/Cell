using Cell.Core.Persistence;

namespace CellTest.TestUtilities
{
    public class DictionaryFileIO : IFileIO
    {
        private readonly TestFileSystem _testFileSystem = new();

        public void CopyDirectory(string from, string to)
        {
            _testFileSystem.CopyDirectory(from, to);
        }

        public async Task CopyDirectoryAsync(string from, string to)
        {
            await _testFileSystem.CopyDirectoryAsync(from, to);
        }

        public void CreateDirectory(string path)
        {
            _testFileSystem.CreateDirectory(path);
        }

        public void DeleteDirectory(string path)
        {
            _testFileSystem.DeleteDirectory(path);
        }

        public void DeleteFile(string path)
        {
            _testFileSystem.DeleteFile(path);
        }

        public bool DirectoryExists(string path)
        {
            return _testFileSystem.GetDirectory(path) != null;
        }

        public bool Exists(string path)
        {
            var directoryPath = Path.GetDirectoryName(path);
            if (directoryPath is null) return false;
            var directory = _testFileSystem.GetDirectory(directoryPath);
            if (directory is null) return false;
            var fileName = Path.GetFileName(path);
            return directory.ContainsKey(fileName) && directory[fileName] is string;
        }

        public IEnumerable<string> GetDirectories(string path)
        {
            var directory = _testFileSystem.GetDirectory(path);
            return directory?.Where(x => x.Value is Dictionary<string, object>).Select(x => Path.Combine(path, x.Key)) ?? [];
        }

        public IEnumerable<string> GetFiles(string path)
        {
            var directory = _testFileSystem.GetDirectory(path);
            if (directory is null) return [];
            return directory.Where(x => x.Value is string).Select(x => Path.Combine(path, x.Key));
        }

        public void MoveDirectory(string from, string to)
        {
            throw new NotImplementedException();
        }

        public string ReadFile(string path)
        {
            return _testFileSystem.ReadFile(path) ?? throw new FileNotFoundException(path);
        }

        public Task<string> ReadFileAsync(string path)
        {
            return Task.FromResult(ReadFile(path));
        }

        public void WriteFile(string path, string version)
        {
            var directoryName = Path.GetDirectoryName(path);
            if (directoryName is null) return;
            _testFileSystem.CreateDirectory(directoryName);
            _testFileSystem.WriteFile(path, version);
        }

        public async Task ZipDirectoryAsync(string path, string zipPath)
        {
            CopyDirectory(path, zipPath);
        }

        internal void DeleteAll()
        {
            _testFileSystem.DeleteAll();
        }
    }
}