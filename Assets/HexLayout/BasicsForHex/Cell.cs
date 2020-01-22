using OSGI_Datatypes.ArchitectureElements;
using System.Collections.Generic;
using UnityEngine;
using Assets;

namespace HexLayout.Basics
{

    public class Cell
    {
        //b factors for neigbouring cell score according to Yang and Buik-Aghai
        //b1 factor for neigbouring cells of same region
        private static float b1 = 1.5f;// b1 = 3f;
                                       //b2 factor for neighbouring cells of different region
        private static float b2 = 1.1f;// b2 = 2f;

        private static float hdt = 0.5f;

        public enum KoordinateMode
        {
            grid,
            circle
        }

        private HexGrid grid;

        private int gridKx;
        private int gridKz;
        private int circleKring;
        private int circleKplace;
        private bool isMainDiagonal;

        private Region assignedRegion;

        private Region blockedRegion;

        private CompUnitMaster compUnit;


        private int timeTag;


        public Cell(KoordinateMode kMode, int firstK, int secondK, HexGrid grid)
        {
            this.grid = grid;
            assignedRegion = null;

            switch (kMode)
            {
                case KoordinateMode.grid:
                    gridKx = firstK;
                    gridKz = secondK;
                    int[] circleKs = HexHelper.ConvertGridToCircularCoordinate(gridKx, gridKz);
                    circleKring = circleKs[0];
                    circleKplace = circleKs[1];
                    break;

                case KoordinateMode.circle:
                    circleKring = firstK;
                    circleKplace = secondK;
                    int[] gridKs = HexHelper.ConvertCicularToGridCoordinate(circleKring, circleKplace);
                    gridKx = gridKs[0];
                    gridKz = gridKs[1];
                    break;
            }
            //Set isMainDiagonalProperty
            if (firstK == 0 && secondK == 0)
            {
                isMainDiagonal = true;
            }
            else
            {
                if (HexHelper.Modulo(circleKplace, circleKring) == 0)
                {
                    isMainDiagonal = true;
                }
                else
                {
                    isMainDiagonal = false;
                }
            }
            if (grid == null)
            {
                return;
            }
            //propagate growth corridor of region outwards if necessary
            Cell innerLeft = CellGetCircleInnerLeftWithoutCreate();
            if (innerLeft != null)
            {
                blockedRegion = innerLeft.GetBlockedForRegion();
            }
            else
            {
                blockedRegion = null;
            }
        }

        #region Mesh_Generation_Support_Functions
        public Vector3[] GetVerticeList(int heightDif)
        {
            return HexHelper.CreateHexagonVertices(gridKx, gridKz, Constants.standardHeight + Constants.heightFactor * heightDif);
        }
        public float GetHeight(int heightDif)
        {
            return Constants.standardHeight + Constants.heightFactor * heightDif;
        }
        public int[] GetTrianglesList(int nrOfHexInGrid)
        {
            return HexHelper.CreateHexagonTriangles(nrOfHexInGrid);
        }
        public Vector3[] GetNormals()
        {
            return HexHelper.CreateHexagonNormals();
        }
        #endregion

        public int ComparatorCirclePos(Cell other)
        {
            if (this.circleKring > other.circleKring)
            {
                return 1;
            }
            else if (this.circleKring < other.circleKring)
            {
                return -1;
            }
            else
            {
                return this.circleKplace.CompareTo(other.circleKplace);
            }
        }

        // --- Getter & Setter

        public int[] GetGridCoordinates()
        {
            return new int[] { gridKx, gridKz };
        }
        public int[] GetCircleCoordinates()
        {
            return new int[] { circleKring, circleKplace };
        }

        public float[] GetAbsoluteCoordinates()
        {
            return HexHelper.GetCenterCoordinates(gridKx, gridKz);
        }
        public bool IsMainDiagonal()
        {
            return isMainDiagonal;
        }


        #region Neigbourhood_Functions
        public List<Cell> GetNeighboursForCoastLine()
        {
            return new List<Cell>(new Cell[]{
            GetGridUpperNeighbour(),
            GetGridUpperLeftNeighbour(),
            GetGridLowerLeftNeighbour(),
            GetGridLowerNeighbour(),
            GetGridLowerRightNeighbour(),
            GetGridUpperRightNeighbour(),
        });
        }
        public List<Cell> GetNeighboursClockwiseByGrid()
        {
            return new List<Cell>(new Cell[]{
            GetGridUpperNeighbour(),
            GetGridUpperRightNeighbour(),
            GetGridUpperLeftNeighbour(),
            GetGridLowerNeighbour(),
            GetGridLowerLeftNeighbour(),
            GetGridLowerRightNeighbour()
        });
        }
        public List<Cell> GetNeighboursClockwiseByCircle()
        {
            if (circleKring == 0 && circleKplace == 0)
            {
                return new List<Cell>(new Cell[]{
            GetGridUpperNeighbour(),
            GetGridUpperRightNeighbour(),
            GetGridLowerRightNeighbour(),
            GetGridLowerNeighbour(),
            GetGridLowerLeftNeighbour(),
            GetGridUpperLeftNeighbour()
            });
            }
            if (isMainDiagonal)
            {
                return new List<Cell>(new Cell[]{
            GetCircleLeft(),
            GetCircleOuterLeft(),
            GetCircleOuter(),
            GetCircleOuterRight(),
            GetCircleRight(),
            GetCircleInner()
            });
            }
            else
            {
                return new List<Cell>(new Cell[]{
            GetCircleLeft(),
            GetCircleOuterLeft(),
            GetCircleOuterRight(),
            GetCircleRight(),
            GetCircleInnerRight(),
            GetCircleInnerLeft(),
            });
            }
        }
        /// <summary>
        /// Returns NeighboursClockwiseByGrid that are not assigned to a region
        /// </summary>
        /// <returns></returns>
        public List<Cell> GetFreeNeighbours()
        {
            List<Cell> result = new List<Cell>();
            foreach (Cell n in GetNeighboursClockwiseByGrid())
            {
                if (!n.IsAssignedToRegion())
                {
                    result.Add(n);
                }
            }
            return result;
        }
        public Cell GetGridUpperNeighbour()
        {
            return grid.GetCellByGridCreateRing(gridKx, gridKz + 1);
        }
        public Cell GetGridUpperRightNeighbour()
        {
            if (gridKx % 2 == 0)
            {
                return grid.GetCellByGridCreateRing(gridKx + 1, gridKz);
            }
            else
            {
                return grid.GetCellByGridCreateRing(gridKx + 1, gridKz + 1);
            }
        }
        public Cell GetGridUpperLeftNeighbour()
        {
            if (gridKx % 2 == 0)
            {
                return grid.GetCellByGridCreateRing(gridKx - 1, gridKz);
            }
            else
            {
                return grid.GetCellByGridCreateRing(gridKx - 1, gridKz + 1);
            }
        }
        public Cell GetGridLowerNeighbour()
        {
            return grid.GetCellByGridCreateRing(gridKx, gridKz - 1);
        }
        public Cell GetGridLowerRightNeighbour()
        {
            if (gridKx % 2 == 0)
            {
                return grid.GetCellByGridCreateRing(gridKx + 1, gridKz - 1);
            }
            else
            {
                return grid.GetCellByGridCreateRing(gridKx + 1, gridKz);
            }
        }
        public Cell GetGridLowerLeftNeighbour()
        {
            if (gridKx % 2 == 0)
            {
                return grid.GetCellByGridCreateRing(gridKx - 1, gridKz - 1);
            }
            else
            {
                return grid.GetCellByGridCreateRing(gridKx - 1, gridKz);
            }
        }
        public Cell GetCircleRight()
        {
            return grid.GetCellByCircleCreateRing(circleKring, HexHelper.Modulo((circleKplace + 1), (circleKring * 6)));
        }
        public Cell GetCircleLeft()
        {
            return grid.GetCellByCircleCreateRing(circleKring, HexHelper.Modulo((circleKplace - 1), (circleKring * 6)));
        }
        public Cell GetCircleOuter()
        {
            if (isMainDiagonal)
            {
                return grid.GetCellByCircleCreateRing(circleKring + 1, circleKplace / circleKring * (circleKring + 1));
            }
            return null;
        }
        public Cell GetCircleOuterRight()
        {
            if (isMainDiagonal)
            {
                return grid.GetCellByCircleCreateRing(circleKring + 1, HexHelper.Modulo((circleKplace / circleKring * (circleKring + 1) + 1), ((circleKring + 1) * 6)));
            }
            else
            {
                int[] sixthInfo = HexHelper.GetSixthInRingAndOffset(circleKring, circleKplace);
                return grid.GetCellByCircleCreateRing(circleKring + 1, HexHelper.Modulo((circleKplace + sixthInfo[0] + 1), ((circleKring + 1) * 6)));
            }
        }
        //voarallem zur Festlegung der Regionen Hauptwachstumsrichtung gedacht
        public Cell GetCircleOuterRightWithoutCreate()
        {
            if (gridKx == 0 && gridKz == 0)
            {
                return grid.GetCellByGrid(0, 1);
            }
            if (isMainDiagonal)
            {
                return grid.GetCellByCircle(circleKring + 1, HexHelper.Modulo((circleKplace / circleKring * (circleKring + 1) + 1), ((circleKring + 1) * 6)));
            }
            else
            {
                int[] sixthInfo = HexHelper.GetSixthInRingAndOffset(circleKring, circleKplace);
                return grid.GetCellByCircle(circleKring + 1, HexHelper.Modulo((circleKplace + sixthInfo[0] + 1), ((circleKring + 1) * 6)));
            }
        }
        public Cell GetCircleOuterLeft()
        {
            if (isMainDiagonal)
            {
                return grid.GetCellByCircleCreateRing(circleKring + 1, HexHelper.Modulo((circleKplace / circleKring * (circleKring + 1) - 1), ((circleKring + 1) * 6)));
            }
            else
            {
                int[] sixthInfo = HexHelper.GetSixthInRingAndOffset(circleKring, circleKplace);
                return grid.GetCellByCircleCreateRing(circleKring + 1, HexHelper.Modulo((circleKplace + sixthInfo[0]), ((circleKring + 1) * 6)));
            }
        }
        public Cell GetCircleInner()
        {
            if (isMainDiagonal)
            {
                return grid.GetCellByCircleCreateRing(circleKring + 1, circleKplace / circleKring * (circleKring - 1));
            }
            return null;
        }
        public Cell GetCircleInnerRight()
        {
            if (!isMainDiagonal)
            {
                int[] sixthInfo = HexHelper.GetSixthInRingAndOffset(circleKring, circleKplace);
                return grid.GetCellByCircleCreateRing(circleKring - 1, HexHelper.Modulo((circleKplace - sixthInfo[0]), ((circleKring - 1) * 6)));
            }
            return null;
        }
        public Cell GetCircleInnerLeft()
        {
            if (!isMainDiagonal)
            {
                int[] sixthInfo = HexHelper.GetSixthInRingAndOffset(circleKring, circleKplace);
                return grid.GetCellByCircleCreateRing(circleKring - 1, HexHelper.Modulo((circleKplace - sixthInfo[0] - 1), ((circleKring - 1) * 6)));
            }
            return null;

        }
        //vorallem zur Festlegung der Regionen Hauptwachstumsrichtung gedacht
        public Cell CellGetCircleInnerLeftWithoutCreate()
        {
            if (!isMainDiagonal)
            {
                int[] sixthInfo = HexHelper.GetSixthInRingAndOffset(circleKring, circleKplace);
                return grid.GetCellByCircle(circleKring - 1, HexHelper.Modulo((circleKplace - sixthInfo[0] - 1), ((circleKring - 1) * 6)));
            }
            return null;
        }
        public bool IsNeighbourOf(Cell other)
        {
            int xDif = Mathf.Abs(this.gridKx - other.gridKx);
            if (xDif >= 2)
            {
                return false;
            }
            else if (xDif == 1)
            {
                if (this.gridKx % 2 == 0)
                {
                    //Case 1 this in even col -> other has to be in same or -1 z-Direction
                    if (this.gridKz == other.gridKz || this.gridKz - 1 == other.gridKz)
                    {
                        return true;
                    }
                    else return false;
                }
                else
                {
                    //Case 2 this in odd col -> other has to be in same or +1 z-Direction
                    if (this.gridKz == other.gridKz || this.gridKz + 1 == other.gridKz)
                    {
                        return true;
                    }
                    else return false;
                }

            }
            else //xDif == 0
            {
                if (Mathf.Abs(this.gridKz - other.gridKz) == 1)
                {
                    return true;
                }
                else return false;
            }
        }
        #endregion

        public void SetAssignedRegion(Region ar, int time)
        {
            if (assignedRegion != null)
            {
                Debug.LogError("Cell already assinged");
            }
            timeTag = time;
            assignedRegion = ar;

            int[] sixthInof = HexHelper.GetSixthInRingAndOffset(circleKring, circleKplace);
            if (sixthInof[0] == 0 || sixthInof[0] == 1 || (sixthInof[0] == 2 && sixthInof[1] == 0))
            {
                grid.CellAssigned(this);
            }


        }
        public int GetTimeTag()
        {
            return timeTag;
        }

        public bool SetBlockedRegion(Region br)
        {
            if (blockedRegion != null)
            {
                Debug.LogError("Cell alreadyBlocked");
                return false;
            }
            blockedRegion = br;
            return true;
        }

        public void UnblockRegion()
        {
            blockedRegion = null;
        }

        public bool IsAssignedToRegion()
        {
            if (assignedRegion != null)
            {
                return true;
            }
            return false;
        }

        public bool IsBlockedForRegion()
        {
            if (blockedRegion != null)
            {
                return true;
            }
            else return false;
        }

        public void SetCompUnitMaster(CompUnitMaster cum)
        {
            if (compUnit != null)
            {
                Debug.LogError("Cell already assinged");
            }
            compUnit = cum;
        }

        public CompUnitMaster GetCompUnit()
        {
            return compUnit;
        }

        public Region GetBlockedForRegion()
        {
            return blockedRegion;
        }


        public float GetScore(Region region)
        {
            if (assignedRegion != null)
            {
                return 0.0f;
            }
            if (blockedRegion != null && blockedRegion != region)
            {
                return 0.0f;
            }
            float factor = 1f;
            if (blockedRegion != null && blockedRegion == region)
            {
                factor = 1.5f;
            }
            int sameRegionNeigbours = 0;
            int otherRegionNeigbours = 0;
            foreach (Cell cell in GetNeighboursClockwiseByGrid())
            {
                if (cell.assignedRegion != null)
                {
                    if (cell.assignedRegion == region)
                    {
                        sameRegionNeigbours++;
                    }
                    else
                    {
                        otherRegionNeigbours++;
                    }
                }
            }
            float score = Mathf.Pow(b1, sameRegionNeigbours) * Mathf.Pow(b2, otherRegionNeigbours);
            return factor * score;
        }

        /// <summary>
        /// 
        /// Zur Wahl einer Startzelle für neue Region
        /// Je weniger belegte Nachbarn, desto höher der Score, maximale anzahl belegter Nachbarn = 6
        /// </summary>
        /// <returns></returns>
        public float GetReverseScore()
        {
            if (IsBlockedForRegion() || IsAssignedToRegion())
            {
                return 0.0f;
            }
            int count = 0;
            foreach (Cell neighbour in GetNeighboursClockwiseByGrid())
            {
                if (neighbour.IsBlockedForRegion())
                {
                    //maynot return 0 if no other possibility
                    return 0.1f;
                }
                if (neighbour.IsAssignedToRegion())
                {
                    count++;
                }
            }

            /*//Version 1 Faktor vielstaaten-Grenzgebiete
            List<RegionController> neighbourRegionList = GetNeighbouringRegions();
            float factor = neighbourRegionList.Count;*/

            //Version 2 Faktor größer je weiter von blocked Region entfernt

            int stepsRight = 0;
            int stepsLeft = 0;

            Cell leftRecNeighbour = this.GetCircleLeft();
            while (!leftRecNeighbour.IsBlockedForRegion()&&stepsLeft < this.circleKring*6)
            {
                stepsLeft++;
                leftRecNeighbour = leftRecNeighbour.GetCircleLeft();
            }
            Cell rightRecNeighbour = this.GetCircleRight();
            while (!rightRecNeighbour.IsBlockedForRegion()&&stepsRight < this.circleKring*6)
            {
                stepsRight++;
                rightRecNeighbour = rightRecNeighbour.GetCircleRight();
            }

            float factor = stepsLeft * stepsRight;

            //float factor = 1f;

            return factor * (Mathf.Pow(b1, 6) - Mathf.Pow(b1, count));
        }


        public List<Region> GetNeighbouringRegions()
        {
            List<Region> resultList = new List<Region>();
            foreach (Cell neighbour in GetNeighboursClockwiseByGrid())
            {
                if (neighbour.assignedRegion != null && !resultList.Contains(neighbour.assignedRegion))
                {
                    resultList.Add(neighbour.assignedRegion);
                }
            }
            return resultList;
        }
    }
}
