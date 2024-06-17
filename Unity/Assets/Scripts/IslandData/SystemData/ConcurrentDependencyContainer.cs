using System.Collections.Concurrent;
using System.Collections.Generic;

namespace IslandData.SystemData
{
    public abstract class ConcurrentDependencyContainer
    {
        private readonly ConcurrentBag<WeightedDependency> _imports;
        private readonly ConcurrentBag<WeightedDependency> _exports;

        private IIslandSystemElement _baseElement;

        protected ConcurrentDependencyContainer()
        {
            _imports = new ConcurrentBag<WeightedDependency>();
            _exports = new ConcurrentBag<WeightedDependency>();
        }

        
        protected void SetBaseElement(IIslandSystemElement baseElement)
        {
            _baseElement = baseElement;
        }   
        
        /// <summary>
        /// Add a import relationship.
        /// </summary>
        public void AddConcurrentImport(IIslandSystemElement target, string rType)
        {
            _imports.Add(new WeightedDependency(){Target = target, Label = rType});
            target.AddConcurrentExport(_baseElement, rType);
        }
        
        /// <summary>
        /// This method is automatically called by other elements by their AddConcurrentImport method.
        /// </summary>
        public void AddConcurrentExport(IIslandSystemElement target, string rType)
        {
            _exports.Add(new WeightedDependency(){Target = target, Label = rType});
        }

        public List<WeightedDependency> GetCleanImportList()
        {
            return ConvertToWeightedList(_imports);
        }
        
        public List<WeightedDependency> GetCleanExportList()
        {
            return ConvertToWeightedList(_exports);
        }
        
        private static List<WeightedDependency> ConvertToWeightedList (ConcurrentBag<WeightedDependency> bag)
        {
           
            // Create exactly one weighted dependency for each element in the bag.
            var weightList = new List<WeightedDependency>();
            foreach (var item in bag)
            {
                if (weightList.Find(x => x.Target.UID == item.Target.UID) == null) // TODO same target but different label?
                {
                    weightList.Add(new WeightedDependency()
                    {
                        Target = item.Target,
                        Label = item.Label,
                        Weight = 1
                    });
                }
            }

            // UnityEngine.Debug.Log("before: " + bag.Count + " and after: " + weightList.Count);
            
            // Apply weight to dependencies.
            foreach (var element in weightList)
            {
                switch (element.Target)
                {
                    case Region region:
                    {
                        var dep = weightList.Find(x => x.Target.UID == region.Parent.UID);

                        if (dep == null)
                        {
                            UnityEngine.Debug.LogWarning($"Problem with weighted dependencies for element {element.Target.Name} and region {region.Name} with the parent {region.Parent.Name}");
                            continue;
                        }
                    
                        dep.Weight++;
                        break;
                    }
                    case Building building:
                    {
                        var dep = weightList.Find(x => x.Target.UID == building.Parent.UID);
                    
                        if (dep == null)
                        {
                            UnityEngine.Debug.LogWarning($"Problem with weighted dependencies for element {element.Target.Name} and building {building.Name} (uid={building.UID})with the parent {building.Parent.Name} (uid={building.Parent.UID})");
                            continue;
                        }
                    
                        dep.Weight++;
                        break;
                    }
                }
            }
            
            return weightList;
        }

    }
}