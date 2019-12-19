using System;
using System.Collections.Generic;
using System.Linq;
using DynamicGraphAlgoImplementation.Layouter;
using GraphBasics;
using UnityEngine;

namespace DynamicGraphAlgoImplementation.NewLayouter
{
    /// <summary>
    /// Benutzt IslandViz Kräfte wie in originalVersion und in Martin Misiak Masterarbeit S. 39/40
    /// </summary>
    /// <typeparam name="V"></typeparam>
    /// <typeparam name="E"></typeparam>
    public class IslandVizForceDirectedLayouter<V, E> : AbstractForceDirectedLayouter<V, E>
        where V : GraphVertex
        where E : DirectedEdge<V>, IWeightedEdge
    {
        protected float c1;
        protected float c3;
        protected float c4;
        protected double startTemp;
        protected double temp;
        protected float maxImport;


        public IslandVizForceDirectedLayouter(float r, float c1, float c3, float maxImport, double startTempFrac,
            int iterations) : base(r, iterations)
        {
            this.c1 = c1;
            this.c3 = c3;
            this.maxImport = maxImport;
            this.startTemp = radius * startTempFrac;
        }

        public void setMaxImport(float mi)
        {
            if (graph.EdgeCount()!=0 && mi == 0)
            {
                Debug.LogError("MaxImport is zero");
            }
            this.maxImport = mi;
        }

        public float getMaxImport()
        {
            return maxImport;
        }
        
        public override void initialise()
        {
            base.initialise();
            float area = Mathf.PI * Mathf.Pow(radius, 2.0f);
            //c3 als optimale Federlänge wird analog festgelegt zu Fruchterman
            c4 = c3 * Mathf.Sqrt(area / (float) graph.VertexCount());
        }

        public override Vector2 calculateAttraction(V vertex)
        {
            IEnumerable<E> outEdges;
            graph.TryGetOutEdges(vertex, out outEdges);
            List<E> edgeList = outEdges.ToList();

            Vector2 attraction = Vector2.zero;

            foreach (E importEdge in edgeList)
            {
                V otherVertex = importEdge.GetTarget();
                E contraEdge;
                graph.TryGetEdge(otherVertex, vertex, out contraEdge);
                float importSum = importEdge.getWeight();
                if (contraEdge != null)
                {
                    importSum += contraEdge.getWeight();
                }
                if(importSum == 0)
                {
                    //To avoid division by zero
                    importSum = 1;
                }

                float springEquilibriumLength = 2.0f + otherVertex.getRadius() + vertex.getRadius() +
                    c3 * (float) (maxImport / importSum);
                Vector2 posOther = positions[otherVertex].position;
                Vector2 posVertex = positions[vertex].position;
                Vector2 direction = posOther - posVertex;
                //Vector2 direction = positions[otherVertex].position - positions[vertex].position;

                attraction += c1 * direction.normalized *
                              Mathf.Log((direction.magnitude / springEquilibriumLength));
                if(float.IsPositiveInfinity(attraction.x)||float.IsNegativeInfinity(attraction.x)||float.IsPositiveInfinity(attraction.y)||float.IsNegativeInfinity(attraction.y))
                {
                    Debug.LogError("Attraction Infinity");
                }
                if (float.IsNaN(attraction.x) || float.IsNaN(attraction.y))
                {
                    Debug.LogError("AttractionIsNaN");
                }
            }
            

            return attraction;
        }

        public override Vector2 calculateRepulsion(V vertex)
        {
            Vector2 repulsion = Vector2.zero;
            foreach (V v in graph.GetVertices())
            {
                //TODO : alle Knoten stoßen sich ab, nicht nur die ohne ecke
                if (vertex != v && !(graph.ContainsEdge(vertex, v) || graph.ContainsEdge(v, vertex)))
                {
                    Vector2 direction = positions[v].position - positions[vertex].position;
                    float difference = direction.magnitude - vertex.getRadius() - v.getRadius() - 2.0f;
                    if (difference < 0)
                    {
                        repulsion += difference * direction.normalized;
                    }
                    else
                    { 
                        repulsion -= direction.normalized * c4 / Mathf.Pow(direction.magnitude, 2);
                    }
                    if (float.IsPositiveInfinity(repulsion.x) || float.IsNegativeInfinity(repulsion.x) || float.IsPositiveInfinity(repulsion.y) || float.IsNegativeInfinity(repulsion.y))
                    {
                        Console.WriteLine("Repulsion Infinity");
                    }
                    if (float.IsNaN(repulsion.x) || float.IsNaN(repulsion.y))
                    {
                        Debug.LogError("Repulsion is NaN ");
                    }
                }
            }
            return repulsion;
        }

        public override Vector2 calculateHistoryForce(V vertex)
        {
            return Vector2.zero;
        }

        public override Vector2 calculateForceToCenter(V vertex)
        {
            Vector2 dir = positions[vertex].position;
            return -0.5f * c1 * 1.0f/dir.magnitude *dir;
        }

        public override void calculateNewPositions()
        {
            foreach (V vertex in graph.GetVertices())
            {
                Vector2 newPos;
                if (displacements[vertex].magnitude == 0.0f)
                {
                    newPos = positions[vertex].position;
                }
                else
                {
                    newPos = positions[vertex].position +
                                     (displacements[vertex] / displacements[vertex].magnitude) *
                                     (float)Math.Min(displacements[vertex].magnitude, temp);
                    if (newPos.magnitude > radius)
                    {
                        newPos = newPos / newPos.magnitude * radius;
                    }
                }

                positions[vertex] = new VertexPositionData(newPos, newPos);
            }
        }

        public override void coolMovement(int i)
        {
            double t = i / (double) iterations;
            if (i < 1.0 / 3.0 * iterations)
            {
                temp = startTemp * (1 - 3.0 / 2.0 * t);
            }
            else if (i < 2.0 / 3.0 * iterations)
            {
                temp = startTemp * (0.5 - 3.0 / 4.0 * (t - 1.0 / 3.0));
            }
            else
            {
                temp = startTemp / 4.0;
            }
        }
    }
}