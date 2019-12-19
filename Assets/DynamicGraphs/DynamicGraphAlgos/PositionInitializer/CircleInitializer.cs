using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DynamicGraphAlgoImplementation.PositionInitializer
{
    public class CircleInitializer<V> : PositionInitializer<V>
    where V:GraphVertex
    {
        private readonly double maxRadius;
        
        public CircleInitializer(double maxRadius)
        {
            this.maxRadius = maxRadius;
        }
        
        public void initializePosition(IEnumerable<V> vertexList)
        {
            int vertexCount = vertexList.Count();
            double partDegree = 2 * Math.PI / (double) vertexCount;

            int i = 0;
            foreach (V v in vertexList)
            {
                double x = Math.Sin(partDegree * i)* maxRadius / 2.0;
                double z = Math.Cos(partDegree * i) * maxRadius / 2.0;
                
                v.setPosition(new Vector3((float)x, 0.0f, (float)z));

                i++;
            }
        }
        
        
    }
}