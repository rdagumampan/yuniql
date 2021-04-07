using System;
using System.Collections.Generic;
using System.Text;

namespace Yuniql.Core
{
    public interface IPrinter
    {
        void Print();
        void AddRow(params object[] row);
    }
}
