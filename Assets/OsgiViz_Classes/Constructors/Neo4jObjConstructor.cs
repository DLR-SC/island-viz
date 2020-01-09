using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neo4j.Driver;
using System.Linq;
using System;
using Neo4j.Driver.V1;
using OsgiViz.Core;
using UnityEngine.Assertions;
using QuickGraph;
using OsgiViz.SoftwareArtifact;
using OsgiViz.Relations;

/// <summary>
/// This class TODO
/// </summary>
public class Neo4jOsgiConstructor : MonoBehaviour {

    private Neo4JDriver.Neo4J neo4j; // Neo4J Server Connection instance
    private OsgiProject osgiProject;

    Dictionary<CypherCode, string> cypherStrings = new Dictionary<CypherCode, string>() // TODO
        {
            { CypherCode.Bundle, "Bundle" },
            { CypherCode.BundleID, "bundleSymbolicName" },
            { CypherCode.BundleName, "name"},
            { CypherCode.Package, "Package"},
            { CypherCode.PackageID, "fileName"},
            { CypherCode.Class, "Class"},
            { CypherCode.ClassName, "name"},
            { CypherCode.ClassLOC, "linesOfCode"},
            { CypherCode.Service, "Service"},
            { CypherCode.ServiceName, "name"},
            { CypherCode.ServiceID, "fileName"},
            { CypherCode.ServiceComponent, "ServiceComponent"},
            { CypherCode.Package_Contains_Class, "CONTAINS"},
            { CypherCode.Bundle_Export_Package, "EXPORTS"},
            { CypherCode.Bundle_Import_Package, "IMPORTS"},
            { CypherCode.Bundle_Contains_ServiceComponent, "CONTAINS"},
            { CypherCode.Bundle_Contains_Package, "HAS"},
            { CypherCode.ServiceComponent_Publisches_CompilationUnit, "PUBLISHES"}
        };

    enum CypherCode
    {
        Bundle,
        BundleID,
        BundleName,
        Package,
        PackageID,
        Class,
        ClassName,
        ClassLOC,
        Service,
        ServiceName,
        ServiceID,
        ServiceComponent,
        Package_Contains_Class,
        Bundle_Export_Package,
        Bundle_Import_Package,
        Bundle_Contains_ServiceComponent,
        Bundle_Contains_Package,
        ServiceComponent_Publisches_CompilationUnit
    }

    private void Start()
    {
        neo4j = new Neo4JDriver.Neo4J("bolt://localhost:7687", "neo4j", "123");
    }

    /// <summary>
    /// Extracts the OsgiProject from the Neo4J server and stores it into a local variable.
    /// </summary>
    /// <returns></returns>
    public IEnumerator Construct()
    {
        Debug.Log("Starting OSGi-Project construction!");

        osgiProject = new OsgiProject("Default"); // TODO


        IStatementResult result = null;
        try
        {
            // Find all bundles
            result = neo4j.Transaction("MATCH (b:" + cypherStrings[CypherCode.Bundle] + ") RETURN b.bundleSymbolicName as symbolicName");
        }
        catch(Exception e)
        {
            IslandVizUI.Instance.UpdateLoadingScreenUI("Connecting to Neo4J", "<color=red>Connection failed!</color>");
            throw e;
        }        
        List<string> bundlesymbolicNameList = result.Select(record => record["symbolicName"].As<string>()).ToList();

        IslandVizUI.Instance.UpdateLoadingScreenUI("OSGi-Project from Neo4J", "");
        yield return null;

        result = neo4j.Transaction("MATCH (b:Bundle) RETURN b.name as name");
        List<string> bundleNameList = result.Select(record => record["name"].As<string>()).ToList();

        if (bundleNameList == null || bundlesymbolicNameList == null || bundleNameList.Count == 0 || bundlesymbolicNameList.Count == 0)
        {
            Debug.LogError("Neo4jOsgiConstructor: Project does not contain any Bundles!");
        }
        else
        {
            Debug.Log("Neo4jOsgiConstructor: Project contains " + bundleNameList.Count + " bundles.");
        }

        long maxLOC = 0;

        for (int bundleID = 0; bundleID < bundleNameList.Count; bundleID++)
        {
            Bundle bundle = new Bundle(bundleNameList[bundleID], bundlesymbolicNameList[bundleID], osgiProject);

            result = neo4j.Transaction("MATCH (b:Bundle {bundleSymbolicName: '" + bundlesymbolicNameList[bundleID] + "'})-[h:HAS]->(p:Package) RETURN p.fileName as name"); // EXPORTS?
            List<string> packageFileNameList = result.Select(record => record["name"].As<string>()).ToList();

            if (packageFileNameList != null && packageFileNameList.Count > 0)
            {
                foreach (var packageFileName in packageFileNameList)
                {
                    Package package = new Package(bundle, packageFileName);

                    result = neo4j.Transaction("MATCH (p:Package{fileName: '" + packageFileName + "'})-[h:CONTAINS]->(c:Class) " +
                        "RETURN c.name as className");
                    List<string> classNameList = result.Select(record => record["className"].As<string>()).ToList();
                    result = neo4j.Transaction("MATCH (p:Package{fileName: '" + packageFileName + "'})-[h:CONTAINS]->(c:Class) " +
                        "RETURN c.visibility as classModifier");
                    List<string> classModifier = result.Select(record => record["classModifier"].As<string>()).ToList();
                    result = neo4j.Transaction("MATCH (p:Package{fileName: '" + packageFileName + "'})-[h:CONTAINS]->(c:Class) " +
                        "RETURN c.linesOfCode as classLOC");
                    List<string> classLOC = result.Select(record => record["classLOC"].As<string>()).ToList();

                    // TODO add Interfaces

                    if (classNameList != null && classNameList.Count > 0 && classModifier != null && classModifier.Count > 0 && classLOC != null && classLOC.Count > 0)
                    {
                        for (int classID = 0; classID < classNameList.Count; classID++)
                        {
                            if (classLOC[classID] == null || classLOC[classID] == "Null")
                            {
                                classLOC[classID] = "0";
                            }

                            CompilationUnit compilationUnit = new CompilationUnit(classNameList[classID], type.Class, StringToModifier(classModifier[classID]), 
                                long.Parse(classLOC[classID]), package); // TODO 

                            //TODO: Add support for additional information about a compilation unit(Methods, references to others)

                            if (compilationUnit.getLoc() > maxLOC)
                            {
                                maxLOC = compilationUnit.getLoc();
                            }
                            package.addCompilationUnit(compilationUnit);
                        }
                    }                    
                    bundle.addPackage(package);
                }
            }
            osgiProject.addBundle(bundle);
            IslandVizUI.Instance.UpdateLoadingScreenUI("OSGi-Project from Neo4J", osgiProject.getBundles().Count + "/" + bundleNameList.Count + " Bundles loaded");
            yield return null;
        }

        GlobalVar.maximumLOCinProject = maxLOC;

        IslandVizUI.Instance.UpdateLoadingScreenUI("OSGi-Project from Neo4J", "Loading Services...");

        yield return null;

        // Find all services
        result = neo4j.Transaction("MATCH (s:Service) RETURN s.fileName as fileName");
        List<string> serviceFileNameList = result.Select(record => record["fileName"].As<string>()).ToList();
        result = neo4j.Transaction("MATCH (s:Service) RETURN s.name as name");
        List<string> serviceNameList = result.Select(record => record["name"].As<string>()).ToList();
        if (serviceFileNameList == null || serviceNameList == null || serviceFileNameList.Count == 0 || serviceNameList.Count == 0)
        {
            Debug.LogError("Neo4jOsgiConstructor: Project does not contain any Services!");
        }
        else
        {
            Debug.Log("Neo4jOsgiConstructor: Project contains " + serviceNameList.Count + " services.");
        }

        foreach (var serviceName in serviceNameList)
        {
            CompilationUnit serviceCU = FindCompilationUnit(serviceName); // = null;

            result = neo4j.Transaction("MATCH (i:Interface{name: '" + serviceName + "'}) RETURN i.name as name");
            List<string> interfaceList = result.Select(record => record["name"].As<string>()).ToList();

            if (interfaceList != null && interfaceList.Count > 0 && serviceCU != null)
            {
                //serviceCU = FindCompilationUnit(serviceName);
                serviceCU.setServiceDeclaration(true);
            }

            Service service = new Service(serviceName, serviceCU);
            osgiProject.addService(service);
        }

        // Resolve import/export + construct ServiceComponents + build dependency graph
        BidirectionalGraph<GraphVertex, GraphEdge> dependencyGraph = osgiProject.getDependencyGraph();

        IslandVizUI.Instance.UpdateLoadingScreenUI("OSGi-Project from Neo4J", "...");

        //Construct and resolve ServiceComponents
        foreach (var bundle in osgiProject.getBundles())
        {
            //Resolve Exports for Bundle

            result = neo4j.Transaction("MATCH (b:Bundle{bundleSymbolicName: '" + bundle.getSymbolicName() + "'})-[h:EXPORTS]->(p:Package) " +
                        "RETURN p.fileName as name");
            List<string> packageFileNameList = result.Select(record => record["name"].As<string>()).ToList();

            if (packageFileNameList != null && packageFileNameList.Count > 0)
            {
                foreach (var packageFileName in packageFileNameList)
                {
                    Package p = FindPackage(packageFileName);
                    if (p != null)
                    {
                        p.setExport(true);
                        bundle.addExportedPackage(p);
                    }
                }                
            }


            //Resolve Imports for Bundle

            result = neo4j.Transaction("MATCH (b:Bundle{bundleSymbolicName: '" + bundle.getSymbolicName() + "'})-[h:IMPORTS]->(p:Package) " +
                        "RETURN p.fileName as name");
            packageFileNameList = result.Select(record => record["name"].As<string>()).ToList();

            if (packageFileNameList != null && packageFileNameList.Count > 0)
            {
                foreach (var packageFileName in packageFileNameList)
                {
                    Package p = FindPackage(packageFileName);
                    if (p != null && bundle.getName() != p.getBundle().getName()) // Ignore self Import redundancy
                    {
                        bundle.addImportedPackage(p);

                        //Package dependency
                        //Check if Vertices already in Graph

                        List<GraphVertex> allVertices = dependencyGraph.Vertices.ToList();
                        GraphVertex vert1 = allVertices.Find(v => (string.Equals(v.getName(), bundle.getName())));
                        GraphVertex vert2 = allVertices.Find(v => (string.Equals(v.getName(), p.getBundle().getName())));

                        if (vert1 == null)
                            vert1 = new GraphVertex(bundle.getName());
                        if (vert2 == null)
                            vert2 = new GraphVertex(p.getBundle().getName());

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

                        if (bidirectionalEdgeWeight > osgiProject.getMaxImportCount())
                            osgiProject.setMaxImportCount((int)bidirectionalEdgeWeight);
                    }
                }
            }

            //Construct and resolve ServiceComponents
            List<Service> serviceList = osgiProject.getServices();

            result = neo4j.Transaction("MATCH (b:Bundle {bundleSymbolicName: '" + bundle.getSymbolicName() + "'})-[h:Contains]->(sc:ServiceComponent) RETURN sc.fileName as name"); // EXPORTS?
            List<string> serviceComponentFileNames = result.Select(record => record["name"].As<string>()).ToList();

            if (serviceComponentFileNames != null && serviceComponentFileNames.Count > 0)
                continue;

            foreach (var serviceComponentFileName in serviceComponentFileNames)
            {
                result = neo4j.Transaction("MATCH (sc:ServiceComponent {fileName: '" + serviceComponentFileName + "'})-[h:PUBLISHES]->(c) RETURN c.fileName as name"); // EXPORTS?
                string compilationUnitName = result.First()["name"].As<string>();

                if (compilationUnitName != null && compilationUnitName != "")
                {
                    FindCompilationUnit(compilationUnitName).setServiceComponentImpl(true);
                }

                ServiceComponent sc = new ServiceComponent(serviceComponentFileName, FindCompilationUnit(compilationUnitName));

                result = neo4j.Transaction("MATCH (sc:ServiceComponent {fileName: '" + serviceComponentFileName + "'})-[h:PROVIDES]->(s:Service) RETURN s.fileName as name"); // EXPORTS?
                List<string> serviceFileNames = result.Select(record => record["name"].As<string>()).ToList();

                if (serviceFileNames != null && serviceFileNames.Count > 0)
                {
                    foreach (var serviceFileName in serviceFileNames)
                    {
                        sc.addProvidedService(FindService(serviceFileName));
                        FindService(serviceFileName).addImplementingComponent(sc);
                    }
                }

                result = neo4j.Transaction("MATCH (sc:ServiceComponent {fileName: '" + serviceComponentFileName + "'})<-[h:REFERENCES]-(s:Service) RETURN s.fileName as name"); // EXPORTS?
                serviceFileNames = result.Select(record => record["name"].As<string>()).ToList();

                if (serviceFileNames != null && serviceFileNames.Count > 0)
                {
                    foreach (var serviceFileName in serviceFileNames)
                    {
                        sc.addReferencedService(FindService(serviceFileName));
                        FindService(serviceFileName).addReferencingComponent(sc);
                    }
                }
                bundle.addServiceComponent(sc);
            }
        }

        Debug.Log("Finished OSGi-Project construction!");
    }



    // ################
    // Public Getter Methods
    // ################
    
    /// <summary>
    /// Returns the extracted data from the Neo4J server.
    /// </summary>
    /// <returns></returns>
    public OsgiProject GetOsgiProject()
    {
        return osgiProject;
    }



    // ################
    // Helper Functions
    // ################

    private modifier StringToModifier (string input) // TODO
    {
        //return (modifier)Enum.Parse(typeof(modifier), input);
        return modifier.Default;
    }

    /// <summary>
    /// Search for a CompilationUnit in the local OsgiProject.
    /// </summary>
    /// <param name="fileName">The fileName of the CompilationUnit.</param>
    /// <returns></returns>
    private CompilationUnit FindCompilationUnit (string fileName)
    {
        foreach (var b in osgiProject.getBundles())
        {
            foreach (var p in b.getPackages())
            {
                foreach (var cu in p.getCompilationUnits())
                {
                    if (cu.getName() == fileName)
                        return cu;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Search for a Package in the local OsgiProject.
    /// </summary>
    /// <param name="fileName">The fileName of the Package.</param>
    /// <returns></returns>
    private Package FindPackage (string fileName)
    {
        foreach (var b in osgiProject.getBundles())
        {
            foreach (var p in b.getPackages())
            {
                if (p.getName() == fileName)
                    return p;
            }
        }
        return null;
    }

    /// <summary>
    /// Search for a Service in the local OsgiProject.
    /// </summary>
    /// <param name="fileName">The fileName of the Service.</param>
    /// <returns></returns>
    private Service FindService(string fileName)
    {
        foreach (var s in osgiProject.getServices())
        {
            if (s.getName() == fileName)
            {
                return s;
            }
        }
        return null;
    }
}
