using System;
using DynamicGraphAlgoImplementation.NewLayouter;
using DynamicGraphAlgoImplementation.PositionInitializer;
using UnityEngine;

namespace DynamicGraphAlgoImplementation.HistoryGraphManager
{
    public class HistoryGraphManager_Master : HistoryGraphManager
    {
        public HistoryGraphManager_Master(HistoryGraph historyGraph) : base(historyGraph)
        {
            
        }

        /// <summary>
        /// Initialisiert Layouter und Berechnet MasterGraph-Layout
        /// </summary>
        /// <param name="r">Radius of GraphDrawing Area (default 20.0)</param>
        /// <param name="c1">"c1">Attraction (default 1.0)</param>
        /// <param name="c3">Repulsion (defualt 2.0f)</param>
        /// <param name="startTempFrac">Maximal Movement Per Iteration (default 0.1)</param>
        /// <param name="iterations">(default 1000)</param>
        /// <param name="mentalDelta">Maximale Abweichung bei Kraftansatz zusätzlich zu Masterlayout(default 20.0)</param>
        
        public override void Init(float r, float c1, float c3, float startTempFrac, int iterations, float mentalDelta)
        {
            //Debug.Log("Start Layouting MasterGraph");
            IslandVizForceDirectedLayouter<MasterVertex, MasterEdge> masterLayouter = 
                new IslandVizForceDirectedLayouter<MasterVertex, MasterEdge>(r, c1, c3, 
                    historyGraph.getMasterGraphMaxEdgeWeight(),startTempFrac,  iterations);
            masterLayouter.setGraph(historyGraph.getMasterGraph());
            masterLayouter.setInitialiser(new RandomInitializer<MasterVertex>(r));
           
            masterLayouter.layout();
       
            graphLayouter = new IslandVizForceDirectedLayouter_Master(r, c1, c3, historyGraph.getMasterGraphMaxEdgeWeight(), startTempFrac, iterations, mentalDelta);
            graphLayouter.setInitialiser(new MasterInitializer<HistoryGraphVertex>());

        }

        public override void Init(float r, float c1, float c2, float startTempFrac, int iterations, float c5, int maxDepth)
        {
            throw new NotImplementedException();
        }
    }
}