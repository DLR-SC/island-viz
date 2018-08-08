using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace OsgiViz.Core
{
    public enum type { Class, AbstractClass, Interface, Enum, Unknown };
    public enum modifier { Public, Private, Protected, Static, Final, Default };

    public enum PdaPageType { Inspect, Settings, Default };

    public delegate void callbackMethod();

    public enum Status
    {
        Idle,
        Working,
        Finished,
        Failed
    };

    //Global
    public static class GlobalVar
    {
        public const string projectmodelPath = "Resources/rce_23_05_2017.model";
        //public const string projectmodelPath = "Resources/rce_lite.model";

        //Scale multiplicator of the voronoi cells. Many other variables depend on this scale
        public const float voronoiCellScalefactor = 20f;

        //The number of existing building models(One model for each storey atm.)
        public const int numLocLevels = 8;
        //Building scale
        public const float cuScale  = voronoiCellScalefactor * 0.0075f;   
        public const float dockScaleMult = 0.005f * voronoiCellScalefactor;
        //Smallest dock size
        public const float minDockSize = 0.02f * voronoiCellScalefactor;
        //The percentage of the islands, that is above the ocean. Range [0 - 1]
        public const float islandAboveOcean = 0.9f * 2;
        //The island height profile. Each array entry results in one additional ring of claimed coast cells. The value in the entry is assigned to the island height. 0 is the height level of the regions.
        public static float[] islandHeightProfile = { -0.03f * voronoiCellScalefactor, -0.034f * voronoiCellScalefactor, -0.036f * voronoiCellScalefactor, -0.037f * voronoiCellScalefactor, -0.0375f * voronoiCellScalefactor };

        public const float depArrowWidth = 0.015f * voronoiCellScalefactor;

        //Rendering Variables
        public const float hologramOutlineWidth = 0.02f;
        public static Vector3 hologramOutlineColor = new Vector3(0, 1.0f, 0f);

        //Hierarchical subdivision distances
        public static float subdivisionDistanceIslandSquared  = 400f;
        public static float subdivisionDistanceCountrySquared = 144f;
        public static float subdivisionDistanceCUSquared = 0f;

        //Text Variables
        //Label font size
        public const float minTextSizeInRadians  = Mathf.Deg2Rad*1.0f;
        //Maximal label width before it starts to extend downwards
        public const float maxLabelWidthInRadians = Mathf.Deg2Rad*10f;
        //The thickness of the labels
        public const float labelDepth = 0.005f;
        //TODO: Rather be given implicitly based on maxLabelWidthInRadians
        public const float labelOffset = 0.1f;

        //Height of the holotable
        public static float hologramTableHeight = 1.25f;

        //Viewport-Manipulation
        // The maximal distance the visualization can be draged from the island-world center.
        // 1.0 = the distance of the furthest island from the island-world center
        public const float translationCutoff = 1f + 1.25f;

        // 1.0 = Zoom-Level to fit entire island-world into hologram
        public const float minZoomCutoff = 0.005f;
        public const float maxZoomCutoff = 1.25f;

        //Service-Nodes
        public const float serviceNodeSize = 0.025f * voronoiCellScalefactor;
        public const int groupsPerSlice = 4;
        public const float startingHeight = 1.20f;
        public const float heightStep = 0.02f;

        //Dynamic Variables - written to by IslandViz - Modification is useless ;)
        public static int islandNumber = 100;
        public static float inverseHologramScale = 1.0f;
        public static float worldRadius = 1f;
        public static Vector3 worldCenter = Vector3.zero;
        public static long maximumLOCinProject = 0;
        public static bool recenterValid = false;
    }

    public class JavaParser
    {

        public static modifier getModifierFromString(string inputStr)
        {
            modifier result = modifier.Default;
            
            if (string.Compare(inputStr, "Public", true) == 0)
                result = modifier.Public;
            if (string.Compare(inputStr, "Private", true) == 0)
                result = modifier.Private;
            if (string.Compare(inputStr, "Protected", true) == 0)
                result = modifier.Public;
            if (string.Compare(inputStr, "Static", true) == 0)
                result = modifier.Static;
            if (string.Compare(inputStr, "Final", true) == 0)
                result = modifier.Final;
            if (string.Compare(inputStr, "Default", true) == 0)
                result = modifier.Default;

            return result;
        }

        public static type getTypeFromString(string inputStr)
        {
            type result = type.Unknown;

            if (string.Compare(inputStr, "Class", true) == 0)
                result = type.Class;
            if (string.Compare(inputStr, "AbstractClass", true) == 0)
                result = type.AbstractClass;
            if (string.Compare(inputStr, "Interface", true) == 0)
                result = type.Interface;
            if (string.Compare(inputStr, "Enum", true) == 0)
                result = type.Enum;

            return result;
        }


    }

}