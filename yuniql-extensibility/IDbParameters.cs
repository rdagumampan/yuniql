using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Text;

namespace Yuniql.Extensibility
{
    /// <summary>
    /// Db Parameters collection interface
    /// </summary>
    public interface IDbParameters
    {
        /// <summary>
        /// Adds the parameter into collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="direction">The parameter direction.</param>
        void AddParameter<T>(string name, T value, ParameterDirection direction = ParameterDirection.Input);

        /// <summary>
        /// Adds the parameter without value into collection. Usefull for "Output" and "ReturnValue" parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The parameter name.</param>
        /// <param name="direction">The parameter direction.</param>
        void AddParameter<T>(string name, ParameterDirection direction);

        /// <summary>
        /// Adds the parameters.
        /// </summary>
        /// <param name="dbParameters">The database parameters.</param>
        void AddParameters(IDbParameters dbParameters);

        /// <summary>
        /// Gets Db parameter with specified name.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <returns></returns>
        T GetParameter<T>(string name);

        /// <summary>
        /// Copy to target data parameter collection.
        /// </summary>
        /// <returns></returns>
        public void CopyToDataParameterCollection(IDataParameterCollection target);
    }
}
