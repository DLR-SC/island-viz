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
        }

        

        public IEnumerator ConstructFDLayout(OsgiProject project, float stepSize, int simulationSteps, System.Random rng)
        {
            dependencyGraph = project.getDependencyGraph();
            
            Debug.Log("Starting forcedirected graph layout construction.");
            IslandVizUI.Instance.UpdateLoadingScreenUI("Forcedirected Graph Layout Construction", "");
            yield return null;

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
                Vector2 startPos = new Vector2((float)rng.NextDouble() * rr, (float)rng.NextDouble() * rr);
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

                    if (stepCounter % 1000 == 0)
                    {
                        IslandVizUI.Instance.UpdateLoadingScreenUI("Forcedirected Graph Layout Construction", (((float)stepCounter / (float)simulationSteps) * 100f).ToString("0.0") + "%");
                        yield return null;
                    }
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
        }

        //Public method to construct the physical distribution of islands in a certain volume
        public IEnumerator ConstructRndLayout(BidirectionalGraph<GraphVertex, GraphEdge> dependencyGraph, Vector3 distrBoxBegin, Vector3 distrBoxEnd, float minDistance, int maxIterations, System.Random rng)
        {            
            Debug.Log("Starting graph layout construction.");
            int overlappingBundles = 0;
            
            foreach (GraphVertex vertex in dependencyGraph.Vertices)
            {
                
                Vector3 diagonalVec = distrBoxEnd - distrBoxBegin;
                Vector3 rndVec = distrBoxBegin + new Vector3(diagonalVec.x * (float)rng.NextDouble(),
                                                             diagonalVec.y * (float)rng.NextDouble(),
                                                             diagonalVec.z * (float)rng.NextDouble());
                int iteration = 0;
                while (!CheckOverlap(rndVec, vertex.getIsland(), minDistance) && iteration <= maxIterations)
                {
                    rndVec = distrBoxBegin + new Vector3(diagonalVec.x * (float)rng.NextDouble(),
                                                         diagonalVec.y * (float)rng.NextDouble(),
                                                         diagonalVec.z * (float)rng.NextDouble());
                    iteration++;
                }

                vertex.setPosition(rndVec);
                if (iteration >= maxIterations)
                    overlappingBundles++;           
            }

            Debug.Log("Graph layout is computed! Number of overlapping bundles: " + overlappingBundles);
            yield return null;
        }

        private Boolean CheckOverlap(Vector3 newPosition, CartographicIsland newIsland, float minDistance)
        {
            int cc = 0;

            if (dependencyGraph == null)
            {
                Debug.LogWarning("Graph_Layout_Constructor.CheckOverlap: dependencyGraph is null!");
                return true;
            }

            foreach (GraphVertex existingVertex in dependencyGraph.Vertices)
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