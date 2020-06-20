namespace Yuniql.Core.Factories {

    ///<inheritdoc/>
    public interface IMigrationServiceFactory {
        ///<inheritdoc/>
        IMigrationService Create(string platform);
    }
}
