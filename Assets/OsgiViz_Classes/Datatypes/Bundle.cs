using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsgiViz.Core;

namespace OsgiViz.SoftwareArtifact
{
    public class Bundle
    {
        //Variables
        private string bundleName;
        private string bundleSymbolicName;
        private List<Package> packages;
        private List<Package> exportedPckgs;
        private List<Package> importedPckgs;
        private List<ServiceComponent> serviceComponents;
        private OsgiProject project;

        public Bundle(string name, string symbName, OsgiProject p)
        {
            bundleName = name;
            bundleSymbolicName = symbName;
            packages = new List<Package>();
            exportedPckgs = new List<Package>();
            importedPckgs = new List<Package>();
            serviceComponents = new List<ServiceComponent>();
            project = p;
        }

        public OsgiProject getParentProject()
        {
            return project;
        }
        public List<ServiceComponent> getServiceComponents()
        {
            return serviceComponents;
        }
        public List<Package> getExportedPackages()
        {
            return exportedPckgs;
        }
        public List<Package> getImportedPackages()
        {
            return importedPckgs;
        }
        public List<Package> getPackages()
        {
            return packages;
        }
        public string getName()
        {
            return bundleName;
        }
        public string getSymbolicName()
        {
            return bundleSymbolicName;
        }
        public long getCuCount()
        {
            long result = 0;

            foreach (Package pckg in packages)
                result += pckg.getCuCount();

            return result;
        }

        public void addPackage(Package f)
        {
            packages.Add(f);
        }
        public void addExportedPackage(Package f)
        {
            exportedPckgs.Add(f);
        }
        public void addImportedPackage(Package f)
        {
            importedPckgs.Add(f);
        }
        public void addServiceComponent(ServiceComponent sc)
        {
            serviceComponents.Add(sc);
        }
       

    }
}
