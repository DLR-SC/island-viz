using System.Collections.Generic;
using Unity.Entities;

namespace IslandData.SystemData
{
    public interface IIslandSystemElement
    {
        public List<WeightedDependency> Imports { get; }
        public List<WeightedDependency> Exports { get; }
        public long UID { get; }
        public string Name { get; }
        public Entity Entity { get; set; }

        public void AddConcurrentImport(IIslandSystemElement element, string rType);
        public void AddConcurrentExport(IIslandSystemElement element, string rType);

        public object GetProperty(string propertyName);
        public IReadOnlyList<string> GetLabels();
    }
}
