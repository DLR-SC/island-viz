using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;

namespace HexLayout.Basics
{
    public static class HexHelper
    {

        private static float b_2 = Constants.hexCellRadius * Mathf.Sin(30.0f / 180.0f * Mathf.PI);
        private static float h_2 = Constants.hexCellRadius * Mathf.Cos(30.0f / 180.0f * Mathf.PI);

        public static float[] GetCenterCoordinates(int xGrid, int zGrid)
        {
            float xKCenter, zKCenter;

            if (xGrid % 2 == 0)
            {
                xKCenter = (Constants.hexCellRadius + b_2) * xGrid;
                zKCenter = h_2 * 2 * zGrid;
            }
            else
            {
                zKCenter = h_2 + h_2 * 2 * zGrid;
                if (xGrid >= 1)
                {
                    xKCenter = Constants.hexCellRadius + b_2 + (Constants.hexCellRadius + b_2) * (xGrid - 1);
                }
                else
                {
                    xKCenter = -1.0f * (Constants.hexCellRadius + b_2) + (Constants.hexCellRadius + b_2) * (xGrid + 1);
                }
            }
            return new float[] { xKCenter, zKCenter };
        }


        public static Vector3[] CreateHexagonVertices(int xGrid, int zGrid, float height)
        {
            float[] centerK = GetCenterCoordinates(xGrid, zGrid);
            float xKCenter = centerK[0];
            float zKCenter = centerK[1];

            Vector3[] vertices = new Vector3[6]
            {
            new Vector3(b_2 + xKCenter, height, h_2 + zKCenter),
            new Vector3(-b_2 + xKCenter, height, h_2 + zKCenter),
            new Vector3(-Constants.hexCellRadius + xKCenter, height, 0f + zKCenter),
            new Vector3(-b_2 + xKCenter, height, -h_2 + zKCenter),
            new Vector3(b_2 + xKCenter, height, -h_2 + zKCenter),
            new Vector3(Constants.hexCellRadius + xKCenter, height, 0f + zKCenter)
            };
            /*List<Vector3> vertices = new List<Vector3>();
            vertices.Add(new Vector3(b_2 + xKCenter, 0f, h_2 + zKCenter));
            vertices.Add(new Vector3(-b_2 + xKCenter, 0f, h_2 + zKCenter));
            vertices.Add(new Vector3(-radius + xKCenter, 0f, 0f + zKCenter));
            vertices.Add(new Vector3(-b_2 + xKCenter, 0f, -h_2 + zKCenter));
            vertices.Add(new Vector3(b_2 + xKCenter, 0f, -h_2 + zKCenter));
            vertices.Add(new Vector3(radius + xKCenter, 0f, 0f + zKCenter));*/

            return vertices;
        }

        public static int[] CreateHexagonTriangles(int hexagonIndex)
        {
            //Index of first Vertex in Array
            int i = 6 * hexagonIndex;
            int[] triangles = new int[12]{
            i,i+2,i+1,
            i,i+3,i+2,
            i,i+4,i+3,
            i,i+5,i+4
        };
            return triangles;
        }

        public static Vector3[] CreateHexagonNormals()
        {
            return new Vector3[6]
            {
            -Vector3.up,
            -Vector3.up,
            -Vector3.up,
            -Vector3.up,
            -Vector3.up,
            -Vector3.up,
            };
        }

        public static int[] ConvertCicularToGridCoordinate(int ring, int place)
        {
            if (ring == 0)
            {
                return new int[] { 0, 0 };
            }

            int itemsOfRing = ring * 6;
            //nullte Diagonale
            if (place == 0)
            {
                return new int[] { 0, ring };
            }
            //vor erster Diagonale
            else if (place < ring)
            {
                int z = ring - ((place - 1) / 2 + 1);
                return new int[] { place, z };
            }
            //erste Diagonale
            else if (place == ring)
            {
                return new int[] { ring, ring / 2 };
            }
            //vor zweiter Diagonale (Senkrecht Runter)
            else if (place < itemsOfRing / 3)
            {
                int fracPlace = place - itemsOfRing / 6;
                int z = ring / 2 - fracPlace;
                return new int[] { ring, z };
            }
            //zweite Diagonale
            else if (place == itemsOfRing / 3)
            {
                return new int[] { ring, -1 * ((ring - 1) / 2 + 1) };
            }
            //vor dritter Diagonale
            else if (place < itemsOfRing / 2)
            {
                int fracPlace = place - itemsOfRing / 3;

                int z = -1 * ((ring - 1) / 2 + 1);
                if (ring % 2 == 0)
                {
                    z = z - (fracPlace + 1) / 2;
                }
                else
                {
                    z = z - fracPlace / 2;
                }
                return new int[] { ring - fracPlace, z };
            }
            //dritte Diagonale (negative Z-Achse)
            else if (place == itemsOfRing / 2)
            {
                return new int[] { 0, -1 * ring };
            }
            //vor vierter Diagonale
            else if (place < itemsOfRing / 3 * 2)
            {
                int fracPlace = place - itemsOfRing / 2;
                int z = -ring + fracPlace / 2;
                return new int[] { -1 * fracPlace, z };
            }
            //vierte Diagonlae
            else if (place == itemsOfRing / 3 * 2)
            {
                return new int[] { -1 * ring, -1 * ((ring - 1) / 2 + 1) };
            }
            //vor fünfter Diagonale (Senkrecht rauf)
            else if (place < itemsOfRing / 6 * 5)
            {
                int fracPlace = place - itemsOfRing / 3 * 2;
                int z = -1 * ((ring - 1) / 2 + 1) + fracPlace;
                return new int[] { -1 * ring, z };
            }
            //Fünfte Diagonale
            else if (place == itemsOfRing / 6 * 5)
            {
                return new int[] { -1 * ring, (ring / 2) };
            }
            //vor nullterDiagonale
            else
            {
                int fracPlace = place - itemsOfRing / 6 * 5;
                int z = (ring / 2);
                if (ring % 2 == 0)
                {
                    z = z + fracPlace / 2;
                }
                else
                {
                    z = z + (fracPlace + 1) / 2;
                }
                return new int[] { -1 * ring + fracPlace, z };

            }
        }

        public static int[] ConvertGridToCircularCoordinate(int x, int z)
        {
            if (x == 0)
            {
                int ring = Mathf.Abs(z);
                int itemsOfRing = ring * 6;
                int place;
                if (z >= 0)
                {
                    place = 0;
                }
                else
                {
                    place = itemsOfRing / 2;
                }
                return new int[] { ring, place };
            }
            else
            {
                Vector2Int verticalRange = GetMinMaxZCoordinateOfVerticalSixth(x);
                int ring, place;
                if (verticalRange.x <= z && z <= verticalRange.y)
                {
                    ring = Mathf.Abs(x);
                    if (x > 0)
                    {
                        int dif = verticalRange.y - z;
                        place = ring + dif;
                    }
                    else
                    {
                        int dif = z - verticalRange.x;
                        place = 4 * ring + dif;
                    }
                }
                else if (z > verticalRange.y && x > 0)
                {
                    ring = x + (z - verticalRange.y);
                    place = x;
                }
                else if (z > verticalRange.y && x < 0)
                {
                    ring = Mathf.Abs(x) + (z - verticalRange.y);
                    place = ring * 6 + x;
                }
                else if (z < verticalRange.x && x > 0)
                {
                    ring = x + verticalRange.x - z;
                    place = ring * 3 - x;
                }
                else
                {
                    ring = Mathf.Abs(x) + verticalRange.x - z;
                    place = ring * 3 + Mathf.Abs(x);
                }
                return new int[] { ring, place };
            }
        }

        public static Vector2Int GetCircularOuterNeigbour(int ring, int place)
        {
            int[] sixthInfo = GetSixthInRingAndOffset(ring, place);

            return new Vector2Int(ring + 1, place + sixthInfo[0] + 1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ring"></param>
        /// <param name="place"></param>
        /// <returns>[Which sixth 0..5 of the ring],[which place in the sixth 0..ring-1 (0=maindiagonal)]</returns>
        public static int[] GetSixthInRingAndOffset(int ring, int place)
        {
            if (ring == 0)
            {
                return new int[] { 0, 0 };
            }
            return new int[] { place / ring, Modulo(place, ring) };
        }

        public static Vector3 GetNormalForSide(int v1, int v2)
        {
            if (v1 == 0 && v2 == 1)
            {
                //obere Seite Hexagon
                return Vector3.back;
            }
            else if (v1 == 1 && v2 == 2)
            {
                //oben links
                return new Vector3(Mathf.Cos(Mathf.PI / 3f), 0f, -Mathf.Sin(Mathf.PI / 3f));

            }
            else if (v1 == 2 && v2 == 3)
            {
                //unten links
                return new Vector3(Mathf.Cos(Mathf.PI / 3f), 0f, Mathf.Sin(Mathf.PI / 3f));

            }
            else if (v1 == 3 && v2 == 4)
            {
                //untere Seite Hexagon
                return Vector3.forward;
            }
            else if (v1 == 4 && v2 == 5)
            {
                //unten rechts
                return new Vector3(-Mathf.Cos(Mathf.PI / 3f), 0f, Mathf.Sin(Mathf.PI / 3f));

            }
            else if (v1 == 5 && v2 == 0)
            {
                //oben Rechts
                return new Vector3(-Mathf.Cos(Mathf.PI / 3f), 0f, -Mathf.Sin(Mathf.PI / 3f));
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Für umrechnung Grid -> Circle
        /// Die Verticalen Kanten des Sechsecks(zwischen ersten-zweiter und vierter-fünfter Hauptdiagonale) gehen von Minz bis Max z
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static Vector2Int GetMinMaxZCoordinateOfVerticalSixth(int x)
        {
            if (x % 2 == 0)
            {
                return new Vector2Int(-1 * Mathf.Abs(x) / 2, Mathf.Abs(x) / 2);
            }
            else
            {
                return new Vector2Int(-1 * ((Mathf.Abs(x) - 1) / 2 + 1), (Mathf.Abs(x) - 1) / 2);
            }
        }

        public static int Modulo(int dividend, int baseValue)
        {
            if(baseValue == 0)
            {
                return 0;
            }
            return ((dividend % baseValue) + baseValue) % baseValue;
        }
    }
}