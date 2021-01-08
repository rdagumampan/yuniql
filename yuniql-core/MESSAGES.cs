namespace Yuniql.Core
{
    public static class MESSAGES
    {
        public const string ManualResolvingAfterFailureMessage = @"You must fix and execute the failed script manually and then run the migration with --continue-after-failure argument. It will ensure that migration will skip this script and continue with next script in the failed version.";
        public const string TransactionalAfterFailureMessage = @"The target database will initiate a rollback attempt to revert all applicable DML and DDL changes. The database auto-created will be kept but would be empty.";
    }
}
