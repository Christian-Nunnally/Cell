using System.IO;

namespace Cell.Persistence
{
    internal class FileIO : IFileIO
    {
        public void CopyDirectory(string fromPath, string toPath)
        {
            CopyFilesRecursively(fromPath, toPath);
        }

        private static void CopyFilesRecursively(string from, string to)
        {
            CreateAllDirectories(from, to);
            CopyAllFiles(from, to);
        }

        private static void CopyAllFiles(string from, string to)
        {
            foreach (string newPath in Directory.GetFiles(from, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(from, to), true);
            }
        }

        private static void CreateAllDirectories(string from, string to)
        {
            foreach (string directory in Directory.GetDirectories(from, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(directory.Replace(from, to));
            }
        }

        public void DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public void MoveDirectory(string oldFullPath, string newFullPath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(newFullPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(newFullPath)!);
            }
            Directory.Move(oldFullPath, newFullPath);
        }

        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteFile(string path, string version)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, version);
        }
    }
}
