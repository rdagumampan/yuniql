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
        DEFAULT,
        /// <summary>
        /// Environment variable is the source of the configuration parameter
        /// </summary>
        ENVIRONMENT_VARIABLE,
        /// <summary>
        /// command line is the source of the configuration parameter
        /// </summary>
        CMD_LINE_OPTIONS
    }
}
