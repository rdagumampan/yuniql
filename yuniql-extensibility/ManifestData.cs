using System;

namespace Yuniql.Extensibility{
    
    /// <summary>
    /// container for supported platform and versions.
    /// </summary>
    public class ManifestData
    {
        ///<summary>
        ///The name of the supported Database.
        ///</summary>
        public string Name;
        
        ///<summary>
        ///Versions that are supported for each Database platform.
        ///</summary>
        public string SupportedVersions;
        
        ///<summary>
        /// An example string showing how to use the CLI interface for each Database. 
        ///</summary>
        public string Usage; 

        ///<summary>
        /// Useful link to samples of the Database and Yuniql in use. 
        ///</summary>
        public string Samples;
        
        ///<summary>
        ///outputs a formatted version of the Manifest Data.
        ///</summary>
        public void printData()
        {
            var info = string.Format(@"
        Name: {0}
        SupportedVersions: {1}
        Usage:{2}
        Samples: {3}
        ",Name,SupportedVersions,Usage,Samples);
            Console.WriteLine(info);
        }
    }
    
}