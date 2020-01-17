using System.Collections;
using System;
using UnityEngine;
using OSGI_Datatypes.ComposedTypes;
using OSGI_Datatypes.OrganisationElements;
using HexLayout.Basics;
using OsgiViz.SoftwareArtifact;

namespace OSGI_Datatypes.ArchitectureElements
{
    public class CompUnitMaster: IComparable {

        Timeline<CompilationUnit> realCompUnits;
        PackageMaster parent;

        //visualization hexGrid Position
        Vector2Int gridPos;
        Cell cell;

        private long maxLoc;


        public CompUnitMaster(Commit c, CompilationUnit firstE, PackageMaster p)
        {
            parent = p;
            p.AddContaindCompUnit(this);
            realCompUnits = new Timeline<CompilationUnit>();
            realCompUnits.Add(c, firstE);
            gridPos = new Vector2Int(-9999, -9999);
            maxLoc = firstE.getLoc();
        }

        public void AddElement(Commit c, CompilationUnit cu)
        {
            realCompUnits.Add(c, cu);
            if(maxLoc < cu.getLoc())
            {
                maxLoc = cu.getLoc();
            }
        }

        public PackageMaster GetParent()
        {
            return parent;
        }
        public CompilationUnit GetElement(Commit c)
        {
            return realCompUnits.Get(c);
        }

        public TimelineStatus RelationOfCommitToTimeline(Commit c)
        {
            return realCompUnits.RelationOfCommitToTimeline(c);
        }

        public bool HasValidGridInfo()
        {
            return (gridPos.x != -9999 || gridPos.y != -9999);
        }

        public void SetGridInfo(int p1, int p2)
        {
            gridPos = new Vector2Int(p1, p2);
        }
        public Vector2Int GetGridPos()
        {
            return gridPos;
        }

        public void SetCell(Cell c)
        {
            cell = c;
        }
        public Cell GetCell()
        {
            return cell;
        }

        public Commit GetStart(SortTypes sortType)
        {
            return realCompUnits.GetStart(sortType);
        }

        public CompilationUnit GetStartElement(SortTypes sortType)
        {
            return GetElement(GetStart(sortType));
        }

        public CompilationUnit GetEndElement(SortTypes sortType)
        {
            return GetElement(GetEnd(sortType));
        }

        public Commit GetEnd(SortTypes sortType)
        {
            return realCompUnits.GetEnd(sortType);
        }

        public long GetMaxLoc()
        {
            return maxLoc;
        }

        public int CompareTo(object other)
        {
            if (other == null)
                return 1;
            CompUnitMaster otherMaster = other as CompUnitMaster;
            if (otherMaster == null)
                return 1;
            int startCompare = GetStart(SortTypes.byTime).CompareTo(otherMaster.GetStart(SortTypes.byTime));
            if (startCompare != 0)
            {
                return startCompare;
            }
            return GetEnd(SortTypes.byTime).CompareTo(otherMaster.GetEnd(SortTypes.byTime));
        }

    }
}