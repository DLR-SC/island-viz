using System;
using UnityEngine;

namespace DynamicGraphAlgoImplementation.NewLayouter
{
    public class IslandVizForceDirectedLayouter_History : IslandVizForceDirectedLayouter<HistoryGraphVertex, HistoryGraphEdge>
    {
        //Faktor Federhärte History Force
        protected float c5;
        protected int historyDepth;

        public IslandVizForceDirectedLayouter_History(float r, float c1, float c3, float maxImport,
            double startTempFrac, int iterations, float c5, int maxDepth) : base(r, c1, c3, maxImport, startTempFrac, iterations)
        {
            this.historyDepth = maxDepth;
            this.c5 = c5;
        }

        public override Vector2 calculateHistoryForce(HistoryGraphVertex vertex)
        {
            Vector2 histForce = Vector2.zero;

            int depthCount = 1;
            HistoryGraphVertex histVertex = vertex;

            while (depthCount <= historyDepth && histVertex.getPrevious() != null)
            {
                histVertex = histVertex.getPrevious();
                Vector2 direction = histVertex.getXZPosAs2D() - positions[vertex].position;
                if (direction.magnitude != 0)
                {
                    histForce += c5 * c1 * 1 / Mathf.Pow(2, depthCount - 1) * direction.normalized *
                                 Mathf.Pow(direction.magnitude, 2) / c4;
                }
                depthCount++;
            }
            
            
            
            return histForce;
            
        }
    }
}