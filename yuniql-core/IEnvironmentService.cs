namespace Yuniql.Core
{
    public interface IEnvironmentService
    {
        string GetEnvironmentVariable(string name);

        string GetCurrentDirectory();
    }
}
