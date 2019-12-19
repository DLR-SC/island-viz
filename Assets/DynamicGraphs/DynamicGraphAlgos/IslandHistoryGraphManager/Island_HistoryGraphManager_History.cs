using DynamicGraphAlgoImplementation.NewLayouter;
using DynamicGraphAlgoImplementation.PositionInitializer;
using UnityEngine;
using System.Collections;

namespace DynamicGraphAlgoImplementation.HistoryGraphManager
{
    public class Island_HistoryGraphManager_History : IslandHistoryGraphManager
    {
        public Island_HistoryGraphManager_History(HistoryGraph hGraph) : base(hGraph)
        {
        }

        /// <summary>
        /// Initialisiert Layouter
        /// </summary>
        /// <param name="r">Radius of GraphDrawing Area (default 20.0)</param>
        /// <param name="c1">Attraction (default 1.0)</param>
        /// <param name="c3">Repulsion (defualt 2.0f)</param>
        /// <param name="startTempFrac">Maximal Movement Per Iteration (default 0.1) </param>
        /// <param name="iterations">(default 1000)</param>
        /// <param name="c5">Faktor c1*c5 Attraction to former Positions(zw. 8 und 32)</param>
        /// <param name="maxDepth">maximale Tiefe der Vorgänger für Attraction HistoryForce (default 3)</param>
        public override IEnumerator Init(float r, float c1, float c3, float startTempFrac,
            int iterations, float c5, int maxDepth)
        {
            //(maxImport = 0, da sowieso bei jedem Frame Neu Gesetzt)
            graphLayouter = new IslandVizForceDirectedLayouter_History
                (r, c1, c3, 0.0f, startTempFrac, iterations, c5, maxDepth);
            graphLayouter.setInitialiser(new HistoryInitialiser<HistoryGraphVertex>(r, "circle"));
            yield return null;
        }

        public override IEnumerator Init(float r, float c1, float c3, float startTempFrac, int iterations, float mentalDelta)
        {
            throw new System.NotImplementedException();
        }

      
    }
}