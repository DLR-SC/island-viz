using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ArchitectureElements;
using System;
using OSGI_Datatypes.ComposedTypes;

namespace HexLayout.Basics
{
    public class HexGrid
    {
        public enum KoordinateMode
        {
            grid,
            circle
        }
        private List<Cell> cellList;
        private Dictionary<int, Dictionary<int, Cell>> cellsByGrid;
        private Dictionary<int, Dictionary<int, Cell>> cellsByCircle;
        private Dictionary<Commit, int> outerAssingedRingTotalDict;
        private Dictionary<Commit, int> outerAssignedRingFirstTwoSixthsDict;


        private int outerAssignedRingTotal;
        private int outerAssignedRingFirstTwoSixths;

        public HexGrid(int xmin, int xmax, int zmin, int zmax)
        {
            outerAssignedRingTotal = -1;
            outerAssignedRingFirstTwoSixths = -1;
            outerAssingedRingTotalDict = new Dictionary<Commit, int>();
            outerAssignedRingFirstTwoSixthsDict = new Dictionary<Commit, int>();

            //Initialise Collections
            cellList = new List<Cell>();
            cellsByGrid = new Dictionary<int, Dictionary<int, Cell>>();
            cellsByCircle = new Dictionary<int, Dictionary<int, Cell>>();

            //Create Starting Cells and Add To CellsByGrid
            for (int x = xmin; x <= xmax; x++)
            {
                cellsByGrid.Add(x, new Dictionary<int, Cell>());
                for (int z = zmin; z <= zmax; z++)
                {
                    Cell newCell = new Cell(Cell.KoordinateMode.grid, x, z, this);
                    cellList.Add(newCell);
                    cellsByGrid[x].Add(z, newCell);
                }
            }

            //AddCells To CellsByRing
            cellList.Sort((x, y) => (x.ComparatorCirclePos(y)));
            foreach (Cell cell in cellList)
            {
                int[] ringCoordinates = cell.GetCircleCoordinates();
                if (!cellsByCircle.ContainsKey(ringCoordinates[0]))
                {
                    cellsByCircle.Add(ringCoordinates[0], new Dictionary<int, Cell>());
                }
                cellsByCircle[ringCoordinates[0]].Add(ringCoordinates[1], cell);
            }
        }

        public HexGrid(int nrOfRings)
        {
            outerAssignedRingTotal = -1;
            outerAssignedRingFirstTwoSixths = -1;
            outerAssingedRingTotalDict = new Dictionary<Commit, int>();
            outerAssignedRingFirstTwoSixthsDict = new Dictionary<Commit, int>();
            
            //Initialise Collections
            cellList = new List<Cell>();
            cellsByGrid = new Dictionary<int, Dictionary<int, Cell>>();
            cellsByCircle = new Dictionary<int, Dictionary<int, Cell>>();

            Cell center = new Cell(Cell.KoordinateMode.circle, 0, 0, this);
            cellsByGrid.Add(0, new Dictionary<int, Cell>() { { 0, center } });
            cellsByCircle.Add(0, new Dictionary<int, Cell>() { { 0, center } });
            cellList.Add(center);

            for (int ring = 1; ring <= nrOfRings; ring++)
            {
                CreateRing(ring);
            }
        }

        public HexGrid()
        {
            cellList = new List<Cell>();
            cellsByGrid = new Dictionary<int, Dictionary<int, Cell>>();
            cellsByCircle = new Dictionary<int, Dictionary<int, Cell>>();
            outerAssignedRingTotal = -1;
            outerAssignedRingFirstTwoSixths = -1;
            outerAssingedRingTotalDict = new Dictionary<Commit, int>();
            outerAssignedRingFirstTwoSixthsDict = new Dictionary<Commit, int>();
        }

        private void AddCellToGrid(Cell cell)
        {
            cellList.Add(cell);

            int[] gridK = cell.GetGridCoordinates();
            if (!cellsByGrid.ContainsKey(gridK[0]))
            {
                cellsByGrid.Add(gridK[0], new Dictionary<int, Cell>());
            }
            cellsByGrid[gridK[0]].Add(gridK[1], cell);

            int[] circleK = cell.GetCircleCoordinates();
            if (!cellsByCircle.ContainsKey(circleK[0]))
            {
                cellsByCircle.Add(circleK[0], new Dictionary<int, Cell>());
            }
            cellsByCircle[circleK[0]].Add(circleK[1], cell);
        }

        private void CreateRing(int ring)
        {
            if (cellsByCircle.ContainsKey(ring))
            {
                FillRing(ring);
            }
            cellsByCircle.Add(ring, new Dictionary<int, Cell>());
            FillRing(ring);
        }

        private void FillRing(int ring)
        {
            if (!cellsByCircle.ContainsKey(ring))
            {
                CreateRing(ring);
            }
            else
            {
                if (ring == 0 && cellsByCircle[ring].Count != 1)
                {
                    Cell newCell = new Cell(Cell.KoordinateMode.circle, 0, 0, this);
                    AddCellToGrid(newCell);
                }
                else if(cellsByCircle[ring].Count != 6 * ring)
                {
                    int i = 0;
                    while(i<6*ring && cellsByCircle[ring].Count < 6 * ring)
                    {
                        if (!cellsByCircle[ring].ContainsKey(i))
                        {
                            Cell newCell = new Cell(Cell.KoordinateMode.circle, ring, i, this);
                            AddCellToGrid(newCell);
                        }
                        i++;
                    }
                }
            }
        }

        public void FillRingToOuterAssigned()
        {
            for(int i = 0; i<=outerAssignedRingTotal; i++)
            {
                FillRing(i);
            }
        }

        public Cell GetCellByGrid(int x, int z)
        {
            if (ExistCellByGridCoordinate(x, z))
            {
                return cellsByGrid[x][z];
            }
            return null;
        }

        public Cell GetCellByGridCreateRing(int x, int z)
        {
            Cell cell = GetCellByGrid(x, z);
            if (cell == null)
            {
                cell = new Cell(Cell.KoordinateMode.grid, x, z, this);
                AddCellToGrid(cell);
            }
            int[] circleKoordinates = cell.GetCircleCoordinates();
            FillRing(circleKoordinates[0]);
            return cell;
        }

        public Cell GetCellByGridCreateCell(int x, int z)
        {
            Cell cell = GetCellByGrid(x, z);
            if (cell != null)
            {
                return cell;
            }
            cell = new Cell(Cell.KoordinateMode.grid, x, z, this);
            AddCellToGrid(cell);
            return cell;
        }

        public Cell GetCellByCircle(int ring, int place)
        {
            if (ExistCellByCircleCoordinate(ring, place))
            {
                return cellsByCircle[ring][place];
            }
            return null;
        }

        public Cell GetCellByCircleCreateRing(int ring, int place)
        {
            Cell result = GetCellByCircle(ring, place);
            if (result == null)
            {
                result = new Cell(Cell.KoordinateMode.circle, ring, place, this);
                AddCellToGrid(result);
            }
           
            FillRing(ring);
            return result;
        }

        public Cell GetCellByCircleCreateCell(int ring, int place)
        {
            Cell result = GetCellByCircle(ring, place);
            if (result != null)
            {
                return result;
            }
            result = new Cell(Cell.KoordinateMode.circle, ring, place, this);
            AddCellToGrid(result);
            return result;
        }

        public bool ExistCellByGridCoordinate(int x, int z)
        {
            if (!cellsByGrid.ContainsKey(x))
            {
                return false;
            }
            if (cellsByGrid[x].ContainsKey(z))
            {
                return true;
            }
            return false;
        }

        public bool ExistCellByCircleCoordinate(int ring, int place)
        {
            if (!cellsByCircle.ContainsKey(ring))
            {
                return false;
            }
            if (cellsByCircle[ring].ContainsKey(place))
            {
                return true;
            }
            return false;
        }

        public Cell GetRandomFreeCell()
        {
            int tries = 0;
            while (tries < 5)
            {
                Cell cell = cellList[UnityEngine.Random.Range(0, cellList.Count)];
                if (!cell.IsAssignedToRegion())
                {
                    return cell;
                }
                tries++;
            }
            return null;
        }

        public List<Cell> GetCells()
        {
            return cellList;
        }

        /// <summary>
        /// Updates Outer Assigned Values
        /// </summary>
        /// <param name="cell"></param>
        public void CellAssigned(Cell cell)
        {
            int[] circleCoordinates = cell.GetCircleCoordinates();
            int[] sixthInfo = HexHelper.GetSixthInRingAndOffset(circleCoordinates[0], circleCoordinates[1]);

            //Update outer Assigned ring total
            if (circleCoordinates[0] > outerAssignedRingTotal)
            {
                outerAssignedRingTotal = circleCoordinates[0];
            }
            //Update outer Assigned in first and second sixth if cell is there
            if(sixthInfo[0]==0 || sixthInfo[0]==1 || sixthInfo[0]==2 && sixthInfo[1] == 0)
            {
                if(circleCoordinates[0]> outerAssignedRingFirstTwoSixths)
                {
                    outerAssignedRingFirstTwoSixths = circleCoordinates[0];
                }
            }
        }

        public void SaveOuterAssigned(Commit commit)
        {
            outerAssingedRingTotalDict.Add(commit, outerAssignedRingTotal);
            outerAssignedRingFirstTwoSixthsDict.Add(commit, outerAssignedRingFirstTwoSixths);
        }

        public int GetOuterAssignedFirstTwoSixths(Commit commit)
        {
            int value = 0;
            outerAssignedRingFirstTwoSixthsDict.TryGetValue(commit, out value);
            return value;
        }

        public int GetOuterAssignedTotal(Commit commit)
        {
            int value = 0;
            outerAssingedRingTotalDict.TryGetValue(commit, out value);
            return value;
        }

        public int GetOuterAssignedRing()
        {
            return outerAssignedRingTotal;
        }

        public void FillOuterAssignedDictionarys(List<Commit> commitList)
        {
            foreach(Commit commit in commitList)
            {
                outerAssignedRingTotal = 0;
                outerAssignedRingFirstTwoSixths = 0;
                foreach(Cell cell in cellList)
                {
                    CompUnitMaster cum = cell.GetCompUnit();
                    if (cum!=null)
                    {
                        TimelineStatus stat = cum.RelationOfCommitToTimeline(commit);
                        if(stat==TimelineStatus.present || stat == TimelineStatus.notPresentAnymore)
                        {
                            int[] cellPos = cell.GetCircleCoordinates();
                            int[] sixthInfo = HexHelper.GetSixthInRingAndOffset(cellPos[0], cellPos[1]);

                            if (cellPos[0] > outerAssignedRingTotal)
                            {
                                outerAssignedRingTotal = cellPos[0];
                            }
                            if((sixthInfo[0]==0|| sixthInfo[0]== 1 || sixthInfo[0]==2&&sixthInfo[1]==0) && cellPos[0] > outerAssignedRingFirstTwoSixths)
                            {
                                outerAssignedRingFirstTwoSixths = cellPos[0];
                            }
                        }

                    }
                }
                outerAssingedRingTotalDict.Add(commit, outerAssignedRingTotal);
                outerAssignedRingFirstTwoSixthsDict.Add(commit, outerAssignedRingFirstTwoSixths);
            }


        }
    }
}