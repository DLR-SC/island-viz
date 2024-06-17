using System.Collections.Concurrent;
using System.Collections.Generic;
using IslandData.Construction;
using Unity.Entities;

namespace IslandData.SystemData
{
    /// <summary>
    /// This class represents an island in the <see cref="IslandSystem"/>.
    /// </summary>
    public class Island : ConcurrentDependencyContainer, IIslandSystemElement
    {
        public long UID { get; }
        public string Name { get; }
        public Entity Entity { get; set; }

        public ConcurrentBag<Region> Regions { get; }
        public List<WeightedDependency> Imports { get; }
        public List<WeightedDependency> Exports { get; }
        

        private readonly IReadOnlyDictionary<string, object> _properties;
        private readonly IReadOnlyList<string> _labels;

        public Island(IReadOnlyDictionary<string, object> properties, IReadOnlyList<string> labels, long uid, ScriptableConversionRules rules)
        {
            _properties = properties;
            _labels = labels;
            UID = uid;
            Name = GetProperty(rules.IslandNodeNameProperty) != null ? (string)GetProperty(rules.IslandNodeNameProperty) : "NaV";
            
            Imports = new List<WeightedDependency>();
            Exports = new List<WeightedDependency>();
            Regions = new ConcurrentBag<Region>();

            SetBaseElement(this);
        }

        public object GetProperty(string propertyName)
        {
            return _properties[propertyName] != null ? _properties[propertyName] : null;
        }

        public IReadOnlyList<string> GetLabels()
        {
            return _labels;
        }
    }
}
