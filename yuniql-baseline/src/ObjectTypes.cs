using System;
using System.Collections.Generic;
using System.Text;

namespace Yuniql.Extensions
{
    public enum ObjectTypes
    {
          Users
        , Roles
        , Schemas                   //supportted
        , Assemblies
        , AsymmetricKeys
        , Certificates
        , XmlSchemaCollections      //supportted
        , FileGroups
        , FullTextCatalogs
        , FullTextStopLists
        , LogFiles
        , PartitionFunctions
        , PartitionSchemes
        , PlanGuides
        , UserDefinedTypes          //supportted
        , UserDefinedDataTypes      //supportted
        , UserDefinedTableTypes     //supportted
        , UserDefinedAggregates     
        , ApplicationRoles
        , Rules
        , Defaults                  //supportted
        , Tables                    //supportted
        , StoredProcedures          //supportted
        , UserDefinedFunctions      //supportted
        , Views                     //supportted
        , DatabaseAuditSpecifications
        , SearchPropertyLists
        , Sequences                 //supportted
        , ServiceBroker
        , SymmetricKeys
        , Triggers                  //supportted
        , Synonyms
        , ExtendedStoredProcedures
        , ExtendedProperties
    }
}
