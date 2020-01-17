//heid_el, 17.01.2020 CompUnitElement obsolete
//replaced by original IslandViz CompilationUnit Class

/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.OrganisationElements;

namespace OSGI_Datatypes.ArchitectureElements
{
    public class CompUnitElement
    {
        //data
        int neoId;
        string name;
        string qalifiedName;
        int loc;
        bool isInterface;
        Service service;
        List<ServiceComponent> serviceComponents;

        //hierarchy
        PackageElement parentPackage;

        //history
        Dictionary<Branch, CompUnitElement> previous;
        Dictionary<Branch, CompUnitElement> next;
        CompUnitMaster master;


        public CompUnitElement(int id, string n, string qn, int l, bool isIf, PackageElement p)
        {
            neoId = id;
            name = n;
            qalifiedName = qn;
            loc = l;
            parentPackage = p;
            p.AddCompUnit(this);
            isInterface = isIf;
            previous = new Dictionary<Branch, CompUnitElement>();
            next = new Dictionary<Branch, CompUnitElement>();
            serviceComponents = new List<ServiceComponent>();
        }

        #region Additional_Creation_Methods
        public void AddPrevious(Branch b, CompUnitElement prev, bool forwadsConnection)
        {
            previous.Add(b, prev);
            if (forwadsConnection)
            {
                prev.AddNext(b, this, false);
            }
        }
        public void AddNext(Branch b, CompUnitElement nextP, bool backwardsConnection)
        {
            next.Add(b, nextP);
            if (backwardsConnection)
            {
                nextP.AddPrevious(b, this, false);
            }
        }
        public void SetMaster(CompUnitMaster m, Commit c)
        {
            master = m;
            master.AddElement(c, this);
        }

        public void SetService(Service s)
        {
            if(service != null)
            {
                Debug.LogWarning("Compilation Unit " + name + " is assigned to more than one service");
                return;
            }
            service = s;
        }
        public void AddServiceComponent(ServiceComponent sc)
        {
            serviceComponents.Add(sc);
        }
        
        #endregion


        #region Standard_Getter_Setter
        public int GetNeoId()
        {
            return neoId;
        }
        public string GetName()
        {
            return name;
        }
        public int GetLoc()
        {
            return loc;
        }
        public PackageElement GetParentPackage()
        {
            return parentPackage;
        }
        public CompUnitElement GetPrevious(Branch b)
        {
            CompUnitElement cuP;
            previous.TryGetValue(b, out cuP);
            return cuP;
        }
        public CompUnitElement GetNext(Branch b)
        {
            CompUnitElement cuN;
            next.TryGetValue(b, out cuN);
            return cuN;
        }
        public CompUnitMaster GetMaster()
        {
            return master;
        }
        
        #endregion

    }
}*/