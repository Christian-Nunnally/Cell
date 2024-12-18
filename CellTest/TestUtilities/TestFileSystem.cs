﻿


namespace CellTest.TestUtilities
{
    public class TestFileSystem
    {
        private readonly Dictionary<string, object> _root = [];
        public void CreateDirectory(string path)
        {
            var directories = path.Split('\\');
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
            var directories = path.Split('\\');
            var fileName = directories[^1];
            var parentDirPath = string.Join("\\", directories[..^1]);
            var parentDir = GetDirectory(parentDirPath);
            if (parentDir is null) return;
            if (!parentDir.TryGetValue(fileName, out object? value)) return;
            if (value is not string) return;
            parentDir.Remove(fileName);
        }

        public Dictionary<string, object>? GetDirectory(string path)
        {
            var directories = path.Split('\\');
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
            var directories = path.Split('\\');
            var fileName = directories[^1];
            var parentDirPath = string.Join("\\", directories[..^1]);
            var parentDir = GetDirectory(parentDirPath);
            if (parentDir is null) return null;
            if (!parentDir.TryGetValue(fileName, out var content)) return null;
            if (content is not string stringContent) return null;
            return stringContent;
        }

        public void WriteFile(string path, string content)
        {
            var directories = path.Split('\\');
            var fileName = directories[^1];
            var parentDirPath = string.Join("\\", directories[..^1]);
            var parentDir = GetDirectory(parentDirPath);
            if (parentDir is null) return;
            parentDir[fileName] = content;
        }

        public void CopyDirectory(string from, string to)
        {
            var fromDir = GetDirectory(from)?.ToDictionary();
            if (fromDir is null) return;
            CreateDirectory(to);
            var toDir = GetDirectory(to);
            if (toDir is null) return;
            foreach (var (key, value) in fromDir)
            {
                if (value is string content)
                {
                    toDir[key] = content;
                }
                else if (value is Dictionary<string, object>)
                {
                    CopyDirectory($"{from}\\{key}", $"{to}\\{key}");
                }
            }
        }

        public async Task CopyDirectoryAsync(string from, string to)
        {
            var fromDir = GetDirectory(from)?.ToDictionary();
            if (fromDir is null) return;
            CreateDirectory(to);
            var toDir = GetDirectory(to);
            if (toDir is null) return;
            foreach (var (key, value) in fromDir)
            {
                if (value is string content)
                {
                    toDir[key] = content;
                }
                else if (value is Dictionary<string, object>)
                {
                    await CopyDirectoryAsync($"{from}\\{key}", $"{to}\\{key}");
                }
            }
        }

        public void DeleteDirectory(string path)
        {
            var directories = path.Split('\\');
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

        internal void DeleteAll()
        {
            _root.Clear();
        }
    }
}
