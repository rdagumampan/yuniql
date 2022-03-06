using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Yuniql.Core;

namespace Yuniql.UnitTests
{
    [TestClass]
    public class TestClassBase
    {
        [TestInitialize]
        public void Setup()
        {
            var traceService = new FileTraceService();
            var directoryService = new DirectoryService(traceService);
            var environmentService = new EnvironmentService();
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(traceService, directoryService, fileService);

            var configurationService = new ConfigurationService(environmentService, workspaceService, traceService);
            configurationService.Reset();
        }

        public bool AreEqual(List<KeyValuePair<string, string>> kvp1, IEnumerable<string> kvs)
        {
            var kvp2 = kvs.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();
            return AreEqual(kvp1, kvp2);
        }

        public bool AreEqual(List<KeyValuePair<string, string>> kvp1, List<KeyValuePair<string, string>> kvp2)
        {
            var result = kvp1.Count == kvp2.Count;
            if (result)
                result = kvp1.TrueForAll(kv1 => kvp2.Exists(kv2 => kv2.Key == kv1.Key && kv2.Value == kv1.Value));

            return result;
        }
    }

}
