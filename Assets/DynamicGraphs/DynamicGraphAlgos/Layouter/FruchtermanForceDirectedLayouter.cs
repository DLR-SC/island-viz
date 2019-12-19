using System;
using System.Collections.Generic;
using System.Linq;
using DynamicGraphAlgoImplementation.Layouter;
using GraphBasics;
using UnityEngine;


namespace DynamicGraphAlgoImplementation.NewLayouter
{
    /// <summary>
    /// Original Fruchterman Algo, mit Parameter k für gleichmäßige Knotenverteilung auf der Fläche
    /// </summary>
    /// <typeparam name="V"></typeparam>
    /// <typeparam name="E"></typeparam>
    public class FruchtermanForceDirectedLayouter<V, E> : AbstractForceDirectedLayouter<V, E>
        where V : GraphVertex
        where E : DirectedEdge<V>
    {
        private double c;
        private double k;
        private double startTemp;
        private double temp;
        private bool useHistory;
        private int relevantAncestors;


        public FruchtermanForceDirectedLayouter(float r, double c, double startTempFrac, int iterations) : base(r,
            iterations)
        {
            this.c = c;
            this.startTemp = radius * startTempFrac;
        }

        public override void initialise()
        {
            base.initialise();
            double area = Math.PI * Math.Pow(radius, 2.0);
            k = c * Math.Sqrt(area / (double) graph.VertexCount());
            this.temp = startTemp;
        }

        public override Vector2 calculateAttraction(V vertex)
        {
            Vector2 att = Vector2.zero;
            IEnumerable<E> edgeList;
            graph.TryGetOutEdges(vertex, out edgeList);
            foreach (E e in edgeList)
            {
                Vector2 delta = positions[vertex].oldPosition - positions[e.GetTarget()].oldPosition;
                att -= (delta / delta.magnitude) * (float) (attraction(delta.magnitude));
            }

            return att;
        }

        public override Vector2 calculateRepulsion(V vertex)
        {
            Vector2 rep = Vector2.zero;
            foreach (V u in graph.GetVertices())
            {
                if (vertex != u)
                {
                    Vector2 delta = positions[vertex].oldPosition - positions[u].oldPosition;
                    rep += (delta / delta.magnitude) * (float) repulsion(delta.magnitude);
                }
            }

            return rep;
        }

        public override Vector2 calculateHistoryForce(V vertex)
        {
            if (useHistory && vertex is HistoryGraphVertex)
            {
                Vector2 hist = Vector2.zero;

                int depth = 1;
                HistoryGraphVertex ancestor = vertex as HistoryGraphVertex;
                while ((ancestor = ancestor.getPrevious()) != null && depth <= relevantAncestors)
                {
                    Vector2 ancestor2DPos = new Vector2(ancestor.getPosition().x, ancestor.getPosition().z);

                    Vector2 direction = ancestor2DPos - positions[vertex].oldPosition;

                    if (direction.magnitude != 0.0f)
                    {
                        hist += (direction / direction.magnitude) * (float) (attraction(direction.magnitude)) /
                                (float) depth;
                    }

                    depth++;
                }

                return hist;
            }
            else
            {
                return Vector2.zero;
            }
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

        private double attraction(double dist)
        {
            return Math.Pow(dist, 2.0) / k;
        }

        private double repulsion(double dist)
        {
            return Math.Pow(k, 2.0) / dist;
        }

        public void setUseHistory(int relAnc)
        {
            useHistory = true;
            this.relevantAncestors = relAnc;
        }

        public void setDontUseHistory()
        {
            useHistory = false;
        }
    }
}