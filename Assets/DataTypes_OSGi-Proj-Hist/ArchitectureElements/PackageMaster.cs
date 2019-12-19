using System;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ComposedTypes;
using HexLayout.Basics;

namespace OSGI_Datatypes.ArchitectureElements
{
    public class PackageMaster:IComparable
    {
        Timeline<PackageElement> realPackages;
        BundleMaster parent;
        List<CompUnitMaster> containedCompUnits;

        //visialization Info
        Vector2Int growCorridorStart;
        Cell startCell;
        Region region;



        public PackageMaster(Commit c, PackageElement firstE, BundleMaster bm)
        {
            realPackages = new Timeline<PackageElement>();
            realPackages.Add(c, firstE);
            parent = bm;
            bm.AddContaindPackage(this);
            containedCompUnits = new List<CompUnitMaster>();
            growCorridorStart = new Vector2Int(-9999, -9999);
        }

        public void AddElement(Commit c, PackageElement p)
        {
            realPackages.Add(c, p);
        }
        public PackageElement GetElement(Commit c)
        {
            return realPackages.Get(c);
        }

        public TimelineStatus RelationOfCommitToTimeline(Commit c)
        {
            return realPackages.RelationOfCommitToTimeline(c);
        }
        public void AddContaindCompUnit(CompUnitMaster cum)
        {
            if (!containedCompUnits.Contains(cum))
            {
                containedCompUnits.Add(cum);
            }
        }

        public List<CompUnitMaster> GetContainedMasterCompUnits()
        {
            return containedCompUnits;
        }
        public Commit GetStart(SortTypes sortType)
        {
            return realPackages.GetStart(sortType);
        }
        public Commit GetEnd(SortTypes sortType)
        {
            return realPackages.GetEnd(sortType);
        }

        public PackageElement GetStartElement(SortTypes sortType)
        {
            return GetElement(GetStart(sortType));
        }

        public PackageElement GetEndElement(SortTypes sortType)
        {
            return GetElement(GetEnd(sortType));
        }


        public bool HasValidGrowCorridorStart()
        {
            return (growCorridorStart.x != -9999 || growCorridorStart.y != -9999);
        }
        public void SetGrowCorridorStart(int p1, int p2)
        {
            growCorridorStart = new Vector2Int(p1, p2);
        }
        public Vector2Int GetGrowCorridorStart()
        {
            return growCorridorStart;
        }

        public void SetStartCell(Cell sc)
        {
            startCell = sc;
        }
        public Cell GetStartCell()
        {
            return startCell;
        }

        public void SetRegion(Region r)
        {
            region = r;
        }
        public Region GetRegion()
        {
            return region;
        }
        public int CompareTo(object other)
        {
            if (other == null)
                return 1;
            PackageMaster otherMaster = other as PackageMaster;
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

        public List<CompUnitMaster> GetInitialCompUnits()
        {
            List<CompUnitMaster> resList = new List<CompUnitMaster>();
            foreach(CompUnitMaster cum in containedCompUnits)
            {
                if (cum.GetStart(SortTypes.byBranch) == GetStart(SortTypes.byBranch))
                {
                    resList.Add(cum);
                }
            }
            return resList;
        }

        /// <summary>
        /// Dictionary associates contained CompUnit with Commit when they first appear in Package
        /// only if first commit of compUnit is not equal to first commit of package (initial compUnits of package will be dealt with differently)
        /// </summary>
        /// <returns></returns>
        public Dictionary<Commit, List<CompUnitMaster>> GetDictOfNewCompUnitsByCommit()
        {
            Dictionary<Commit, List<CompUnitMaster>> resDict = new Dictionary<Commit, List<CompUnitMaster>>();
            Commit packStart = GetStart(SortTypes.byBranch);
            foreach (CompUnitMaster cu in containedCompUnits)
            {
                Commit start = cu.GetStart(SortTypes.byBranch);
                if (start != packStart)
                {
                    if (!resDict.ContainsKey(start))
                    {
                        resDict.Add(start, new List<CompUnitMaster>());
                    }
                    resDict[start].Add(cu);
                } 
            }
            return resDict;
        }

        public int GetMaxLoc()
        {
            int maxLoc = 0;
            foreach(CompUnitMaster cum in containedCompUnits)
            {
                if(maxLoc < cum.GetMaxLoc())
                {
                    maxLoc = cum.GetMaxLoc();
                }
            }
            return maxLoc;
        }

        #endregion

    }
}