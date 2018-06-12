using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OsgiViz.SoftwareArtifact
{
    public class ServiceComponent {

        private string name;
        private CompilationUnit implementationCU;
        private List<Service> providedServices;
        private List<Service> referencedServices;

        public ServiceComponent(string n, CompilationUnit cu)
        {
            name = n;
            providedServices = new List<Service>();
            referencedServices = new List<Service>();
            implementationCU = cu;
        }

        public CompilationUnit getImplementationCU()
        {
            return implementationCU;
        }
        public List<Service> getProvidedServices()
        {
            return providedServices;
        }
        public List<Service> getReferencedServices()
        {
            return referencedServices;
        }
        public string getName()
        {
            return name;
        }

        public void addProvidedService(Service s)
        {
            providedServices.Add(s);
        }
        public void addReferencedService(Service s)
        {
            referencedServices.Add(s);
        }




    }
}