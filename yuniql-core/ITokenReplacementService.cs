using System.Collections.Generic;

namespace Yuniql.Core
{
    /// <summary>
    /// Interface for implementing replacement of tokens in the script using the pattern ${TOKEN_KEY}. 
    /// Throws exception and fails the migration when some tokens not replaced due to missing token values passed from the client.
    /// </summary>
    public interface ITokenReplacementService
    {
        /// <summary>
        /// Runs token replacement process.
        /// </summary>
        /// <param name="tokens">List of token Key/Value pairs.</param>
        /// <param name="sqlStatement">Raw SQL statement where tokens maybe present.</param>
        /// <returns>SQL statement where tokens are successfully replaced.</returns>
        string Replace(List<KeyValuePair<string, string>> tokens, string sqlStatement);
    }
}