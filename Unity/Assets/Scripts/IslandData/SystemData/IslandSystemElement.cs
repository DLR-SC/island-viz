using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Entities;

namespace IslandData.SystemData
{
    /// <summary>
    /// Base class of all island system elements.
    /// </summary>
    public abstract class IslandSystemElement
    {
        /// <summary>
        /// The unique id referencing its graph db node.
        /// </summary>
        public long UID { get; }

        /// <summary>
        /// The name of this element.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type (graph db node label) of this element.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// The height of this element.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The entity of this element in the visualization.
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// Contains all <see cref="IslandSystemElement"/>s this element imports.
        /// </summary>
        public ConcurrentBag<IslandSystemElement> ConcurrentImports { get; }

        /// <summary>
        /// Contains all <see cref="IslandSystemElement"/>s this element exports.
        /// </summary>
        public ConcurrentBag<IslandSystemElement> ConcurrentExports { get; }

        /// <summary>
        /// Contains all <see cref="IslandSystemElement"/>s this element imports.
        /// </summary>
        public List<IslandSystemElement> Imports { get; }

        /// <summary>
        /// Contains all <see cref="IslandSystemElement"/>s this element exports.
        /// </summary>
        public List<IslandSystemElement> Exports { get; }


        public IslandSystemElement (string name, string label, long uid, int height)
        {
            UID = uid;
            Name = name;
            Label = label;
            Height = height;

            ConcurrentImports = new ConcurrentBag<IslandSystemElement>();
            ConcurrentExports = new ConcurrentBag<IslandSystemElement>();

            Imports = new List<IslandSystemElement>();
            Exports = new List<IslandSystemElement>();
        }
        
    }
}
