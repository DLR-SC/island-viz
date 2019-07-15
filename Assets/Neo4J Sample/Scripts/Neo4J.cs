using UnityEngine;
using Neo4j.Driver;
using System;
using Neo4j.Driver.V1;

namespace Neo4JExample
{
    // The following example is based on the HelloWorldExample on https://neo4j.com/developer/dotnet/.

    public class Neo4J : IDisposable
    {
        // Connection with a Neo4J database, providing a access point via the ISession method. 
        private readonly IDriver driver;
        private bool debug;

        // The class constructor.
        public Neo4J(string uri, string user, string password, bool debugLogs)
        {
            // New IDriver instance.
            driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
            // Optional debug lines
            debug = debugLogs;
            if (debug)
            {
                Debug.Log("Neo4j .NET driver: Connected to Neo4J database " + driver.Uri + " with username " 
                    + user);
            }
        }

        // Executes a given command and returns the result in a IStatementResult.
        // A command must be a string representing a Cypher statement.
        // An example for a command would be "MATCH (cloudAtlas {title: \"Cloud Atlas\"}) RETURN 
        // cloudAtlas.released".
        public IStatementResult Transaction (string command)
        {
            if (debug)
            {
                Debug.Log("Neo4j .NET driver: Executing statement #" + command + "#");
            }            
            using (var session = driver.Session())
            {
                return session.WriteTransaction(tx =>
                {
                    // Runs the statement and returns a result.
                    return tx.Run(command);
                });
            }
        }

        // Implement IDisposable.
        public void Dispose()
        {
            // Dispose managed resources.
            driver?.Dispose();
        }
    }
}


