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

namespace OsgiViz.SideThreadConstructors
{


    public class OsgiProjectConstructor
    {

        private callbackMethod cb;
        private OsgiProject currentProject;
        private Status status;
        Thread _thread;
        private readonly string redundantString_A = "http://www.example.org/OSGiApplicationModel#//";
        private JSONObject jsonObj;

        public OsgiProjectConstructor()
        {
            currentProject = null;
            status = Status.Idle;
            jsonObj = null;
        }

        //Public method to construct an OsgiProject from a JSONObject in a separate thread
        public void Construct(JSONObject jObj, callbackMethod m)
        {
            cb = m;
            jsonObj = jObj;
            _thread = new Thread(ConstructProject);
            _thread.Start();  
        }

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

        private void ConstructProject()
        {

            #region OsgiProject
            status = Status.Working;
            Debug.Log("Starting OSGi-Project construction!");
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
                        int pkgIdx = resolvePackageReferenceIdx(jsonPkg);
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
                    }
                }
                currentProject.addBundle(currentBundle);
            }
            GlobalVar.maximumLOCinProject = maxLOC;
            #endregion

            #region Services
            List<Bundle> bundles = currentProject.getBundles();
            tmp = jsonObj.GetField("services");
            if(tmp != null)
            {
                List<JSONObject> serviceJsonList = tmp.list;
                foreach(JSONObject jsonService in serviceJsonList)
                {
                    
                    string serviceName = jsonService.GetField("interfaceName").str;
                    tmp = jsonService.GetField("interface");
                    CompilationUnit serviceCU = null;
                    if(tmp != null)
                    {
                        Vector3 cuIdx = resolveCompilationUnitRef(tmp);
                        serviceCU = bundles[(int)cuIdx.x].getPackages()[(int)cuIdx.y].getCompilationUnits()[(int)cuIdx.z];
                        serviceCU.setServiceDeclaration(true);
                    }
                    Service service = new Service(serviceName, serviceCU);
                    currentProject.addService(service);
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
                if(tmp != null)
                {
                    List<Vector2> exportList = resolvePckgFragmentRefList(tmp.list);
                    foreach(Vector2 indexVec in exportList)
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
                    List<Vector2> importList = resolvePckgFragmentRefList(tmp.list);
                    foreach (Vector2 indexVec in importList)
                    {
                        Package resolvedFragment = bundles[(int)indexVec.x].getPackages()[(int)indexVec.y];
                        // Ignore self Import redundancy
                        if (string.Compare(bundles[i].getName(), resolvedFragment.getBundle().getName()) != 0 )
                        {
                            bundles[i].addImportedPackage(resolvedFragment);

                            //Package dependency
                            //Check if Vertices already in Graph
                            List<GraphVertex> allVertices = dependencyGraph.Vertices.ToList();
                            GraphVertex vert1 = allVertices.Find(v => (string.Equals(v.getName(), bundles[i].getName())));
                            GraphVertex vert2 = allVertices.Find(v => (string.Equals(v.getName(), bundles[(int)indexVec.x].getName())));

                            if(vert1 == null)
                                vert1 = new GraphVertex(bundles[i].getName());
                            if(vert2 == null)
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
                            Debug.Log("Spotted import redundancy in: " + bundles[i].getName() );
                        }
                    }
                }
                //Construct and resolve ServiceComponents
                List<Service> serviceList = currentProject.getServices();
                tmp = jsonBundle.GetField("components");
                if(tmp != null)
                {
                    foreach(JSONObject jsonComponent in tmp.list)
                    {
                        string scName = jsonComponent.GetField("name").str;
                        Vector3 implIdx = resolveCompilationUnitRef(jsonComponent.GetField("implementation"));
                        CompilationUnit resolvedCu = bundles[(int)implIdx.x].getPackages()[(int)implIdx.y].getCompilationUnits()[(int)implIdx.z];
                        resolvedCu.setServiceComponentImpl(true);
                        ServiceComponent sc = new ServiceComponent(scName, resolvedCu);

                        tmp = jsonComponent.GetField("providedServices");
                        if(tmp != null)
                        {
                            List<int> serviceRefs = resolveServiceReferenceIdxList(tmp);
                            foreach (int s in serviceRefs)
                            {
                                sc.addProvidedService(serviceList[s]);
                                serviceList[s].addImplementingComponent(sc);
                            }
                        }
                        tmp = jsonComponent.GetField("referencedServices");
                        if (tmp != null)
                        {
                            List<int> serviceRefs = resolveServiceReferenceIdxList(tmp);
                            foreach (int s in serviceRefs)
                            {
                                sc.addReferencedService(serviceList[s]);
                                serviceList[s].addReferencingComponent(sc);
                            }
                        }
                        bundles[i].addServiceComponent(sc);
                    }
                }


                i++;
            }
            #endregion
            Debug.Log("Max Import-count: " + currentProject.getMaxImportCount()); 
            status = Status.Finished;
            Debug.Log("Finished OSGi-Project construction!");
            cb();
        }


        //Input: A list of JSONObjects containing a $ref string of the format "//@bundles.X/@packageFragments.Y"
        //Output: A list of Vector2(X,Y) representing Bundle and Packagefragment number
        private List<Vector2> resolvePckgFragmentRefList( List<JSONObject> pckgFragmentRefList)
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

        //Input: A JSONObject containing a $ref string of the format "//@bundles.X/@packageFragments.Y"
        //Output: A Vector2(X,Y) representing Bundle and Packagefragment number
        private Vector2 resolvePckgFragmentRef(JSONObject pckgFragment)
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

        //Input: A JSONObject containing a $ref string of the format "//@bundles.X/@packageFragments.Y/@compilationUnits.Z..."
        //Output: A Vector3(X,Y,Z) representing Bundle, Packagefragment and CompilationUnit number
        private Vector3 resolveCompilationUnitRef(JSONObject CuRef)
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


        //Input: A JSONObject containing a $ref string of the format "//@packages.X"
        //Output: A int representing the Package number X
        private int resolvePackageReferenceIdx(JSONObject pckg)
        {
            int result;
            string rawRefString = pckg.GetField("$ref").str;
            string processedString = "";
            processedString = rawRefString.Replace("//@packages.", "");
            result = int.Parse(processedString);

            return result;
        }

        //Input: A JSONObject containing a list of $ref strings of the format "//@packages.X"
        //Output: A List<int> representing the Package numbers X
        private List<int> resolvePackageReferenceIdxList(JSONObject pckg)
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

        //Input: A JSONObject containing a list of $ref strings of the format "//@services.X"
        //Output: A List<int> representing the service numbers X
        private List<int> resolveServiceReferenceIdxList(JSONObject serviceRef)
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


    }


}

