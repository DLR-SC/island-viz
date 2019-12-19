using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DynamicGraphAlgoImplementation.Layouter;
using DynamicGraphAlgoImplementation.PositionInitializer;
using GraphBasics;
using UnityEngine;

namespace DynamicGraphAlgoImplementation.NewLayouter
{
    public abstract class AbstractForceDirectedLayouter<V, E>
    where V: GraphVertex
    where E: DirectedEdge<V>
    {
        protected PositionInitializer<V> PositionInitializer;
        
        protected DirectedGraph<V, E> graph;
        protected Dictionary<V, VertexPositionData> positions;
        protected Dictionary<V, Vector2> displacements;

        protected float radius;
        protected int iterations;

        protected AbstractForceDirectedLayouter(float radius, int iterations)
        {
            this.radius = radius;
            this.iterations = iterations;
        }
        
        public IEnumerator layout()
        {
            //Initialise
            initialise();
            positions = new Dictionary<V, VertexPositionData>();
            displacements = new Dictionary<V, Vector2>();
            
            foreach (V v in graph.GetVertices())
            {
                positions.Add(v, new VertexPositionData(new Vector2(v.getPosition().x, v.getPosition().z), new Vector2(v.getPosition().x, v.getPosition().z))); 
                displacements.Add(v, Vector2.zero);
            }
            yield return null;

            for (int i = 0; i < iterations; i++)
            {
                foreach (V v in graph.GetVertices())
                {
                    Vector2 disp = Vector2.zero;
                    disp += calculateAttraction(v);
                    disp += calculateRepulsion(v);
                    disp += calculateHistoryForce(v);
                    disp += calculateForceToCenter(v);
                    displacements[v] = disp;
                    /*displacements[v] += calculateAttraction(v);
                    displacements[v] += calculateRepulsion(v);
                    displacements[v] += calculateHistoryForce(v);
                    displacements[v] += calculateForceToCenter(v);*/
                }

                calculateNewPositions();
                coolMovement(i);
                if (i % 20 == 0)
                {
                    yield return null;
                }
            }
            CheckOverlaps();
            yield return null;
            foreach (V v in graph.GetVertices())
            {
                if(float.IsNaN(positions[v].position.x)|| float.IsNaN(positions[v].position.y))
                {
                    Debug.LogError("Position IsNaN");
                }
                v.setPosition(new Vector3(positions[v].position.x, 0.0f, positions[v].position.y));
            }
            yield return null;
        }

        public void setGraph(DirectedGraph<V, E> g)
        {
            graph = g;
        }

        public void setInitialiser(PositionInitializer<V> initializer)
        {
            this.PositionInitializer = initializer;
        }
        
        public void setRadius(float r)
        {
            radius = r;
        }

        //public abstract void initialise();

        public virtual void initialise()
        {
            PositionInitializer.initializePosition(graph.GetVertices());
        }
        
        public abstract Vector2 calculateAttraction(V vertex);

        public abstract Vector2 calculateRepulsion(V vertex);

        public abstract Vector2 calculateHistoryForce(V vertex);

        public abstract Vector2 calculateForceToCenter(V vertex);

        public abstract void calculateNewPositions();

        public abstract void coolMovement(int i);

        public void CheckOverlaps()
        {
            List<V> vertexList = graph.GetVertices();
            for(int i = 0; i < vertexList.Count; i++)
            {
                V vertex = vertexList[i];
                for(int j = i+1; j < vertexList.Count; j++)
                {
                    V othervertex = vertexList[j];
                    Vector2 direction = positions[vertex].position - positions[othervertex].position;
                    float dif = direction.magnitude - (vertex.getRadius() + othervertex.getRadius() + 0.5f);
                    if(dif < 0)
                    {
                        Vector2 newPosVertex = positions[vertex].position + 0.5f * Mathf.Abs(dif) * direction.normalized;
                        positions[vertex] = new VertexPositionData(newPosVertex, newPosVertex);

                        Vector2 newPosOthervertex = positions[othervertex].position - 0.5f * Mathf.Abs(dif) * direction.normalized;
                        positions[othervertex] = new VertexPositionData(newPosOthervertex, newPosOthervertex);

                    }
                }
            }
        }
    }
}