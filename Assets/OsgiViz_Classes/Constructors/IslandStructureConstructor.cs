using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TriangleNet.Geometry;
using TriangleNet.Meshing.Algorithm;
using TriangleNet.Voronoi;
using QuickGraph;
using System.Linq;
using OsgiViz.Core;
using OsgiViz.SoftwareArtifact;
using OsgiViz.Island;
using OsgiViz.Relations;

using VFace = TriangleNet.Topology.DCEL.Face;
using TnetMesh = TriangleNet.Mesh;
using VHEdge = TriangleNet.Topology.DCEL.HalfEdge;
using VVertex = TriangleNet.Topology.DCEL.Vertex;

namespace OsgiViz.SideThreadConstructors
{
    /// <summary>
    /// This class creates CartographicIslands from a OsgiProject.
    /// </summary>
    public class IslandStructureConstructor
    {
        private OsgiProject osgiProject;
        private List<CartographicIsland> islands;
        private Status status;
        private System.Random RNG;
        private int expansionFactor;
        private float minCohesion;
        private float maxCohesion;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expansionF"></param>
        /// <param name="minCoh"></param>
        /// <param name="maxCoh"></param>
        public IslandStructureConstructor(int expansionF, float minCoh, float maxCoh)
        {
            osgiProject = null;
            islands = new List<CartographicIsland>();
            status = Status.Idle;
            RNG = new System.Random(0);
            expansionFactor = expansionF;
            minCohesion = minCoh;
            maxCohesion = maxCoh;
        }

        /// <summary>
        /// This Coroutine creates a list of CartographicIslands from a OsgiProject.
        /// This method is called by the MainThreadConstructor.
        /// </summary>
        /// <param name="proj">The OsgiProject from which the CartographicIslands are created.</param>
        public IEnumerator Construct (OsgiProject proj)
        {
            osgiProject = proj;

            status = Status.Working;
            Debug.Log("Starting with the construction of the IslandStructures");

            foreach (Bundle bundle in osgiProject.getBundles())
            {
                islands.Add(constructIslandFromBundle(bundle));
                //yield return null;
            }
            
            status = Status.Finished;
            Debug.Log("Finished with the construction of the IslandStructures");

            yield return null; // TODO remove
        }

        
        // TODO move into the coroutine & optimize (GC)
        private CartographicIsland constructIslandFromBundle(Bundle b)
        {
            int rngSeed = b.getName().GetHashCode() + 200;
            RNG = new System.Random(rngSeed);

            #region VoronoiPlane 
            TriangleNet.Configuration conf = new TriangleNet.Configuration();
            List<Vertex> vertices = Helperfunctions.createPointsOnPlane(GlobalVar.voronoiCellScalefactor, GlobalVar.voronoiCellScalefactor, 50, 50, 1.0f, RNG);
            BoundedVoronoi voronoi = Helperfunctions.createRelaxedVoronoi(vertices, 1);
            #endregion

            #region initFirstCell
            VFace firstCell = Helperfunctions.closestCell(0, 0, voronoi);
            Dictionary<int, VFace> startingCandidates = new Dictionary<int, VFace>();
            startingCandidates.Add(firstCell.ID, firstCell);
            #endregion

            List<Package> packages = b.getPackages();
            CartographicIsland island = new CartographicIsland(b);
            island.setVoronoi(voronoi);
            #region sort package list
            packages.Sort((x, y) => x.getCompilationUnits().Count.CompareTo(y.getCompilationUnits().Count));
            packages.Reverse();
            #endregion
            //Compute maximal compilation unit count in bundle
            float maxCUCountInIsland = 0;
            foreach (Package package in packages)
            {
                long cuCount = package.getCuCount();
                if (cuCount > maxCUCountInIsland)
                    maxCUCountInIsland = cuCount;
            }
            #region construct regions
            foreach (Package package in packages)
            {
                float cohesionMult = (float)package.getCuCount() / maxCUCountInIsland;
                cohesionMult *= maxCohesion;
                cohesionMult = Mathf.Max(minCohesion, cohesionMult);
                Dictionary<int, VFace> newCandidates = ConstructRegionFromPackage(package, island, startingCandidates, cohesionMult);
                UpdateAndFuseCandidates(startingCandidates, newCandidates);
            }
            #endregion

            #region Shape island coast
            //Advance startingCandidates X cells outwards and ajdust the height of all vertices
            shapeCoastArea(startingCandidates, GlobalVar.islandHeightProfile);
            #endregion

            #region WeightedCenter & set coast
            List<VFace> coastlineList = new List<VFace>();
            Vector3 weightedCenter = Vector3.zero;
            foreach (KeyValuePair<int, VFace> kvp in startingCandidates)
            {
                coastlineList.Add(kvp.Value);
                float x = (float)kvp.Value.generator.X;
                float z = (float)kvp.Value.generator.Y;
                Vector3 tilePos = new Vector3(x, 0, z);
                weightedCenter += tilePos;
            }
            weightedCenter /= startingCandidates.Count;
            island.setWeightedCenter(weightedCenter);
            island.setCoastlineCells(coastlineList);
            #endregion

            #region conservative Radius
            List<float> radii = new List<float>();
            foreach (KeyValuePair<int, VFace> kvp in startingCandidates)
            {
                float x = (float)kvp.Value.generator.X - island.getWeightedCenter().x;
                float z = (float)kvp.Value.generator.Y - island.getWeightedCenter().z;
                float radius = Mathf.Sqrt(x * x + z * z);
                radii.Add(radius);
            }
            float maxRadius = Helperfunctions.computeMax(radii);
            island.setRadius(maxRadius);
            #endregion

            #region TnetMeshesConstruction
            foreach (List<VFace> cellMap in island.getPackageCells())
                island.addPackageMesh(ConstructTnetMeshFromCellmap(cellMap));

            island.setCoastlineMesh(ConstructTnetMeshFromCellmap(coastlineList));
            #endregion

            #region link dependency vertex

            //Find graph vertex associated with the island
            BidirectionalGraph<GraphVertex, GraphEdge> depGraph = b.getParentProject().getDependencyGraph();
            List<GraphVertex> allVertices = depGraph.Vertices.ToList();
            GraphVertex vert = allVertices.Find(v => string.Equals(v.getName() , b.getName()) );
            if (vert != null)
            {
                //Link GraphVertex-Island
                vert.setIsland(island);
                island.setDependencyVertex(vert);
            }

            #endregion

            Debug.Log("Finished Construciton of Island " + b.getName());

            return island;
        }

        /// <summary>
        /// Update dictA and fuse dictB into it.
        /// </summary>
        /// <param name="dictA">A Dictionary containting TODO</param>
        /// <param name="dictB">A Dictionary containting TODO</param>
        private void UpdateAndFuseCandidates(Dictionary<int, VFace> dictA, Dictionary<int, VFace> dictB)
        {
            //update dictA
            List<int> keysToRemove = new List<int>();
            foreach (KeyValuePair<int, VFace> kvp in dictA)
            {
                if (kvp.Value.mark != 0)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }                
            foreach (int key in keysToRemove)
            {
                dictA.Remove(key);
            }

            //Fuse dictB into dictA
            foreach (KeyValuePair<int, VFace> kvp in dictB)
            {
                if (!dictA.ContainsKey(kvp.Key))
                {
                    dictA.Add(kvp.Key, kvp.Value);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        /// <param name="island"></param>
        /// <param name="startingCandidates"></param>
        /// <param name="b">b[1, 10]: cohesion factor. higher b -> more compact "cohesive" islands.</param>
        /// <returns>The unused candidates.</returns>
        private Dictionary<int, VFace> ConstructRegionFromPackage(Package package, CartographicIsland island, Dictionary<int, VFace> startingCandidates, float b)
        {
            BoundedVoronoi islandVoronoi = island.getVoronoi();
            List<CompilationUnit> cuList = package.getCompilationUnits();
            List<VFace> cellMap = new List<VFace>();
            Dictionary<int, VFace> newCandidates = new Dictionary<int, VFace>();          
            VFace startingCell = SelectFromCandidates(startingCandidates, b).Value;
            
            int maxIterations = 10;
            int counter = 0;
            while (ExpandCountries(cuList, cellMap, newCandidates, startingCell, b) == false && counter < maxIterations)
            {
                //Debug.Log("Backtracking");
                startingCell = SelectFromCandidates(startingCandidates, b).Value;
                counter++;
            }

            island.addPackage(package);
            island.addPackageCells(cellMap);
            return newCandidates;
        }

        /// <summary>
        /// Writes into cellMap and endCandidates.
        /// </summary>
        /// <param name="cuList"></param>
        /// <param name="cellMap"></param>
        /// <param name="endCandidates"></param>
        /// <param name="startingCell"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool ExpandCountries(List<CompilationUnit> cuList, List<VFace> cellMap, Dictionary<int, VFace> endCandidates, VFace startingCell, float b)
        {
            Dictionary<int, VFace> candidates = new Dictionary<int, VFace>();
            candidates.Add(startingCell.ID, startingCell);

            for(int i=1; i < cuList.Count*expansionFactor+1; i++)
            {

                if (candidates.Count == 0)
                {
                    cellMap.Clear();
                    endCandidates.Clear();
                    return false;
                }

                    //Select cell from candidates
                KeyValuePair<int, VFace> selectedCell = SelectFromCandidates(candidates, b);
                    //Mark cell in islandVoronoi
                selectedCell.Value.mark = i;
                    //Add cell to package dictionary
                cellMap.Add(selectedCell.Value);
                    //Remove cell from future candidates list
                candidates.Remove(selectedCell.Key);
                    //Add viable candidates around cell

                foreach (VHEdge edge in selectedCell.Value.EnumerateEdges())
                {
                    VFace nCell = edge.twin.face;
                    //If cell is OK, not occupied and not already in candidateList, add to candidate list
                    if (Helperfunctions.checkCell(nCell, -GlobalVar.voronoiCellScalefactor * 0.4f, GlobalVar.voronoiCellScalefactor * 0.4f,
                        -GlobalVar.voronoiCellScalefactor * 0.4f, GlobalVar.voronoiCellScalefactor * 0.4f) && nCell.mark == 0 && !candidates.ContainsKey(nCell.ID))
                        candidates.Add(nCell.ID, nCell);
                }

            }

            //transfer candidates into endCandidates Dict
            foreach (KeyValuePair<int, VFace> kvp in candidates)
                endCandidates.Add(kvp.Key, kvp.Value);

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cellmap"></param>
        /// <returns></returns>
        private List<TnetMesh> ConstructTnetMeshFromCellmap(List<VFace> cellmap)
        {
            List<Vertex> vertices = new List<Vertex>();
            List<TnetMesh> result = new List<TnetMesh>();
            TriangleNet.Configuration conf = new TriangleNet.Configuration();
            SweepLine sl = new SweepLine();

            foreach (VFace face in cellmap)
            {
                vertices.Clear();
                foreach (VHEdge he in face.EnumerateEdges())
                {
                    Vertex v = new Vertex(he.Origin.X, he.Origin.Y);
                    v.Z = he.Origin.Z;
                    if(!vertices.Contains(v))
                        vertices.Add(v);
                }
                TriangleNet.Mesh tMesh = (TriangleNet.Mesh)sl.Triangulate(vertices, conf);
                result.Add(tMesh);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="candidates"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private KeyValuePair<int, VFace> SelectFromCandidates(Dictionary<int, VFace> candidates, float b)
        {
            KeyValuePair<int, VFace> selectedCandidate = new KeyValuePair<int,VFace>();
            List<float> candidateScores = new List<float>();
            float totalScore = 0f;

            //Fill CandidateScores
            int counter = 0;
            foreach (KeyValuePair<int, VFace> kvp in candidates)
            {
                int n = 0;
                foreach (VHEdge edge in kvp.Value.EnumerateEdges())
                {
                    int mark = edge.twin.face.mark;
                    if (mark != 0)
                        n++;
                }

                float score = Mathf.Pow(b, n);
                candidateScores.Add(score);
                totalScore += score;
                counter++;
            }
            //Normalize CandidateScores
            for (int i = 0; i < candidateScores.Count; i++)
                candidateScores[i] = candidateScores[i] / totalScore;

            //Select candidate based on probability
            float rndNumber = (float)RNG.NextDouble();
            counter = 0;
            foreach (KeyValuePair<int, VFace> kvp in candidates)
            {
                rndNumber -= candidateScores[counter];
                if (rndNumber <= 0f)
                {
                    selectedCandidate = kvp;
                    break;
                }
                counter++;
            }

            return selectedCandidate;
        }


        /// <summary>
        /// Expands the coastlineCells outwards by hp.length cells and applies the height profile hp during expansion.
        /// </summary>
        /// <param name="coastline"></param>
        /// <param name="hp"></param>
        private void shapeCoastArea(Dictionary<int, VFace> coastline, float[] hp)
        {
            Dictionary<int, VFace> newestCoastline = new Dictionary<int, VFace>(coastline);

            for (int i = 0; i < hp.Length; i++)
            {
                //Expand cells
                newestCoastline.Clear();
                foreach (KeyValuePair<int, VFace> kvp in coastline)
                {
                    foreach (VHEdge edge in kvp.Value.EnumerateEdges())
                    {
                        VFace nCell = edge.twin.face;
                        if (Helperfunctions.checkCell(nCell, -GlobalVar.voronoiCellScalefactor * 0.4f, GlobalVar.voronoiCellScalefactor * 0.4f,
                            -GlobalVar.voronoiCellScalefactor * 0.4f, GlobalVar.voronoiCellScalefactor * 0.4f) && nCell.mark == 0 && !coastline.ContainsKey(nCell.ID)
                            && !newestCoastline.ContainsKey(nCell.ID))
                            newestCoastline.Add(nCell.ID, nCell);
                    }
                }
                //Adjust height and Add to coastline
                foreach (KeyValuePair<int, VFace> kvp in newestCoastline)
                {
                    coastline.Add(kvp.Key, kvp.Value);
                    foreach (VHEdge edge in kvp.Value.EnumerateEdges())
                    {
                            edge.Origin.Z += hp[i];
                    }
                }
                
            }
            //Remove the last expansion from the coastline, due to artifacts
            foreach (KeyValuePair<int, VFace> kvp in newestCoastline)
                coastline.Remove(kvp.Key);
        }

        // get & set
        public List<CartographicIsland> getIslandStructureList()
        {
            return islands;
        }
        public Status getStatus()
        {
            return status;
        }
        public void setStatus(Status newStatus)
        {
            status = newStatus;
        }





        /*
        private void populateIslandCoastDistance(CartographicIsland island, int hf)
        {
            //Init land height to Inf
            List<List<VFace>> fragCellList = island.getPackageCells();
            foreach (List<VFace> cellMap in fragCellList)
                foreach (VFace face in cellMap)
                    face.coastDistance = int.MaxValue;
                        

            //Enqueue the coast region first
            Queue<VFace> cellQueue = new Queue<VFace>();
            foreach (VFace face in island.getCoastlineCells())
            {
                face.coastDistance = 0;
                cellQueue.Enqueue(face);
            }

            while (cellQueue.Count > 0)
            {
                VFace v = cellQueue.Dequeue();
                foreach (VHEdge edge in v.EnumerateEdges())
                {
                    VFace adjFace = edge.twin.face;
                    int newDistance = v.coastDistance + hf;
                    if (newDistance < adjFace.coastDistance)
                    {
                        adjFace.coastDistance = newDistance;
                        cellQueue.Enqueue(adjFace);
                    }
                }
            }
        }
        */

        /*
        private void computeIslandHeight(CartographicIsland island, float hf)
        {
            //Init land height to Inf
            List<List<VFace>> fragCellList = island.getPackageCells();
            foreach(List<VFace> cellMap in fragCellList)
                foreach (VFace face in cellMap)
                    foreach (VHEdge edge in face.EnumerateEdges())
                        edge.Origin.Z = Mathf.Infinity;

            //Enqueue the coast region first
            Queue<VVertex> vertexQueue = new Queue<VVertex>();
            foreach (VFace face in island.getCoastlineCells())
            {
                foreach (VHEdge edge in face.EnumerateEdges())
                    vertexQueue.Enqueue(edge.Origin);
            }

            while (vertexQueue.Count > 0)
            {
                VVertex v = vertexQueue.Dequeue();
                foreach (VHEdge edge in v.EnumerateEdges())
                {
                    VVertex adjVert = edge.Next.Origin;
                    float newElevation = (float)v.Z + hf;
                    if (newElevation < adjVert.Z)
                    {
                        adjVert.Z = newElevation;
                        vertexQueue.Enqueue(adjVert);
                    }
                }
            }

        }
        */
    }

}
