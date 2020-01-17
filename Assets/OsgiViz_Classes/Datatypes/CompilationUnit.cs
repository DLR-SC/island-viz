using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using OsgiViz.Core;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ArchitectureElements;

namespace OsgiViz.SoftwareArtifact
{

    public class CompilationUnit
    {

        //Variables
        int neoId;
        private string name;
        private type type;
        private modifier modif;
        private long loc;
        private Package containingPackage;
        private bool isService;
        private bool isServiceComponent;
        private GameObject goRepresentation;

        //History
        Dictionary<Branch, CompilationUnit> previous;
        Dictionary<Branch, CompilationUnit> next;
        CompUnitMaster master;

        public CompilationUnit(string n, type t, modifier m, long l, Package containingPackage)
        {
            name  = n;
            type  = t;
            modif = m;
            loc   = l;
            this.containingPackage = containingPackage;
            isService = false;
            isServiceComponent = false;
            goRepresentation = null;
        }

        public CompilationUnit(int id, string n, type t, modifier m, long l, Package p)
        {
            neoId = id;
            name = n;
            type = t;
            modif = m;
            loc = l;
            containingPackage = p;
            p.addCompilationUnit(this);
            isService = false;
            isServiceComponent = false;
            goRepresentation = null;
            previous = new Dictionary<Branch, CompilationUnit>();
            next = new Dictionary<Branch, CompilationUnit>();
        }

        #region HistoryElements

        public void AddPrevious(Branch b, CompilationUnit prev, bool forwadsConnection)
        {
            previous.Add(b, prev);
            if (forwadsConnection)
            {
                prev.AddNext(b, this, false);
            }
        }
        public void AddNext(Branch b, CompilationUnit nextP, bool backwardsConnection)
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
        public CompilationUnit GetPrevious(Branch b)
        {
            CompilationUnit cuP;
            previous.TryGetValue(b, out cuP);
            return cuP;
        }
        public CompilationUnit GetNext(Branch b)
        {
            CompilationUnit cuN;
            next.TryGetValue(b, out cuN);
            return cuN;
        }
        public CompUnitMaster GetMaster()
        {
            return master;
        }

        #endregion

        public int GetNeoId()
        {
            return neoId;
        }

        public Package getContainingPackage()
        {
            return containingPackage;
        }
        public GameObject getGameObject()
        {
            return goRepresentation;
        }
        public string getName()
        {
            return name;
        }
        public long getLoc()
        {
            return loc;
        }
        public bool declaresService()
        {
            return isService;
        }
        public bool implementsServiceComponent()
        {
            return isServiceComponent;
        }

        public void setGameObject(GameObject go)
        {
            goRepresentation = go;
        }
        public void setServiceDeclaration(bool value)
        {
            isService = value;
        }
        public void setServiceComponentImpl(bool value)
        {
            isServiceComponent = value;
        }
        public modifier GetModifier ()
        {
            return modif;
        }
        public type GetType ()
        {
            return type;
        }



    }
}