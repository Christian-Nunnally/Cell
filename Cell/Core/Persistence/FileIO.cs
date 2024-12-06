using System.IO;
using System.IO.Compression;

namespace Cell.Core.Persistence
{
    /// <summary>
    /// Allows for file IO operations with the real file system.
    /// </summary>
    public class FileIO : IFileIO
    {
        /// <summary>
        /// Copies a directory from one location to another.
        /// </summary>
        /// <param name="from">Path to the directory to copy.</param>
        /// <param name="to">Path to the directory to paste in to.</param>
        public void CopyDirectory(string from, string to)
        {
            CopyFilesRecursively(from, to);
        }

        /// <summary>
        /// Creates a directory at the given path.
        /// </summary>
        /// <param name="path">The path to the directory to create.</param>
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Deletes the directory at the given path recursively.
        /// </summary>
        /// <param name="path">The path of the directory to delete.</param>
        public void DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
        }

        /// <summary>
        /// Deletes the file at the given path.
        /// </summary>
        /// <param name="path">The path of the file to delete.</param>
        public void DeleteFile(string path)
        {
            if (!File.Exists(path)) return;
            File.Delete(path);
        }

        /// <summary>
        /// Checks if a directory exists at the given path.
        /// </summary>
        /// <param name="path">The path to check for the existence of a directory.</param>
        /// <returns>True if a directory exists at the given path.</returns>
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// Checks if a file exists at the given path.
        /// </summary>
        /// <param name="path">The path to check for the existence of a file.</param>
        /// <returns>True if a file exists at the given path.</returns>
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Returns a list of directories at the given path.
        /// </summary>
        /// <param name="path">The directory to look for directories within.</param>
        /// <returns>The list of paths of the directories in the given path.</returns>
        public IEnumerable<string> GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        /// <summary>
        /// Gets a list of files in the given directory.
        /// </summary>
        /// <param name="path">The path to get the list of files within.</param>
        /// <returns>A list of the file names directly in the given path.</returns>
        public IEnumerable<string> GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        /// <summary>
        /// Moves a directory from one location to another.
        /// </summary>
        /// <param name="from">The path of the directory to move.</param>
        /// <param name="to">The path to put the new directory.</param>
        public void MoveDirectory(string from, string to)
        {
            if (!Directory.Exists(Path.GetDirectoryName(to)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(to)!);
            }
            Directory.Move(from, to);
        }

        /// <summary>
        /// Reads the contents of a file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The contents of the file.</returns>
        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        /// <summary>
        /// Reads the contents of a file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The contents of the file.</returns>
        public Task<string> ReadFileAsync(string path)
        {
            return File.ReadAllTextAsync(path);
        }

        /// <summary>
        /// Writes the given text to a file. Will overwrite existing file.
        /// </summary>
        /// <param name="path">The path of the file to write.</param>
        /// <param name="text">The text to write/</param>
        public void WriteFile(string path, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text);
        }

        /// <summary>
        /// Zips a directory and all of its contents to the given zip file path.
        /// </summary>
        /// <param name="path">The path to the directory to zip.</param>
        /// <param name="zipPath">The path to the zip file that will be created.</param>
        public async Task ZipDirectoryAsync(string path, string zipPath)
        {
            await Task.Run(() =>
            {
                Directory.CreateDirectory(Path.GetDirectoryName(zipPath)!);
                ZipFile.CreateFromDirectory(path, zipPath);
            });
        }

        private static void CopyAllFiles(string from, string to)
        {
            foreach (string newPath in Directory.GetFiles(from, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(from, to), true);
            }
        }

        private static void CopyFilesRecursively(string from, string to)
        {
            CreateAllDirectories(from, to);
            CopyAllFiles(from, to);
        }

        private static void CreateAllDirectories(string from, string to)
        {
            foreach (string directory in Directory.GetDirectories(from, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(directory.Replace(from, to));
            }
        }

        public async Task CopyDirectoryAsync(string from, string to)
        {
            CopyFilesRecursively(from, to);
        }
    }
}
