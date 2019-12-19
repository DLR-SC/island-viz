using System;
using System.Collections.Generic;
using System.Linq;
using DynamicGraphAlgoImplementation.Layouter;
using GraphBasics;
using UnityEngine;

namespace DynamicGraphAlgoImplementation.NewLayouter
{
    /// <summary>
    /// Force Directed Graph Layout for Weighted Graph (Vertices & Edges weighted) according to Collberg 2003 A System for Graph-Based Visualisation Of The Evolution Of Software
    /// </summary>
    /// <typeparam name="V"></typeparam>
    /// <typeparam name="E"></typeparam>
    public class WeightedGraphForceDirectedLayouter<V, E> : AbstractForceDirectedLayouter<V, E>
        where V : GraphVertex, IWeightedVertex
        where E : DirectedEdge<V>, IWeightedEdge
    {
        private float c1;
        private float c3;
        private float c4;
        private double startTemp;
        private double temp;


        public WeightedGraphForceDirectedLayouter(float r, float c1, float c3, double startTempFrac,
            int iterations) : base(r, iterations)
        {
            this.c1 = c1;
            this.c3 = c3;
            this.startTemp = radius * startTempFrac;
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
                
                float springEquilibriumLength =
                    c3 * Mathf.Sqrt(vertex.GetWeight() * otherVertex.GetWeight()) / importEdge.getWeight();

                Vector2 direction = positions[otherVertex].position - positions[vertex].position;

                attraction += c1 * importEdge.getWeight() * Mathf.Pow(direction.magnitude, 2.0f) *
                              direction.normalized / Mathf.Pow(springEquilibriumLength, 2.0f);
            }

            return attraction;
        }

        public override Vector2 calculateRepulsion(V vertex)
        {
            Vector2 repulsion = Vector2.zero;
            foreach (V v in graph.GetVertices())
            {
                if (vertex != v && !(graph.ContainsEdge(vertex, v) || graph.ContainsEdge(v, vertex)))
                {
                    Vector2 direction = positions[v].position - positions[vertex].position;
                    repulsion -= c4 *Mathf.Sqrt(vertex.GetWeight()*v.GetWeight())/ Mathf.Pow(direction.magnitude, 2) * direction.normalized;
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
            return Vector2.zero;
        }

        public override void calculateNewPositions()
        {
            foreach (V vertex in graph.GetVertices())
            {
                Vector2 newPos = positions[vertex].position +
                                 (displacements[vertex] / displacements[vertex].magnitude) *
                                 (float) Math.Min(displacements[vertex].magnitude, temp);
                if (newPos.magnitude > radius)
                {
                    newPos = newPos / newPos.magnitude * radius;
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