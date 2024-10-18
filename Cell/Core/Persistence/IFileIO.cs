namespace Cell.Core.Persistence
{
    /// <summary>
    /// Interface for file IO operations.
    /// </summary>
    public interface IFileIO
    {
        /// <summary>
        /// Copies a directory from one location to another.
        /// </summary>
        /// <param name="from">Path to the directory to copy.</param>
        /// <param name="to">Path to the directory to paste in to.</param>
        void CopyDirectory(string from, string to);

        /// <summary>
        /// Deletes the directory at the given path recursively.
        /// </summary>
        /// <param name="path">The path of the directory to delete.</param>
        void DeleteDirectory(string path);

        /// <summary>
        /// Deletes the file at the given path.
        /// </summary>
        /// <param name="path">The path of the file to delete.</param>
        void DeleteFile(string path);

        /// <summary>
        /// Checks if a directory exists at the given path.
        /// </summary>
        /// <param name="path">The path to check for the existence of a directory.</param>
        /// <returns>True if a directory exists at the given path.</returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Checks if a file exists at the given path.
        /// </summary>
        /// <param name="path">The path to check for the existence of a file.</param>
        /// <returns>True if a file exists at the given path.</returns>
        bool Exists(string path);

        /// <summary>
        /// Returns a list of directories at the given path.
        /// </summary>
        /// <param name="path">The directory to look for directories within.</param>
        /// <returns>The list of paths of the directories in the given path.</returns>
        IEnumerable<string> GetDirectories(string path);

        /// <summary>
        /// Gets a list of files in the given directory.
        /// </summary>
        /// <param name="path">The path to get the list of files within.</param>
        /// <returns>A list of the file names directly in the given path.</returns>
        IEnumerable<string> GetFiles(string path);

        /// <summary>
        /// Moves a directory from one location to another.
        /// </summary>
        /// <param name="from">The path of the directory to move.</param>
        /// <param name="to">The path to put the new directory.</param>
        void MoveDirectory(string from, string to);

        /// <summary>
        /// Reads the contents of a file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The contents of the file.</returns>
        string ReadFile(string path);

        /// <summary>
        /// Writes the given text to a file. Will overwrite existing file.
        /// </summary>
        /// <param name="path">The path of the file to write.</param>
        /// <param name="text">The text to write/</param>
        void WriteFile(string path, string text);

        /// <summary>
        /// Zips a directory and all of its contents to the given zip file path.
        /// </summary>
        /// <param name="path">The path to the directory to zip.</param>
        /// <param name="zipPath">The path to the zip file that will be created.</param>
        void ZipDirectory(string path, string zipPath);

        /// <summary>
        /// Creates a directory at the given path.
        /// </summary>
        /// <param name="path">The path to the directory to create.</param>
        void CreateDirectory(string path);
    }
}
