using System;

namespace Yuniql.Extensibility
{
    /// <summary>
    /// Representation of supported platform and versions.
    /// </summary>
    public class ManifestData
    {
        ///<summary>
        ///The name of the database.
        ///</summary>
        public string Name;

        ///<summary>
        /// An example string showing how to use the CLI interface. 
        ///</summary>
        public string Usage;

        ///<summary>
        ///The versions of the database.
        ///</summary>
        public string DocumentationUrl;

        ///<summary>
        /// A useful link to samples of the database and yuniql in use. 
        ///</summary>
        public string SamplesUrl;

        ///<summary>
        ///Outputs a formatted version of the manifest data.
        ///</summary>
        public void printData()
        {
            var info = string.Format(@"
        Name: {0}
        Usage:{2}
        SupportedVersions: {1}
        Samples: {3}
        ", Name, DocumentationUrl, Usage, SamplesUrl);
            Console.WriteLine(info);
        }
    }
}