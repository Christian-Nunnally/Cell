
namespace CellTest.TestUtilities
{
    internal class TestFileSystem
    {
        private readonly Dictionary<string, object> _root = [];
        public void CreateDirectory(string path)
        {
            var directories = path.Split('/');
            var currentDir = _root;
            for (int i = 0; i < directories.Length; i++)
            {
                var dir = directories[i];
                if (string.IsNullOrEmpty(dir)) continue;
                if (!currentDir.ContainsKey(dir))
                {
                    currentDir[dir] = new Dictionary<string, object>();
                }
                currentDir = (Dictionary<string, object>)currentDir[dir];
            }
        }

        public void DeleteFile(string path)
        {
            var directories = path.Split('/');
            var fileName = directories[^1];
            var parentDirPath = string.Join("/", directories[..^1]);
            var parentDir = GetDirectory(parentDirPath);
            if (parentDir == null) return;
            if (!parentDir.TryGetValue(fileName, out object? value)) return;
            if (value is not string) return;
            parentDir.Remove(fileName);
        }

        public Dictionary<string, object>? GetDirectory(string path)
        {
            var directories = path.Split('/');
            var currentDir = _root;
            foreach (var dir in directories)
            {
                if (string.IsNullOrEmpty(dir)) continue;
                if (!currentDir.TryGetValue(dir, out object? value)) return null;
                if (value is not Dictionary<string, object> subDir) return null;
                currentDir = subDir;
            }
            return currentDir;
        }

        public string? ReadFile(string path)
        {
            var directories = path.Split('/');
            var fileName = directories[^1];
            var parentDirPath = string.Join("/", directories[..^1]);
            var parentDir = GetDirectory(parentDirPath);
            if (parentDir == null) return null;
            if (!parentDir.TryGetValue(fileName, out var content)) return null;
            if (content is not string stringContent) return null;
            return stringContent;
        }

        public void WriteFile(string path, string content)
        {
            var directories = path.Split('/');
            var fileName = directories[^1];
            var parentDirPath = string.Join("/", directories[..^1]);
            var parentDir = GetDirectory(parentDirPath);
            if (parentDir == null) return;
            parentDir[fileName] = content;
        }

        internal void DeleteDirectory(string path)
        {
            var directories = path.Split('/');
            var currentDir = _root;

            int i = 0;
            foreach (var dir in directories)
            {
                if (i == directories.Length - 1)
                {
                    currentDir.Remove(dir);
                    return;
                }
                i++;
                if (string.IsNullOrEmpty(dir)) continue;
                if (!currentDir.TryGetValue(dir, out object? value)) return;
                if (value is not Dictionary<string, object> subDir) return;
                currentDir = subDir;
            }
        }
    }
}
