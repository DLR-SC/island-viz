using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using QuickGraph;
using System.Linq;
using OsgiViz.Relations;
using OsgiViz.Core;
using OsgiViz.Island;

namespace OsgiViz.SideThreadConstructors
{

    public class Graph_Layout_Constructor
    {
        private callbackMethod cb;
        Thread _thread;
        private Status status;
        private BidirectionalGraph<GraphVertex, GraphEdge> dependencyGraph;
        private OsgiProject project;
        private System.Random RNG;

        private struct VertexPositionData
        {
            public Vector2 position;
            public Vector2 oldPosition;

            public VertexPositionData(Vector2 pos, Vector2 oldPos)
            {
                position = pos;
                oldPosition = oldPos;
            }
        }

        public Graph_Layout_Constructor()
        {
            status = Status.Idle;
            RNG = new System.Random(0);
        }

        //Public method to construct the physical distribution of islands in a certain volume
        public void ConstructRndLayout(BidirectionalGraph<GraphVertex, GraphEdge> g, callbackMethod m, Vector3 min, Vector3 max, float minDis, int maxIt)
        {
            cb = m;
            dependencyGraph = g;
            ComputeRNDGraphLayoutThreaded(min,max,minDis,maxIt);
        }

        public void ConstructFDLayout(OsgiProject proj, callbackMethod m, float stepSize, int simulationSteps)
        {
            cb = m;
            project = proj;
            dependencyGraph = proj.getDependencyGraph();
            ComputeForceDirectedLayoutThreaded(simulationSteps, stepSize);
        }

        private void ComputeForceDirectedLayoutThreaded(int simulationSteps, float stepSize)
        {
            status = Status.Working;
            Debug.Log("Starting forcedirected graph layout construction.");

            //Attract Strength multi
            float c1 = 10.0f;
            //"Spring" length for maximal dependency strength
            float c3 = 1.0f;
            //Repulsion
            float c4 = 5.0f;
            //Attract-To-Center
            float c5 = 0.005f;
            //Friction
            float c6 = 0.105f;
            //TimeStep
            float t = stepSize;
                       

            Dictionary<GraphVertex, VertexPositionData> simulationData = new Dictionary<GraphVertex, VertexPositionData>();
            #region init start values
            foreach (GraphVertex vert in dependencyGraph.Vertices)
            {
                float rr = 20f;
                Vector2 startPos = new Vector2((float)RNG.NextDouble() * rr, (float)RNG.NextDouble() * rr);
                simulationData.Add(vert, new VertexPositionData(startPos, startPos));
            }
            #endregion
            int stepCounter = 0;

            while (stepCounter < simulationSteps)
            {

                foreach (GraphVertex thisVert in dependencyGraph.Vertices)
                {
                    // total force affecting "thisVert"
                    Vector2 netForce = Vector2.zero; 

                    #region Attraction
                    IEnumerable<GraphEdge> outEdges;
                    dependencyGraph.TryGetOutEdges(thisVert, out outEdges);
                    List<GraphEdge> edgeList = outEdges.ToList();
                    Vector2 springForce = Vector2.zero;
                    foreach (GraphEdge importEdge in edgeList)
                    {
                        GraphVertex otherVert = importEdge.Target;
                        Vector2 direction = simulationData[otherVert].position - simulationData[thisVert].position;

                        float springEquilibriumLength = (thisVert.getIsland().getRadius() + otherVert.getIsland().getRadius()) + c3 * (project.getMaxImportCount() / importEdge.getWeight());

                        springForce += c1 * direction.normalized * Mathf.Log((direction.magnitude / springEquilibriumLength));
                    }
                    IEnumerable<GraphEdge> inEdges;
                    dependencyGraph.TryGetInEdges(thisVert, out inEdges);
                    edgeList = inEdges.ToList();
                    /*
                    foreach (GraphEdge exportEdge in edgeList)
                    {
                        GraphVertex otherVert = exportEdge.Source;
                        Vector2 direction = simulationData[otherVert].position - simulationData[thisVert].position;

                        float springEquilibriumLength = (thisVert.getIsland().getRadius() + otherVert.getIsland().getRadius()) + c3 / exportEdge.getWeight();

                        springForce += c1 * direction.normalized * Mathf.Log((direction.magnitude / springEquilibriumLength));
                    }
                    */
                    netForce += springForce;
                    #endregion

                    
                    #region Repulsion
                    foreach (GraphVertex otherVert in dependencyGraph.Vertices)
                    {
                        if (otherVert == thisVert || (edgeList.Find(x => (x.Source == otherVert) || (x.Target == otherVert))) != null)
                            continue;
                        Vector2 direction = simulationData[thisVert].position - simulationData[otherVert].position;

                        float distanceToBounds = direction.magnitude - (thisVert.getIsland().getRadius() + otherVert.getIsland().getRadius());

                        if (distanceToBounds < 0.0f)
                        {
                            Vector2 constrainedPosition = simulationData[thisVert].position - direction.normalized * distanceToBounds;
                            simulationData[thisVert] = new VertexPositionData(constrainedPosition, constrainedPosition);
                            distanceToBounds = 0f;
                        }
                        netForce += (direction.normalized * c4) / (Mathf.Pow(distanceToBounds + 0.1f, 2f));
                    }
                    #endregion



                    #region Attract-to-Center
                    netForce -= simulationData[thisVert].position * c5;
                    #endregion

                    #region position computation through Verlet-Integration
                    Vector2 currentVelocity = (simulationData[thisVert].position - simulationData[thisVert].oldPosition) / t;
                    Vector2 resistance = currentVelocity * t * c6;

                    Vector2 newPosition = 2.0f * simulationData[thisVert].position - simulationData[thisVert].oldPosition + t * t * netForce;
                    newPosition -= resistance;
                    Vector2 oldPosition = simulationData[thisVert].position;
                    VertexPositionData vpd = new VertexPositionData(newPosition, oldPosition);
                    simulationData[thisVert] = vpd;
                    #endregion

                    stepCounter++;
                }
            }

            #region assign computed positions to graph vertices
            foreach (GraphVertex vert in dependencyGraph.Vertices)
            {
                VertexPositionData vpd = simulationData[vert];
                Vector3 pos = new Vector3(vpd.position.x, 0, vpd.position.y);
                vert.setPosition(pos);
            }
            #endregion


            status = Status.Finished;
            Debug.Log("Forcedirected Graph layout is computed!");
            cb();
        }

        private void ComputeRNDGraphLayoutThreaded(Vector3 distrBoxBegin, Vector3 distrBoxEnd, float minDistance, float maxIterations)
        {
            status = Status.Working;
            Debug.Log("Starting graph layout construction.");
            int overlappingBundles = 0;
            
            foreach (GraphVertex vertex in dependencyGraph.Vertices)
            {
                
                Vector3 diagonalVec = distrBoxEnd - distrBoxBegin;
                Vector3 rndVec = distrBoxBegin + new Vector3(diagonalVec.x * (float)RNG.NextDouble(),
                                                             diagonalVec.y * (float)RNG.NextDouble(),
                                                             diagonalVec.z * (float)RNG.NextDouble());
                int iteration = 0;
                while (!CheckOverlap(rndVec, vertex.getIsland(), minDistance) && iteration <= maxIterations)
                {
                    rndVec = distrBoxBegin + new Vector3(diagonalVec.x * (float)RNG.NextDouble(),
                                                         diagonalVec.y * (float)RNG.NextDouble(),
                                                         diagonalVec.z * (float)RNG.NextDouble());
                    iteration++;
                }

                vertex.setPosition(rndVec);
                if (iteration >= maxIterations)
                    overlappingBundles++;
           
            }

            status = Status.Finished;
            Debug.Log("Graph layout is computed! Number of overlapping bundles: " + overlappingBundles);
            cb();
        }

        private Boolean CheckOverlap(Vector3 newPosition, CartographicIsland newIsland, float minDistance)
        {
            int cc = 0;
            foreach(GraphVertex existingVertex in dependencyGraph.Vertices)
            {
                Vector3 existingPos = existingVertex.getPosition();
                float distance = Vector3.Distance(existingPos, newPosition);
                float existingRadius = existingVertex.getIsland().getRadius();
                float newRadius = newIsland.getRadius();
                if (distance <= (minDistance + (existingRadius + newRadius)))
                    return false;
            }
            return true;
        }

        public Status getStatus()
        {
            return status;
        }

        public void setStatus(Status stat)
        {
            status = stat;
        }

        public BidirectionalGraph<GraphVertex, GraphEdge> getGraph()
        {
            return dependencyGraph;
        }

    }

}