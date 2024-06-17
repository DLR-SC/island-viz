
using Unity.Mathematics;

namespace IslandEntities.Construction
{
    /// <summary>
    /// Contains the position data used for island region hex tiles.
    /// </summary>
    public struct HexPosition
    {
        // Fixed Hex placement. Only change this when you changed the Hex meshes.
        private static readonly float _hexYOffset = 0.433f;
        private static readonly float _hexXStep = 0.75f;
        private static readonly float _hexYStep = 0.866f;


        public static readonly HexPosition[] NeighborsOdd = new HexPosition[]
        {
            new HexPosition (0,1), // N
            new HexPosition (1,0), // NE
            new HexPosition (1,-1), // SE
            new HexPosition (0,-1), // S
            new HexPosition (-1,-1), // SW
            new HexPosition (-1,0), // NW
        };

        public static readonly HexPosition[] NeighborsEven = new HexPosition[]
        {
            new HexPosition (0,1), // N
            new HexPosition (1,1), // NE
            new HexPosition (1,0), // SE
            new HexPosition (0,-1), // S
            new HexPosition (-1,0), // SW
            new HexPosition (-1,1), // NW
        };

        public enum HexDirection
        {
            N, NE, SE, S, SW, NW
        }

        public int x;
        public int y;

        public HexPosition (int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static HexPosition operator + (HexPosition a, HexPosition b)
        {
            return new HexPosition(a.x + b.x, a.y + b.y);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is HexPosition hexPos))
                return false;

            return x == hexPos.x && y == hexPos.y;
        }

        public static HexPosition[] GetNeighbors(HexPosition position) // TODO
        {
            HexPosition[] positions = new HexPosition[4];
            positions[0] = position + new HexPosition(0, 1);
            positions[1] = position + new HexPosition(0, -1);
            positions[2] = position + new HexPosition(1, 0);
            positions[3] = position + new HexPosition(-1, 0);
            return positions;
        }

        public static UnityEngine.Vector2 ToVector2 (HexPosition hexPosition)
        {
            return new UnityEngine.Vector2(hexPosition.x, hexPosition.y);
        }


        public float3 ToLocalWorldPosition ()
        {
            return new float3(x * _hexXStep, 0, (x % 2 == 0 ? _hexYOffset : 0f) + y * _hexYStep);
        }

        public float2 ToLocalWorldPosition2D ()
        {
            return new float2(x * _hexXStep, (x % 2 == 0 ? _hexYOffset : 0f) + y * _hexYStep);
        }

        public float2 AsFloat2 ()
        {
            return new float2(x, y);
        }
    }
}
