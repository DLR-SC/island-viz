using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBasics;
//using OsgiViz.Core;

namespace DynamicGraphAlgoImplementation
{
    public class GraphEdge : DirectedEdge<GraphVertex>, IWeightedEdge
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






    
