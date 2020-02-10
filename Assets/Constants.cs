using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class Constants
    {
        //Constants about using Database Layout or new, write to DB
        public static bool useValuesFromDBWherePossible = true;
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
        public static Vector2[] colVals = new Vector2[] {
            new Vector2(0.1f, 0.1f), //rot (unten links)
            new Vector2(0.3f, 0.1f), //gelb
            new Vector2(0.7f, 0.1f), //dunkelgrün
            new Vector2(0.9f, 0.1f), //türkis (unten rechts)
            new Vector2(0.1f, 0.3f), //dunkelblau (zweite zeile links)
            new Vector2(0.3f, 0.3f), //violet
            new Vector2(0.5f, 0.3f), //pink
            new Vector2(0.7f, 0.3f), //orange
        };
        public static Vector2 colValNewHighlight = new Vector2(0.5f, 0.1f);
        public static Vector2 colValChangeHighlight = new Vector2(0.9f, 0.3f);
        public static Vector2 colValChangeBuildingHighlight = new Vector2(0.1f, 0.9f);
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
        public static float planeRadius = 1000;

        //time to complete island movement if movement lasts to long, island will be placed to target position
        public static float timeToMove = 15f;

        public static float GetRadiusFromRing(int ring)
        {
            return ring * 2 * hexCellRadius;
        }
    }
}