using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickGraph;
using OsgiViz.Core;


namespace OsgiViz.Relations
{
    public class GraphEdge : Edge<GraphVertex>
    {
        private float weight;

        public GraphEdge(GraphVertex source, GraphVertex target)
            : base(source, target)
        {
            weight = 1f;
        }

        public float getWeight()
        {
            return weight;
        }

        public void incrementWeight(float i)
        {
            weight += i;
        }


    }
}


