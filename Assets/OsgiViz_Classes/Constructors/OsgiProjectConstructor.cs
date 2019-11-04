using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.Assertions;
using QuickGraph;
using System.Linq;
using OsgiViz.Core;
using OsgiViz.SoftwareArtifact;
using OsgiViz.Relations;
using Neo4j.Driver;
using Neo4j.Driver.V1;

namespace OsgiViz.SideThreadConstructors
{

    /// <summary>
    /// This class creates a OsgiProject from a JSONObject.
    /// </summary>
    public class OsgiProjectConstructor
    {

        private OsgiProject currentProject;
        private Status status;
        private readonly string redundantString_A = "http://www.example.org/OSGiApplicationModel#//";
        private JSONObject jsonObj;

        /// <summary>
        /// Constructor
        /// </summary>
        public OsgiProjectConstructor()
        {
            currentProject = null;
            status = Status.Idle;
            jsonObj = null;
        }






        public IEnumerator Neo4jConstruct(IStatementResult jObj)
        {
            // jsonObj = jObj; // TODO

            status = Status.Working;
            Debug.Log("Starting OSGi-Project construction!");

            #region OsgiProject            
            JSONObject tmp = jsonObj.GetField("name"); 
            Assert.IsNotNull(tmp, "Projectname could not be found!");
            currentProject = new OsgiProject(tmp.str);
            #endregion
            #region Bundle,Fragments,Compilation Units
            tmp = jsonObj.GetField("bundles"); // MATCH (b:Bundle) RETURN b.name
            Assert.IsNotNull(tmp, "Project does not contain any Bundles!");
            List<JSONObject> jsonBundleList = tmp.list;
            List<JSONObject> jsonPackageList = jsonObj.GetField("packages").list; // MATCH (p:Package) RETURN p.name
            long maxLOC = 0;
            foreach (JSONObject jsonBundle in jsonBundleList)
            {
                string name = jsonBundle.GetField("name").str; 
                string symbName = jsonBundle.GetField("symbolicName").str; // MATCH (b:Bundle {name: 'name'}) RETURN b.symbolicName
                Bundle currentBundle = new Bundle(name, symbName, currentProject);
                //Create Fragments
                tmp = jsonBundle.GetField("packageFragments"); // MATCH (b:Bundle {name: 'name'}) RETURN b.symbolicName
                if (tmp != null)
                {
                    List<JSONObject> fragList = tmp.list;
                    foreach (JSONObject frag in fragList)
                    {
                        JSONObject jsonPkg = frag.GetField("package");
                        int pkgIdx = ResolvePackageReferenceIdx(jsonPkg);
                        string fragName = jsonPackageList[pkgIdx].GetField("qualifiedName").str;
                        Package currentFragment = new Package(currentBundle, fragName);
                        //Create Compilation Units
                        if (frag.GetField("compilationUnits") != null)
                        {
                            List<JSONObject> jsonCompUnits = frag.GetField("compilationUnits").list;
                            foreach (JSONObject jsonCU in jsonCompUnits)
                            {
                                JSONObject tlt = jsonCU.GetField("topLevelType");
                                string tempStr = tlt.GetField("eClass").str;
                                tempStr = tempStr.Replace(redundantString_A, "");
                                type type = JavaParser.getTypeFromString(tempStr);
                                tempStr = tlt.GetField("visibility").str;
                                tempStr = tempStr.Replace(redundantString_A, "");
                                modifier mod = JavaParser.getModifierFromString(tempStr);
                                long loc = jsonCU.GetField("LOC").i;
                                if (loc > maxLOC)
                                    maxLOC = loc;
                                CompilationUnit compUnit = new CompilationUnit(tlt.GetField("name").str, type, mod, loc, currentFragment);
                                //TODO: Add support for additional information about a compilation unit(Methods, references to others)
                                currentFragment.addCompilationUnit(compUnit);
                            }
                        }
                        currentBundle.addPackage(currentFragment);
                        yield return null;
                    }
                }
                currentProject.addBundle(currentBundle);
            }
            GlobalVar.maximumLOCinProject = maxLOC;
            #endregion

            #region Services
            List<Bundle> bundles = currentProject.getBundles();
            tmp = jsonObj.GetField("services");
            if (tmp != null)
            {
                List<JSONObject> serviceJsonList = tmp.list;
                foreach (JSONObject jsonService in serviceJsonList)
                {

                    string serviceName = jsonService.GetField("interfaceName").str;
                    tmp = jsonService.GetField("interface");
                    CompilationUnit serviceCU = null;
                    if (tmp != null)
                    {
                        Vector3 cuIdx = ResolveCompilationUnitRef(tmp);
                        serviceCU = bundles[(int)cuIdx.x].getPackages()[(int)cuIdx.y].getCompilationUnits()[(int)cuIdx.z];
                        serviceCU.setServiceDeclaration(true);
                    }
                    Service service = new Service(serviceName, serviceCU);
                    currentProject.addService(service);
                    yield return null;
                }
            }
            #endregion
            #region Resolve import/export + construct ServiceComponents + build dependency graph
            int i = 0;
            BidirectionalGraph<GraphVertex, GraphEdge> dependencyGraph = currentProject.getDependencyGraph();
            foreach (JSONObject jsonBundle in jsonBundleList)
            {
                //Resolve Exports for Bundle
                tmp = jsonBundle.GetField("exports");
                if (tmp != null)
                {
                    List<Vector2> exportList = ResolvePckgFragmentRefList(tmp.list);
                    foreach (Vector2 indexVec in exportList)
                    {
                        Package resolvedFragment = bundles[(int)indexVec.x].getPackages()[(int)indexVec.y];
                        resolvedFragment.setExport(true);
                        bundles[i].addExportedPackage(resolvedFragment);
                    }
                }
                //Resolve Imports for Bundle
                tmp = jsonBundle.GetField("imports");
                if (tmp != null)
                {
                    List<Vector2> importList = ResolvePckgFragmentRefList(tmp.list);
                    foreach (Vector2 indexVec in importList)
                    {
                        Package resolvedFragment = bundles[(int)indexVec.x].getPackages()[(int)indexVec.y];
                        // Ignore self Import redundancy
                        if (string.Compare(bundles[i].getName(), resolvedFragment.getBundle().getName()) != 0)
                        {
                            bundles[i].addImportedPackage(resolvedFragment);

                            //Package dependency
                            //Check if Vertices already in Graph
                            List<GraphVertex> allVertices = dependencyGraph.Vertices.ToList();
                            GraphVertex vert1 = allVertices.Find(v => (string.Equals(v.getName(), bundles[i].getName())));
                            GraphVertex vert2 = allVertices.Find(v => (string.Equals(v.getName(), bundles[(int)indexVec.x].getName())));

                            if (vert1 == null)
                                vert1 = new GraphVertex(bundles[i].getName());
                            if (vert2 == null)
                                vert2 = new GraphVertex(bundles[(int)indexVec.x].getName());

                            dependencyGraph.AddVertex(vert1);
                            dependencyGraph.AddVertex(vert2);
                            GraphEdge edge;
                            bool edgePresent = dependencyGraph.TryGetEdge(vert1, vert2, out edge);
                            if (edgePresent && dependencyGraph.AllowParallelEdges)
                            {
                                edge.incrementWeight(1f);
                            }
                            else
                            {
                                edge = new GraphEdge(vert1, vert2);
                                dependencyGraph.AddEdge(edge);
                            }

                            GraphEdge opposingEdge;
                            float bidirectionalEdgeWeight = edge.getWeight();
                            bool oppEdgePresent = dependencyGraph.TryGetEdge(vert2, vert1, out opposingEdge);
                            if (oppEdgePresent)
                            {

                                bidirectionalEdgeWeight += opposingEdge.getWeight();
                            }

                            if (bidirectionalEdgeWeight > currentProject.getMaxImportCount())
                                currentProject.setMaxImportCount((int)bidirectionalEdgeWeight);

                        }
                        else
                        {
                            Debug.Log("Spotted import redundancy in: " + bundles[i].getName());
                        }
                        //yield return null;
                    }
                }
                //Construct and resolve ServiceComponents
                List<Service> serviceList = currentProject.getServices();
                tmp = jsonBundle.GetField("components");
                if (tmp != null)
                {
                    foreach (JSONObject jsonComponent in tmp.list)
                    {
                        string scName = jsonComponent.GetField("name").str;
                        Vector3 implIdx = ResolveCompilationUnitRef(jsonComponent.GetField("implementation"));
                        CompilationUnit resolvedCu = bundles[(int)implIdx.x].getPackages()[(int)implIdx.y].getCompilationUnits()[(int)implIdx.z];
                        resolvedCu.setServiceComponentImpl(true);
                        ServiceComponent sc = new ServiceComponent(scName, resolvedCu);

                        tmp = jsonComponent.GetField("providedServices");
                        if (tmp != null)
                        {
                            List<int> serviceRefs = ResolveServiceReferenceIdxList(tmp);
                            foreach (int s in serviceRefs)
                            {
                                sc.addProvidedService(serviceList[s]);
                                serviceList[s].addImplementingComponent(sc);
                            }
                        }
                        tmp = jsonComponent.GetField("referencedServices");
                        if (tmp != null)
                        {
                            List<int> serviceRefs = ResolveServiceReferenceIdxList(tmp);
                            foreach (int s in serviceRefs)
                            {
                                sc.addReferencedService(serviceList[s]);
                                serviceList[s].addReferencingComponent(sc);
                            }
                        }
                        bundles[i].addServiceComponent(sc);
                        yield return null;
                    }
                }
                i++;
            }
            #endregion
            Debug.Log("Max Import-count: " + currentProject.getMaxImportCount());
            status = Status.Finished;
            Debug.Log("Finished OSGi-Project construction!");
        }




        /// <summary>
        /// The Coroutine, that creates the OsgiProject from a JSONObject.
        /// This method is called by the MainThreadConstructor.
        /// The OsgiProject is stored in the local variable "currentProject".
        /// </summary>
        /// <param name="jObj">The JSONObject from which the OsgiProject is generated.</param>
        public IEnumerator Construct(JSONObject jObj)
        {
            jsonObj = jObj;

            status = Status.Working;
            Debug.Log("Starting OSGi-Project construction!");

            #region OsgiProject            
            JSONObject tmp = jsonObj.GetField("name");
            Assert.IsNotNull(tmp, "Projectname could not be found!");
            currentProject = new OsgiProject(tmp.str);
            #endregion
            #region Bundle,Fragments,Compilation Units
            tmp = jsonObj.GetField("bundles");
            Assert.IsNotNull(tmp, "Project does not contain any Bundles!");
            List<JSONObject> jsonBundleList = tmp.list;
            List<JSONObject> jsonPackageList = jsonObj.GetField("packages").list;
            long maxLOC = 0;
            foreach (JSONObject jsonBundle in jsonBundleList)
            {
                string name = jsonBundle.GetField("name").str;
                string symbName = jsonBundle.GetField("symbolicName").str;
                Bundle currentBundle = new Bundle(name, symbName, currentProject);
                //Create Fragments
                tmp = jsonBundle.GetField("packageFragments");
                if (tmp != null)
                {
                    List<JSONObject> fragList = tmp.list;
                    foreach (JSONObject frag in fragList)
                    {
                        JSONObject jsonPkg = frag.GetField("package");
                        int pkgIdx = ResolvePackageReferenceIdx(jsonPkg);
                        string fragName = jsonPackageList[pkgIdx].GetField("qualifiedName").str;
                        Package currentFragment = new Package(currentBundle, fragName);
                        //Create Compilation Units
                        if (frag.GetField("compilationUnits") != null)
                        {
                            List<JSONObject> jsonCompUnits = frag.GetField("compilationUnits").list;
                            foreach (JSONObject jsonCU in jsonCompUnits)
                            {
                                JSONObject tlt = jsonCU.GetField("topLevelType");
                                string tempStr = tlt.GetField("eClass").str;
                                tempStr = tempStr.Replace(redundantString_A, "");
                                type type = JavaParser.getTypeFromString(tempStr);
                                tempStr = tlt.GetField("visibility").str;
                                tempStr = tempStr.Replace(redundantString_A, "");
                                modifier mod = JavaParser.getModifierFromString(tempStr);
                                long loc = jsonCU.GetField("LOC").i;
                                if (loc > maxLOC)
                                    maxLOC = loc;
                                CompilationUnit compUnit = new CompilationUnit(tlt.GetField("name").str, type, mod, loc, currentFragment);
                                //TODO: Add support for additional information about a compilation unit(Methods, references to others)
                                currentFragment.addCompilationUnit(compUnit);
                            }
                        }
                        currentBundle.addPackage(currentFragment);
                        yield return null;
                    }
                }
                currentProject.addBundle(currentBundle);
            }
            GlobalVar.maximumLOCinProject = maxLOC;
            #endregion

            #region Services
            List<Bundle> bundles = currentProject.getBundles();
            tmp = jsonObj.GetField("services");
            if (tmp != null)
            {
                List<JSONObject> serviceJsonList = tmp.list;
                foreach (JSONObject jsonService in serviceJsonList)
                {

                    string serviceName = jsonService.GetField("interfaceName").str;
                    tmp = jsonService.GetField("interface");
                    CompilationUnit serviceCU = null;
                    if (tmp != null)
                    {
                        Vector3 cuIdx = ResolveCompilationUnitRef(tmp);
                        serviceCU = bundles[(int)cuIdx.x].getPackages()[(int)cuIdx.y].getCompilationUnits()[(int)cuIdx.z];
                        serviceCU.setServiceDeclaration(true);
                    }
                    Service service = new Service(serviceName, serviceCU);
                    currentProject.addService(service);
                    yield return null;
                }
            }
            #endregion
            #region Resolve import/export + construct ServiceComponents + build dependency graph
            int i = 0;
            BidirectionalGraph<GraphVertex, GraphEdge> dependencyGraph = currentProject.getDependencyGraph();
            foreach (JSONObject jsonBundle in jsonBundleList)
            {
                //Resolve Exports for Bundle
                tmp = jsonBundle.GetField("exports");
                if (tmp != null)
                {
                    List<Vector2> exportList = ResolvePckgFragmentRefList(tmp.list);
                    foreach (Vector2 indexVec in exportList)
                    {
                        Package resolvedFragment = bundles[(int)indexVec.x].getPackages()[(int)indexVec.y];
                        resolvedFragment.setExport(true);
                        bundles[i].addExportedPackage(resolvedFragment);
                    }
                }
                //Resolve Imports for Bundle
                tmp = jsonBundle.GetField("imports");
                if (tmp != null)
                {
                    List<Vector2> importList = ResolvePckgFragmentRefList(tmp.list);
                    foreach (Vector2 indexVec in importList)
                    {
                        Package resolvedFragment = bundles[(int)indexVec.x].getPackages()[(int)indexVec.y];
                        // Ignore self Import redundancy
                        if (string.Compare(bundles[i].getName(), resolvedFragment.getBundle().getName()) != 0)
                        {
                            bundles[i].addImportedPackage(resolvedFragment);

                            //Package dependency
                            //Check if Vertices already in Graph
                            List<GraphVertex> allVertices = dependencyGraph.Vertices.ToList();
                            GraphVertex vert1 = allVertices.Find(v => (string.Equals(v.getName(), bundles[i].getName())));
                            GraphVertex vert2 = allVertices.Find(v => (string.Equals(v.getName(), bundles[(int)indexVec.x].getName())));

                            if (vert1 == null)
                                vert1 = new GraphVertex(bundles[i].getName());
                            if (vert2 == null)
                                vert2 = new GraphVertex(bundles[(int)indexVec.x].getName());

                            dependencyGraph.AddVertex(vert1);
                            dependencyGraph.AddVertex(vert2);
                            GraphEdge edge;
                            bool edgePresent = dependencyGraph.TryGetEdge(vert1, vert2, out edge);
                            if (edgePresent && dependencyGraph.AllowParallelEdges)
                            {
                                edge.incrementWeight(1f);
                            }
                            else
                            {
                                edge = new GraphEdge(vert1, vert2);
                                dependencyGraph.AddEdge(edge);
                            }

                            GraphEdge opposingEdge;
                            float bidirectionalEdgeWeight = edge.getWeight();
                            bool oppEdgePresent = dependencyGraph.TryGetEdge(vert2, vert1, out opposingEdge);
                            if (oppEdgePresent)
                            {

                                bidirectionalEdgeWeight += opposingEdge.getWeight();
                            }

                            if (bidirectionalEdgeWeight > currentProject.getMaxImportCount())
                                currentProject.setMaxImportCount((int)bidirectionalEdgeWeight);

                        }
                        else
                        {
                            Debug.Log("Spotted import redundancy in: " + bundles[i].getName());
                        }
                        //yield return null;
                    }
                }
                //Construct and resolve ServiceComponents
                List<Service> serviceList = currentProject.getServices();
                tmp = jsonBundle.GetField("components");
                if (tmp != null)
                {
                    foreach (JSONObject jsonComponent in tmp.list)
                    {
                        string scName = jsonComponent.GetField("name").str;
                        Vector3 implIdx = ResolveCompilationUnitRef(jsonComponent.GetField("implementation"));
                        CompilationUnit resolvedCu = bundles[(int)implIdx.x].getPackages()[(int)implIdx.y].getCompilationUnits()[(int)implIdx.z];
                        resolvedCu.setServiceComponentImpl(true);
                        ServiceComponent sc = new ServiceComponent(scName, resolvedCu);

                        tmp = jsonComponent.GetField("providedServices");
                        if (tmp != null)
                        {
                            List<int> serviceRefs = ResolveServiceReferenceIdxList(tmp);
                            foreach (int s in serviceRefs)
                            {
                                sc.addProvidedService(serviceList[s]);
                                serviceList[s].addImplementingComponent(sc);
                            }
                        }
                        tmp = jsonComponent.GetField("referencedServices");
                        if (tmp != null)
                        {
                            List<int> serviceRefs = ResolveServiceReferenceIdxList(tmp);
                            foreach (int s in serviceRefs)
                            {
                                sc.addReferencedService(serviceList[s]);
                                serviceList[s].addReferencingComponent(sc);
                            }
                        }
                        bundles[i].addServiceComponent(sc);
                        yield return null;
                    }
                }
                i++;
            }
            #endregion
            //Debug.Log("Max Import-count: " + currentProject.getMaxImportCount());
            status = Status.Finished;
            Debug.Log("Finished OSGi-Project construction!");
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pckgFragmentRefList">A list of JSONObjects containing a $ref string of the format "//@bundles.X/@packageFragments.Y".</param>
        /// <returns>A list of Vector2(X,Y) representing Bundle and Packagefragment number.</returns>
        private List<Vector2> ResolvePckgFragmentRefList(List<JSONObject> pckgFragmentRefList)
        {
            List<Vector2> result = new List<Vector2>();
            foreach (JSONObject reference in pckgFragmentRefList)
            {
                string rawRefString = reference.GetField("$ref").str;
                string processedString = "";
                processedString = rawRefString.Replace("//@bundles.", "");
                processedString = processedString.Replace("/@packageFragments.", ",");
                string[] valueString = processedString.Split(',');
                result.Add(new Vector2(float.Parse(valueString[0]), float.Parse(valueString[1])));
            }

            return result;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pckgFragment">A JSONObject containing a $ref string of the format "//@bundles.X/@packageFragments.Y".</param>
        /// <returns>A Vector2(X,Y) representing Bundle and Packagefragment number.</returns>
        private Vector2 ResolvePckgFragmentRef(JSONObject pckgFragment)
        {
            Vector2 result = new Vector2();
            string rawRefString = pckgFragment.GetField("$ref").str;
            string processedString = "";
            processedString = rawRefString.Replace("//@bundles.", "");
            processedString = processedString.Replace("/@packageFragments.", ",");
            string[] valueString = processedString.Split(',');
            result = new Vector2(float.Parse(valueString[0]), float.Parse(valueString[1]));

            return result;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CuRef">A JSONObject containing a $ref string of the format "//@bundles.X/@packageFragments.Y/@compilationUnits.Z..."</param>
        /// <returns>A Vector3(X,Y,Z) representing Bundle, Packagefragment and CompilationUnit number.</returns>
        private Vector3 ResolveCompilationUnitRef(JSONObject CuRef)
        {
            Vector3 result = new Vector3();
            string rawRefString = CuRef.GetField("$ref").str;
            string processedString = "";
            processedString = rawRefString.Replace("//@bundles.", "");
            processedString = processedString.Replace("/@packageFragments.", ",");
            processedString = processedString.Replace("/@compilationUnits.", ",");
            //Cut off after compilation unit, since we dont support nested types...yet
            int idx = processedString.LastIndexOf(",");
            processedString = processedString.Remove(idx + 2);

            string[] valueString = processedString.Split(',');
            result = new Vector3(float.Parse(valueString[0]), float.Parse(valueString[1]), float.Parse(valueString[2]));

            return result;
        }
               
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pckg">A JSONObject containing a $ref string of the format "//@packages.X".</param>
        /// <returns>A int representing the Package number X.</returns>
        private int ResolvePackageReferenceIdx(JSONObject pckg)
        {
            int result;
            string rawRefString = pckg.GetField("$ref").str;
            string processedString = "";
            processedString = rawRefString.Replace("//@packages.", "");
            result = int.Parse(processedString);

            return result;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pckg">A JSONObject containing a list of $ref strings of the format "//@packages.X".</param>
        /// <returns>A List<int> representing the Package numbers X.</returns>
        private List<int> ResolvePackageReferenceIdxList(JSONObject pckg)
        {
            List<int> result = new List<int>();
            foreach (JSONObject listEntry in pckg.list)
            {
                string rawRefString = listEntry.GetField("$ref").str;
                string processedString = "";
                processedString = rawRefString.Replace("//@packages.", "");
                int idx = int.Parse(processedString);
                result.Add(idx);
            }
            
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceRef">A JSONObject containing a list of $ref strings of the format "//@services.X".</param>
        /// <returns>A List<int> representing the service numbers X.</returns>
        private List<int> ResolveServiceReferenceIdxList(JSONObject serviceRef)
        {
            List<int> result = new List<int>();
            foreach (JSONObject listEntry in serviceRef.list)
            {
                string rawRefString = listEntry.GetField("$ref").str;
                string processedString = "";
                processedString = rawRefString.Replace("//@services.", "");
                int idx = int.Parse(processedString);
                result.Add(idx);
            }

            return result;
        }


        // get & set

        public OsgiProject getProject()
        {
            return currentProject;
        }
        public Status getStatus()
        {
            return status;
        }
        public void setStatus(Status newStatus)
        {
            status = newStatus;
        }
    }

}

