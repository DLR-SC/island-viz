using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class Constants
    {
        //Constants about using Database Layout or new, write to DB
        public static bool useValuesFromDBWherePossible = false;
        public static bool writeNewValuesToDB = true;

        //Radius for IslandLayout Hexagon Basic Cell
        public static float hexCellRadius = 1.0f;
        //Factors for Island Coastline heights
        public static float standardHeight = 2f;
        public static float heightFactor = 0.5f;
        //Constant for Dock Position
        public static float dockYPos = 0.5f;
        //IslandHeight Time dependent currently default false, as true not implemented yet
        public static bool timeDepHight = false;

        //Colors for regions
        public static Vector3[] colVals = new Vector3[] {
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
        //Color for Coastline
        public static Vector3 coastlineColor = new Vector3(238, 197, 145);
        //Color for DeathArea
        public static Vector3 deathAreaColor = new Vector3(150, 150, 150);


        public enum LayoutOption
        {
            Master,
            HistoryForce
        };
        public static LayoutOption layoutAlgo = LayoutOption.HistoryForce;
        //Number of predecessors whose position is taken into account for history-force based Graph Layout Algo
        public static int historyForceLayoutDepth = 4;
        //Maximal Radius of complete archipelago
        public static float planeRadius = 450;

        //time to complete island movement if movement lasts to long, island will be placed to target position
        public static float timeToMove = 15f;

        public static float GetRadiusFromRing(int ring)
        {
            return ring * 2 * hexCellRadius;
        }
    }
}