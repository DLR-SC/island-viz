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
        //Changed from 123 for History Database
        neo4j = new Neo4JDriver.Neo4J("bolt://localhost:7687", "neo4j", "asdf");
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

        List<string> interfaceNameList = new List<string>();


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

                    // Classes

                    result = neo4j.Transaction("MATCH (p:Package{fileName: '" + packageFileName + "'})-[h:CONTAINS]->(c:Class) " +
                        "RETURN c.name as className");
                    List<string> classNameList = result.Select(record => record["className"].As<string>()).ToList();
                    result = neo4j.Transaction("MATCH (p:Package{fileName: '" + packageFileName + "'})-[h:CONTAINS]->(c:Class) " +
                        "RETURN c.visibility as classModifier");
                    List<string> classModifier = result.Select(record => record["classModifier"].As<string>()).ToList();
                    result = neo4j.Transaction("MATCH (p:Package{fileName: '" + packageFileName + "'})-[h:CONTAINS]->(c:Class) " +
                        "RETURN c.linesOfCode as classLOC");
                    List<string> classLOC = result.Select(record => record["classLOC"].As<string>()).ToList();

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

                    // Interfaces

                    result = neo4j.Transaction("MATCH (p:Package{fileName: '" + packageFileName + "'})-[h:CONTAINS]->(i:Interface) " +
                        "RETURN i.name as interfaceName");
                    interfaceNameList = result.Select(record => record["interfaceName"].As<string>()).ToList();
                    result = neo4j.Transaction("MATCH (p:Package{fileName: '" + packageFileName + "'})-[h:CONTAINS]->(i:Interface) " +
                        "RETURN i.visibility as interfaceModifier");
                    List<string> interfaceModifier = result.Select(record => record["interfaceModifier"].As<string>()).ToList();                    

                    if (interfaceNameList != null && interfaceNameList.Count > 0 && interfaceModifier != null && interfaceModifier.Count > 0)
                    {
                        for (int classID = 0; classID < interfaceNameList.Count; classID++)
                        {
                            CompilationUnit compilationUnit = new CompilationUnit(interfaceNameList[classID], type.Interface, StringToModifier(interfaceModifier[classID]),
                                0, package); 

                            package.addCompilationUnit(compilationUnit);
                        }
                    }

                    // Enums

                    result = neo4j.Transaction("MATCH (p:Package{fileName: '" + packageFileName + "'})-[h:CONTAINS]->(e:Enumeration) " +
                        "RETURN e.name as enumName");
                    List<string> enumNameList = result.Select(record => record["enumName"].As<string>()).ToList();
                    result = neo4j.Transaction("MATCH (p:Package{fileName: '" + packageFileName + "'})-[h:CONTAINS]->(e:Enumeration) " +
                        "RETURN e.visibility as enumModifier");
                    List<string> enumModifier = result.Select(record => record["enumModifier"].As<string>()).ToList();
                    result = neo4j.Transaction("MATCH (p:Package{fileName: '" + packageFileName + "'})-[h:CONTAINS]->(e:Enumeration) " +
                        "RETURN e.linesOfCode as classLOC");
                    List<string> enumLOC = result.Select(record => record["classLOC"].As<string>()).ToList();

                    if (enumNameList != null && enumNameList.Count > 0 && enumModifier != null && enumModifier.Count > 0)
                    {
                        for (int enumID = 0; enumID < enumNameList.Count; enumID++)
                        {
                            if (enumLOC[enumID] == null || enumLOC[enumID] == "Null")
                            {
                                enumLOC[enumID] = "0";
                            }

                            CompilationUnit compilationUnit = new CompilationUnit(enumNameList[enumID], type.Enum, StringToModifier(enumModifier[enumID]),
                                long.Parse(enumLOC[enumID]), package);

                            package.addCompilationUnit(compilationUnit);
                        }
                    }

                    // TODO Abstract Class

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
            //CompilationUnit serviceCU = FindCompilationUnit(serviceName); // = null;

            CompilationUnit serviceCU = null;

            //result = neo4j.Transaction("MATCH (i:Interface{name: '" + serviceName + "'}) RETURN i.name as name");
            //List<string> interfaceList = result.Select(record => record["name"].As<string>()).ToList();

            //if (interfaceList != null && interfaceList.Count > 0 && serviceCU != null)
            if (interfaceNameList.Contains(serviceName))
            {
                //serviceCU = FindCompilationUnit(serviceName);
                //serviceCU.setServiceDeclaration(true);
                FindCompilationUnit(serviceName).setServiceDeclaration(true);
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

            result = neo4j.Transaction("MATCH (b:Bundle {bundleSymbolicName: '" + bundle.getSymbolicName() + "'})-[h:Contains]->(sc:ServiceComponent) RETURN sc.name as name"); 
            List<string> serviceComponentNames = result.Select(record => record["name"].As<string>()).ToList();

            if (serviceComponentNames != null && serviceComponentNames.Count > 0)
                continue;

            foreach (var serviceComponentFileName in serviceComponentNames)
            {
                result = neo4j.Transaction("MATCH (sc:ServiceComponent {fileName: '" + serviceComponentFileName + "'})-[h:PUBLISHES]->(c) RETURN c.name as name"); 
                string compilationUnitPublishedClassName = result.First()["name"].As<string>();

                if (compilationUnitPublishedClassName != null && compilationUnitPublishedClassName != "")
                {
                    FindCompilationUnit(compilationUnitPublishedClassName).setServiceComponentImpl(true);
                }

                ServiceComponent sc = new ServiceComponent(serviceComponentFileName, FindCompilationUnit(compilationUnitPublishedClassName));

                result = neo4j.Transaction("MATCH (sc:ServiceComponent {fileName: '" + serviceComponentFileName + "'})-[h:PROVIDES]->(s:Service) RETURN s.fileName as name"); 
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

    /// <summary>
    /// Read Software Structure from Lynns Neo4J Database for one commit with given id
    /// </summary>
    /// <param name="commitNeoId"></param>
    /// <returns></returns>
    public IEnumerator Construct(int commitNeoId)
    {
        Debug.Log("Starting OSGi-Project construction!");

        osgiProject = new OsgiProject("Default"); // TODO

        IStatementResult result = null;
        try
        {
            // Find all bundles
            result = neo4j.Transaction("MATCH (c:CommitImpl)-[:HAS]->(b:BundleImpl) WHERE id(c)=" + commitNeoId + " RETURN id(b), b.symbolicName, b.name");
        }
        catch (Exception e)
        {
            IslandVizUI.Instance.UpdateLoadingScreenUI("Connecting to Neo4J", "<color=red>Connection failed!</color>");
            throw e;
        }
        var bundleList = result.ToList();

        IslandVizUI.Instance.UpdateLoadingScreenUI("OSGi-Project from Neo4J", "");
        yield return null;

        List<string> interfaceNameList = new List<string>();


        if (bundleList == null || bundleList.Count == 0)
        {
            Debug.LogError("Neo4jOsgiConstructor: Project does not contain any Bundles!");
        }
        else
        {
            Debug.Log("Neo4jOsgiConstructor: Project contains " + bundleList.Count + " bundles.");
        }

        long maxLOC = 0;

        for (int bundleID = 0; bundleID < bundleList.Count; bundleID++)
        {
            //CreateBundle
            var bundleInfo = bundleList[bundleID];
            Bundle bundle = new Bundle(bundleInfo["b.name"].As<string>(), bundleInfo["b.symbolicName"].As<string>(), osgiProject);
            bundle.SetNeoId(bundleInfo["id(b)"].As<int>());
            osgiProject.addBundle(bundle);

            //GetPackages
            result = neo4j.Transaction("MATCH (b:BundleImpl)-[:USE]->(pf:PackageFragmentImpl)-[:BELONGS_TO]->(p:PackageImpl) WHERE id(b)=" + bundle.GetNeoId() + " RETURN id(pf), p.name, p.qualifiedName");

            var packageList = result.ToList();
            if (packageList != null && packageList.Count > 0)
            {
                for (int packageID = 0; packageID < packageList.Count; packageID++)
                {
                    //Create Package
                    var packageInfo = packageList[packageID];
                    Package package = new Package(bundle, packageInfo["p.name"].As<string>());
                    package.SetNeoId(packageInfo["id(pf)"].As<int>());
                    bundle.addPackage(package);

                    //Get CompUnits
                    result = neo4j.Transaction("MATCH (p:PackageFragmentImpl)-[:HAS]->(cu:CompilationUnitImpl)-[:CLASS]->(c) WHERE id(p)=" + package.GetNeoId() +
                    " AND (c:ClassImpl OR c:InterfaceImpl) AND NOT exists(() -[:NESTED_TYPES]->(c)) " +
                    "return id(cu), c.name, c:InterfaceImpl, cu.LOC");

                    var cuList = result.ToList();

                    if (cuList != null && cuList.Count > 0)
                    {
                        for (int classID = 0; classID < cuList.Count; classID++)
                        {
                            var cuInfo = cuList[classID];
                            CompilationUnit compilationUnit;
                            string name = cuInfo["c.name"].As<string>();
                            int loc = 0;
                            if (cuInfo["cu.LOC"] != null)
                            {
                                loc = cuInfo["cu.LOC"].As<int>();
                            }
                            if (cuInfo["c:InterfaceImpl"].As<bool>())
                            {
                                compilationUnit = new CompilationUnit(name, type.Interface, modifier.Default, loc, package);
                            }
                            else
                            {
                                compilationUnit = new CompilationUnit(name, type.Class, modifier.Default, loc, package);
                            }
                            if (compilationUnit.getLoc() > maxLOC)
                            {
                                maxLOC = compilationUnit.getLoc();
                            }
                            package.addCompilationUnit(compilationUnit);
                        }
                    }
                }

            }
            IslandVizUI.Instance.UpdateLoadingScreenUI("OSGi-Project from Neo4J", osgiProject.getBundles().Count + "/" + bundleList.Count + " Bundles loaded");
            yield return null;
        }

        GlobalVar.maximumLOCinProject = maxLOC;

        // Resolve import/export + construct ServiceComponents + build dependency graph
        BidirectionalGraph<GraphVertex, GraphEdge> dependencyGraph = osgiProject.getDependencyGraph();

        IslandVizUI.Instance.UpdateLoadingScreenUI("OSGi-Project from Neo4J", "...");

        //Construct and resolve ServiceComponents
        foreach (Bundle bundle in osgiProject.getBundles())
        {
            //Resolve Exports for Bundle
            result = neo4j.Transaction("MATCH (b:BundleImpl)-[h:EXPORT]->(pf:PackageFragmentImpl)-[:BELONGS_TO]->(p:PackageImpl) WHERE id(b)=" + bundle.GetNeoId()+
                        " RETURN p.name as name");
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

            result = neo4j.Transaction("MATCH (b:BundleImpl)-[h:IMPORT]->(pf:PackageFragmentImpl)-[:BELONGS_TO]->(p:PackageImpl) WHERE id(b)=" + bundle.GetNeoId()+
                        " RETURN p.name as name");
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

    private modifier StringToModifier (string input) // Public, Private, Protected, Static, Final, Default 
    {
        if (input == "public")
        {
            return modifier.Public;
        }
        else if (input == "private")
        {
            return modifier.Private;
        }
        else if (input == "protected")
        {
            return modifier.Protected;
        }
        else if (input == "static")
        {
            return modifier.Static;
        }
        else if (input == "final")
        {
            return modifier.Final;
        }
        else
        {
            return modifier.Default;
        }
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
