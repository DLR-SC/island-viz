using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4JDriver;
using Neo4j.Driver.V1;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ArchitectureElements;
using System.Collections;
using UnityEngine;

public class Neo4JWriterLayout
{
    private static Neo4J database;

    public static void SetDatabase(Neo4J neo)
    {
        database = neo;
    }

    public static IEnumerator WriteCellPositions(List<Dictionary<string, object>> parameters)
    { 
        string statement = "UNWIND {paramList} as row " +
            "MATCH(n) WHERE id(n) = row.id " +
            "SET n.posX = row.posX " +
            "SET n.posZ = row.posZ";
        Dictionary<string, object> dict = new Dictionary<string, object> { { "paramList", parameters } };

        IStatementResult result = database.WriteTransaktion(statement, dict);

        yield return null;
    }
    public static IEnumerator WriteStartCellPositions(List<Dictionary<string, object>> parameters)
    {
        string statement = "UNWIND {paramList} as row " +
            "MATCH(n) WHERE id(n) = row.id " +
            "SET n.startCellX = row.startCellX " +
            "SET n.startCellZ = row.startCellZ";
        Dictionary<string, object> dict = new Dictionary<string, object> { {"paramList", parameters } };

        IStatementResult result = database.WriteTransaktion(statement, dict);
      
        yield return null;
    }

    public static IEnumerator WriteCommitIslandsLayouted(List<int> commitIds)
    {
        string statement = "MATCH (n) WHERE id(n) in $paramList " +
             "SET n.islandsLayouted = true";
        Dictionary<string, object> dict = new Dictionary<string, object> { { "paramList", commitIds } };

        IStatementResult result = database.WriteTransaktion(statement, dict);

        yield return null;
    }
   


}
