using UnityEngine;

namespace DynamicGraphAlgoImplementation.Layouter
{
    public struct VertexPositionData
    {
        public Vector2 position;
        public Vector2 oldPosition;

        public VertexPositionData(Vector2 pos, Vector2 oldPos)
        {
            position = pos;
            oldPosition = oldPos;
        }
    }
}