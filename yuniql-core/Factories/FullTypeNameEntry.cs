using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Yuniql.Core.Factories {
    ///<inheritdoc/>
    public struct FullTypeNameEntry {

        ///<inheritdoc/>
        public readonly string AssemblyName;
        
        ///<inheritdoc/>
        public readonly string TypeName;

        ///<inheritdoc/>
        public FullTypeNameEntry(string fullTypeName) {
            if (string.IsNullOrWhiteSpace(fullTypeName)) throw new MissingTypeNameException();

            var nameParts = fullTypeName.Split(',');
            if (nameParts.Length != 2) throw new InvalidTypeNameFormatException(fullTypeName, nameParts.Length);

            AssemblyName = nameParts[0].Trim();
            TypeName = nameParts[1].Trim();
        }
    }

    ///<inheritdoc/>
    public class MissingTypeNameException : ArgumentException {
        
        ///<inheritdoc/>
        public MissingTypeNameException() 
            : base("TypeName should not be null or empty"){

        }
    }

    ///<inheritdoc/>
    public class InvalidTypeNameFormatException : ArgumentException {
        
        ///<inheritdoc/>
        public InvalidTypeNameFormatException(string typeName, int segmentCount)
              : base($"Expected segment for {typeName} is 2 but actual is {segmentCount}") {
        }
    }

        
    ///<inheritdoc/>
    public class TypeLoadFailedException : ArgumentException {
        ///<inheritdoc/>
        public TypeLoadFailedException(string typeName, string assemblyName)
            : base($"Type {typeName} not found in assembly {assemblyName}") {

        }

        ///<inheritdoc/>
        public TypeLoadFailedException(string typeName, string assemblyName, string message)
           : base($"Type {typeName} not found in assembly {assemblyName}. Details: {message}") {

        }
    }

    ///<inheritdoc/>
    public class AssemblyLoadFailedException : ArgumentException {

        ///<inheritdoc/>
        public AssemblyLoadFailedException(string assemblyName, string errorDetails)
            : base($"Assembly {assemblyName} not found. Details {errorDetails}") {

        }
    }
}
