using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicGraphAlgoImplementation.PositionInitializer
{
    public class GridInitializer<V> : PositionInitializer<V>
    where V:GraphVertex
    {
        private readonly double maxRadius;

        public GridInitializer(double maxRadius)
        {
            this.maxRadius = maxRadius;
        }

        public void initializePosition(IEnumerable<V> vertexList)
        {
            int vertexCount = vertexList.Count();
            int verticesPerLine = (int) Math.Sqrt((double) vertexCount) + 1;
            double oneDdistance = maxRadius / (double) verticesPerLine;

            int i = 0, row = 0, col = 0;
            foreach (V v in vertexList)
            {
                double x = -0.5 * maxRadius + oneDdistance * col;
                double z = 0.5 * maxRadius + oneDdistance * row;
                v.setPosition(new Vector3((float) x, 0.0f, (float) z));

                col++;
                if (col == verticesPerLine)
                {
                    row++;
                    col = 0;
                }
            }
        }
    }
}