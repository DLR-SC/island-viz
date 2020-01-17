using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.ArchitectureElements;
using System;
using OsgiViz.SoftwareArtifact;

namespace OSGI_Datatypes.OrganisationElements
{
    public class Commit : IComparable
    {
        //MetaData
        int commitId;
        Author author;
        Branch branch;
        long time;
        List<Issue> issues;
        string commitIdString;
        string commitMessage;

        //Artefakts
        List<Bundle> bundles;
        List<ArchitectureElements.Service> services;
        List<ArchitectureElements.ServiceComponent> serviceComponents;

        //History
        Dictionary<Branch, Commit> next;
        Dictionary<Branch, Commit> previous;

        //VizInfo
        bool isGraphLayouted;
        bool isIslandsLayouted;

        public Commit(Author a, Branch b, long t, int id, string idSt, string msg, List<Issue> issueList)
        {
            author = a;
            branch = b;
            time = t;
            commitId = id;
            commitIdString = idSt;
            commitMessage = msg;
            issues = issueList;

            bundles = new List<Bundle>();
            services = new List<ArchitectureElements.Service>();
            serviceComponents = new List<ArchitectureElements.ServiceComponent>();
            next = new Dictionary<Branch, Commit>();
            previous = new Dictionary<Branch, Commit>();
        }

        #region Aditional_Creation_Mathods
        public void AddBundle(Bundle b)
        {
            bundles.Add(b);
        }
        public void AddService(ArchitectureElements.Service s)
        {
            services.Add(s);
        }
        public void AddServiceComponent(ArchitectureElements.ServiceComponent sc)
        {
            serviceComponents.Add(sc);
        }
        public void AddNext(Branch b, Commit c)
        {
            if (!next.ContainsKey(b))
            {
                next.Add(b, c);
            }
            else if (next[b] != c)
            {
                Debug.LogError("Error at OSGI_Datatypes.OrganisationElements.Commit - AddNext(): A next Commit for the chosen branch already exists");
            }

        }
        public void AddPrevious(Branch b, Commit c)
        {
            previous.Add(b, c);
        }
        #endregion

        /// <summary>
        /// Compares By Time and by Branch hierarchy as second criterion
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if(obj == null)
            {
                return 1;
            }
            if (obj is Commit)
            {
                Commit other = (Commit)obj;
                int timeCompRes = GetTime().CompareTo(other.GetTime());
                if (timeCompRes != 0)
                {
                    return timeCompRes;
                }
                else
                {
                    return GetBranch().CompareTo(other.GetBranch());
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Compares By Branch first (higher ranked branch->commit earlier in order) and time in same branch as second criterion
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareToByBranchFirst(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is Commit)
            {
                Commit other = (Commit)obj;
                int branchCompRes = GetBranch().CompareTo(other.GetBranch());
                if(branchCompRes != 0)
                {
                    return branchCompRes;
                }
                return GetTime().CompareTo(other.GetTime());
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public Dictionary<int, Bundle> GetBundleDictionary()
        {
            Dictionary<int, Bundle> bundleDict = new Dictionary<int, Bundle>();

            foreach(Bundle b in bundles){
                bundleDict.Add(b.GetNeoId(), b);
            }
            return bundleDict;
        }

        public Dictionary<int, Package> GetPackageDictionary()
        {
            Dictionary<int, Package> packageDict = new Dictionary<int, Package>();

            foreach(Bundle b in bundles)
            {
                Dictionary<int, Package> partPackDict = b.GetPackageDictionary();
                foreach(KeyValuePair<int, Package> kvp in partPackDict)
                {
                    packageDict.Add(kvp.Key, kvp.Value);
                }
            }

            return packageDict;

        }

        public Dictionary<int, CompilationUnit> GetCompUnitDictionary()
        {
            Dictionary<int, CompilationUnit> cuDict = new Dictionary<int, CompilationUnit>();

            foreach(Bundle bundle in bundles)
            {
                foreach(Package package in bundle.getPackages())
                {
                    foreach(CompilationUnit cu in package.getCompilationUnits())
                    {
                        cuDict.Add(cu.GetNeoId(), cu);
                    }
                }
            }
            return cuDict;
        }

        public int GetBundleCount()
        {
            return bundles.Count;
        }
        public int GetPackageCount()
        {
            int result = 0;
            foreach(Bundle b in bundles)
            {
                result += b.getPackages().Count;
            }
            return result;
        }
        public int GetCompUnitCount()
        {
            int result = 0;
            foreach(Bundle b in bundles)
            {
                foreach(Package p in b.getPackages())
                {
                    result += p.getCompilationUnits().Count;
                }
            }
            return result;
        }

        #region Standard_Getter_Setter
        public int GetNeoId()
        {
            return commitId;
        }
        public long GetTime()
        {
            return time;
        }

        public Branch GetBranch()
        {
            return branch;
        }


        #endregion

        #region Layout_Administration
        public bool IsGraphLayouted()
        {
            return isGraphLayouted;
        }
        public bool IsIslandsLayouted()
        {
            return isIslandsLayouted;
        }

        public void SetIsGraphLayouted(bool value)
        {
            isGraphLayouted = value;
        }
        public void SetIsIslandsLayouted(bool value)
        {
            isIslandsLayouted = value;
        }
        
        
        public Commit GetPrevious(Branch b)
        {
            Commit c = null;
            previous.TryGetValue(b, out c);
            return c;
        }
        public Commit GetNext(Branch b)
        {
            Commit c = null;
            next.TryGetValue(b, out c);
            return c;
        }


        public List<Bundle> GetBundles()
        {
            return bundles;
        }
        #endregion
    }
}

