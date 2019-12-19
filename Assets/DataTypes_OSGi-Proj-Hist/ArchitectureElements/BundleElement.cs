using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.OrganisationElements;


namespace OSGI_Datatypes.ArchitectureElements
{
    public class BundleElement
    {
        //data
        int neoId;
        string name;
        string symbolicName;

        Commit commit;

        //hierarchy
        List<PackageElement> packages;
        List<PackageElement> exportedPackages;
        List<PackageElement> importedPackages;

        //history
        Dictionary<Branch, BundleElement> previous;
        Dictionary<Branch, BundleElement> next;
        BundleMaster master;

        //Visualization
        Vector2 position;
        float radius;

        public BundleElement(int id, string n, string sn, Commit c)
        {
            neoId = id;
            name = n;
            symbolicName = sn;
            commit = c;
            position = Vector2.negativeInfinity;

            c.AddBundle(this);

            packages = new List<PackageElement>();
            exportedPackages = new List<PackageElement>();
            importedPackages = new List<PackageElement>();
            previous = new Dictionary<Branch, BundleElement>();
            next = new Dictionary<Branch, BundleElement>();
        }

        #region Additional_Creation_Methods
        public void AddPackage(PackageElement p, bool isExported)
        {
            packages.Add(p);
            if (isExported)
            {
                exportedPackages.Add(p);
            }
        }
        public void AddImportedPackage(PackageElement p)
        {
            importedPackages.Add(p);
            p.IncreaseNrOfImports(1);
        }
        public void AddRequiredBundle(BundleElement other)
        {
            foreach(PackageElement p in other.GetPackages())
            {
                AddImportedPackage(p);
            }
        }

        public void AddPrevious(Branch b, BundleElement prev, bool forwadsConnection)
        {
            previous.Add(b, prev);
            if (forwadsConnection)
            {
                prev.AddNext(b, this, false);
            }
        }
        public void AddNext(Branch b, BundleElement nextB, bool backwardsConnection)
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
        #endregion

        public Dictionary<int, PackageElement> GetPackageDictionary()
        {
            Dictionary<int, PackageElement> packDict = new Dictionary<int, PackageElement>();
            foreach(PackageElement p in packages)
            {
                packDict.Add(p.GetNeoId(), p);
            }
            return packDict;
        }

        #region Standard_Getter_Setter
        public int GetNeoId()
        {
            return neoId;
        }
        public string GetName()
        {
            return name;
        }
        public string GetSymbolicName()
        {
            return symbolicName;
        }
        public List<PackageElement> GetPackages()
        {
            return packages;
        }
        public List<PackageElement> GetExportedPackages()
        {
            return exportedPackages;
        }
        public List<PackageElement> GetImportedPackages()
        {
            return importedPackages;
        }
        public BundleElement GetNext(Branch b)
        {
            BundleElement bundle;
            next.TryGetValue(b, out bundle);
            return bundle;
        }
        public BundleElement GetPrevious(Branch b)
        {
            BundleElement bundle;
            previous.TryGetValue(b, out bundle);
            return bundle;
        }
        public BundleMaster GetMaster()
        {
            return master;
        }
        public Commit GetCommit()
        {
            return commit;
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

        #endregion

    }
}