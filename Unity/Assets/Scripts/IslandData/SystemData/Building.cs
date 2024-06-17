using System.Collections.Generic;
using IslandData.Construction;
using Unity.Entities;

namespace IslandData.SystemData
{
    public class Building : ConcurrentDependencyContainer, IIslandSystemElement
    {   
        public Region Parent { get; set; }
        public List<WeightedDependency> Imports { get; }
        public List<WeightedDependency> Exports { get; }
        public long UID { get; }
        public string Name { get; }
        public int Type { get; set; }
        public Entity Entity { get; set; }
        
        public int Height { get; }
        

        private readonly IReadOnlyDictionary<string, object> _properties;
        private readonly IReadOnlyList<string> _labels;

        
        public Building(IReadOnlyDictionary<string, object> properties, IReadOnlyList<string> labels, long uid, ScriptableConversionRules rules)
        {
            _properties = properties;
            _labels = labels;
            UID = uid;
            Name = GetProperty(rules.BuildingNodeNameProperty) != null ? (string)GetProperty(rules.BuildingNodeNameProperty) : "NaV";
            Height = GetProperty(rules.BuildingNodeHeightProperty) != null ? int.Parse((string)GetProperty(rules.BuildingNodeHeightProperty)) : 0;
            
            Imports = new List<WeightedDependency>();
            Exports = new List<WeightedDependency>();
            
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
