using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Yuniql.Core;

namespace Yuniql.CLI
{
    /// <summary>
    /// This class is used to print configuration variable as json text
    /// </summary>
    public class JsonPrinter : IPrinter
    {
        private readonly string[] titles =  new string[] { "property","value","source"};
        private readonly List<string[]> rows = new List<string[]>();    
        
        public void AddRow(params object[] row)
        {
            if (row.Length != titles.Length)
            {
                throw new Exception($"Added row length [{row.Length}] is not equal to title row length [{titles.Length}]");
            }
            rows.Add(row.Select(o => o.ToString()).ToArray());
        }

        public void Print()
        {
            var jarray = new JArray("Properties");
            var jproperty = new JProperty("Properties", jarray);
            var jobject = new JObject(jproperty);
            foreach (var row in rows)
            {
                JObject obj = new JObject();
                for (int i = 0; i < row.Length; i++)
                {
                    obj.Add(new JProperty(titles[i], row[i]));
                }
                jarray.Add(obj);
            }
            Console.WriteLine(jobject.ToString());
        }
    }
}
