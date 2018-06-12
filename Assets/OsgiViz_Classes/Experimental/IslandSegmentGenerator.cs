using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace OsgiViz
{
    public class IslandSegmentGenerator : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        //Modified Tube-generation algorithm from http://wiki.unity3d.com/index.php/ProceduralPrimitives
        //closedTube: If true, ignores startRadian & endRadian and creates a regular closed tube. If false, creates an open tube segment ranging from startRadian to endRadian.
        public static GameObject generateTube(float innerRadiusBottom, float outerRadiusBottom, float innerRadiusTop, float outerRadiusTop, float height,
                                              float startRadian, float endRadian, int sides, bool closedTube)
        {
            GameObject result = new GameObject();

            MeshFilter filter = result.AddComponent<MeshFilter>();
            MeshRenderer mr = result.AddComponent<MeshRenderer>();

            Mesh mesh = filter.mesh;
            mesh.Clear();

            if (closedTube)
            {
                startRadian = 0f;
                endRadian = Mathf.PI * 2f;
            }

            int nbVerticesCap = sides * 2 + 2;
            int nbVerticesSides = sides * 2 + 2;
            #region Vertices

            // bottom + top + sides + segment caps
            int nbVertOpenTube = nbVerticesCap * 2 + nbVerticesSides * 2 + 8;
            int nbVertClosedTube   = nbVerticesCap * 2 + nbVerticesSides * 2;
            Vector3[] vertices;
            if (closedTube)
                vertices = new Vector3[nbVertClosedTube];
            else
                vertices = new Vector3[nbVertOpenTube];
            int vert = 0;

            // Bottom cap
            int sideCounter = 0;
            while (vert < nbVerticesCap)
            {
                if (sideCounter == sides && closedTube)
                    sideCounter = 0;

                float r1 =  startRadian + ((float)(sideCounter++) / sides * (endRadian - startRadian));
                float cos = Mathf.Cos(r1);
                float sin = Mathf.Sin(r1);
                vertices[vert] = new Vector3(cos * (innerRadiusBottom), 0f, sin * (innerRadiusBottom));
                vertices[vert + 1] = new Vector3(cos * (outerRadiusBottom), 0f, sin * (outerRadiusBottom));
                vert += 2;
            }

            // Top cap
            sideCounter = 0;
            while (vert < nbVerticesCap * 2)
            {
                if (sideCounter == sides && closedTube)
                    sideCounter = 0;

                float r1 = startRadian + ((float)(sideCounter++) / sides * (endRadian - startRadian));
                float cos = Mathf.Cos(r1);
                float sin = Mathf.Sin(r1);
                vertices[vert] = new Vector3(cos * (innerRadiusTop), height, sin * (innerRadiusTop));
                vertices[vert + 1] = new Vector3(cos * (outerRadiusTop), height, sin * (outerRadiusTop));
                vert += 2;
            }

            // Sides (out)
            sideCounter = 0;
            while (vert < nbVerticesCap * 2 + nbVerticesSides)
            {
                if (sideCounter == sides && closedTube)
                    sideCounter = 0;

                float r1 = startRadian + ((float)(sideCounter++) / sides * (endRadian - startRadian));
                float cos = Mathf.Cos(r1);
                float sin = Mathf.Sin(r1);
                vertices[vert] = new Vector3(cos * (outerRadiusTop), height, sin * (outerRadiusTop));
                vertices[vert + 1] = new Vector3(cos * (outerRadiusBottom), 0, sin * (outerRadiusBottom));
                vert += 2;
            }

            // Sides (in)
            sideCounter = 0;
            while (vert < nbVertClosedTube)
            {
                if (sideCounter == sides && closedTube)
                    sideCounter = 0;

                float r1 = startRadian + ((float)(sideCounter++) / sides * (endRadian - startRadian));
                float cos = Mathf.Cos(r1);
                float sin = Mathf.Sin(r1);
                vertices[vert] = new Vector3(cos * (innerRadiusTop), height, sin * (innerRadiusTop));
                vertices[vert + 1] = new Vector3(cos * (innerRadiusBottom), 0, sin * (innerRadiusBottom));
                vert += 2;
            }

            // Open segments
            if (!closedTube)
            {
                vertices[vert++] = vertices[0];
                vertices[vert++] = vertices[1];
                vertices[vert++] = vertices[nbVerticesCap];
                vertices[vert++] = vertices[nbVerticesCap + 1];

                vertices[vert++] = vertices[nbVerticesCap - 2];
                vertices[vert++] = vertices[nbVerticesCap - 1];
                vertices[vert++] = vertices[2*nbVerticesCap - 2];
                vertices[vert] = vertices[2*nbVerticesCap - 1];
            }
            #endregion

            #region Normales

            // bottom + top + sides
            Vector3[] normales = new Vector3[vertices.Length];
            vert = 0;

            // Bottom cap
            while (vert < nbVerticesCap)
            {
                normales[vert++] = Vector3.down;
            }

            // Top cap
            while (vert < nbVerticesCap * 2)
            {
                normales[vert++] = Vector3.up;
            }

            // Sides (out)
            sideCounter = 0;
            while (vert < nbVerticesCap * 2 + nbVerticesSides)
            {
                if (sideCounter == sides && closedTube)
                    sideCounter = 0;

                float r1 = startRadian + ((float)(sideCounter++) / sides * (endRadian - startRadian));
                normales[vert] = new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1));
                normales[vert + 1] = normales[vert];
                vert += 2;
            }

            // Sides (in)
            sideCounter = 0;
            while (vert < nbVertClosedTube)
            {
                if (sideCounter == sides && closedTube)
                    sideCounter = 0;

                float r1 = startRadian + ((float)(sideCounter++) / sides * (endRadian - startRadian));
                normales[vert] = -(new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1)));
                normales[vert + 1] = normales[vert];
                vert += 2;
            }

            // Open segments
            if (!closedTube)
            {
                Vector3 vecA = vertices[nbVertOpenTube - 7] - vertices[nbVertOpenTube - 8];
                Vector3 vecB = vertices[nbVertOpenTube - 6] - vertices[nbVertOpenTube - 8];
                Vector3 normal = Vector3.Cross(vecB, vecA);
                normal = Vector3.Normalize(normal);
                normales[vert++] = normal;
                normales[vert++] = normal;
                normales[vert++] = normal;
                normales[vert++] = normal;

                vecA = vertices[nbVertOpenTube - 4] - vertices[nbVertOpenTube - 3];
                vecB = vertices[nbVertOpenTube - 1] - vertices[nbVertOpenTube - 3];
                normal = Vector3.Cross(vecB, vecA);
                normal = Vector3.Normalize(normal);
                normales[vert++] = normal;
                normales[vert++] = normal;
                normales[vert++] = normal;
                normales[vert]   = normal;
            }

            #endregion

            #region UVs
            Vector2[] uvs = new Vector2[vertices.Length];

            vert = 0;
            // Bottom cap
            sideCounter = 0;
            while (vert < nbVerticesCap)
            {
                float t = (float)(sideCounter++) / sides;
                uvs[vert++] = new Vector2(0f, t);
                uvs[vert++] = new Vector2(1f, t);
            }

            // Top cap
            sideCounter = 0;
            while (vert < nbVerticesCap * 2)
            {
                float t = (float)(sideCounter++) / sides;
                uvs[vert++] = new Vector2(0f, t);
                uvs[vert++] = new Vector2(1f, t);
            }

            // Sides (out)
            sideCounter = 0;
            while (vert < nbVerticesCap * 2 + nbVerticesSides)
            {
                float t = (float)(sideCounter++) / sides;
                uvs[vert++] = new Vector2(t, 0f);
                uvs[vert++] = new Vector2(t, 1f);
            }

            // Sides (in)
            sideCounter = 0;
            while (vert < nbVertClosedTube)
            {
                float t = (float)(sideCounter++) / sides;
                uvs[vert++] = new Vector2(t, 0f);
                uvs[vert++] = new Vector2(t, 1f);
            }

            // Open segments
            if (!closedTube)
            {

                uvs[vert++] = new Vector2(0, 0);
                uvs[vert++] = new Vector2(0, 1);
                uvs[vert++] = new Vector2(1, 0);
                uvs[vert++] = new Vector2(1, 1);

                uvs[vert++] = new Vector2(0, 1);
                uvs[vert++] = new Vector2(0, 0);
                uvs[vert++] = new Vector2(1, 1);
                uvs[vert] = new Vector2(1, 0);
            }

            #endregion

            #region Triangles
            int nbFace = sides * 4;
            if (!closedTube)
                nbFace += 2;
            int nbTriangles = nbFace * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbIndexes];

            // Bottom cap
            int i = 0;
            sideCounter = 0;
            while (sideCounter < sides)
            {
                int current = sideCounter * 2;
                int next = sideCounter * 2 + 2;

                triangles[i++] = next + 1;
                triangles[i++] = next;
                triangles[i++] = current;

                triangles[i++] = current + 1;
                triangles[i++] = next + 1;
                triangles[i++] = current;

                sideCounter++;
            }

            // Top cap
            while (sideCounter < sides * 2)
            {
                int current = sideCounter * 2 + 2;
                int next = sideCounter * 2 + 4;

                triangles[i++] = current;
                triangles[i++] = next;
                triangles[i++] = next + 1;

                triangles[i++] = current;
                triangles[i++] = next + 1;
                triangles[i++] = current + 1;

                sideCounter++;
            }

            // Sides (out)
            while (sideCounter < sides * 3)
            {
                int current = sideCounter * 2 + 4;
                int next = sideCounter * 2 + 6;

                triangles[i++] = current;
                triangles[i++] = next;
                triangles[i++] = next + 1;

                triangles[i++] = current;
                triangles[i++] = next + 1;
                triangles[i++] = current + 1;

                sideCounter++;
            }


            // Sides (in)
            while (sideCounter < sides * 4)
            {
                int current = sideCounter * 2 + 6;
                int next = sideCounter * 2 + 8;

                triangles[i++] = next + 1;
                triangles[i++] = next;
                triangles[i++] = current;

                triangles[i++] = current + 1;
                triangles[i++] = next + 1;
                triangles[i++] = current;

                sideCounter++;
            }

            // Segment Caps
            if (!closedTube)
            {
                triangles[i++] = nbVertOpenTube - 5;
                triangles[i++] = nbVertOpenTube - 7;
                triangles[i++] = nbVertOpenTube - 8;
                triangles[i++] = nbVertOpenTube - 5;
                triangles[i++] = nbVertOpenTube - 8;
                triangles[i++] = nbVertOpenTube - 6;

                triangles[i++] = nbVertOpenTube - 2;
                triangles[i++] = nbVertOpenTube - 4;
                triangles[i++] = nbVertOpenTube - 3;
                triangles[i++] = nbVertOpenTube - 2;
                triangles[i++] = nbVertOpenTube - 3;
                triangles[i++] = nbVertOpenTube - 1;
            }

            #endregion

            mesh.vertices = vertices;
            mesh.normals = normales;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            return result;
        }

    }
}
