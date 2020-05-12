using System;

//https://github.com/22222/CsvTextFieldParser
namespace Yuniql.Extensibility.BulkCsvParser
{
    /// <summary>
    /// An exception that is thrown when the <see cref="CsvTextFieldParser.ReadFields"/> method cannot parse a row using the specified format.
    /// </summary>
    /// <remarks>
    /// Based on <code>Microsoft.VisualBasic.FileIO.MalformedLineException.MalformedLineException</code>.
    /// </remarks>
    public class CsvMalformedLineException : FormatException
    {
        /// <summary>
        /// Constructs an exception with a specified message and a line number.
        /// </summary>
        public CsvMalformedLineException(string message, long lineNumber)
            : base(message)
        {
            LineNumber = lineNumber;
        }

        /// <summary>
        /// Constructs an exception with a specified message, a line number, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        public CsvMalformedLineException(string message, long lineNumber, Exception innerException)
            : base(message, innerException)
        {
            LineNumber = lineNumber;
        }

        /// <summary>
        /// The line number of the malformed line.
        /// </summary>
        public long LineNumber { get; }
    }
}

