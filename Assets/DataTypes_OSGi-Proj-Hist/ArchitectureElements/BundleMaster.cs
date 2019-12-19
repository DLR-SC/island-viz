using System.Collections.Generic;
using HexLayout.Basics;
using OSGI_Datatypes.ComposedTypes;
using OSGI_Datatypes.OrganisationElements;



namespace OSGI_Datatypes.ArchitectureElements
{
    public class BundleMaster
    {
        Timeline<BundleElement> realBundles;
        List<PackageMaster> containedPackages;

        HexGrid grid;

        public BundleMaster(Commit c, BundleElement firstE)
        {
            realBundles = new Timeline<BundleElement>();
            realBundles.Add(c, firstE);
            containedPackages = new List<PackageMaster>();
        }

        public void AddElement(Commit c, BundleElement p)
        {
            realBundles.Add(c, p);
        }

        public BundleElement GetElement(Commit c)
        {
            return realBundles.Get(c);
        }

        public void AddContaindPackage(PackageMaster cp)
        {
            if (!containedPackages.Contains(cp))
            {
                containedPackages.Add(cp);
            }
        }

        public void SetGrid(HexGrid hg)
        {
            grid = hg;
        }

        public HexGrid GetGrid()
        {
            return grid;
        }

        public List<PackageMaster> GetContainedMasterPackages()
        {
            return containedPackages;
        }

        public Commit GetStart(SortTypes sortType)
        {
            return realBundles.GetStart(sortType);
        }
        public Commit GetEnd(SortTypes sortType)
        {
            return realBundles.GetEnd(sortType);
        }
        public TimelineStatus RelationOfCommitToTimeline(Commit c)
        {
            return realBundles.RelationOfCommitToTimeline(c);
        }
        public int CompareTo(object other)
        {
            if (other == null)
                return 1;
            BundleMaster otherMaster = other as BundleMaster;
            if (otherMaster == null)
                return 1;
            int startCompare = GetStart(SortTypes.byTime).CompareTo(otherMaster.GetStart(SortTypes.byTime));
            if (startCompare != 0)
            {
                return startCompare;
            }
            return GetEnd(SortTypes.byTime).CompareTo(otherMaster.GetEnd(SortTypes.byTime));
        }

        #region Functions_For_Layout_Creation

        public List<Commit> GetCommitsByTimeline(SortTypes sortType)
        {
            return realBundles.GetTimeline(sortType);
        }

        public List<CompUnitMaster> GetContainedMasterCompUnits()
        {
            List<CompUnitMaster> resList = new List<CompUnitMaster>();
            foreach(PackageMaster pack in containedPackages)
            {
                resList.AddRange(pack.GetContainedMasterCompUnits());
            }
            return resList;
        }

        /// <summary>
        /// Dictionary associates containedPackages with Commit when they first appear in Bundle
        /// </summary>
        /// <returns></returns>
        public Dictionary<Commit, List<PackageMaster>> GetDictOfNewPackagesByCommit()
        {
            Dictionary < Commit, List < PackageMaster >> resDict = new Dictionary<Commit, List<PackageMaster>>();
            foreach(PackageMaster pack in containedPackages)
            {
                Commit start = pack.GetStart(SortTypes.byBranch);
                if (!resDict.ContainsKey(start))
                {
                    resDict.Add(start, new List<PackageMaster>());
                }
                resDict[start].Add(pack);
            }
            return resDict;
        }


        /// <summary>
        /// Dictionary associates contained Packages with Commit when they lastly appear in Bundle
        /// only if last commit of Package is not equal to last commit of bundle
        /// </summary>
        /// <returns></returns>
        public Dictionary<Commit, List<PackageMaster>> GetDictOfDeletedPackagesByCommit()
        {
            Dictionary<Commit, List<PackageMaster>> resDict = new Dictionary<Commit, List<PackageMaster>>();
            Commit bundleEnd = GetEnd(SortTypes.byTime);
            foreach (PackageMaster pack in containedPackages)
            {
                Commit end = pack.GetEnd(SortTypes.byTime);
                if(bundleEnd != end)
                {
                    if (!resDict.ContainsKey(end))
                    {
                        resDict.Add(end, new List<PackageMaster>());
                    }
                    resDict[end].Add(pack);
                }
            }
            return resDict;
        }

        /// <summary>
        /// Dictionary associates contained CompUntis with Commit when they first appear in bundle
        /// only if CompUnits first Commit not equal to its package's first appear (inital CompUnits of a package will be dealt with differently)
        /// </summary>
        /// <returns></returns>
        public Dictionary<Commit, List<CompUnitMaster>> GetDictOfNewCompUnitsByCommit()
        {
            Dictionary<Commit, List<CompUnitMaster>> resDict = new Dictionary<Commit, List<CompUnitMaster>>();
            foreach(PackageMaster pack in containedPackages)
            {
                Dictionary<Commit, List<CompUnitMaster>> packDict = pack.GetDictOfNewCompUnitsByCommit();
                foreach(KeyValuePair<Commit, List<CompUnitMaster>> kvp in packDict)
                {
                    if (!resDict.ContainsKey(kvp.Key))
                    {
                        resDict.Add(kvp.Key, new List<CompUnitMaster>());
                    }
                    resDict[kvp.Key].AddRange(kvp.Value);
                }
            }
                       
            return resDict; 
        }

        public int GetMaxLoc()
        {
            int maxLoc = 0;

            foreach(PackageMaster pm in containedPackages)
            {
                int l = pm.GetMaxLoc();
                if(maxLoc < l)
                {
                    maxLoc = l;
                }
            }

            return maxLoc;
        }

        public void SetRadiusToElements()
        {
            if (grid == null)
            {
                return;
            }
            foreach(KeyValuePair<Commit, BundleElement> kvp in realBundles.GetDict())
            {
                int radius = grid.GetOuterAssignedTotal(kvp.Key);
                float radiusF = 2f * radius + 2f;
                kvp.Value.SetRadius(radiusF);
            }
        }

        #endregion


    }
}
