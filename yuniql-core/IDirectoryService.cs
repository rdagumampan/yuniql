using System.IO;

namespace Yuniql.Core
{
    public interface IDirectoryService
    {
        string[] GetDirectories(string path, string searchPattern);

        string[] GetDirectories(string path, string searchPattern, SearchOption searchOption);

        string[] GetFiles(string path, string searchPattern);

        public bool Exists(string path);

        string GetFileCaseInsensitive(string path, string fileName);
    }
}