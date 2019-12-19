using System;
using System.Collections.Generic;
using GraphBasics;
using UnityEngine;

namespace DynamicGraphAlgoImplementation
{
    /// <summary>
    /// Datensatz für GraphHistorie: Liste der Einzelframes, Mastergraph und Zusatzinformationen
    /// </summary>
    public class HistoryGraph

    {
        private DirectedGraph<MasterVertex, MasterEdge> masterGraph;
        private float masterGraphMaxEdgeWeight;
        
        private List<DirectedGraph<HistoryGraphVertex, HistoryGraphEdge>> _graphs;
        private List<float> _maxImports;
        private List<bool> _layouted;
        private string sequenceName;

        public HistoryGraph(string Name)
        {
            sequenceName = Name;
            _graphs = new List<DirectedGraph<HistoryGraphVertex, HistoryGraphEdge>>();
            _maxImports = new List<float>();
            _layouted = new List<bool>();
            masterGraph = new DirectedGraph<MasterVertex, MasterEdge>();
            masterGraphMaxEdgeWeight = 0.0f;
        }

        public string GetName()
        {
            return sequenceName;
        }

        public void SetName(string Name)
        {
            this.sequenceName = Name;
        }

        public DirectedGraph<MasterVertex, MasterEdge> getMasterGraph()
        {
            return masterGraph;
        }
        
        public void addNextFrame(DirectedGraph<HistoryGraphVertex, HistoryGraphEdge> frame)
        {
            _graphs.Add(frame);
            _layouted.Add(false);
            foreach (HistoryGraphVertex v in frame.GetVertices())
            {
                v.getMaster().IncrementWeight();
                v.getMaster().SetHistoryGraphVertexWithIndex(v, _graphs.Count-1);
                if (!masterGraph.ContainsVertex(v.getMaster()))
                {
                    masterGraph.AddVertex(v.getMaster());
                }
            }

            float maxEdgeWeight = 0.0f; 

            foreach (HistoryGraphEdge e in frame.GetEdges())
            {
                /*Debug wegen Division durch 0
                if (e.getWeight() == 0)
                {
                    Console.WriteLine("Edge with Weight 0");
                }*/
                
                e.getMaster().incrementAppearanceCount();
                e.getMaster().incrementWeight(e.getWeight());
                if (!masterGraph.ContainsEdge(e.getMaster()))
                {
                    masterGraph.AddEdge(e.getMaster());
                }
                //Im Frame höchstes Kantengewicht
                if (e.getWeight() > maxEdgeWeight)
                {
                    maxEdgeWeight = e.getWeight();
                }
                //Global höchstes Kantengewicht
                if (e.getMaster().getWeight() > masterGraphMaxEdgeWeight)
                {
                    masterGraphMaxEdgeWeight = e.getMaster().getWeight();
                }
            }
            _maxImports.Add(maxEdgeWeight);
        }

        public List<DirectedGraph<HistoryGraphVertex, HistoryGraphEdge>> getFrames()
        {
            return _graphs;
        }

        public int getFrameCount()
        {
            return _graphs.Count;
        }

        public float getMaxImportOfFrame(int index)
        {
            return _maxImports[index];
        }
        
        public DirectedGraph<HistoryGraphVertex, HistoryGraphEdge> getFrameAt(int index)
        {
            return _graphs[index];
        }

        public float getMasterGraphMaxEdgeWeight()
        {
            return masterGraphMaxEdgeWeight;
        }

        public bool frameIsLayouted(int index)
        {
            return _layouted[index];
        }

        public void setIsLayouted(int index)
        {
            _layouted[index] = true;
        }
        
        public void WriteHistoryGraphInDotFormatToFileSystem(String basicPath, bool fixedPosition,
            bool includeGlobalEdges, bool invisEdges)
        {
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minZ = float.PositiveInfinity;
            float maxZ = float.NegativeInfinity;

            if (includeGlobalEdges)
            {
                getBoundingBoxOfGlobalGraph(out minX, out maxX, out minZ, out maxZ);
            }

            DateTime now = System.DateTime.Now;
            String dateString = now.Year.ToString("0000") + now.Month.ToString("00") + now.Day.ToString("00") + "-" +
                                now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
            int i = 1;
            foreach (DirectedGraph<HistoryGraphVertex, HistoryGraphEdge> frame in _graphs)
            {
                string filename = basicPath + "\\" + dateString + "_" + sequenceName + "_" + i + ".dot";
                string graphString;
                if (includeGlobalEdges)
                {
                    graphString = Writer_BidirectionalGraph.writeWithBox(frame, sequenceName + "_" + i, fixedPosition, minX, maxX, minZ, maxZ, invisEdges);
                }
                else
                {
                    graphString = Writer_BidirectionalGraph.write(frame, sequenceName + "_" + i, fixedPosition, invisEdges);
                }
                
                graphString = graphString.Replace("\n", Environment.NewLine);

                System.IO.File.WriteAllText(@filename, graphString);
                i++;
            }
        }

        public void WriteMasterGraphToFileSystem(String basicPath)
        {
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minZ = float.PositiveInfinity;
            float maxZ = float.NegativeInfinity;

            getBoundingBoxOfGlobalGraph(out minX, out maxX, out minZ, out maxZ);
            
            DateTime now = System.DateTime.Now;
            String dateString = now.Year.ToString("0000") + now.Month.ToString("00") + now.Day.ToString("00") + "-" +
                                now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");

            string filename = basicPath + "\\" + dateString + "_" + sequenceName + "_" + "master" + ".dot";
            
            String graphString = Writer_BidirectionalGraph.writeWithBox(masterGraph, sequenceName + "_" + "master", true, minX, maxX, minZ, maxZ, true);

            graphString = graphString.Replace("\n", Environment.NewLine);

            System.IO.File.WriteAllText(@filename, graphString);
        }


        public void WriteHistoryGraphsChangeInDotFormatToFileSystem(String basicPath, bool fixedPosition)
        {
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minZ = float.PositiveInfinity;
            float maxZ = float.NegativeInfinity;
            
            getBoundingBoxOfGlobalGraph(out minX, out maxX, out minZ, out maxZ);
            
            
            DateTime now = System.DateTime.Now;
            String dateString = now.Year.ToString("0000") + now.Month.ToString("00") + now.Day.ToString("00") + "-" +
                                now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");

            for (int i = 0; i < _graphs.Count - 1; i++)
            {
                string filename = basicPath + "\\" + dateString + "_" + sequenceName + "_" + (i + 1) + "-" + (i + 2) +
                                  ".dot";
                string graphString = Writer_BidirectionalGraph.WriteGraphsTransition(_graphs[i], _graphs[i + 1], minX,
                    maxX, minZ, maxZ, sequenceName + "_" + (i + 1) + "_" + (i + 2));
                
                graphString = graphString.Replace("\n", Environment.NewLine);
                
                System.IO.File.WriteAllText(@filename, graphString);
            }
           
        }
        
        /*public void WriteHistoryGraphInDotFormatToFileSystemStandard(String basicPath)
        {
            DateTime now = System.DateTime.Now;
            String dateString = now.Year.ToString("0000") + now.Month.ToString("00") + now.Day.ToString("00") + "-" +
                                now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
            int i = 1;
            foreach (BidirectionalGraph<HistoryGraphVertex, HistoryGraphEdge> frame in _graphs)
            {
                string filename = basicPath + "\\" + dateString + "_" + sequenceName + "_" + i + ".dot";
                string graphString = Writer_BidirectionalGraph.writeStandard(frame);

                System.IO.File.WriteAllText(@filename, graphString);
                i++;
            }
        }*/

        public void getBoundingBoxOfGlobalGraph(out float rMinX, out float rMaxX, out float rMinZ, out float rMaxZ)
        {
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minZ = float.PositiveInfinity;
            float maxZ = float.NegativeInfinity;

            foreach (DirectedGraph<HistoryGraphVertex, HistoryGraphEdge> frame in _graphs)
            {
                foreach (HistoryGraphVertex v in frame.GetVertices())
                {
                    float x = v.getPosition().x;
                    float z = v.getPosition().z;
                    if (x > maxX)
                    {
                        maxX = x;
                    }

                    if (x < minX)
                    {
                        minX = x;
                    }

                    if (z > maxZ)
                    {
                        maxZ = z;
                    }

                    if (z < minZ)
                    {
                        minZ = z;
                    }
                }
            }
            rMinX = minX;
            rMaxX = maxX;
            rMinZ = minZ;
            rMaxZ = maxZ;
        }


        public void AddMasterVertex(MasterVertex v)
        {
            this.masterGraph.AddVertex(v);
        }

        public void AddMasterEdge(MasterEdge e)
        {
            this.masterGraph.AddEdge(e);
        }
    }
}