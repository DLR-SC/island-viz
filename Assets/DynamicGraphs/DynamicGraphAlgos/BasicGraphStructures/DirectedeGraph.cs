using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphBasics
{
    public class DirectedGraph<V, E>
    where E : DirectedEdge<V>
    {
        List<V> vertexList;
        List<E> edgeList;
        Dictionary<V, List<E>> incommingEdges;
        Dictionary<V, List<E>> outgoingEdges;

        public DirectedGraph()
        {
            vertexList = new List<V>();
            edgeList = new List<E>();
            incommingEdges = new Dictionary<V, List<E>>();
            outgoingEdges = new Dictionary<V, List<E>>();
        }

        public void AddVertex(V vertex)
        {
            if (!vertexList.Contains(vertex))
            {
                vertexList.Add(vertex);
                incommingEdges.Add(vertex, new List<E>());
                outgoingEdges.Add(vertex, new List<E>());
            }
        }

        public void AddEdge(E edge)
        {
            V source = edge.GetSource();
            V target = edge.GetTarget();

            AddVertex(source);
            AddVertex(target);

            outgoingEdges[source].Add(edge);
            incommingEdges[target].Add(edge);
            edgeList.Add(edge);
        }

        public bool ContainsVertex(V vertex)
        {
            return vertexList.Contains(vertex);
        }

        public List<V> GetVertices()
        {
            return vertexList;
        }
        public bool ContainsEdge(E edge)
        {
            return edgeList.Contains(edge);
        }
        public bool ContainsEdge(V source, V target)
        {
            if (!outgoingEdges.ContainsKey(source))
            {
                return false;
            }
            if (!incommingEdges.ContainsKey(target))
            {
                return false;
            }
            foreach(E edge in outgoingEdges[source])
            {
                if (edge.GetTarget().Equals(target))
                {
                    return true;
                }
            }
            return false;
        }

        public int VertexCount()
        {
            return vertexList.Count;
        }
        public int EdgeCount()
        {
            return edgeList.Count;
        }

        public List<E> GetEdges()
        {
            return edgeList;
        }
        public List<E> GetIncomingEdges(V vertex)
        {
            return incommingEdges[vertex];
        }
        public void TryGetInEdges(V vertex, out IEnumerable<E> edgeList)
        {
            if (incommingEdges.ContainsKey(vertex))
            {
                edgeList = incommingEdges[vertex];
            }
            else
            {
                edgeList = new List<E>();
            }
        }
        public List<E> GetOutgoingEdges(V vertex)
        {
            return outgoingEdges[vertex];
        }
        public void TryGetOutEdges(V vertex, out IEnumerable<E> edgeList)
        {
            if (outgoingEdges.ContainsKey(vertex))
            {
                edgeList = outgoingEdges[vertex];
            }
            else
            {
                edgeList = new List<E>();
            }
        }
        public List<E> GetEdges(V vertex)
        {
            List<E> res = null;
            if (incommingEdges.ContainsKey(vertex))
            {
                res.AddRange(GetIncomingEdges(vertex));
            }
            if (outgoingEdges.ContainsKey(vertex))
            {
                res.AddRange(GetOutgoingEdges(vertex));
            }
            if (res.Count == 0)
            {
                return null;
            }
            return res;
        }
        public void TryGetEdge(V source, V target, out E edge)
        {
            if (!outgoingEdges.ContainsKey(source))
            {
                edge = null;
                return;
            }
            if (!incommingEdges.ContainsKey(target))
            {
                edge = null;
                return;
            }
            foreach(E e in outgoingEdges[source])
            {
                if (e.GetTarget().Equals(target))
                {
                    edge = e;
                    return;
                }
            }
            edge = null;
        }

        public int GetOutDegree(V vertex)
        {
            if (outgoingEdges.ContainsKey(vertex))
            {
                return GetOutgoingEdges(vertex).Count;
            }
            return 0;
        }
        public int GetInDegree(V vertex)
        {
            if (incommingEdges.ContainsKey(vertex))
            {
                return GetIncomingEdges(vertex).Count;
            }
            return 0;
        }
        public int GetDegree(V vertex)
        {
            int res = 0;
            if (outgoingEdges.ContainsKey(vertex))
            {
                res += GetOutgoingEdges(vertex).Count;
            }
            if (incommingEdges.ContainsKey(vertex))
            {
                res += GetIncomingEdges(vertex).Count;
            }
            return res;
        }
    }
}