using System;

namespace Yuniql.Extensibility{
    
    /// <summary>
    /// container for supported platform and versions.
    /// </summary>
    public class ManifestData
    {
        public string Name;
        public string SupportedVersions;
        public string Usage; 
        public string Samples;
        
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