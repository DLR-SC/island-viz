using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphBasics
{
    public class DirectedEdge<V>
    {

        V source;
        V target;

        public DirectedEdge(V s, V t)
        {
            source = s;
            target = t;
        }

        public V GetSource()
        {
            return source;
        }

        public V GetTarget()
        {
            return target;
        }
    }
}

