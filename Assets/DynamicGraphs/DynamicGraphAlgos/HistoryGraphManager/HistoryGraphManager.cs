using System.Runtime.InteropServices;
using DynamicGraphAlgoImplementation.NewLayouter;
using GraphBasics;
using UnityEngine;
using System.Collections;

namespace DynamicGraphAlgoImplementation.HistoryGraphManager
{
    public abstract class HistoryGraphManager : ScriptableObject
    {
        protected HistoryGraph historyGraph;
        protected IslandVizForceDirectedLayouter<HistoryGraphVertex, HistoryGraphEdge> graphLayouter;
        protected PlaneMainController controller;

        public HistoryGraphManager(HistoryGraph hGraph)
        {
            this.historyGraph = hGraph;
        }

        public IEnumerator LayoutFrame(int index)
        {
            //Check Index Range
            if (index >= historyGraph.getFrameCount())
            {
                yield break;
            }

            //Check if Frame is Already Layouted
            if (historyGraph.frameIsLayouted(index))
            {
                yield break;
            }

            //Check im Previous Frames are already Layouted
            int sequenceStartIndex = index - 1;
            while (sequenceStartIndex >= 0 && !historyGraph.frameIsLayouted(sequenceStartIndex))
            {
                sequenceStartIndex--;
            }

            //Debug.Log("HistoryGraphManager has to Layout " + (index - sequenceStartIndex) + " Frames.");
            //Layout all Frames that are still unlayouted until wanted frame
            for (int i = sequenceStartIndex + 1; i <= index; i++)
            {
                Debug.Log("Layouting " + i);
                graphLayouter.setGraph(historyGraph.getFrameAt(i));
                graphLayouter.setMaxImport(historyGraph.getMaxImportOfFrame(i));
                yield return graphLayouter.layout();
                historyGraph.setIsLayouted(i);
            }
            //Debug.Log("Layouting Frames finished");
        }

      /*  public void LayoutFrame(int index)
        {
            //Check Index Range
            if (index >= historyGraph.getFrameCount())
            {
                return;
            }

            //Check if Frame is Already Layouted
            if (historyGraph.frameIsLayouted(index))
            {
                return;
            }

            //Check im Previous Frames are already Layouted
            int sequenceStartIndex = index - 1;
            while (sequenceStartIndex >= 0 && !historyGraph.frameIsLayouted(sequenceStartIndex))
            {
                sequenceStartIndex--;
            }

            //Debug.Log("HistoryGraphManager has to Layout " + (index - sequenceStartIndex) + " Frames.");
            //Layout all Frames that are still unlayouted until wanted frame
            for (int i = sequenceStartIndex + 1; i <= index; i++)
            {
                graphLayouter.setGraph(historyGraph.getFrameAt(i));
                graphLayouter.setMaxImport(historyGraph.getMaxImportOfFrame(i));
                graphLayouter.layout();
                historyGraph.setIsLayouted(i);
            }
            //Debug.Log("Layouting Frames finished");
        }*/

        public void SetHistoryGraph(HistoryGraph historyGraph)
        {
            this.historyGraph = historyGraph;
        }


        /// <summary>
        /// Für Initialisierung HistoryGraphManager_Master
        /// </summary>
        /// <param name="r">Radius of GraphDrawing Area (default 20.0)</param>
        /// <param name="c1">"c1">Attraction (default 1.0)</param>
        /// <param name="c3">Repulsion (defualt 2.0f)</param>
        /// <param name="startTempFrac">Maximal Movement Per Iteration (default 0.1)</param>
        /// <param name="iterations">(default 1000)</param>
        /// <param name="mentalDelta">Maximale Abweichung bei Kraftansatz zusätzlich zu Masterlayout(default 20.0)</param>
        public abstract void Init(float r, float c1, float c3, float startTempFrac, int iterations, float mentalDelta);

        /// <summary>
        /// Für Initialisierung HistoryGraphManager_History
        /// </summary>
        /// <param name="r">Radius of GraphDrawing Area (default 20.0)</param>
        /// <param name="c1">Attraction (default 1.0)</param>
        /// <param name="c3">Repulsion (defualt 2.0f)</param>
        /// <param name="startTempFrac">Maximal Movement Per Iteration (default 0.1) </param>
        /// <param name="iterations">(default 1000)</param>
        /// <param name="c5">Faktor c1*c5 Attraction to former Positions(zw. 8 und 32)</param>
        /// <param name="maxDepth">maximale Tiefe der Vorgänger für Attraction HistoryForce (default 3)</param>
        public abstract void Init(float r, float c1, float c3, float startTempFrac, int iterations, float c5, int maxDepth);

        public void SetPlaneMainController(PlaneMainController pmc)
        {
            this.controller = pmc;
        }
       
        public void InstantiateVertexRepresentations(GameObject vertexRepresentationPrefab)
        {
            foreach(MasterVertex mv in historyGraph.getMasterGraph().GetVertices())
            {
                GameObject representation = Instantiate(vertexRepresentationPrefab, new Vector3(0,0,0), Quaternion.identity);
                representation.gameObject.name = "Vertex Representation " + mv.getName();
                representation.GetComponent<VertexRepresentation>().Initialize(mv, controller);
            }
        }

        public int NrOfVerticesToBeEnabledInFirstStep()
        {
            int result = 0;
            foreach (MasterVertex mv in historyGraph.getMasterGraph().GetVertices())
            {
                if (mv.ExistsHistoryGraphVertexAtIndex(0))
                {
                    result++;
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction">"forward" or "backward"</param>
        /// <returns>Vector3: disable, move, enable</returns>
        public Vector3Int NrOfVertexChanges(string direction)
        {
            //controller CurrentFrameIndex noch beim AltenFrame
            int currentIndex = controller.GetCurrentFrameIndex();
            int nextIndex = 0;
            if (direction.Equals("forward"))
            {
                nextIndex = currentIndex + 1;
            }else if (direction.Equals("backward"))
            {
                nextIndex = currentIndex - 1;
            }
            int disable = 0;
            int move = 0;
            int enable = 0;
            foreach(MasterVertex mv in historyGraph.getMasterGraph().GetVertices())
            {
                if(mv.ExistsHistoryGraphVertexAtIndex(currentIndex)&& mv.ExistsHistoryGraphVertexAtIndex(nextIndex))
                {
                    move++;
                }else if(mv.ExistsHistoryGraphVertexAtIndex(currentIndex) && !mv.ExistsHistoryGraphVertexAtIndex(nextIndex))
                {
                    disable++;
                }else if(!mv.ExistsHistoryGraphVertexAtIndex(currentIndex) && mv.ExistsHistoryGraphVertexAtIndex(nextIndex))
                {
                    enable++;
                }
            }
            return new Vector3Int(disable, move, enable);
        }
    }
}