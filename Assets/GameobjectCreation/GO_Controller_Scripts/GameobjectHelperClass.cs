using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameobjectHelperClass
{
    public static void setUVsToSingularCoord(Vector2 newUV, MeshFilter mesh)
    {
        Vector3[] uvs = mesh.mesh.vertices;
        Vector2[] newUVs = new Vector2[uvs.Length];
        for (int i = 0; i < uvs.Length; i++)
            newUVs[i] = newUV;

        mesh.sharedMesh.uv = newUVs;
    }
}

