namespace Yuniql.Core
{
    public interface IFileService
    {
        string ReadAllText(string path);

        bool Exists(string path);
    }
}