using System.IO;

namespace Yuniql.Core
{
    public class FileService : IFileService
    {
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
