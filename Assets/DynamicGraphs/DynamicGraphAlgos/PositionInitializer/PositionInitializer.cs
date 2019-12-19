using System.Collections.Generic;

namespace DynamicGraphAlgoImplementation.PositionInitializer
{
    public interface PositionInitializer<V>
    where V:GraphVertex
    {
        void initializePosition(IEnumerable<V> vertexList);
    }
}