using System;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicGraphAlgoImplementation.PositionInitializer
{
    public class HistoryInitialiser<V> : PositionInitializer<V>
    where V:HistoryGraphVertex
    {

        private PositionInitializer<V> newVerticesInitialiser;

        public HistoryInitialiser(double maxRadius, string type)
        {
            switch (type)
            {
                case "circle":
                    newVerticesInitialiser = new CircleInitializer<V>(maxRadius);
                    break;
                case "grid":
                    newVerticesInitialiser = new GridInitializer<V>(maxRadius);
                    break;
                default:
                    newVerticesInitialiser = new RandomInitializer<V>(maxRadius);
                    break;
            }
        }
        
      /*  public void initializePosition(List<V> vertexList)
        {
            newVerticesInitialiser.initializePosition(vertexList);
        }*/
        
        public void initializePosition(IEnumerable<V> vertexList)
        {
            List<V> verticesWithOutPrevious = new List<V>();
            foreach (V v in vertexList)
            {
                if (v.getPrevious() != null)
                {
                    HistoryGraphVertex vPrev = v.getPrevious();
                    v.setPosition(new Vector3(vPrev.getPosition().x, vPrev.getPosition().y, vPrev.getPosition().z));
                }
                else
                {
                    verticesWithOutPrevious.Add(v);
                }    
            }
            newVerticesInitialiser.initializePosition(verticesWithOutPrevious);
        }
    }
}