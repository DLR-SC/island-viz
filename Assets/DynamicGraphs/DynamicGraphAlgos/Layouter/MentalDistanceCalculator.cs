using System;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicGraphAlgoImplementation.Layouter
{
    /// <summary>
    /// Berechnet die Mentale Distanz zwischen GraphLayout und MasterGraph-/Vorgänger-/MasterGraph des Nachfolgers - Layout
    /// entsprechend Diehl & Görg "Graphs, They Are Changing" (2002)
    /// als Summe der Vectorbeträge
    /// </summary>
    public class MentalDistanceCalculator
    {
        public static bool independentDistance(Dictionary<HistoryGraphVertex, Vector2> graph, float maxDelta)
        {
            float dist = calculateDistanceToOwnMaster(graph);
            if (dist > maxDelta)
            {
                return false;
            }
            return true;
        }

        public static bool predecessorDependentDistance(Dictionary<HistoryGraphVertex, Vector2> graph, float maxDelta)
        {
            float dist = calculateDistanceToOwnMaster(graph);
            if (dist > maxDelta)
            {
                return false;
            }
            float dist2 = calculateDistanceToPredecessor(graph);
            if (dist2 > maxDelta)
            {
                return false;
            }
            return true;
        }

        public static bool contextDependentDistance(Dictionary<HistoryGraphVertex, Vector2> graph,
            float maxDelta)
        {
            float dist = calculateDistanceToOwnMaster(graph);
            if (dist > maxDelta)
            {
                return false;
            }
            float dist2 = calculateDistanceToPredecessor(graph);
            if (dist2 > maxDelta)
            {
                return false;
            }
            float dist3 = calculateDistanceToNextMaster(graph);
            if (dist3 > maxDelta)
            {
                return false;
            }
            return true;
        }

        private static float calculateDistanceToOwnMaster(Dictionary<HistoryGraphVertex, Vector2> graph)
        {
            float mentalDelta = 0;
            foreach (HistoryGraphVertex v in graph.Keys)
            {
                Vector2 delta = graph[v] - v.getMaster().getXZPosAs2D();
                mentalDelta += delta.magnitude;
            }
            return mentalDelta;
        }


        private static float calculateDistanceToPredecessor(
            Dictionary<HistoryGraphVertex, Vector2> graph)
        {
            float mentalDelta = 0;
            foreach (HistoryGraphVertex v in graph.Keys)
            {
                if (v.getPrevious() != null)
                {
                    Vector2 delta = graph[v] - v.getPrevious().getXZPosAs2D();
                    mentalDelta += delta.magnitude;
                }
            }
            return mentalDelta;
        }

        private static float calculateDistanceToNextMaster(
            Dictionary<HistoryGraphVertex, Vector2> graph)
        {
            float mentalDelta = 0;
            foreach (HistoryGraphVertex v in graph.Keys)
            {
                if (v.getNext() != null)
                {
                    //getNext().getMaster ist gleich getMaster, da derselbeMaster
                    Vector2 delta = graph[v] - v.getMaster().getXZPosAs2D();
                    mentalDelta += delta.magnitude;
                }
            }
            return mentalDelta;
        }
        
        
        
    }
}