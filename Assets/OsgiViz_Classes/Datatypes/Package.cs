using OSGI_Datatypes.ArchitectureElements;
using OSGI_Datatypes.OrganisationElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OsgiViz.SoftwareArtifact
{

    public class Package
    {
        int neoId;
        private bool exported;
        private string name;
        int nrOfImports;
        private List<CompilationUnit> compilationUnits;
        private Bundle parentBundle;

        //history
        Dictionary<Branch, Package> previous;
        Dictionary<Branch, Package> next;
        PackageMaster master;

        public Package(Bundle parentBund, string n)
        {
            name = n;
            parentBundle  = parentBund;
            compilationUnits = new List<CompilationUnit>();
        }

        public Package(int id, string n, Bundle parentB, bool isExp)
        {
            neoId = id;
            name = n;
            nrOfImports = 0;

            compilationUnits = new List<CompilationUnit>();
            parentBundle = parentB;
            parentBundle.addPackage(this);
            if (isExp)
            {
                parentBundle.addExportedPackage(this);
            }
            previous = new Dictionary<Branch, Package>();
            next = new Dictionary<Branch, Package>();
        }

        #region HistoryElements
        public void AddPrevious(Branch b, Package prev, bool forwadsConnection)
        {
            previous.Add(b, prev);
            if (forwadsConnection)
            {
                prev.AddNext(b, this, false);
            }
        }
        public void AddNext(Branch b, Package nextP, bool backwardsConnection)
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
        public Package GetPrevious(Branch b)
        {
            Package p;
            previous.TryGetValue(b, out p);
            return p;
        }
        public Package GetNext(Branch b)
        {
            Package p;
            next.TryGetValue(b, out p);
            return p;
        }
        public PackageMaster GetMaster()
        {
            return master;
        }
        #endregion

        public int GetNeoId()
        {
            return neoId;
        }

        public void SetNeoId(int id)
        {
            neoId = id;
        }
        public string getName()
        {
            return name;
        }
        public long getLOC()
        {
            long result = 0;

            foreach (CompilationUnit cu in compilationUnits)
            {
                result += cu.getLoc();
            }

            return result;
        }
        public List<CompilationUnit> getCompilationUnits()
        {
            return compilationUnits;
        }
        public Bundle getBundle()
        {
            return parentBundle;
        }
        public long getCuCount()
        {
            long result = 0;

            foreach (CompilationUnit cu in compilationUnits)
                result++;

            return result;
        }
        public bool isExported()
        {
            return exported;
        }

        public void addCompilationUnit(CompilationUnit cu)
        {
            compilationUnits.Add(cu);
        }

        public void setParentBundle(Bundle parent)
        {
            parentBundle = parent;
        }
        public void setExport(bool exp)
        {
            exported = exp;
        }


        public Dictionary<int, CompilationUnit> GetCompUnitDictionary()
        {
            Dictionary<int, CompilationUnit> childDict = new Dictionary<int, CompilationUnit>();

            foreach (CompilationUnit ch in compilationUnits)
            {
                childDict.Add(ch.GetNeoId(), ch);
            }

            return childDict;
        }

        public void IncreaseNrOfImports(int dif)
        {
            nrOfImports += dif;
        }
    }


}
