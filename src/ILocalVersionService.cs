namespace ArdiLabs.Yuniql
{
    public interface ILocalVersionService
    {
        string GetLatestVersion(string workingPath);
        string IncrementMajorVersion(string workingPath, string sqlFileName);
        string IncrementMinorVersion(string workingPath, string sqlFileName);
        void Init(string workingPath);
    }
}