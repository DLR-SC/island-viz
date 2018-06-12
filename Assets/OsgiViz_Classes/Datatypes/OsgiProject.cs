using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickGraph;
using OsgiViz.SoftwareArtifact;
using OsgiViz.Relations;

namespace OsgiViz.Core
{
    public class OsgiProject
    {
        private string projectName;
        private List<Bundle> bundles;
        private List<Service> services;
        private BidirectionalGraph<GraphVertex, GraphEdge> dependencyGraph;
        private int maximalImportCount;
        private int sizeInMemory;

        public OsgiProject(string pn)
        {
            projectName = pn;
            bundles = new List<Bundle>();
            services = new List<Service>();
            dependencyGraph = new BidirectionalGraph<GraphVertex, GraphEdge>(true);
            maximalImportCount = 0;
        }

        public BidirectionalGraph<GraphVertex, GraphEdge> getDependencyGraph()
        {
            return dependencyGraph;
        }
        public List<Service> getServices()
        {
            return services;
        }
        public List<Bundle> getBundles()
        {
            return bundles;
        }
        public string getProjectName()
        {
            return projectName;
        }
        public int getMemoryConsumption()
        {
            return sizeInMemory;
        }
        public long getNumberOfCUs()
        {
            long result = 0;
            foreach (Bundle b in bundles)
            {
                result += b.getCuCount();
            }
            return result;
        }
        public int getMaxImportCount()
        {
            return maximalImportCount;
        }

        public void addService(Service s)
        {
            services.Add(s);
        }
        public void addBundle(Bundle bun)
        {
            bundles.Add(bun);
        }

        public void setMaxImportCount(int newMax)
        {
            maximalImportCount = newMax;
        }

    }

}
