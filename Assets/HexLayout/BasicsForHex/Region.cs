using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.ArchitectureElements;

namespace HexLayout.Basics
{

    public class Region
    {
        PackageMaster package;
        List<Cell> assignedCells;


        public Region(PackageMaster pack)
        {
            package = pack;
            assignedCells = new List<Cell>();
        }

        public List<Cell> GetAssignedCells()
        {
            return assignedCells;
        }
        public void AddAssignedCell(Cell newCell)
        {
            if (!assignedCells.Contains(newCell))
            {
                assignedCells.Add(newCell);
                nrOfCells++;
            }
            else
            {
                // Debug.LogWarning(gameObject.name + " trying to add Cell to assinged Cells that is already present");
            }
        }

        public void AddAssignedCells(List<Cell> newAssignedCellList)
        {
            foreach (Cell newCell in newAssignedCellList)
            {
                if (!assignedCells.Contains(newCell))
                {
                    assignedCells.Add(newCell);
                    nrOfCells++;
                }
                else
                {
                    // Debug.LogWarning(gameObject.name + " trying to add Cell to assinged Cells that is already present");
                }
            }
        }




        public int nrOfCells;

        List<Cell> borderCells;

        GameObject goRegion;
        GameObject goBorder;
        GameObject goGrowDir;

        private static Color[] colors = new Color[] { Color.black, Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.yellow };

        private static Vector3[] colVals = new Vector3[]
        {
        new Vector3(255,255,102), //gelb
        new Vector3(255,153,0), //Mausorange
        new Vector3(255,124,128), //Lachsfarben
        new Vector3(255,0,0), // rot
        //new Vector3(153,0,0), // dunkelrot
        new Vector3(255,153,255), //hell lila
        new Vector3(255,51,153), // pink
        new Vector3(204,0,204), // violet
        new Vector3(153,102,255), // veilchen lila
        new Vector3(0,0,255), // blau
        new Vector3(102,153,255), // himmelblau
        //new Vector3(0,0,153), // dunkelblau
        new Vector3(0,255,255), //cyan
        new Vector3(0,255,153), //hellgrün
        new Vector3(153,255,153), //gelbgrün
        new Vector3(0,255,0), //grün
                              //new Vector3(0,128,0), //dunkelgrün
                              //new Vector3(153,102,51), //braun
                              //new Vector3(150,150,150), // grau
                              //new Vector3(0,0,0), // schwarz
        };

        // Use this for initialization


        public void UpdateDisplayRegion(int time)
        {
            goRegion.GetComponent<GORegionNet>().RenewMesh(assignedCells, time);
        }

        public void UpdateDisplayBorder()
        {
            goBorder.GetComponent<GOBorderNet>().RenewMesh(borderCells);
        }

        public int RequestGrowingArea()
        {
            return UnityEngine.Random.Range(0, 5);
        }



        public List<Cell> GetBorderCells()
        {
            return borderCells;
        }

        public void SetBorderCells(List<Cell> currentBorderCells)
        {
            borderCells = currentBorderCells;
        }

        public void SetGrowRegion(List<Cell> GrowRegion)
        {
            goGrowDir.GetComponent<GOBorderNet>().RenewMesh(GrowRegion);
        }

        public PackageMaster GetPackageMaster()
        {
            return package;
        }
    }
}