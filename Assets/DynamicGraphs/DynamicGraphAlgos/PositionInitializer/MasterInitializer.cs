using System.Collections.Generic;
using UnityEngine;

namespace DynamicGraphAlgoImplementation.PositionInitializer
{
    public class MasterInitializer<V> : PositionInitializer<V>
    where V: HistoryGraphVertex
    {
        public void initializePosition(IEnumerable<V> vertexList)
        {
            foreach (V v in vertexList)
            {
                MasterVertex m = v.getMaster();
                v.setPosition(new Vector3(m.getPosition().x, m.getPosition().y, m.getPosition().z));
            }
        }
    }
}