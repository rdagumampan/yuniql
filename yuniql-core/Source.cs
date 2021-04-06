using System;
using System.Collections.Generic;
using System.Text;

namespace Yuniql.Core
{
    /// <summary>
    /// Information source for yuniql configuration values
    /// </summary>
    public enum Source
    {
        /// <summary>
        /// Default value assigned to configuration parameter
        /// </summary>
        Default,
        /// <summary>
        /// Environment variable is the source of the configuration parameter
        /// </summary>
        Environment_variable,
        /// <summary>
        /// command line is the source of the configuration parameter
        /// </summary>
        CmdLine_Options
    }
}
