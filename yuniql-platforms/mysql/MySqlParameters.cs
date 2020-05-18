using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Yuniql.Extensibility;

namespace Yuniql.MySql
{
    /// <summary>
    /// MySql parameters
    /// </summary>
    public class MySqlParameters : IDbParameters
    {
        private readonly Dictionary<string, MySqlParameter> _parameters = new Dictionary<string, MySqlParameter>();

        /// <summary>
        /// Get the collection of MySql parameters
        /// </summary>
        public IEnumerable<MySqlParameter> Parameters
        {
            get
            {
                return _parameters.Values;
            }
        }

        /// <summary>
        /// Adds the parameter to collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="direction">The parameter direction.</param>
        public void AddParameter<T>(string name, T value, ParameterDirection direction = ParameterDirection.Input)
        {
            if (value is ParameterDirection)
            {
                throw new ArgumentException($@"Not supported parameter value type ""{typeof(ParameterDirection)}""");
            }

            MySqlParameter parameter = this.CreateParameter(name, direction);
            parameter.Value = (object)value ?? DBNull.Value;
            this.AddParameter(parameter);
        }

        /// <summary>
        /// Adds the parameter without value into collection. Usefull for "Output" and "ReturnValue" parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The parameter name.</param>
        /// <param name="direction">The parameter direction.</param>
        public void AddParameter<T>(string name, ParameterDirection direction)
        {
            MySqlParameter parameter = this.CreateParameter(name, direction);
            parameter.Value = default(T); //this sets correct DbType
            this.AddParameter(parameter);
        }

        /// <summary>
        /// Adds the parameters.
        /// </summary>
        /// <param name="dbParameters">The database parameters.</param>
        public void AddParameters(IDbParameters dbParameters)
        {
            foreach (MySqlParameter parameter in ((MySqlParameters)dbParameters).Parameters)
            {
                this.AddParameter(parameter.ParameterName, parameter.Value, parameter.Direction);
            };
        }

        /// <summary>
        /// Gets Db parameter with specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The parameter name.</param>
        /// <returns></returns>
        public T GetParameter<T>(string name)
        {
            return (T)Convert.ChangeType(this._parameters[name].Value, typeof(T));
        }

        private MySqlParameter CreateParameter(string name, ParameterDirection direction)
        {
            MySqlParameter parameter = new MySqlParameter();
            parameter.ParameterName = name;
            parameter.Direction = direction;
            return parameter;
        }

        private void AddParameter(MySqlParameter parameter)
        {
            if (_parameters.ContainsKey(parameter.ParameterName))
            {
                throw new Exception($@"Parameter with name ""{parameter.ParameterName}"" already exists in parameters collection");
            }

            //add parameter to collection
            _parameters[parameter.ParameterName] = parameter;
        }

        /// <summary>
        /// Builds platform specific parameters.
        /// </summary>
        /// <returns></returns>
        public void CopyToDataParameterCollection(IDataParameterCollection targetParameterCollection)
        {
            foreach (MySqlParameter mySqlParameter in Parameters)
            {
                targetParameterCollection.Add(mySqlParameter);
            }
        }
    }
}
