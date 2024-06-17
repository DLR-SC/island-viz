using System.Collections.Generic;
using Neo4j.Driver;

namespace IslandData.Construction
{
    /// <summary>
    /// This contains all data extracted from the neo4j database.
    /// </summary>
    public struct Neo4jData
    {
        /// <summary>
        /// All loaded Neo4j nodes.
        /// </summary>
        public List<INode> Nodes { set; get; }

        /// <summary>
        /// All loaded Neo4j relationships.
        /// </summary>
        public List<IRelationship> Relationships { set; get; }
    }
}
