using System;
using System.Collections.Generic;
using DynamicGraphAlgoImplementation.Layouter;
using UnityEngine;

namespace DynamicGraphAlgoImplementation.NewLayouter
{
    public class IslandVizForceDirectedLayouter_Master : IslandVizForceDirectedLayouter<HistoryGraphVertex, HistoryGraphEdge>
    {
        protected float mentalDelta;
        
        public IslandVizForceDirectedLayouter_Master(float r, float c1, float c3, float maxImport, double startTempFrac,
            int iterations, float mentalDelta):base(r, c1, c3, maxImport, startTempFrac, iterations)
        {
            this.mentalDelta = mentalDelta;
        }
        
        public override void calculateNewPositions()
        {
            Dictionary<HistoryGraphVertex, Vector2> newPositions = new Dictionary<HistoryGraphVertex, Vector2>();
            foreach (HistoryGraphVertex vertex in graph.GetVertices())
            {
                Vector2 newPos;
                if (displacements[vertex].magnitude != 0.0)
                {
                    newPos = positions[vertex].position +
                                     (displacements[vertex] / displacements[vertex].magnitude) *
                                     (float) Math.Min(displacements[vertex].magnitude, temp);
                    if (newPos.magnitude > radius)
                    {
                        newPos = newPos / newPos.magnitude * radius;
                    }
                }
                else
                {
                    newPos = positions[vertex].position;
                }

                //positions[vertex] = new VertexPositionData(newPos, newPos);
                newPositions[vertex] = newPos;
            }

            if (MentalDistanceCalculator.contextDependentDistance(newPositions, mentalDelta))
            {
                foreach (HistoryGraphVertex vertex in graph.GetVertices())
                {
                    positions[vertex] = new VertexPositionData(newPositions[vertex], newPositions[vertex]);
                }
            }
        }
    }
}