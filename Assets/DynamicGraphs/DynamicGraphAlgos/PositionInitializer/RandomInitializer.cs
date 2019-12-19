using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace DynamicGraphAlgoImplementation.PositionInitializer
{
    public class RandomInitializer<V> : PositionInitializer<V>
        where V : GraphVertex

    {
    private readonly double maxRadius;
    private System.Random rand = new Random();

    public RandomInitializer(double maxRadius)
    {
        this.maxRadius = maxRadius;
    }

    public void initializePosition(IEnumerable<V> vertexList)
    {
        foreach (V v in vertexList)
        {
            double r = maxRadius * rand.NextDouble();
            double phi = 2 * Math.PI * rand.NextDouble();

            v.setPosition(new Vector3((float) (Math.Sin(phi) * r), 0.0f, (float) (Math.Cos(phi) * r)));
        }
    }

    }
}