using ArdiLabs.Yuniql.SqlServer;

namespace ArdiLabs.Yuniql
{
    public class DataServiceFactory : IDataServiceFactory
    {
        public IDataService Create(string platform)
        {
            if (platform.Equals("sqlserver"))
            {
                return new SqlServerDataService();
            }

            return null;
        }
    }
}
