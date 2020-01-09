using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using OsgiViz.Core;


namespace OsgiViz.SoftwareArtifact
{

    public class CompilationUnit
    {

        //Variables
        private string name;
        private type type;
        private modifier modif;
        private long loc;
        private Package containingPackage;
        private bool isService;
        private bool isServiceComponent;
        private GameObject goRepresentation;

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