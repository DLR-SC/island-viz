using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexLayout.Basics;

public class LayoutController : MonoBehaviour {

    public int nrOfRegions;
    public int totalCells;
    public bool displayHight;
    

    HexGrid grid;

    int state;
    int nrOfRegionsToCreate;
    int time;

    private bool oldDisplayHight;

    List<Region> regions;
    Dictionary<Region, List<Cell>> borderCellsOfRegions;
    GameObject coastLine;


    // Use this for initialization
    void Awake () {
        //Gitter
        grid = new HexGrid(4);

        //Region Child adminitration
        regions = new List<Region>();
        borderCellsOfRegions = new Dictionary<Region, List<Cell>>();

        //CoastLine
        coastLine = transform.Find("CoastLine").gameObject;
        coastLine.GetComponent<GOCoastLine>().Init(grid);


        state = 0;
        nrOfRegionsToCreate = -1;

        nrOfRegions = 0;
        totalCells = 0;
        time = 0;
        oldDisplayHight = displayHight;

    }
	
	// Update is called once per frame
	void Update () {
        if(state==0)
        {
            if (Input.GetKeyDown("right")) //Wachstum vorhandener Regionen
            {
                if (oldDisplayHight == displayHight)
                {
                    state = 1;
                    time++;
                    StartCoroutine(LayoutStateMachine());
                }
                else
                {
                    state = 1;
                    oldDisplayHight = displayHight;
                    int timeSign;
                    if (displayHight)
                    {
                        timeSign = time;
                    }
                    else
                    {
                        timeSign = 0;
                    }
                    StartCoroutine(AdjustDisplayToHightReq(timeSign));
                }
            }
        }
        else if(state == 2) //Anzahl neuer Regionen
        {
            nrOfRegionsToCreate = UnityEngine.Random.Range(0, 3);
            state = 3;
        }
        else if(state == 3 && nrOfRegionsToCreate > 0) //Anlegen neuer Regionen
        {
            //Add one new Region per Update-Call
            //Create New Region GameObject
            GameObject go = new GameObject("Region" + regions.Count);
            go.transform.parent = this.gameObject.transform;
            go.transform.localPosition = Vector3.zero;
            Region goC = null;// = go.AddComponent<Region>();
            regions.Add(goC);
            nrOfRegions++;

            //Find Suitable StartCell
            Cell startCell = GetNewRegionStartCell();
            List<Cell> corridor = GetGrowDirection(startCell);

            while(corridor == null)
            {
                //Repeat until suitable start Cell is found
                Debug.LogWarning("Creating " + go.name + " - repeat find StartCell");
                startCell = GetNewRegionStartCell();
                corridor = GetGrowDirection(startCell);
            }
            if(corridor.Count == 0)
            {
                Cell startOfCorridor = startCell.GetCircleOuterRight();
                corridor.Add(startOfCorridor);
            }

            //Assign CorridorCells
            foreach(Cell cCell in corridor)
            {
                cCell.SetBlockedRegion(goC);
            }

            //Assign StartCell To Region
            startCell.SetAssignedRegion(goC, time);
            RemoveCellFromBorderLists(startCell, goC);
            
            //Add Border Of new Region
            borderCellsOfRegions.Add(goC, startCell.GetFreeNeighbours());

            //Add additional Cells to new Region
            List<Cell> assignedCells = new List<Cell> { startCell };
            totalCells++;
            int nrOfAdditionalCells= goC.RequestGrowingArea();
            //Debug.Log(goC.gameObject.name + " created with " + (nrOfAdditionalCells + 1) + " additionalCells");

            for (int i = 0; i < nrOfAdditionalCells; i++)
            {
                Cell cell = AddNextCellToRegion(goC);
                if (cell == null)
                {
                    Debug.LogWarning("Adding Cell to " + go.name + " was unsuccessful");
                    continue;
                }
                assignedCells.Add(cell);
                totalCells++;
            }


            goC.AddAssignedCells(assignedCells);
            //TODO wenn höhe abhängig von Zeit gewünscht
            if (displayHight)
            {
                goC.UpdateDisplayRegion(time);
                coastLine.GetComponent<GOCoastLine>().UpdateCoastLine(time);
            }
            else
            {
                goC.UpdateDisplayRegion(0);
                coastLine.GetComponent<GOCoastLine>().UpdateCoastLine(0);
            }

            goC.SetGrowRegion(corridor);

            nrOfRegionsToCreate--;
            return;
        }
        else if(state == 3 && nrOfRegionsToCreate == 0)
        {
            nrOfRegionsToCreate = -1;
            state = 4;
        }
        //if(working && finishedGrowing && finishedNewRegions)
        else if(state == 4)
        {
            StartCoroutine(BorderDisplay());
            
        }
        else if(state == 5)
        {
            int outerRing = grid.GetOuterAssignedRing();
            if(outerRing < 2)
            {
                outerRing = 2;
            }
            //transform.Find("Docks").GetComponent<DockPlacer>().SetDockPositions(2*Constants.hexCellRadius * (outerRing));
            state = 0;
        }

		
	}

    public IEnumerator LayoutStateMachine()
    {

        foreach(Region region in regions)
        {
            List<Cell> newCells = new List<Cell>();
            int nrOfNewCells = region.RequestGrowingArea();
            //Debug.Log(region.gameObject.name + " requests " + nrOfNewCells + " additionalCells");

            for(int i = 0; i<nrOfNewCells; i++)
            {
                Cell cell = AddNextCellToRegion(region);
                if (cell == null)
                {
                    yield return null;
                    continue;
                }
                newCells.Add(cell);
                totalCells++;
                yield return null;
            }

            region.AddAssignedCells(newCells);
            //TODO wenn zeitabhängige ZellHöhe
            if (displayHight)
            {
                region.UpdateDisplayRegion(time);
                coastLine.GetComponent<GOCoastLine>().UpdateCoastLine(time);
            }
            else
            {
                region.UpdateDisplayRegion(0);
                coastLine.GetComponent<GOCoastLine>().UpdateCoastLine(0);
            }
            yield return null;

            //ProcessBorder(region);
        }

        //Letze Anweisung Coroutine, damit Anlegen neuer Regionen anspringen kann.
        state = 2;
    }

    public IEnumerator AdjustDisplayToHightReq(int argument)
    {
        foreach (Region region in regions)
        {
            region.UpdateDisplayRegion(argument);
            yield return null;
        }
        coastLine.GetComponent<GOCoastLine>().UpdateCoastLine(argument);
        state =  0;
    }

    public IEnumerator BorderDisplay()
    {
        foreach(KeyValuePair<Region, List<Cell>> kvp in borderCellsOfRegions)
        {
            kvp.Key.SetBorderCells(kvp.Value);
            kvp.Key.UpdateDisplayBorder();
            yield return null;
        }
        state = 5;
    }

    private Cell AddNextCellToRegion(Region region)
    {
        //Chose and Assign next Cell
        Cell nextCell = GetNextCellFromBorder(region);
        if (nextCell == null)
        {
            return null;
        }
        nextCell.SetAssignedRegion(region, time);

        //Remove Cell from other Borders
        RemoveCellFromBorderLists(nextCell, region);

        //Update Regions Border List
        AddCellsToBorder(nextCell.GetFreeNeighbours(), region);

        borderCellsOfRegions[region].Remove(nextCell);

        return nextCell;
    }

     private void AddCellsToBorder(List<Cell> cellList, Region region)
    {
        foreach(Cell c in cellList)
        {
            if (!borderCellsOfRegions[region].Contains(c))
            {
                borderCellsOfRegions[region].Add(c);
            }
        }
    }

    private void RemoveCellFromBorderLists(Cell cell, Region rightRegion)
    {
        foreach (KeyValuePair<Region, List<Cell>> kvpRegions in borderCellsOfRegions)
        {
            if (kvpRegions.Key != rightRegion)
            {
                kvpRegions.Value.Remove(cell);
            }
        }
    }

    private Cell GetNewRegionStartCell()
    {
        //Anlegen erst Region
        if (borderCellsOfRegions.Count == 0)
        {
            return grid.GetCellByCircleCreateRing(0, 0);
        }

        Dictionary<Cell, float> borderCells = new Dictionary<Cell, float>();

        float scoreCounter = 0f;
        foreach (KeyValuePair<Region, List<Cell>> kvp in borderCellsOfRegions)
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
        if (borderCellsOfRegions[region].Count == 0)
        {
            //Debug.LogError("No Cells in Border of " + region.gameObject.name);
            return null;
        }

        Dictionary<Cell, float> cellScores = new Dictionary<Cell, float>();
        float ScoreCounter = 0f;
        foreach (Cell cell in borderCellsOfRegions[region])
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
        return null;
    }


    public void ProcessBorder(Region region)
    {

        List<List<Cell>> borderSections = SortBorderSections(region, true);


        //NeueVersion: Unendlich langer Block-Koridor
        Cell corridorStart = null;
        while (borderSections.Count > 0)
        {
            List<Cell> currentSection = borderSections[0];
            if (currentSection[0].IsNeighbourOf(currentSection[currentSection.Count - 1]) && regions.Count > 1)
            {
                borderSections.RemoveAt(0);
                continue;
            }
            for(int i = currentSection.Count/2; i>=0; i--)
            {
                if (!currentSection[i].IsBlockedForRegion())
                {
                    corridorStart = currentSection[i];
                    break;
                }
                else if(!currentSection[currentSection.Count - i].IsBlockedForRegion())
                {
                    corridorStart = currentSection[currentSection.Count - i];
                    break;
                }
            }
            if (corridorStart != null)
            {
                break;
            }
            borderSections.RemoveAt(0);
        }
        if (corridorStart == null)
        {
            //Debug.LogError("Keine Zelle als Start für Block-Koridor vorhanden für " + region.gameObject.name);
        }
        List<Cell> corridor = new List<Cell>() { corridorStart };
        corridorStart.SetBlockedRegion(region);
        Cell rightOuter = corridorStart.GetCircleOuterRightWithoutCreate();
        while (rightOuter != null)
        {
            rightOuter.SetBlockedRegion(region);
            corridor.Add(rightOuter);
            rightOuter = rightOuter.GetCircleOuterRightWithoutCreate();
        }
        //region.GetComponent<Region>().SetGrowRegion(corridor);


        /* Alte Version //Update Border Dictionary
        List<Cell> borderCellsOfRegion = new List<Cell>();
        foreach(List<Cell> borderSection in borderSections)
        {
            borderCellsOfRegion.AddRange(borderSection);
        }
        borderCellsOfRegions[region] = borderCellsOfRegion;

        //Alte Version: Assign 3 Cells as blocked
        int nrOfCellsToBlock = 3;
        while (nrOfCellsToBlock > 0 && borderSections.Count > 0)
        {
            //Test if Section is a circle
            List<Cell> currentSection = borderSections[0];
            if (currentSection[0].IsNeighbourOf(currentSection[currentSection.Count - 1]) && regions.Count > 1)
            {
                borderSections.RemoveAt(0);
                continue;
            }
            if (currentSection.Count <= 3)
            {
                foreach(Cell cell in currentSection)
                {
                    if (cell.SetBlockedRegion(region))
                    {
                        nrOfCellsToBlock -= currentSection.Count;
                    }
                }
            }
            else
            {
                int end = currentSection.Count / 2 + 1;
                for (int i = 0; i<currentSection.Count; i++)
                {
                    if (i < currentSection.Count / 2 - 1 || i > end)
                    {
                        currentSection[i].Unblock(region);
                    }
                    else
                    {
                        if (currentSection[i].SetBlockedRegion(region))
                        {
                            nrOfCellsToBlock--;
                        }
                        else
                        {
                            end++;
                        }
                        
                    }
                }
            }
            borderSections.RemoveAt(0);
        }
        yield return null;

        //Unblock all remaining cells
        foreach(List<Cell> remainingSection in borderSections)
        {
            foreach(Cell cell in remainingSection)
            {
                cell.Unblock(region);
            }
        }
        yield return null;*/
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputList">List of BorderCells to be sorted</param>
    /// <returns>[0] ordered List of adjecent cells; [1] List of remaining cells (potentially different coast line)</returns>
    public List<List<Cell>> SortBorderCells(List<Cell> inputList)
    {
        if (inputList.Count == 0)
        {
            return null;
        }
        if(inputList.Count == 1)
        {
            return new List<List<Cell>> { inputList, null };
        }

        List<Cell> orderedResultList = new List<Cell>();
        orderedResultList.Add(inputList[0]);
        inputList.RemoveAt(0);

        bool finished = false;
        int index = 0;
        while (!finished && inputList.Count > 0)
        {
            Cell nextCell = null;
            foreach(Cell cell in inputList)
            {
                if (cell.IsNeighbourOf(orderedResultList[index])){
                    nextCell = cell;
                    break;
                }
            }
            if (nextCell == null)
            {
                //Found one End of border
                finished = true;
            }
            else
            {
                //Add nextCell to orderedBorder
                inputList.Remove(nextCell);
                orderedResultList.Add(nextCell);
                index++;
            }
        }
        if(inputList.Count > 0)
        {
            finished = false;
            while(!finished && inputList.Count > 0)
            {
                Cell nextCell = null;
                foreach (Cell cell in inputList)
                {
                    if (cell.IsNeighbourOf(orderedResultList[0])){
                        nextCell = cell;
                        break;
                    }
                }
                if (nextCell == null)
                {
                    //Found one End of border
                    finished = true;
                }
                else
                {
                    //Add nextCell to orderedBorder
                    inputList.Remove(nextCell);
                    orderedResultList.Insert(0, nextCell);
                }
            }
        }
        return new List<List<Cell>> { orderedResultList, inputList };
    }

    /// <summary>
    /// sorts cells of regions border to D
    /// if necessary distinguishes seperate coast lines
    /// sorts cells within coast line
    /// </summary>
    /// <param name="region"></param>
    /// <param name="applyToBorderDict">if true sorting result is applied to borderCellsOFRegions dictionary</param>
    /// <returns>list of coast lines ordered by length of coast line</returns>
    public List<List<Cell>> SortBorderSections(Region region, bool applyToBorderDict)
    {
        List<List<Cell>> borderSections = new List<List<Cell>>();

        //Sorting Border Cells along the border (if necessary section wise)
        List<List<Cell>> tempList;
        if (applyToBorderDict)
        {
            tempList = SortBorderCells(borderCellsOfRegions[region]);
        }
        else
        {
            tempList = SortBorderCells(new List<Cell>(borderCellsOfRegions[region]));
        }
        borderSections.Add(tempList[0]);

        while (tempList[1] != null && tempList[1].Count > 0)
        {
            tempList = SortBorderCells(tempList[1]);
            borderSections.Add(tempList[0]);
        }

        //sorting coast sections
        borderSections.Sort(delegate (List<Cell> x, List<Cell> y)
        {
            return x.Count.CompareTo(y.Count);
        });

        //write ordered list back to borderCellsOfRegions if necessary
        if (applyToBorderDict)
        {
            List<Cell> borderCellsOfRegion = new List<Cell>();
            foreach (List<Cell> borderSection in borderSections)
            {
                borderCellsOfRegion.AddRange(borderSection);
            }
            borderCellsOfRegions[region] = borderCellsOfRegion;
        }

        return borderSections;
    }
}



