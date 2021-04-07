using System;
using System.Collections.Generic;
using System.Text;
using Yuniql.Core;

namespace Yuniql.CLI
{
    public class JsonPrinter : IPrinter
    {
        private (string property, string value, Source source)[] ParametersList = new (string, string, Source)[5];

        public void AddRow(params object[] row)
        {
            throw new NotImplementedException();
        }

        public void Print()
        {
            throw new NotImplementedException();
        }
    }
}
