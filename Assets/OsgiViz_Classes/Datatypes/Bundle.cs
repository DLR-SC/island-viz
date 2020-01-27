using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGI_Datatypes.ArchitectureElements;
using OSGI_Datatypes.OrganisationElements;
using OsgiViz.Core;
using UnityEngine;

namespace OsgiViz.SoftwareArtifact
{
    public class Bundle
    {
        //Variables
        int neoId;
        private string bundleName;
        private string bundleSymbolicName;
        private List<Package> packages;
        private List<Package> exportedPckgs;
        private List<Package> importedPckgs;
        private Dictionary<Bundle, float> exportReceiverBundles;
        private Dictionary<Bundle, float> importBundles; 
        private List<ServiceComponent> serviceComponents;
        private OsgiProject project;

        //history
        Dictionary<Branch, Bundle> previous;
        Dictionary<Branch, Bundle> next;
        BundleMaster master;

        //Visualization
        Vector2 position;
        float radius;

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

        public Bundle(int id, string n, string sn, Commit c)
        {
            neoId = id;
            bundleName = n;
            bundleSymbolicName = sn;
            position = Vector2.negativeInfinity;

            c.AddBundle(this);

            packages = new List<Package>();
            exportedPckgs = new List<Package>();
            importedPckgs = new List<Package>();
            exportReceiverBundles = new Dictionary<Bundle, float>();
            importBundles = new Dictionary<Bundle, float>();
            previous = new Dictionary<Branch, Bundle>();
            next = new Dictionary<Branch, Bundle>();
        }

        #region HistoryElements

        public void AddPrevious(Branch b, Bundle prev, bool forwadsConnection)
        {
            previous.Add(b, prev);
            if (forwadsConnection)
            {
                prev.AddNext(b, this, false);
            }
        }
        public void AddNext(Branch b, Bundle nextB, bool backwardsConnection)
        {
            next.Add(b, nextB);
            if (backwardsConnection)
            {
                nextB.AddPrevious(b, this, false);
            }
        }
        public void SetMaster(BundleMaster m, Commit c)
        {
            master = m;
            master.AddElement(c, this);
        }

        public Bundle GetNext(Branch b)
        {
            Bundle bundle;
            next.TryGetValue(b, out bundle);
            return bundle;
        }
        public Bundle GetPrevious(Branch b)
        {
            Bundle bundle;
            previous.TryGetValue(b, out bundle);
            return bundle;
        }
        public BundleMaster GetMaster()
        {
            return master;
        }
        #endregion

        public int GetNeoId()
        {
            return neoId;
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
            if (f.getBundle() != this)
            {
                importedPckgs.Add(f);
                if (importBundles == null)
                {
                    return;
                }
                //Add Bundle to importBundle-Dict
                if (importBundles.ContainsKey(f.getBundle()))
                {
                    importBundles[f.getBundle()]++;
                }
                else{
                    importBundles.Add(f.getBundle(), 1);
                }
                f.getBundle().AddExportPartner(this);

            }
        }

        public void AddExportPartner(Bundle partner)
        {
            if (exportReceiverBundles == null)
            {
                return;
            }
            if (exportReceiverBundles.ContainsKey(partner))
            {
                exportReceiverBundles[partner]++;
            }
            else
            {
                exportReceiverBundles.Add(partner, 1);
            }
        }

        public void addServiceComponent(ServiceComponent sc)
        {
            serviceComponents.Add(sc);
        }
        public void AddRequiredBundle(Bundle other)
        {
            foreach (Package p in other.getPackages())
            {
                addImportedPackage(p);
                p.IncreaseNrOfImports(1);
            }
        }

        public Dictionary<int, Package> GetPackageDictionary()
        {
            Dictionary<int, Package> packDict = new Dictionary<int, Package>();
            foreach (Package p in packages)
            {
                packDict.Add(p.GetNeoId(), p);
            }
            return packDict;
        }

        public void SetPosition(float p1, float p2)
        {
            position = new Vector2(p1, p2);
        }
        public Vector2 GetPosition()
        {
            return position;
        }

        public void SetRadius(float r)
        {
            radius = r;
        }
        public float GetRadius()
        {
            return radius;
        }

        public Dictionary<Bundle, float> GetImportedBundles()
        {
            return importBundles;
        }
        public Dictionary<Bundle, float> GetExportReceiverBundles()
        {
            return exportReceiverBundles;
        }

    }
}
