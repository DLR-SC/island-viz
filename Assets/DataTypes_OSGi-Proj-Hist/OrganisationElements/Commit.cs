using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.ArchitectureElements;
using System;

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
        List<BundleElement> bundles;
        List<Service> services;
        List<ServiceComponent> serviceComponents;

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

            bundles = new List<BundleElement>();
            services = new List<Service>();
            serviceComponents = new List<ServiceComponent>();
            next = new Dictionary<Branch, Commit>();
            previous = new Dictionary<Branch, Commit>();
        }

        #region Aditional_Creation_Mathods
        public void AddBundle(BundleElement b)
        {
            bundles.Add(b);
        }
        public void AddService(Service s)
        {
            services.Add(s);
        }
        public void AddServiceComponent(ServiceComponent sc)
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

        public Dictionary<int, BundleElement> GetBundleDictionary()
        {
            Dictionary<int, BundleElement> bundleDict = new Dictionary<int, BundleElement>();

            foreach(BundleElement b in bundles){
                bundleDict.Add(b.GetNeoId(), b);
            }
            return bundleDict;
        }

        public Dictionary<int, PackageElement> GetPackageDictionary()
        {
            Dictionary<int, PackageElement> packageDict = new Dictionary<int, PackageElement>();

            foreach(BundleElement b in bundles)
            {
                Dictionary<int, PackageElement> partPackDict = b.GetPackageDictionary();
                foreach(KeyValuePair<int, PackageElement> kvp in partPackDict)
                {
                    packageDict.Add(kvp.Key, kvp.Value);
                }
            }

            return packageDict;

        }

        public Dictionary<int, CompUnitElement> GetCompUnitDictionary()
        {
            Dictionary<int, CompUnitElement> cuDict = new Dictionary<int, CompUnitElement>();

            foreach(BundleElement bundle in bundles)
            {
                foreach(PackageElement package in bundle.GetPackages())
                {
                    foreach(CompUnitElement cu in package.GetCompUnits())
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
            foreach(BundleElement b in bundles)
            {
                result += b.GetPackages().Count;
            }
            return result;
        }
        public int GetCompUnitCount()
        {
            int result = 0;
            foreach(BundleElement b in bundles)
            {
                foreach(PackageElement p in b.GetPackages())
                {
                    result += p.GetCompUnits().Count;
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


        public List<BundleElement> GetBundles()
        {
            return bundles;
        }
        #endregion
    }
}

