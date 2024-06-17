using System.Diagnostics;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace IslandData.Construction
{
    /// <summary>
    /// This class uses the Neo4J .net driver to load all nodes and relationships from a Neo4J server.
    /// </summary>
    public static class Neo4jDataLoader
    {
        public static async Task<Neo4jData> Load(ScriptableNeo4jConnection con)
        {
            UnityEngine.Debug.Log("<b>Neo4jDataLoader</b>: Loading data from " + con.URI + " ...");

            // Neo4J plugin setup.
            var driver = GraphDatabase.Driver(con.URI, AuthTokens.Basic(con.Username, con.Password));
            var session = driver.AsyncSession(o => o.WithDatabase(con.Database));

            // Print debug.
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // Load nodes.
            string query = "MATCH (n) RETURN n";
            var cursor = await session.RunAsync(query);
            var nodeList = await cursor.ToListAsync(record => record["n"].As<INode>());
            await cursor.ConsumeAsync();

            // Load relationships.
            query = "MATCH ()-[r]->() RETURN r";
            cursor = await session.RunAsync(query);
            var relationshipList = await cursor.ToListAsync(record => record["r"].As<IRelationship>());
            await cursor.ConsumeAsync();

            // Print debug.
            stopWatch.Stop();
            UnityEngine.Debug.Log("<b>Neo4jDataLoader</b>: Data loading done in " + 
                                  stopWatch.Elapsed.TotalSeconds.ToString("0.000") + " seconds.");

            // Store data.
            var data = new Neo4jData()                
            {
                Nodes = nodeList,
                Relationships = relationshipList                    
            };
            
            // Finalize
            await session.CloseAsync();
            await driver.CloseAsync();
            return data;
        }
    }
}

