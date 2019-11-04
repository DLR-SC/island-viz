using UnityEngine;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using Neo4j.Driver.V1;

/// <summary>
/// THIS ONLY WORKS IN UNITY 2019!
/// </summary>
namespace Neo4JDriver
{
    // The following example is based on the HelloWorldExample on https://neo4j.com/developer/dotnet/.

    public class Neo4J : IDisposable
    {
        // Connection with a Neo4J database, providing a access point via the ISession method. 
        private readonly IDriver driver;
        private bool debug = true;
        private ConnectionStatus status;
       

        // The class constructor.
        public Neo4J(string uri, string user, string password)
        {
            status = ConnectionStatus.Connecting;

            driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));

            // TODO Test & Set ConnectionStatus
            // Debug.LogError("Could not connect to " + uri + "!");
            // status = ConnectionStatus.Fail;
            // return;
            
            status = ConnectionStatus.Sucess;
            Debug.Log("Neo4j .NET driver: Connected to Neo4J database " + driver.Uri + " with username " + user);
        }

        // Executes a given command and returns the result in a IStatementResult.
        // A command must be a string representing a Cypher statement.
        // An example for a command would be "MATCH (cloudAtlas {title: \"Cloud Atlas\"}) RETURN 
        // cloudAtlas.released".
        public IStatementResult Transaction (string command)
        {
            IStatementResult result;

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

            driver.Dispose();

            return result;
        }

        // Implement IDisposable.
        public void Dispose()
        {
            // Dispose managed resources.
            driver?.Dispose();
        }





        public ConnectionStatus GetCurrentStatus ()
        {
            return status;
        }
        public enum ConnectionStatus
        {
            Connecting,
            Sucess,
            Fail
        }
    }
}


