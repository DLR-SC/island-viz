//heid_el, 17.01.2020 PackageElement obsolete
//replaced by original IslandViz Package Class

/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.OrganisationElements;


namespace OSGI_Datatypes.ArchitectureElements
{
    public class PackageElement
    {
        //data
        int neoId;
        string name;
        string symbolicName;
        bool isExported;
        int nrOfImports;

        //hierarchy
        List<CompUnitElement> compUnits;
        BundleElement parentBundle;

        //history
        Dictionary<Branch, PackageElement> previous;
        Dictionary<Branch, PackageElement> next;
        PackageMaster master;



        public PackageElement(int id, string n, string sn, BundleElement parentB, bool isExp)
        {
            neoId = id;
            name = n;
            symbolicName = sn;
            isExported = isExp;
            nrOfImports = 0;

            compUnits = new List<CompUnitElement>();
            parentBundle = parentB;
            //Automatically adds to bundles export list if  isExp=true
            parentB.AddPackage(this, isExp);
            previous = new Dictionary<Branch, PackageElement>();
            next = new Dictionary<Branch, PackageElement>();
        }
        
        #region Additional_Creation_Methods
        public void AddCompUnit(CompUnitElement cu)
        {
            compUnits.Add(cu);
        }
        public void AddPrevious(Branch b, PackageElement prev, bool forwadsConnection)
        {
            previous.Add(b, prev);
            if (forwadsConnection)
            {
                prev.AddNext(b, this, false);
            }
        }
        public void AddNext(Branch b, PackageElement nextP, bool backwardsConnection)
        {
            next.Add(b, nextP);
            if (backwardsConnection)
            {
                nextP.AddPrevious(b, this, false);
            }
        }
        public void SetMaster(PackageMaster m, Commit c)
        {
            master = m;
            master.AddElement(c, this);
        }
       
        #endregion

        public Dictionary<int, CompUnitElement> GetCompUnitDictionary()
        {
            Dictionary<int, CompUnitElement> childDict = new Dictionary<int, CompUnitElement>();

            foreach(CompUnitElement ch in compUnits)
            {
                childDict.Add(ch.GetNeoId(), ch);
            }

            return childDict;
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
        public bool IsExported()
        {
            return isExported;
        }
        public void SetExported()
        {
            isExported = true;
        }
        public int GetNrOfImports()
        {
            return nrOfImports;
        }
        public void IncreaseNrOfImports(int dif)
        {
            nrOfImports += dif;
        }
        public List<CompUnitElement> GetCompUnits()
        {
            return compUnits;
        }
        public BundleElement GetParentBundle()
        {
            return parentBundle;
        }
        public PackageElement GetPrevious(Branch b)
        {
            PackageElement p;
            previous.TryGetValue(b, out p);
            return p;
        }
        public PackageElement GetNext(Branch b)
        {
            PackageElement p;
            next.TryGetValue(b, out p);
            return p;
        }
        public PackageMaster GetMaster()
        {
            return master;
        }
        #endregion

    }
}*/