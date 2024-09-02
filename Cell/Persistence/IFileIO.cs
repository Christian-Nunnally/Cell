
namespace Cell.Persistence
{
    public interface IFileIO
    {
        void CopyDirectory(string oldFullPath, string newFullPath);
        void DeleteDirectory(string path);
        void DeleteFile(string path);
        bool Exists(string versionPath);
        void MoveDirectory(string oldFullPath, string newFullPath);
        string ReadFile(string path);
        void WriteFile(string versionPath, string version);
    }
}
