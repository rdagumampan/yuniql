namespace Yuniql.Core
{
    public static class MESSAGES
    {
        public const string ManualResolvingAfterFailureMessage = @"You must fix and execute the failed script manually and then run the migration with  ""--continue-after-failure"" argument. It will ensure that migration will skip this script and continue with next script.";
    }
}
