//heid_el, 17.01.2020 ArchitectureElements.Service obsolete
//replaced by original SoftwareArtifacts.Service Class
/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.OrganisationElements;


namespace OSGI_Datatypes.ArchitectureElements
{
    public class Service {

        int neoId;
        CompUnitElement compUnit;
        List<ServiceComponent> providedScs;
        List<ServiceComponent> referencedScs;

        public Service(int id, CompUnitElement cu)
        {
            neoId = id;
            compUnit = cu;
            cu.SetService(this);
            providedScs = new List<ServiceComponent>();
            referencedScs = new List<ServiceComponent>();
        }
        public void AddProvidingSc(ServiceComponent sc, bool propagate)
        {
            providedScs.Add(sc);
            if (propagate)
            {
                sc.AddProvidedService(this, false);
            }
        }
        public void AddReferencingSc(ServiceComponent sc, bool propagate)
        {
            referencedScs.Add(sc);
            if (propagate)
            {
                sc.AddReferencedService(this, false);
            }
        }

    }
}*/