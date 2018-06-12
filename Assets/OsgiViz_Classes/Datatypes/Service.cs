using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OsgiViz.SoftwareArtifact
{
    public class Service {

        private string name;
        private CompilationUnit linkedCU;
        private List<ServiceComponent> implementingComponents;
        private List<ServiceComponent> referencingComponents;

        public Service(string n, CompilationUnit cu)
        {
            name = n;
            linkedCU = cu;
            referencingComponents  = new List<ServiceComponent>();
            implementingComponents = new List<ServiceComponent>();
        }

        public List<ServiceComponent> getReferencingComponents()
        {
            return referencingComponents;
        }
        public List<ServiceComponent> getImplementingComponents()
        {
            return implementingComponents;
        }
        public string getName()
        {
            return name;
        }
        public CompilationUnit getServiceCU()
        {
            return linkedCU;
        }

        public void addReferencingComponent(ServiceComponent sc)
        {
            referencingComponents.Add(sc);
        }
        public void addImplementingComponent(ServiceComponent sc)
        {
            implementingComponents.Add(sc);
        }
    }
}
