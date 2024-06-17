using System.Collections.Concurrent;
using System.Collections.Generic;
using IslandData.Construction;
using Unity.Entities;

namespace IslandData.SystemData
{
    /// <summary>
    /// This class represents an region (multiples entites) in the <see cref="IslandSystem"/>.
    /// </summary>
    public class Region : ConcurrentDependencyContainer, IIslandSystemElement
    {
        public ConcurrentBag<Building> Buildings { get; }
        public Island Parent { get; set; }
        public List<Entity> Entities { get; } // ECS Data
        
        public List<WeightedDependency> Imports { get; }
        public List<WeightedDependency> Exports { get; }
        public long UID { get; }
        public string Name { get; }
        public Entity Entity { get; set; }


        private readonly IReadOnlyDictionary<string, object> _properties;
        private readonly IReadOnlyList<string> _labels;
        private readonly long _uid;

        public Region(IReadOnlyDictionary<string, object> properties, IReadOnlyList<string> labels, long uid, ScriptableConversionRules rules)
        {
            _properties = properties;
            _labels = labels;
            UID = uid;
            Name = GetProperty(rules.RegionNodeNameProperty) != null ? (string)GetProperty(rules.RegionNodeNameProperty) : "NaV";

            Imports = new List<WeightedDependency>();
            Exports = new List<WeightedDependency>();
            Buildings = new ConcurrentBag<Building>();
            Entities = new List<Entity>();
            
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
        

        public Building GetBuilding (long uid)
        {
            foreach (var item in Buildings)
            {
                if (item.UID == uid)
                    return item;
            }
        
            return null;
        }
    }
}
