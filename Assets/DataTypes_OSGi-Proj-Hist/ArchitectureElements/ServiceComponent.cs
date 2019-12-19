using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.OrganisationElements;


namespace OSGI_Datatypes.ArchitectureElements
{
    public class ServiceComponent
    {
        int neoId;
        string name;
        CompUnitElement compUnit;
        List<Service> providedServices;
        List<Service> referencedServices;

        public ServiceComponent(int id, string n, CompUnitElement cu)
        {
            neoId = id;
            name = n;
            compUnit = cu;
            cu.AddServiceComponent(this);
            providedServices = new List<Service>();
            referencedServices = new List<Service>();
        }
        public void AddProvidedService(Service s, bool propagate)
        {
            providedServices.Add(s);
            if (propagate)
            {
                s.AddProvidingSc(this, false);
            }
        }
        public void AddReferencedService(Service s, bool propagate)
        {
            referencedServices.Add(s);
            if (propagate)
            {
                s.AddReferencingSc(this, false);
            }
        }
    
    }
}