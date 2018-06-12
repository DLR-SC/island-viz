using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Voronoi;
using TriangleNet.Meshing.Algorithm;
using OsgiViz.Core;

using VFace = TriangleNet.Topology.DCEL.Face;
using VHEdge = TriangleNet.Topology.DCEL.HalfEdge;
using TnetMesh = TriangleNet.Mesh;

namespace OsgiViz
{
    public class Helperfunctions : MonoBehaviour {



        public static GameObject createPlane(float widthScale, float heightScale, int widthSegments, int heightSegments, Vector3 position)
        {
            GameObject plane = new GameObject();
            plane.transform.position = position;
            plane.name = "Plane";

            MeshFilter meshFilter = plane.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = plane.AddComponent<MeshRenderer>();

            Mesh m = new Mesh();
            m.name = plane.name + "_Mesh";

            int hCount2 = widthSegments + 1;
            int vCount2 = heightSegments + 1;
            int numTriangles = widthSegments * heightSegments * 6;
            int numVertices = hCount2 * vCount2;

            Vector3[] vertices = new Vector3[numVertices];
            Vector2[] uvs = new Vector2[numVertices];
            int[] triangles = new int[numTriangles];
            Vector4[] tangents = new Vector4[numVertices];
            Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

            int index = 0;
            float uvFactorX = 1.0f / widthSegments;
            float uvFactorY = 1.0f / heightSegments;
            float scaleX = widthScale / widthSegments;
            float scaleY = heightScale / heightSegments;

            for (float y = 0.0f; y < vCount2; y++)
            {
                for (float x = 0.0f; x < hCount2; x++)
                {

                    vertices[index] = new Vector3(x * scaleX - widthScale / 2f, 0.0f, y * scaleY - heightScale / 2f);
                    tangents[index] = tangent;
                    uvs[index++] = new Vector2(x * uvFactorX, y * uvFactorY);
                }
            }

            index = 0;
            for (int y = 0; y < heightSegments; y++)
            {
                for (int x = 0; x < widthSegments; x++)
                {
                    triangles[index] = (y * hCount2) + x;
                    triangles[index + 1] = ((y + 1) * hCount2) + x;
                    triangles[index + 2] = (y * hCount2) + x + 1;

                    triangles[index + 3] = ((y + 1) * hCount2) + x;
                    triangles[index + 4] = ((y + 1) * hCount2) + x + 1;
                    triangles[index + 5] = (y * hCount2) + x + 1;
                    index += 6;
                }
            }

            m.vertices = vertices;
            m.uv = uvs;
            m.triangles = triangles;
            m.tangents = tangents;
            m.RecalculateNormals();

            meshFilter.sharedMesh = m;
            m.RecalculateBounds();
            meshRenderer.material = new Material(Shader.Find("Diffuse"));

            return plane;
        }

        public static List<Vertex> createPointsOnPlane(float widthScale, float heightScale, int widthSegments, int heightSegments, float preturbFactor, System.Random RNG)
        {

            float preturbFactorX = preturbFactor * (widthScale / widthSegments);
            float preturbFactorY = preturbFactor * (heightScale / heightSegments);
            int hCount2 = widthSegments + 1;
            int vCount2 = heightSegments + 1;
            int numTriangles = widthSegments * heightSegments * 6;
            int numVertices = hCount2 * vCount2;

            List<Vertex> vertices = new List<Vertex>();

            int index = 0;
            float uvFactorX = 1.0f / widthSegments;
            float uvFactorY = 1.0f / heightSegments;
            float scaleX = widthScale / widthSegments;
            float scaleY = heightScale / heightSegments;

            for (int y = 0; y < vCount2; y++)
            {
                for (int x = 0; x < hCount2; x++)
                {

                    Vertex newVert = new Vertex(x * scaleX - widthScale / 2f, y * scaleY - heightScale / 2f);
                    newVert.x += (float)RNG.NextDouble() * preturbFactorX;
                    newVert.y += (float)RNG.NextDouble() * preturbFactorY;
                    vertices.Add(newVert);
                }
            }

            return vertices;
        }


        public static List<Vertex> createHexagonalGridPoints(float widthScale, float heightScale, int widthSegments, int heightSegments)
        {

            int hCount2 = widthSegments + 1;
            int vCount2 = heightSegments + 1;
            int numTriangles = widthSegments * heightSegments * 6;
            int numVertices = hCount2 * vCount2;

            List<Vertex> vertices = new List<Vertex>();

            float scaleX = widthScale / widthSegments;
            float scaleY = heightScale / heightSegments;

            for (int y = 0; y < vCount2; y++)
            {
                for (int x = 0; x < hCount2; x++)
                {

                    Vertex newVert = new Vertex((x + (y % 2) * 0.5f) * scaleX - widthScale / 2f, y * scaleY - heightScale / 2f);
                    vertices.Add(newVert);
                }
            }

            return vertices;
        }


        public static void applyHeightNoise(Mesh mesh)
        {
            List<Vector3> oldVerts = new List<Vector3>();
            List<Vector3> newVerts = new List<Vector3>();
            mesh.GetVertices(oldVerts);
            for (int i = 0; i < oldVerts.Count; i++)
            {
                float height = 0.5f - Mathf.Sqrt(oldVerts[i].x * oldVerts[i].x + oldVerts[i].z * oldVerts[i].z);
                newVerts.Add(new Vector3(oldVerts[i].x, height, oldVerts[i].z));
            }
            mesh.SetVertices(newVerts);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        public static Mesh convertTriangleNETMesh(TriangleNet.Meshing.IMesh tMesh)
        {
            List<Vector3> outVertices = new List<Vector3>();
            List<int> outIndices = new List<int>();

            foreach (ITriangle t in tMesh.Triangles)
            {
                for (int j = 2; j >= 0; j--)
                {
                    bool found = false;
                    for (int k = 0; k < outVertices.Count; k++)
                    {
                        if ((outVertices[k].x == t.GetVertex(j).x) && (outVertices[k].z == t.GetVertex(j).y) && (outVertices[k].y == t.GetVertex(j).z))
                        {
                            outIndices.Add(k);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        outVertices.Add(new Vector3((float)t.GetVertex(j).x, (float)t.GetVertex(j).z, (float)t.GetVertex(j).y));
                        outIndices.Add(outVertices.Count - 1);
                    }
                }
            }
            List<Vector2> uvs = new List<Vector2>();
            for (int i = 0; i < outVertices.Count; i++)
                uvs.Add(new Vector2(outVertices[i].x, outVertices[i].z));

            Mesh resultMesh = new Mesh();
            resultMesh.SetVertices(outVertices);
            resultMesh.SetTriangles(outIndices, 0);
            resultMesh.SetUVs(0, uvs);
            resultMesh.RecalculateBounds();
            resultMesh.RecalculateNormals();

            return resultMesh;
        }

        //TODO: Unbounded problem could manifest itself here
        public static VFace closestCell(float x, float y, TriangleNet.Voronoi.BoundedVoronoi voronoi)
        {
            int startingIndex = voronoi.Faces.Count / 2;
            VFace currentCell = voronoi.Faces[startingIndex];
            float currentDistance = Mathf.Sqrt((float)(currentCell.generator.X - x) * (float)(currentCell.generator.X - x) + (float)(currentCell.generator.Y - y) * (float)(currentCell.generator.Y - y));

            while (true)
            {
                TriangleNet.Topology.DCEL.Face nextCell = null;
                bool foundNeighbour = false;
                foreach (TriangleNet.Topology.DCEL.HalfEdge edge in currentCell.EnumerateEdges())
                {
                    VFace nCell = edge.twin.face;
                    float neighbourX = (float)nCell.generator.X;
                    float neighbourY = (float)nCell.generator.Y;
                    float distanceFromNeighbour = Mathf.Sqrt((neighbourX - x) * (neighbourX - x) + (neighbourY - y) * (neighbourY - y));
                    if (distanceFromNeighbour < currentDistance)
                    {
                        foundNeighbour = true;
                        currentDistance = distanceFromNeighbour;
                        nextCell = nCell;
                    }
                }

                if (!foundNeighbour)
                    break;

                currentCell = nextCell;
            }

            return currentCell;
        }

        public static BoundedVoronoi createRelaxedVoronoi(List<Vertex> startingVertices, int numLloydRelaxations)
        {
            TriangleNet.Configuration conf = new TriangleNet.Configuration();
            SweepLine sl = new SweepLine();
            List<Vertex> vertices = new List<Vertex>();

            TnetMesh tMesh = (TnetMesh)sl.Triangulate(startingVertices, conf);
            BoundedVoronoi voronoi = new BoundedVoronoi(tMesh);

            for (int i = 0; i < numLloydRelaxations; i++)
            {
                foreach (VFace face in voronoi.Faces)
                {
                    if (checkCell(face))
                    {
                        Vertex newCentroid = new Vertex(0, 0);
                        int vertexCount = 0;
                        foreach (VHEdge edge in face.EnumerateEdges())
                        {
                            newCentroid.X += edge.Origin.X;
                            newCentroid.Y += edge.Origin.Y;
                            vertexCount++;
                        }
                        newCentroid.X /= vertexCount;
                        newCentroid.Y /= vertexCount;
                        vertices.Add(newCentroid);
                    }
                }

                tMesh = (TnetMesh)sl.Triangulate(vertices, conf);
                voronoi = new BoundedVoronoi(tMesh);
                vertices.Clear();
            }

            return voronoi;
        }


        //Rejects unbounded cells and "infinite" cells
        public static bool checkCell(VFace cell)
        {
            if (cell.Bounded && cell.ID != -1)
                return true;
            else
                return false;
        }

        //Rejects unbounded cells, "infinite" cells and cells outside a certain boundary
        public static bool checkCell(VFace cell, float minX, float maxX, float minY, float maxY)
        {
            if (cell.Bounded && cell.ID != -1 &&
                cell.generator.X > minX && cell.generator.x < maxX &&
                cell.generator.Y > minY && cell.generator.Y < maxY)
                return true;
            else
                return false;
        }

        //Change with different Metric
        public static int mapLOCtoLevel(long loc)
        {

            float mappingValue = Mathf.Sqrt((float)loc);
            float maxLOCMapped = Mathf.Sqrt((float)GlobalVar.maximumLOCinProject);
            float segment = maxLOCMapped / GlobalVar.numLocLevels;
            int level = Mathf.Min(Mathf.FloorToInt(mappingValue / segment), GlobalVar.numLocLevels - 1);

            return level;
        }

        //Change with different Metric
        public static float mapDependencycountToSize(long count)
        {
            if (count == 0)
                return GlobalVar.minDockSize;
            else
                return GlobalVar.minDockSize + (Mathf.Sqrt((float)count) * GlobalVar.dockScaleMult);
        }

        public static float computeMax(List<float> list)
        {
            float max = Mathf.NegativeInfinity;
            if (list.Count == 0)
                return 0;
            foreach (float value in list)
                if (value > max)
                    max = value;

            return max;
        }

        public static long computeMax(List<long> list)
        {
            long max = long.MinValue;
            if (list.Count == 0)
                return 0;
            foreach (long value in list)
                if (value > max)
                    max = value;

            return max;
        }

        public static void scaleFromPivot(Transform inputTransform, Vector3 pivot, Vector3 scale)
        {
            Vector3 diff = inputTransform.position - pivot;
            Vector3 newPos = Vector3.Scale(diff, scale);
            newPos += pivot;

            inputTransform.localScale = Vector3.Scale(scale, inputTransform.localScale);
            inputTransform.position = newPos;
        }

        //Replaces aTan2(), less precise, but faster
        //http://www.freesteel.co.uk/wpblog/2009/06/05/encoding-2d-angles-without-trigonometry/
        public static float DiamondAngle(float x, float y)
        {
            if (y >= 0)
                return (x >= 0 ? y / (x + y) : 1 - x / (-x + y));
            else
                return (x < 0 ? 2 - y / (-x - y) : 3 + x / (x - y));
        }
    }

}