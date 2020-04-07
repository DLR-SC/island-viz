using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Voronoi;
using OsgiViz.SoftwareArtifact;
using OsgiViz.Relations;

using TnetMesh = TriangleNet.Mesh;
using VFace = TriangleNet.Topology.DCEL.Face;

namespace OsgiViz.Island
{
    public class CartographicIsland
    {
        private BoundedVoronoi islandVoronoi;
        private List<Package> packages;
        private Bundle bundle;
        private List<List<VFace>> packageCells;
        private List<VFace> coastlineCells;
        //For each Fragment a list of meshes, where each one represents the geometry of a voronoi cell
        private List<List<TnetMesh>> packageTnetMeshes;
        private List<TnetMesh> coastlineTnetMeshes;
        private float radius;
        private Vector3 weightedCenter;
        private GraphVertex dependencyVertex;
        private GameObject islandGO;

        public CartographicIsland(Bundle b)
        {
            bundle = b;
            packageCells = new List<List<VFace>>();
            packages = new List<Package>();
            packageTnetMeshes = new List<List<TnetMesh>>();
        }



        public GraphVertex getDependencyVertex()
        {
            return dependencyVertex;
        }
        public GameObject getIslandGO()
        {
            return islandGO;
        }
        public Bundle getBundle()
        {
            return bundle;
        }
        public BoundedVoronoi getVoronoi()
        {
            return islandVoronoi;
        }
        public float getRadius()
        {
            return radius;
        }
        public Vector3 getWeightedCenter()
        {
            return weightedCenter;
        }
        public List<List<VFace>> getPackageCells()
        {
            return packageCells;
        }
        public List<VFace> getCoastlineCells()
        {
            return coastlineCells;
        }
        public List<List<TnetMesh>> getPackageMeshes()
        {
            return packageTnetMeshes;
        }
        public List<Package> getPackages()
        {
            return packages;
        }
        public string getName()
        {
            return bundle.getName();
        }
        public List<TnetMesh> getCoastlineMeshes()
        {
            return coastlineTnetMeshes;
        }

        public void addPackageCells(List<VFace> list)
        {
            packageCells.Add(list);
        }
        public void addPackage(Package frag)
        {
            packages.Add(frag);
        }
        public void addPackageMesh(List<TnetMesh> mesh)
        {
            packageTnetMeshes.Add(mesh);
        }

        public void setIslandGO(GameObject i)
        {
            islandGO = i;
        }
        public void setRadius(float r)
        {
            radius = r;
        }
        public void setWeightedCenter(Vector3 wc)
        {
            weightedCenter = wc;
        }
        public void setVoronoi(BoundedVoronoi voronoi)
        {
            islandVoronoi = voronoi;
        }
        public void setCoastlineCells(List<VFace> coastCells)
        {
            coastlineCells = coastCells;
        }
        public void setCoastlineMesh(List<TnetMesh> mesh)
        {
            coastlineTnetMeshes = mesh;
        }
        public void setDependencyVertex(GraphVertex v)
        {
            dependencyVertex = v;
        }

    }
}
