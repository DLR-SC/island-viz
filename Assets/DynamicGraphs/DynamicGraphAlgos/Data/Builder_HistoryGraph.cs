using System;
using System.Collections.Generic;
using GraphBasics;

namespace DynamicGraphAlgoImplementation
{
    public class Builder_HistoryGraph
    {
        //Statistische Parameter
        private int nrOfStartVertex;
        private double probDisappearVertex;
        private double probAppearEdgeFirstTime;
        private double probAppearEdgeLater;
        private double probDisappearEdge;
        private int minAppearVertex;
        private int maxAppearVertex;
        private float maxEdgeStartWeight;
        private float maxEdgeIncrementWeight;

        private List<HistoryGraphVertex> currentVertices;
        private List<HistoryGraphEdge> currentEdges;
        private List<HistoryGraphEdge> absentEdges;

        private int currentVertexCount;

        private HistoryGraph historyGraph;

        //UnityEngine.Random rand;

        //System.Random rand;

        public Builder_HistoryGraph(string historyName)
        {
            nrOfStartVertex = 15;
            probDisappearVertex = 0.05;
            probAppearEdgeFirstTime = 0.35;
            probAppearEdgeLater = 0.1;
            probDisappearEdge = 0.08;
            minAppearVertex = 0;
            maxAppearVertex = 10;
            maxEdgeStartWeight = 8.0f;
            maxEdgeIncrementWeight = 5;

            currentVertices = new List<HistoryGraphVertex>();
            currentEdges = new List<HistoryGraphEdge>();
            absentEdges = new List<HistoryGraphEdge>();

            currentVertexCount = 0;

            historyGraph = new HistoryGraph(historyName);

           // rand = new UnityEngine.Random();

            //rand = new Random();
        }

        public void setVertexGenerationParameters(int nrOfStartV, int maxAppearV)
        {
            this.nrOfStartVertex = nrOfStartV;
            this.maxAppearVertex = maxAppearV;
        }

        public HistoryGraph BuildHistoryGraph(int nrOfFrames)
        {
            if (nrOfFrames <= 0)
            {
                return historyGraph;
            }

            createFirstFrame();

            for (int i = 1; i < nrOfFrames; i++)
            {
                createNextFrame(i);
            }

            return historyGraph;
        }

        private void oldCreateFirstFrame()
        {
            DirectedGraph<HistoryGraphVertex, HistoryGraphEdge> frame =
                new DirectedGraph<HistoryGraphVertex, HistoryGraphEdge>();
            //Vertices
            for (int i = 0; i < nrOfStartVertex; i++)
            {
                HistoryGraphVertex v = new HistoryGraphVertex(nrToName(), true);
                currentVertexCount++;
                currentVertices.Add(v);
                frame.AddVertex(v);
            }

            //Edges
            for (int i = 0; i < currentVertices.Count; i++)
            {
                for (int j = i + 1; j < currentVertices.Count; j++)
                {
                    HistoryGraphEdge e = new HistoryGraphEdge(currentVertices[i], currentVertices[j], true);
                    //if (rand.NextDouble() < probAppearEdgeFirstTime)
                    if (UnityEngine.Random.value < probAppearEdgeFirstTime)
                    {
                        HistoryGraphEdge contraEdge =
                            new HistoryGraphEdge(currentVertices[j], currentVertices[i], true);
                        float weight1 = maxEdgeStartWeight * UnityEngine.Random.value;
                        float weight2 = maxEdgeStartWeight * UnityEngine.Random.value;
                        //float weight1 = maxEdgeStartWeight * (float) rand.NextDouble();
                        //float weight2 = maxEdgeStartWeight * (float) rand.NextDouble();
                        e.setWeight(weight1);
                        contraEdge.setWeight(weight2);
                        frame.AddEdge(e);
                        frame.AddEdge(contraEdge);
                        currentEdges.Add(e);
                    }
                    else
                    {
                        absentEdges.Add(e);
                    }
                }
            }

            historyGraph.addNextFrame(frame);
        }

        private void createFirstFrame()
        {
            DirectedGraph<HistoryGraphVertex, HistoryGraphEdge> frame =
                new DirectedGraph<HistoryGraphVertex, HistoryGraphEdge>();
            //Vertices
            for (int i = 0; i < nrOfStartVertex; i++)
            {
                HistoryGraphVertex v = new HistoryGraphVertex(nrToName(), true);
                currentVertexCount++;
                currentVertices.Add(v);
                frame.AddVertex(v);
            }

            //Edges
            for (int i = 0; i < currentVertices.Count; i++)
            {
                for (int j = 0; j < currentVertices.Count; j++)
                {
                    if (i != j)
                    {
                        HistoryGraphEdge e = new HistoryGraphEdge(currentVertices[i], currentVertices[j], true);
                        //if (rand.NextDouble() < probAppearEdgeFirstTime)
                        if (UnityEngine.Random.value < probAppearEdgeFirstTime)
                        {
                            float weight1 = 1 + maxEdgeStartWeight * UnityEngine.Random.value;

                            //float weight1 = 1 + maxEdgeStartWeight * (float) rand.NextDouble();
                            e.setWeight(weight1);
                            frame.AddEdge(e);
                            currentEdges.Add(e);
                        }
                        else
                        {
                            absentEdges.Add(e);
                        }
                    }
                }
            }
            historyGraph.addNextFrame(frame);
        }


        private void oldCreateNextFrame(int newFrameIndex)
        {
            DirectedGraph<HistoryGraphVertex, HistoryGraphEdge> frame =
                new DirectedGraph<HistoryGraphVertex, HistoryGraphEdge>();


            List<HistoryGraphVertex> oldVertices = currentVertices;
            List<HistoryGraphEdge> oldEdges = currentEdges;
            List<HistoryGraphEdge> oldAbsentEdges = absentEdges;
            currentVertices = new List<HistoryGraphVertex>();
            currentEdges = new List<HistoryGraphEdge>();
            absentEdges = new List<HistoryGraphEdge>();

            //disapper or clone previous vertices
            for (int i = 0; i < oldVertices.Count; i++)
            {
                HistoryGraphVertex oldVertex = oldVertices[i];
                
                //if (rand.NextDouble() < probDisappearVertex)

                if (UnityEngine.Random.value < probDisappearVertex)
                {
                    //Vertex disappears in next graph-> delete all corresponding edges
                    RemoveVertexFromEdgeList(oldVertex, oldEdges);
                    RemoveVertexFromEdgeList(oldVertex, oldAbsentEdges);
                }
                else
                {
                    //Vertex remains in next graph (as clone)
                    HistoryGraphVertex newVertex = oldVertex.CloneAsFollowingVersion(false);
                    currentVertices.Add(newVertex);
                    frame.AddVertex(newVertex);
                }
            }

            //new Vertices
            //int nrOfNewVertices = rand.Next(minAppearVertex, maxAppearVertex);
            int nrOfNewVertices = UnityEngine.Random.Range(minAppearVertex, maxAppearVertex);
            List<HistoryGraphVertex> newVertices = new List<HistoryGraphVertex>();
            for (int i = 0; i < nrOfNewVertices; i++)
            {
                HistoryGraphVertex v = new HistoryGraphVertex(nrToName(), true);
                currentVertexCount++;
                newVertices.Add(v);
                frame.AddVertex(v);
            }

            //Edges between new Vertices
            for (int i = 0; i < newVertices.Count; i++)
            {
                for (int j = i + 1; j < newVertices.Count; j++)
                {
                    HistoryGraphEdge e = new HistoryGraphEdge(newVertices[i], newVertices[j], true);
                    //if (rand.NextDouble() < probAppearEdgeFirstTime)
                    if (UnityEngine.Random.value < probAppearEdgeFirstTime)
                    {
                        HistoryGraphEdge contraEdge = new HistoryGraphEdge(newVertices[j], newVertices[i], true);
                        float weight1 = maxEdgeStartWeight * UnityEngine.Random.value;
                        float weight2 = maxEdgeStartWeight * UnityEngine.Random.value;

                        //float weight1 = maxEdgeStartWeight * (float) rand.NextDouble();
                        //float weight2 = maxEdgeStartWeight * (float) rand.NextDouble();
                        e.setWeight(weight1);
                        contraEdge.setWeight(weight2);
                        frame.AddEdge(e);
                        frame.AddEdge(contraEdge);
                        currentEdges.Add(e);
                    }
                    else
                    {
                        absentEdges.Add(e);
                    }
                }
            }

            //Edges between new Vertices and old Vertices
            for (int i = 0; i < newVertices.Count; i++)
            {
                for (int j = 0; j < currentVertices.Count; j++)
                {
                    HistoryGraphEdge e = new HistoryGraphEdge(newVertices[i], currentVertices[j], true);
                    //if (rand.NextDouble() < probAppearEdgeFirstTime)
                    if (UnityEngine.Random.value < probAppearEdgeFirstTime)
                    {
                        HistoryGraphEdge contraEdge = new HistoryGraphEdge(currentVertices[j], newVertices[i], true);
                        float weight1 = maxEdgeStartWeight * UnityEngine.Random.value;
                        float weight2 = maxEdgeStartWeight * UnityEngine.Random.value;
                        //float weight1 = maxEdgeStartWeight * (float) rand.NextDouble();
                        //float weight2 = maxEdgeStartWeight * (float) rand.NextDouble();
                        e.setWeight(weight1);
                        contraEdge.setWeight(weight2);
                        frame.AddEdge(e);
                        frame.AddEdge(contraEdge);
                        currentEdges.Add(e);
                    }
                    else
                    {
                        absentEdges.Add(e);
                    }
                }
            }

            currentVertices.AddRange(newVertices);

            //Decide which old Edges Remain in new Frame
            foreach (HistoryGraphEdge e in oldEdges)
            {
                HistoryGraphEdge newE = new HistoryGraphEdge(e.GetSource().getNext(), e.GetTarget().getNext(), true);
                //if (rand.NextDouble() < probDisappearEdge)
                if (UnityEngine.Random.value < probDisappearEdge)
                {
                    frame.AddEdge(newE);
                    frame.AddEdge(new HistoryGraphEdge(e.GetTarget().getNext(), e.GetSource().getNext(), true));
                    currentEdges.Add(newE);
                }
                else
                {
                    absentEdges.Add(newE);
                }
            }

            //Decide which former absent edges appear in new Frame
            foreach (HistoryGraphEdge e in oldAbsentEdges)
            {
                HistoryGraphEdge newE = new HistoryGraphEdge(e.GetSource().getNext(), e.GetTarget().getNext(), true);
                //if (rand.NextDouble() < probAppearEdgeLater)
                if (UnityEngine.Random.value < probAppearEdgeLater)
                {
                    frame.AddEdge(newE);
                    frame.AddEdge(new HistoryGraphEdge(e.GetTarget().getNext(), e.GetSource().getNext(), true));
                    currentEdges.Add(newE);
                }
                else
                {
                    absentEdges.Add(newE);
                }
            }

            historyGraph.addNextFrame(frame);
        }

        
         private void createNextFrame(int newFrameIndex)
        {
            DirectedGraph<HistoryGraphVertex, HistoryGraphEdge> frame =
                new DirectedGraph<HistoryGraphVertex, HistoryGraphEdge>();


            List<HistoryGraphVertex> oldVertices = currentVertices;
            List<HistoryGraphEdge> oldEdges = currentEdges;
            List<HistoryGraphEdge> oldAbsentEdges = absentEdges;
            currentVertices = new List<HistoryGraphVertex>();
            currentEdges = new List<HistoryGraphEdge>();
            absentEdges = new List<HistoryGraphEdge>();

            //disapper or clone previous vertices
            for (int i = 0; i < oldVertices.Count; i++)
            {
                HistoryGraphVertex oldVertex = oldVertices[i];
                //if (rand.NextDouble() < probDisappearVertex)
                if (UnityEngine.Random.value < probDisappearVertex)
                {
                    //Vertex disappears in next graph-> delete all corresponding edges
                    RemoveVertexFromEdgeList(oldVertex, oldEdges);
                    RemoveVertexFromEdgeList(oldVertex, oldAbsentEdges);
                }
                else
                {
                    //Vertex remains in next graph (as clone)
                    HistoryGraphVertex newVertex = oldVertex.CloneAsFollowingVersion(false);
                    currentVertices.Add(newVertex);
                    frame.AddVertex(newVertex);
                }
            }

            //new Vertices
            //int nrOfNewVertices = rand.Next(minAppearVertex, maxAppearVertex);
            int nrOfNewVertices = UnityEngine.Random.Range(minAppearVertex, maxAppearVertex);
            List<HistoryGraphVertex> newVertices = new List<HistoryGraphVertex>();
            for (int i = 0; i < nrOfNewVertices; i++)
            {
                HistoryGraphVertex v = new HistoryGraphVertex(nrToName(), true);
                currentVertexCount++;
                newVertices.Add(v);
                frame.AddVertex(v);
            }

            //Edges between new Vertices
            for (int i = 0; i < newVertices.Count; i++)
            {
                for (int j = 0; j < newVertices.Count; j++)
                {
                    if (i != j)
                    {
                        HistoryGraphEdge e = new HistoryGraphEdge(newVertices[i], newVertices[j], true);
                        //if (rand.NextDouble() < probAppearEdgeFirstTime)
                        if (UnityEngine.Random.value < probAppearEdgeFirstTime)
                        {
                            //float weight1 = 1+ maxEdgeStartWeight * (float) rand.NextDouble();
                            float weight1 = 1+ maxEdgeStartWeight * UnityEngine.Random.value;
                            e.setWeight(weight1);
                            frame.AddEdge(e);
                            currentEdges.Add(e);
                        }
                        else
                        {
                            absentEdges.Add(e);
                        }
                    }
                }
            }

            //Edges from new Vertices to old Vertices
            for (int i = 0; i < newVertices.Count; i++)
            {
                for (int j = 0; j < currentVertices.Count; j++)
                {
                    HistoryGraphEdge e = new HistoryGraphEdge(newVertices[i], currentVertices[j], true);
                    //if (rand.NextDouble() < probAppearEdgeFirstTime)
                    if (UnityEngine.Random.value < probAppearEdgeFirstTime)
                    {
                        //float weight1 = 1+maxEdgeStartWeight * (float) rand.NextDouble();
                        float weight1 = 1+maxEdgeStartWeight * UnityEngine.Random.value;
                        e.setWeight(weight1);
                        frame.AddEdge(e);
                        currentEdges.Add(e);
                    }
                    else
                    {
                        absentEdges.Add(e);
                    }
                }
            }
            //Edges from old Vertices to new Vertices
            for (int i = 0; i < currentVertices.Count; i++)
            {
                for (int j = 0; j < newVertices.Count; j++)
                {
                    HistoryGraphEdge e = new HistoryGraphEdge(currentVertices[i], newVertices[j], true);
                    //if (rand.NextDouble() < probAppearEdgeFirstTime)
                    if (UnityEngine.Random.value < probAppearEdgeFirstTime)
                    {
                        //float weight1 = 1 + maxEdgeStartWeight * (float) rand.NextDouble();
                        float weight1 = 1 + maxEdgeStartWeight * UnityEngine.Random.value;
                        e.setWeight(weight1);
                        frame.AddEdge(e);
                        currentEdges.Add(e);
                    }
                    else
                    {
                        absentEdges.Add(e);
                    }
                }
            }

            currentVertices.AddRange(newVertices);

            //Decide which old Edges Remain in new Frame
            foreach (HistoryGraphEdge e in oldEdges)
            {
                HistoryGraphEdge newE = e.CloneAsFollowingVersion();
                //if (rand.NextDouble() >= probDisappearEdge)
                if (UnityEngine.Random.value >= probDisappearEdge)
                {
                    //Decide if weight stays, increases, decreases
                    float incrementWeight = 0.0f;
                    //double random = rand.NextDouble();
                    double random = UnityEngine.Random.value;
                    if (random < 1.0 / 3.0)
                    {
                        //incrementWeight = maxEdgeIncrementWeight * (float) rand.NextDouble();
                        incrementWeight = maxEdgeIncrementWeight * UnityEngine.Random.value;
                    }
                    else if (random < 2.0 / 3.0)
                    {
                        //incrementWeight = -1.0f * maxEdgeIncrementWeight * (float) rand.NextDouble();
                        incrementWeight = -1.0f * maxEdgeIncrementWeight * UnityEngine.Random.value;
                    }
                    newE.incrementWeight(incrementWeight);
                    frame.AddEdge(newE);
                    currentEdges.Add(newE);
                }
                else
                {
                    absentEdges.Add(newE);
                }
            }

            //Decide which former absent edges appear in new Frame
            foreach (HistoryGraphEdge e in oldAbsentEdges)
            {
                HistoryGraphEdge newE = e.CloneAsFollowingVersion();
               // if (rand.NextDouble() < probAppearEdgeLater)
                if (UnityEngine.Random.value < probAppearEdgeLater)
                {
                    //float weight1 = 1 + maxEdgeStartWeight * (float) rand.NextDouble();
                    float weight1 = 1 + maxEdgeStartWeight * UnityEngine.Random.value;
                    newE.setWeight(weight1);
                    frame.AddEdge(newE);
                    currentEdges.Add(newE);
                }
                else
                {
                    absentEdges.Add(newE);
                }
            }
            historyGraph.addNextFrame(frame);
        }
        
        private void RemoveVertexFromEdgeList(HistoryGraphVertex v, List<HistoryGraphEdge> l)
        {
            int i = 0;
            while (i < l.Count)
            {
                if (l[i].ContainsVertex(v))
                {
                    l.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }


        /// <summary>
        /// KnotenNamen im Format xx wobei x=0..9,A...Z
        /// </summary>
        /// <returns>KnotenNamen im Format xx wobei x=0..9,A...Z</returns>
        private String nrToName()
        {
            return currentVertexCount.ToString();
        }
    }
}