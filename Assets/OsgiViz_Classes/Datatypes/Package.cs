using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OsgiViz.SoftwareArtifact
{

    public class Package
    {
        private bool exported;
        private string name;
        private List<CompilationUnit> compilationUnits;
        private Bundle parentBundle;

        public Package(Bundle parentBund, string n)
        {
            name = n;
            parentBundle  = parentBund;
            compilationUnits = new List<CompilationUnit>();
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

        

      

    }


}
