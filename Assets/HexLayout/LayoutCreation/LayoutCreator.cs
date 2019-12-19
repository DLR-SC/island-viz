using OSGI_Datatypes.ArchitectureElements;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ComposedTypes;
using HexLayout.Basics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutCreator
{
    private static int openedRegions = 0;
    private static int setCells = 0;

    private BundleMaster bundle;
    private HexGrid grid;
    private bool useAvailableLayoutInfo;
    private bool writeLayoutToDB;

    private List<Commit> commitsList;
    private Dictionary<Commit, List<PackageMaster>> newPackages;
    private Dictionary<Commit, List<PackageMaster>> deletedPackages;
    private Dictionary<Commit, List<CompUnitMaster>> newCompUnits;
    private Dictionary<Region, List<Cell>> packagesBorderCells;

    private List<Dictionary<string, object>> newCellPos;
    private List<Dictionary<string, object>> newStartCellPos;

    /// <param name="bundle"></param>
    /// <param name="useAvailableLayoutInfo"></param>
    /// <param name="writeLayoutToDB"></param>
    public LayoutCreator(BundleMaster b, bool useAvailLayoutInfo, bool writeToDB)
    {
        bundle = b;
        grid = bundle.GetGrid();
        useAvailableLayoutInfo = useAvailLayoutInfo;
        writeLayoutToDB = writeToDB;
    }
    /// <summary>
    /// LayoutAlgo from LayoutController of conception project "dynamic-hex-layout"
    /// </summary>
    public IEnumerator Create()
    {
        yield return Initialise();
        
        //Iterate over all commits of bundle in timely order (commitsList is ordered by time)
        foreach (Commit commit in commitsList)
        {
            //Add new CompUnits of already exiting packages
            List<CompUnitMaster> newCompUnitsThisCommit = null;
            newCompUnits.TryGetValue(commit, out newCompUnitsThisCommit);
            if (newCompUnitsThisCommit != null && newCompUnitsThisCommit.Count != 0)
            {
                int i = 0;
                foreach (CompUnitMaster cum in newCompUnitsThisCommit)
                {
                    i++;
                    Cell nextCell = AddNextCellToRegion(cum);
                    if (nextCell == null)
                    {
                        Debug.LogWarning("Trying to add CompUnit to grid but no cell available");
                    }

                    if (i % 10 == 0)
                    {
                        yield return null;
                    }
                }
                newCompUnitsThisCommit = null;
                newCompUnits[commit] = null;
            }

            //Add new Packages
            List<PackageMaster> newPackagesThisCommit = null;
            newPackages.TryGetValue(commit, out newPackagesThisCommit);
            if (newPackagesThisCommit != null && newPackagesThisCommit.Count != 0)
            {
                foreach (PackageMaster pack in newPackagesThisCommit)
                {
                    yield return NewRegion(pack);
                }
                newPackagesThisCommit = null;
                newPackages[commit] = null;
            }

            //Free Grow Corridor of Deleted packages;
            List<PackageMaster> deletedPackagesThisCommit = null;
            deletedPackages.TryGetValue(commit, out deletedPackagesThisCommit);
            if (deletedPackagesThisCommit != null && deletedPackagesThisCommit.Count != 0)
            {
                foreach (PackageMaster pack in deletedPackagesThisCommit)
                {
                    FreeBlockedCells(pack);
                    packagesBorderCells.Remove(pack.GetRegion());
                }
                deletedPackagesThisCommit = null;
                deletedPackages[commit] = null;
            }

            //Store currentMaxRadius of Grid
            grid.SaveOuterAssigned(commit);
            yield return null;
        }
        if (writeLayoutToDB)
        {
            if (newCellPos.Count != 0)
            {
                yield return Neo4JWriterLayout.WriteCellPositions(newCellPos);
            }
            if(newStartCellPos.Count != 0)
            {
                yield return Neo4JWriterLayout.WriteStartCellPositions(newStartCellPos);
            }
        }
        bundle.SetRadiusToElements();

    }

    private IEnumerator Initialise()
    {
        //Get Preprocessed Datastructures for Layout Routine
        commitsList = bundle.GetCommitsByTimeline(SortTypes.byBranch);
        yield return null;
        newPackages = bundle.GetDictOfNewPackagesByCommit();
        yield return null;
        deletedPackages = bundle.GetDictOfDeletedPackagesByCommit();
        yield return null;
        newCompUnits = bundle.GetDictOfNewCompUnitsByCommit();
        yield return null;

        //Create Temporary DataStructures
        packagesBorderCells = new Dictionary<Region, List<Cell>>();
        newCellPos = new List<Dictionary<string, object>>();
        newStartCellPos = new List<Dictionary<string, object>>();

    }


    private Cell AddNextCellToRegion(CompUnitMaster cum)
    {
        setCells++;
        //Debug.Log("SetCells " + setCells);
        Region region = cum.GetParent().GetRegion();

        Cell nextCell = GetNextCell(cum);

        if (nextCell == null)
        {
            return null;
        }
        nextCell.SetAssignedRegion(region, 0);
        nextCell.SetCompUnitMaster(cum);
        cum.SetCell(nextCell);
        region.AddAssignedCell(nextCell);

        //Remove Cell from other Borders
        RemoveCellFromBorderLists(nextCell, region);

        //Update Regions Border List
        AddCellsToBorder(nextCell.GetFreeNeighbours(), region);

        packagesBorderCells[region].Remove(nextCell);

        return nextCell;
    }

    private IEnumerator NewRegion(PackageMaster pm)
    {
        //openedRegions++;
        //Debug.Log("Opended new Region " + openedRegions);
        //Create new Region
        Region newRegion = new Region(pm);
        pm.SetRegion(newRegion);

        //Fill all rings for successfull calculation of valid grow corridor 
        grid.FillRingToOuterAssigned();

        //GetStartCell from Data or random and associated grow corridor
        List<Cell> growCorridor = GetStartCell(pm);
        Cell startCell = growCorridor[0];
        //Block grow corridor
        for(int i = 1; i<growCorridor.Count; i++)
        {
            growCorridor[i].SetBlockedRegion(newRegion);
        }
        packagesBorderCells.Add(newRegion, new List<Cell> { startCell });
        //Add all initial cells
        List<CompUnitMaster> initialCells = pm.GetInitialCompUnits();
        foreach(CompUnitMaster compUnitMaster in initialCells)
        {
            AddNextCellToRegion(compUnitMaster);
        }
        yield return null;
    }

    private Cell GetNextCell(CompUnitMaster cum)
    {
        if (useAvailableLayoutInfo && cum.HasValidGridInfo())
        {
            Vector2Int gridPos = cum.GetGridPos();
            Cell cell = grid.GetCellByGridCreateCell(gridPos.x, gridPos.y);
            return cell;
        }
        else
        {
            Cell cell = GetNextCellFromBorder(cum.GetParent().GetRegion());
            if (cell == null)
            {
                return null;
            }
            int[] cellPos = cell.GetGridCoordinates();
            //WriteCellPos to MasterElement
            cum.SetCell(cell);
            cum.SetGridInfo(cellPos[0], cellPos[1]);
            //Prepare To Write In DB:
            int firstElementId = cum.GetStartElement(SortTypes.byBranch).GetNeoId();
            newCellPos.Add(new Dictionary<string, object> { { "id", firstElementId }, { "posX", cellPos[0] }, { "posZ", cellPos[1] } });
            return cell;
        }
    }

    private void AddCellsToBorder(List<Cell> cellList, Region region)
    {
        foreach (Cell c in cellList)
        {
            if (!packagesBorderCells[region].Contains(c))
            {
                packagesBorderCells[region].Add(c);
            }
        }
    }

    private void RemoveCellFromBorderLists(Cell cell, Region rightRegion)
    {
        foreach (KeyValuePair<Region, List<Cell>> kvpRegions in packagesBorderCells)
        {
            if (kvpRegions.Key != rightRegion)
            {
                kvpRegions.Value.Remove(cell);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pm"></param>
    /// <returns>GrowCorridor for new REgion, where List[0] is startCell</returns>
    private List<Cell> GetStartCell(PackageMaster pm)
    {
        Cell startCell = null;
        if (useAvailableLayoutInfo && pm.HasValidGrowCorridorStart())
        {
            Vector2Int growPos = pm.GetGrowCorridorStart();
            startCell = grid.GetCellByGridCreateCell(growPos.x, growPos.y);
        }
        else
        {
            startCell = GetNewRegionStartCell();
            if (startCell == null)
            {
                Debug.LogError("No Start Cell available");
            }
        }
        List<Cell> growCorridor = GetGrowDirection(startCell);
        while(growCorridor == null)
        {
            Debug.LogWarning("New Try to find StartCell");
            startCell = GetNewRegionStartCell();
            growCorridor = GetGrowDirection(startCell);
        }
        growCorridor.Insert(0, startCell);
        //Set To masterElement
        int[] startCellPos = startCell.GetGridCoordinates();
        pm.SetGrowCorridorStart(startCellPos[0], startCellPos[1]);
        pm.SetStartCell(startCell);
        //PrepareToWriteToDatabase
        int firstElementId = pm.GetStartElement(SortTypes.byBranch).GetNeoId();
        newStartCellPos.Add(new Dictionary<string, object> { {"id", firstElementId },{"startCellX", startCellPos[0] },{"startCellZ", startCellPos[1]} });
        return growCorridor;
    }

    private Cell GetNewRegionStartCell()
    {
        //Anlegen erst Region
        if (packagesBorderCells.Count == 0)
        {
            return grid.GetCellByCircleCreateRing(0, 0);
        }

        Dictionary<Cell, float> borderCells = new Dictionary<Cell, float>();

        float scoreCounter = 0f;
        foreach (KeyValuePair<Region, List<Cell>> kvp in packagesBorderCells)
        {
            if (kvp.Value != null)
            {
                foreach (Cell bordercell in kvp.Value)
                {

                    if (!borderCells.ContainsKey(bordercell))
                    {
                        float score = bordercell.GetReverseScore();
                        if (score != 0f)
                        {
                            scoreCounter += score;
                            borderCells.Add(bordercell, score);
                        }
                    }
                }
            }
        }
        if (borderCells.Count == 0)
        {
            return null;
        }

        float random = UnityEngine.Random.Range(0.0f, scoreCounter);

        scoreCounter = 0;

        foreach (KeyValuePair<Cell, float> bordercell in borderCells)
        {
            scoreCounter += bordercell.Value;
            if (random <= scoreCounter)
            {
                return bordercell.Key;
            }
        }

        Debug.LogError("No Start Cell for new Region awailable");
        return null;
    }


    private Cell GetNextCellFromBorder(Region region)
    {
        if (packagesBorderCells[region].Count == 0)
        {
            Debug.LogError("No Cells in Border of a region"/* + region.gameObject.name*/);
            return null;
        }

        Dictionary<Cell, float> cellScores = new Dictionary<Cell, float>();
        float ScoreCounter = 0f;
        foreach (Cell cell in packagesBorderCells[region])
        {
            float score = cell.GetScore(region);
            if (score != 0f)
            {
                ScoreCounter += score;
                cellScores.Add(cell, score);
            }
        }

        float random = UnityEngine.Random.Range(0f, ScoreCounter);
        float tempCounter = 0;
        foreach (KeyValuePair<Cell, float> cellkvp in cellScores)
        {
            tempCounter += cellkvp.Value;
            if (random <= tempCounter)
            {
                return cellkvp.Key;
            }
        }
        Debug.LogError("No Cells choosen for next Cell of a region"/* + region.gameObject.name*/);
        return null;
    }

    public List<Cell> GetGrowDirection(Cell startCell)
    {
        List<Cell> corridor = new List<Cell>();

        Cell rightOuter = startCell.GetCircleOuterRightWithoutCreate();
        while (rightOuter != null)
        {
            if (rightOuter.IsAssignedToRegion() || rightOuter.IsBlockedForRegion())
            {
                return null;
            }
            corridor.Add(rightOuter);
            rightOuter = rightOuter.GetCircleOuterRightWithoutCreate();
        }
        return corridor;
    }

    public List<Cell> GetGrowDirectionToDelete(Cell startCell)
    {
        List<Cell> corridor = new List<Cell>();

        Cell rightOuter = startCell.GetCircleOuterRightWithoutCreate();
        while (rightOuter != null)
        {
            corridor.Add(rightOuter);
            rightOuter = rightOuter.GetCircleOuterRightWithoutCreate();
        }
        return corridor;
    }


    private void FreeBlockedCells(PackageMaster pack)
    {
        Cell startCell = pack.GetStartCell();
        List<Cell> growCorridor = GetGrowDirectionToDelete(startCell);
        growCorridor.Insert(0, startCell);

        foreach(Cell cell in growCorridor)
        {
            cell.UnblockRegion();
        }
    }
}
