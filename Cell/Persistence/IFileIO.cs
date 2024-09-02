
namespace Cell.Persistence
{
    public interface IFileIO
    {
        void CopyDirectory(string from, string to);
        void DeleteDirectory(string path);
        void DeleteFile(string path);
        bool DirectoryExists(string path);
        bool Exists(string path);
        IEnumerable<string> GetDirectories(string path);
        IEnumerable<string> GetFiles(string path);
        void MoveDirectory(string from, string to);
        string ReadFile(string path);
        void WriteFile(string path, string text);
        void ZipDirectory(string path, string zipPath);
    }
}
