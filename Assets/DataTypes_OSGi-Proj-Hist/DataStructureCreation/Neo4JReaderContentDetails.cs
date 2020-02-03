using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4JDriver;
using Neo4j.Driver.V1;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ArchitectureElements;
using System.Collections;
using UnityEngine;
using OsgiViz.SoftwareArtifact;

namespace OSGI_Datatypes.DataStructureCreation
{
    public class Neo4JReaderContentDetails
    {

        private static Neo4J database;
        public static bool isGraphLayouted;
        public static bool isIslandsLayouted;

        public static void SetDatabase(Neo4J neo)
        {
            database = neo;
        }

        public static IEnumerator ReadCommitsBundlesDetails(Project project, Branch b, Commit c, Dictionary<int, Bundle> prevBundleDict, Dictionary<int, Bundle> nextBundleDict, Dictionary<int, Bundle> thisBundleDict, bool readLayoutInfo, bool printInfo)
        {
            if (readLayoutInfo)
            {
                isGraphLayouted = true;
            }

            string statement = "MATCH (c:CommitImpl)-[:HAS]->(b:BundleImpl) WHERE id(c)=$cid " +
             "OPTIONAL MATCH (bprev:BundleImpl)-[:NEXT]->(b) " +
             "OPTIONAL MATCH (b)-[:NEXT]->(bnext:BundleImpl) " +
             "return id(b) as neoId, b.name as bundleName, b.symbolicName as bundleSName, b.posX, b.posZ, " +
             "id(bprev), id(bnext) " +
             "ORDER BY b.symbolicName";
            Dictionary<string, object> parameters = new Dictionary<string, object> { { "cid", c.GetNeoId() } };

            IStatementResult result = database.ReadTransaktion(statement, parameters);
            var resList = result.ToList();

            yield return null;

            for (int i = 0; i < resList.Count; i++)
            {
                var bundleInfo = resList[i];
                //basic infos
                int id = bundleInfo["neoId"].As<int>();
                string name = bundleInfo["bundleName"].As<string>();
                string sName = bundleInfo["bundleSName"].As<string>();

                Bundle bundle = new Bundle(id, name, sName, c);
                if (thisBundleDict != null)
                {
                    thisBundleDict.Add(id, bundle);
                }

                //visualisation infos
                if (readLayoutInfo)
                {
                    if (bundleInfo["b.posX"] != null && bundleInfo["b.posZ"] != null)
                    {
                        float posX = bundleInfo["b.posX"].As<float>();
                        float posZ = bundleInfo["b.posZ"].As<float>();
                        bundle.SetPosition(posX, posZ);
                    }
                    else
                    {
                        isGraphLayouted = false;
                    }
                }
                yield return null;
                //history Infos
                if (prevBundleDict == null || prevBundleDict.Count == 0 || bundleInfo["id(bprev)"] == null )
                {
                    //New Bundle -> new Masterbundle
                    BundleMaster master = new BundleMaster(c, bundle);
                    bundle.SetMaster(master, c);
                    project.AddMasterBundles(master);
                }
                else
                {
                    int prevId = -1;
                    if (bundleInfo["id(bprev)"] != null)
                    {
                        prevId = bundleInfo["id(bprev)"].As<int>();
                    }
                    if (prevId != -1)
                    {
                        Bundle prevBundle;
                        prevBundleDict.TryGetValue(prevId, out prevBundle);
                        if (prevBundle != null)
                        {
                            bundle.AddPrevious(b, prevBundle, true);
                            bundle.SetMaster(prevBundle.GetMaster(), c);
                        }
                        else
                        {
                            Debug.LogWarning("In Commit " + c.GetNeoId() + " found bundle with id " + bundle.GetNeoId() + " that has predecessor with id " + prevId + " but this element cannot be found in local dataset");
                        }
                    }
                }
                //history info just for merge branch cases
                if (nextBundleDict != null)
                {
                    if (bundleInfo["id(bnext)"] != null )
                    {
                        int nextId = -1;
                        if (bundleInfo["id(bnext)"] != null)
                        {
                            nextId = bundleInfo["id(bnext)"].As<int>();
                        }
                        if (nextId != -1)
                        {
                            Bundle nextBundle;
                            nextBundleDict.TryGetValue(nextId, out nextBundle);
                            if (nextBundle != null)
                            {
                                bundle.AddNext(b, nextBundle, true);
                            }
                            else
                            {
                                Debug.LogWarning("In Commit " + c.GetNeoId() + " found bundle with id " + bundle.GetNeoId() + " that has successor with id " + nextId + " but this element cannot be found in local dataset");
                            }
                        }
                    }
                }
                if ((i + 1) % 10 == 0)
                {
                    yield return null;
                }

                if (printInfo)
                {
                    Debug.Log("Read Bundle" + bundle.getName());
                }
            }
        }

        public static IEnumerator ReadBundlesPackageDetails(Branch b, Commit c, Bundle bundle, Dictionary<int, Package> prevPackDict, Dictionary<int, Package> nextPackDict, bool readLayoutInfo, bool printInfo)
        {
            if (readLayoutInfo)
            {
                isIslandsLayouted = true;
            }

            string statement = "MATCH (b:BundleImpl)-[:USE]->(p:PackageFragmentImpl) WHERE id(b)=$bid " +
            "OPTIONAL MATCH(b)-[eRel: EXPORT]->(p) " +
            "OPTIONAL MATCH(pprev:PackageFragmentImpl)-[:NEXT]->(p) " +
            "OPTIONAL MATCH(p)-[:NEXT]->(pnext: PackageFragmentImpl) " +
            "RETURN id(p) as neoId, p.name as name, p.qualifiedName as qName, " +
            "p.startCellX as startX, p.startCellZ as startZ, " +
            "type(eRel) as Exported, " +
            "id(pprev), id(pnext), " +
            "ORDER BY name";

            Dictionary<string, object> parameters = new Dictionary<string, object> { { "bid", bundle.GetNeoId() } };

            IStatementResult result = database.ReadTransaktion(statement, parameters);
            var resList = result.ToList();

            yield return null;


            for (int i = 0; i < resList.Count; i++)
            {
                var packageInfo = resList[i];
                //basic infos
                int id = packageInfo["neoId"].As<int>();
                string name = packageInfo["name"].As<string>();
                string qName = packageInfo["qName"].As<string>();
                bool isExported = false;
                if (packageInfo["Exported"] != null && packageInfo["Exported"].As<string>().Equals("EXPORT"))
                {
                    isExported = true;
                }
                Package package = new Package(id, name, bundle, isExported);

                yield return null;

                //history Infos
                if (prevPackDict == null || prevPackDict.Count == 0 || packageInfo["id(pprev)"] == null )
                {
                    //New Package -> new Masterpackage
                    PackageMaster master = new PackageMaster(c, package, bundle.GetMaster());
                    package.SetMaster(master, c);
                    //visualisation infos (only stored in first Element
                    if (readLayoutInfo)
                    {
                        if (packageInfo["startX"] != null && packageInfo["startZ"] != null)
                        {
                            int posX = packageInfo["startX"].As<int>();
                            int posZ = packageInfo["startZ"].As<int>();
                            master.SetGrowCorridorStart(posX, posZ);
                        }
                        else
                        {
                            isIslandsLayouted = false;
                        }
                    }
                }
                else
                {
                    int prevId = -1;
                    if (packageInfo["id(pprev)"] != null)
                    {
                        prevId = packageInfo["id(pprev)"].As<int>();
                    }
                    if (prevId != -1)
                    {
                        Package prevPackage;
                        prevPackDict.TryGetValue(prevId, out prevPackage);
                        if (prevPackage != null)
                        {
                            package.AddPrevious(b, prevPackage, true);
                            package.SetMaster(prevPackage.GetMaster(), c);
                            prevPackDict.Remove(prevId);
                        }
                        else
                        {
                            Debug.LogWarning("In Commit " + c.GetNeoId() + " found package with id " + package.GetNeoId() + " that has predecessor with id " + prevId + " but this element cannot be found in local dataset");
                        }
                    }
                }
                //history info just for merge branch cases
                if (nextPackDict != null)
                {
                    if (packageInfo["id(pnext)"] != null )
                    {
                        int nextId = -1;
                        if (packageInfo["id(pnext)"] != null)
                        {
                            nextId = packageInfo["id(pnext)"].As<int>();
                        }
                        if (nextId != -1)
                        {
                            Package nextPackage;
                            nextPackDict.TryGetValue(nextId, out nextPackage);
                            if (nextPackage != null)
                            {
                                package.AddNext(b, nextPackage, true);
                            }
                            else
                            {
                                Debug.LogWarning("In Commit " + c.GetNeoId() + " found package with id " + package.GetNeoId() + " that has successor with id " + nextId + " but this element cannot be found in local dataset");
                            }
                        }
                    }
                }
                yield return null;
            }
            if (printInfo)
            {
                Debug.Log("Found " + bundle.getPackages().Count + " Packages in Bundle " + bundle.getName());
            }

            yield return null;
        }


        public static IEnumerator ReadPackagesCompUnitDetails(Branch b, Commit c, Package package, Dictionary<int, CompilationUnit> prevCompUnitDict, Dictionary<int, CompilationUnit> nextCompUnitDict, bool readLayoutInfo, bool printInfo)
        {
            //Remeber: No Correspond relation between compUnits possible (only equal) so no need to search for correspond
            string statement = "MATCH (p:PackageFragmentImpl)-[:HAS]->(cu:CompilationUnitImpl)-[:CLASS]->(c) " +
                "WHERE id(p)= $pid AND(c: ClassImpl OR c: InterfaceImpl) AND NOT exists(() -[:NESTED_TYPES]->(c)) " +
                "OPTIONAL MATCH(cuprev:CompilationUnitImpl)-[:NEXT]->(cu) " +
                "OPTIONAL MATCH(cu)-[:NEXT]->(cunext: CompilationUnitImpl) " +
                "return id(cu) AS neoId, c.name AS name, c.qualifiedName AS qName, c: InterfaceImpl as isInterface, " +
                "cu.loc as loc, cu.cellX as cellX, cu.cellZ as cellZ, " +
                "id(cuprev), id(cunext) ORDER BY name";

            Dictionary<string, object> parameters = new Dictionary<string, object> { { "pid", package.GetNeoId() } };

            IStatementResult result = database.ReadTransaktion(statement, parameters);
            var resList = result.ToList();

            yield return null;

            for (int i = 0; i < resList.Count; i++)
            {
                var cuInfo = resList[i];
                //basic infos
                int id = cuInfo["neoId"].As<int>();
                string name = cuInfo["name"].As<string>();
                string qName = cuInfo["qName"].As<string>();
                bool isInterface = cuInfo["isInterface"].As<bool>();
                OsgiViz.Core.type t = OsgiViz.Core.type.Class;
                if (isInterface)
                {
                    t = OsgiViz.Core.type.Interface;
                }
                int loc = 0;
                if (cuInfo["loc"] != null)
                {
                    loc = cuInfo["loc"].As<int>();
                }

                CompilationUnit compUnit = new CompilationUnit(id, name, t, OsgiViz.Core.modifier.Default, loc, package);

                yield return null;

                //history Infos
                if (prevCompUnitDict == null || prevCompUnitDict.Count == 0 ||  cuInfo["id(cuprev)"] == null)
                {
                    //New Package -> new Masterpackage
                    CompUnitMaster master = new CompUnitMaster(c, compUnit, package.GetMaster());
                    compUnit.SetMaster(master, c);
                    //visualisation infos (only stored in first Element
                    if (readLayoutInfo)
                    {
                        if (cuInfo["cellX"] != null && cuInfo["cellZ"] != null)
                        {
                            int posX = cuInfo["cellX"].As<int>();
                            int posZ = cuInfo["cellZ"].As<int>();
                            master.SetGridInfo(posX, posZ);
                        }
                        else
                        {
                            isIslandsLayouted = false;
                        }
                    }
                }
                else
                {
                    int prevId = -1;
                    if (cuInfo["id(cuprev)"] != null)
                    {
                        prevId = cuInfo["id(cuprev)"].As<int>();
                    }
                    
                    if (prevId != -1)
                    {
                        CompilationUnit prevCompUnit;
                        prevCompUnitDict.TryGetValue(prevId, out prevCompUnit);
                        if (prevCompUnit != null)
                        {
                            compUnit.AddPrevious(b, prevCompUnit, true);
                            compUnit.SetMaster(prevCompUnit.GetMaster(), c);
                            prevCompUnitDict.Remove(prevId);
                        }
                        else
                        {
                            Debug.LogWarning("In Commit " + c.GetNeoId() + " found compUnit with id " + compUnit.GetNeoId() + " that has predecessor with id " + prevId + " but this element cannot be found in local dataset");
                        }
                    }
                }
                //history info just for merge branch cases
                if (nextCompUnitDict != null)
                {
                    if (cuInfo["id(pnext)"] != null)
                    {
                        int nextId = -1;
                        if (cuInfo["id(cunext)"] != null)
                        {
                            nextId = cuInfo["id(cunext)"].As<int>();
                        }
                        if (nextId != -1)
                        {
                            CompilationUnit nextCompUnit;
                            nextCompUnitDict.TryGetValue(nextId, out nextCompUnit);
                            if (nextCompUnit != null)
                            {
                                compUnit.AddNext(b, nextCompUnit, true);
                            }
                            else
                            {
                                Debug.LogWarning("In Commit " + c.GetNeoId() + " found compUnit with id " + compUnit.GetNeoId() + " that has successor with id " + nextId + " but this element cannot be found in local dataset");
                            }
                        }
                    }
                }
                yield return null;
            }

            if (printInfo)
            {
                Debug.Log("Found " + package.getCompilationUnits().Count + " CompUnits in Package " + package.getName());
            }
            yield return null;
        }
        public static IEnumerator ReadCommitsImportRelations(Commit c, bool printInfo)
        {
            Dictionary<int, Bundle> bundleDict = c.GetBundleDictionary();
            Dictionary<int, Package> packageDict = c.GetPackageDictionary();
            yield return null;

            yield return ReadCommitsImportRelations(c, bundleDict, packageDict, printInfo);
        }


        public static IEnumerator ReadCommitsImportRelations(Commit c, Dictionary<int, Bundle> bundleDict, Dictionary<int, Package> packageDict, bool printInfo)
        {
            string statement = "MATCH (c:CommitImpl)-[:HAS]->(b:BundleImpl) WHERE id(c)=$cid " +
                "OPTIONAL MATCH (b)-[:REQUIRED_BUNDLE]->(bReq:BundleImpl) with b, collect(distinct id(bReq)) AS breqlist " +
                "OPTIONAL MATCH (b)-[:IMPORT]->(iPack:PackageFragmentImpl) WHERE not exists((iPack) -[:BUNDLE]->(b)) " +
                "with b, breqlist, collect(distinct id(iPack)) AS ipacklist " +
                "where length(breqlist)> 0 OR length(ipacklist)> 0 " +
                "RETURN id(b) AS bundle, breqlist AS reqBund, ipacklist AS importPacks order by bundle";
            Dictionary<string, object> parameters = new Dictionary<string, object> { { "cid", c.GetNeoId() } };

            IStatementResult result = database.ReadTransaktion(statement, parameters);
            var resList = result.ToList();
            yield return null;

            for (int i = 0; i < resList.Count; i++)
            {
                var bInfo = resList[i];
                int bId = bInfo["bundle"].As<int>();
                List<int> reqBundleIds = null;
                if (bInfo["reqBund"] != null)
                {
                    reqBundleIds = bInfo["reqBund"].As<List<int>>();
                }
                List<int> impPack = null;
                if (bInfo["importPacks"] != null)
                {
                    impPack = bInfo["importPacks"].As<List<int>>();
                }
                //Get Relevant Bundle
                Bundle currentBundle;
                bundleDict.TryGetValue(bId, out currentBundle);
                if (currentBundle == null)
                {
                    Debug.LogWarning("When Adding import relations to commit " + c.GetNeoId() + " found Bundle " + bId + " that wasn't found previously when reading commits artefacts");
                    continue;
                }
                //Required Bundle Relations
                if (reqBundleIds != null && reqBundleIds.Count > 0)
                {
                    for (int j = 0; j < reqBundleIds.Count; j++)
                    {
                        Bundle reqBundle;
                        bundleDict.TryGetValue(reqBundleIds[j], out reqBundle);
                        if (reqBundle == null)
                        {
                            Debug.LogWarning("When Adding import relations to commit " + c.GetNeoId() + " found Bundle " + bId + " that requires Bundle " + reqBundleIds[j] + " that wasn't found previously when reading commits artefacts");
                            continue;
                        }
                        currentBundle.AddRequiredBundle(reqBundle);
                    }
                }
                yield return null;
                //ImportPackage Relation
                if (impPack != null && impPack.Count > 0)
                {
                    for (int j = 0; j < impPack.Count; j++)
                    {
                        Package importedPack;
                        packageDict.TryGetValue(impPack[j], out importedPack);
                        if (importedPack == null)
                        {
                            Debug.LogWarning("When Adding import relations to commit " + c.GetNeoId() + " found Bundle " + bId + " that imports Package " + impPack[j] + " that wasn't found previously when reading commits artefacts");
                            continue;
                        }
                        currentBundle.addImportedPackage(importedPack);
                    }
                }
                if (i % 10 == 0)
                {
                    yield return null;
                }

            }
            yield return null;
        }

        public static IEnumerator ReadCommitsServiceLayer(Commit c, bool printInfo)
        {
            Dictionary<int, CompilationUnit> cuDict = c.GetCompUnitDictionary();
            yield return null;
            yield return ReadCommitsServiceLayer(c, cuDict, printInfo);
        }

        public static IEnumerator ReadCommitsServiceLayer(Commit c, Dictionary<int, CompilationUnit> cuDict, bool printInfo)
        {
            Dictionary<int, Service> serviceDict = new Dictionary<int, Service>();
            yield return ReadCommitsServices(c, cuDict, serviceDict, printInfo);

            yield return ReadCommitsServiceComponents(c, cuDict, serviceDict, printInfo);

            cuDict = null;
            serviceDict = null;

            yield return null;
        }

        private static IEnumerator ReadCommitsServices(Commit c, Dictionary<int, CompilationUnit> compUnitDict, Dictionary<int, Service> serviceDict, bool printInfo)
        {
            string statement = "MATCH (c:CommitImpl)-[:HAS]->(:BundleImpl)-[:HAS]->(:ServiceComponentImpl)-->(s:ServiceImpl)-[:IMPLEMENT]->(i:InterfaceImpl), " +
                "(cu)-[:CLASS]->(i) " +
                "WHERE id(c)=$cid " +
                "with distinct s, cu " +
                "return id(s) AS sId, id(cu) AS cuId order by sId";
            Dictionary<string, object> parameters = new Dictionary<string, object> { { "cid", c.GetNeoId() } };

            IStatementResult result = database.ReadTransaktion(statement, parameters);
            var resList = result.ToList();
            yield return null;


            for (int i = 0; i < resList.Count; i++)
            {
                var sInfo = resList[i];
                int sId = sInfo["sId"].As<int>();
                int cuId = sInfo["cuId"].As<int>();

                //Find CompilationUnit that is Interface
                CompilationUnit cuE;
                compUnitDict.TryGetValue(cuId, out cuE);
                if (cuE == null)
                {
                    Debug.LogWarning("When reading Services of commit " + c.GetNeoId() + " found Service " + sId + " that has interface compUnit " + cuId + " that wasn't found previously when reading commits artefacts");
                    continue;
                }
                Service service = new Service(sId, cuE);
                c.AddService(service);
                serviceDict.Add(sId, service);
                yield return null;
            }
        }

        private static IEnumerator ReadCommitsServiceComponents(Commit c, Dictionary<int, CompilationUnit> compUnitDict, Dictionary<int, Service> serviceDict, bool printInfo)
        {
            string statement = "MATCH (c:CommitImpl)-[:HAS]->(:BundleImpl)-[:HAS]->(sc:ServiceComponentImpl)-[:HAS]->(cl:ClassImpl), " +
                "(cu: CompilationUnitImpl) -[:CLASS]->(cl) " +
                "WHERE id(c) = $idc " +
                "OPTIONAL MATCH (sc)-[:PROVIDED]->(pS: ServiceImpl) " +
                "OPTIONAL MATCH (sc)-[:REFERENCED]->(rS: ServiceImpl) " +
                "WITH sc, COLLECT(DISTINCT id(cu)) as cuList, COLLECT(DISTINCT id(pS)) as pSList, COLLECT(DISTINCT id(rS)) as rSList " +
                "where length(pSList)> 0 or length(rSList)> 0 " +
                "return id(sc), sc.name, cuList, pSList, rSList ORDER BY id(sc)";
            Dictionary<string, object> parameters = new Dictionary<string, object> { { "cid", c.GetNeoId() } };

            IStatementResult result = database.ReadTransaktion(statement, parameters);
            var resList = result.ToList();
            yield return null;

            for (int i = 0; i < resList.Count; i++)
            {
                var scInfo = resList[i];
                int id = scInfo["id(sc)"].As<int>();
                string name = null;
                if (scInfo["sc.name"] != null)
                {
                    name = scInfo["sc.name"].As<string>();
                }
                CompilationUnit cu = null;
                if (scInfo["culist"] != null)
                {
                    List<int> ids = scInfo["culist"].As<List<int>>();
                    if (ids.Count == 0)
                    {
                        Debug.LogWarning("When reading ServicesComponents of commit " + c.GetNeoId() + " found ServiceComponent " + id + " that has no compUnit");
                        continue;
                    }
                    else if (ids.Count > 1)
                    {
                        Debug.LogWarning("When reading ServicesComponents of commit " + c.GetNeoId() + " found ServiceComponent " + id + " that has more than one compUnit. Others than first will be ignored");
                    }
                    compUnitDict.TryGetValue(ids[0], out cu);
                    if (cu == null)
                    {
                        Debug.LogWarning("When reading ServicesComponents of commit " + c.GetNeoId() + " foundServiceComponent " + id + " that has compUnit " + ids[0] + " that wasn't found previously when reading commits artefacts");
                        continue;
                    }
                }
                ServiceComponent sc = new ServiceComponent(id, name, cu);
                c.AddServiceComponent(sc);

                //providedServices
                if (scInfo["pSList"] != null)
                {
                    List<int> psIds = scInfo["pSList"].As<List<int>>();
                    for (int j = 0; j < psIds.Count; j++)
                    {
                        Service pS;
                        serviceDict.TryGetValue(psIds[j], out pS);
                        if (pS == null)
                        {
                            Debug.LogWarning("When reading ServicesComponent " + id + " found provided Service " + psIds[j] + " that wasn't found previously when reading commits services");
                            continue;
                        }
                        sc.addProvidedService(pS);
                    }
                }
                //referenced Services
                if (scInfo["rSList"] != null)
                {
                    List<int> rsIds = scInfo["pSList"].As<List<int>>();
                    for (int j = 0; j < rsIds.Count; j++)
                    {
                        Service rS;
                        serviceDict.TryGetValue(rsIds[j], out rS);
                        if (rS == null)
                        {
                            Debug.LogWarning("When reading ServicesComponent " + id + " found referenced Service " + rsIds[j] + " that wasn't found previously when reading commits services");
                            continue;
                        }
                        sc.addReferencedService(rS);
                    }
                }
                yield return null;
            }
            yield return null;
        }


        #region AlternativeReading

        public static IEnumerator ReadCommitsPackageDetails_New(Branch b, Commit c, Dictionary<int, Package> prevPackDict, Dictionary<int, Package> nextPackDict, Dictionary<int, Package> thisPackDict, Dictionary<int, Bundle> thisBundleDict, bool readLayoutInfo, bool printInfo)
        {
            if (readLayoutInfo)
            {
                isIslandsLayouted = true;
            }

            string statement = "MATCH (c:CommitImpl)-[:HAS]->(b:BundleImpl)-[:USE]->(p:PackageFragmentImpl) WHERE id(c)=$cid " +
            "OPTIONAL MATCH(b)-[eRel: EXPORT]->(p) " +
            "OPTIONAL MATCH(pprev:PackageFragmentImpl)-[:NEXT]->(p) " +
            "OPTIONAL MATCH(p)-[:NEXT]->(pnext: PackageFragmentImpl) " +
            "RETURN id(b) as bundleId, id(p) as neoId, p.name as name, p.qualifiedName as qName, " +
            "p.startCellX as startX, p.startCellZ as startZ, " +
            "type(eRel) as Exported, " +
            "id(pprev), id(pnext) " +
            "ORDER BY bundleId, name";

            Dictionary<string, object> parameters = new Dictionary<string, object> { { "cid", c.GetNeoId() } };

            IStatementResult result = database.ReadTransaktion(statement, parameters);
            var resList = result.ToList();

            yield return null;

            Bundle bundle = null;

            for (int i = 0; i < resList.Count; i++)
            {
                var packageInfo = resList[i];
                int bundleId = packageInfo["bundleId"].As<int>();
                //get Bundle
                if (bundle == null || bundle.GetNeoId() != bundleId)
                {
                    thisBundleDict.TryGetValue(bundleId, out bundle);
                    if (bundle == null)
                    {
                        Debug.LogWarning("In Commit " + c.GetNeoId() + " found package of bundle " + bundleId + ". This bundle cannot be found in local dataset");
                        continue;
                    }
                }

                //basic infos
                int id = packageInfo["neoId"].As<int>();
                string name = packageInfo["name"].As<string>();
                string qName = packageInfo["qName"].As<string>();
                bool isExported = false;
                if (packageInfo["Exported"] != null && packageInfo["Exported"].As<string>().Equals("EXPORT"))
                {
                    isExported = true;
                }
                Package package = new Package(id, name, bundle, isExported);
                thisPackDict.Add(id, package);

                yield return null;

                //history Infos
                if (prevPackDict == null || prevPackDict.Count == 0 || packageInfo["id(pprev)"] == null )
                {
                    //New Package -> new Masterpackage
                    PackageMaster master = new PackageMaster(c, package, bundle.GetMaster());
                    package.SetMaster(master, c);
                    //visualisation infos (only stored in first Element
                    if (readLayoutInfo)
                    {
                        if (packageInfo["startX"] != null && packageInfo["startZ"] != null)
                        {
                            int posX = packageInfo["startX"].As<int>();
                            int posZ = packageInfo["startZ"].As<int>();
                            master.SetGrowCorridorStart(posX, posZ);
                        }
                        else
                        {
                            isIslandsLayouted = false;
                        }
                    }
                }
                else
                {
                    int prevId = -1;
                    if (packageInfo["id(pprev)"] != null)
                    {
                        prevId = packageInfo["id(pprev)"].As<int>();
                    }
                    if (prevId != -1)
                    {
                        Package prevPackage;
                        prevPackDict.TryGetValue(prevId, out prevPackage);
                        if (prevPackage != null)
                        {
                            package.AddPrevious(b, prevPackage, true);
                            package.SetMaster(prevPackage.GetMaster(), c);
                            prevPackDict.Remove(prevId);
                        }
                        else
                        {
                            Debug.LogWarning("In Commit " + c.GetNeoId() + " found package with id " + package.GetNeoId() + " that has predecessor with id " + prevId + " but this element cannot be found in local dataset");
                        }
                    }
                }
                //history info just for merge branch cases
                if (nextPackDict != null)
                {
                    if (packageInfo["id(pnext)"] != null)
                    {
                        int nextId = -1;
                        if (packageInfo["id(pnext)"] != null)
                        {
                            nextId = packageInfo["id(pnext)"].As<int>();
                        }
                        if (nextId != -1)
                        {
                            Package nextPackage;
                            nextPackDict.TryGetValue(nextId, out nextPackage);
                            if (nextPackage != null)
                            {
                                package.AddNext(b, nextPackage, true);
                            }
                            else
                            {
                                Debug.LogWarning("In Commit " + c.GetNeoId() + " found package with id " + package.GetNeoId() + " that has successor with id " + nextId + " but this element cannot be found in local dataset");
                            }
                        }
                    }
                }
                if ((i + 1) % 10 == 0)
                {
                    yield return null;
                }
            }
            if (printInfo)
            {
                Debug.Log("Found " + bundle.getPackages().Count + " Packages in Bundle " + bundle.getName());
            }

            yield return null;
        }

        public static IEnumerator ReadCommitsCompUnitDetails_New(Branch b, Commit c, Dictionary<int, CompilationUnit> prevCompUnitDict, Dictionary<int, CompilationUnit> nextCompUnitDict, Dictionary<int, CompilationUnit> thisCompUnitDict, Dictionary<int, Package> thisPackageDict, bool readLayoutInfo, bool printInfo)
        {
            //Remeber: No Correspond relation between compUnits possible (only equal) so no need to search for correspond
            string statement = "MATCH (com:CommitImpl)-[:HAS]->(b:BundleImpl)-[:USE]->(p:PackageFragmentImpl)-[:HAS]->(cu:CompilationUnitImpl)-[:CLASS]->(c) WHERE id(com)= $cid " +
                "AND(c: ClassImpl OR c: InterfaceImpl) AND NOT exists(() -[:NESTED_TYPES]->(c)) " +
                "OPTIONAL MATCH(cuprev:CompilationUnitImpl)-[:NEXT]->(cu) " +
                "OPTIONAL MATCH(cu)-[:NEXT]->(cunext: CompilationUnitImpl) " +
                "return id(p) AS packageId, id(cu) AS neoId, c.name AS name, c.qualifiedName AS qName, c: InterfaceImpl as isInterface, " +
                "cu.LOC as loc, cu.posX as cellX, cu.posZ as cellZ, " +
                "id(cuprev), id(cunext) ORDER BY packageId, name";

            Dictionary<string, object> parameters = new Dictionary<string, object> { { "cid", c.GetNeoId() } };

            IStatementResult result = database.ReadTransaktion(statement, parameters);
            var resList = result.ToList();

            yield return null;

            Package package = null;

            for (int i = 0; i < resList.Count; i++)
            {
                var cuInfo = resList[i];

                int packageId = cuInfo["packageId"].As<int>();
                //get Package
                if (package == null || package.GetNeoId() != packageId)
                {
                    thisPackageDict.TryGetValue(packageId, out package);
                    if (package == null)
                    {
                        Debug.LogWarning("In Commit " + c.GetNeoId() + " found compUnit of package " + packageId + ". This package cannot be found in local dataset");
                        continue;
                    }
                }

                //basic infos
                int id = cuInfo["neoId"].As<int>();
                string name = cuInfo["name"].As<string>();
                string qName = cuInfo["qName"].As<string>();
                bool isInterface = cuInfo["isInterface"].As<bool>();
                OsgiViz.Core.type t = OsgiViz.Core.type.Class;
                if (isInterface)
                {
                    t = OsgiViz.Core.type.Interface;
                }
                int loc = 0;
                if (cuInfo["loc"] != null)
                {
                    loc = cuInfo["loc"].As<int>();
                }

                CompilationUnit compUnit = new CompilationUnit(id, name, t, OsgiViz.Core.modifier.Default, loc, package);
                thisCompUnitDict.Add(id, compUnit);
                yield return null;

                //history Infos
                if (prevCompUnitDict == null || prevCompUnitDict.Count == 0 || cuInfo["id(cuprev)"] == null)
                {
                    //New Package -> new Masterpackage
                    CompUnitMaster master = new CompUnitMaster(c, compUnit, package.GetMaster());
                    compUnit.SetMaster(master, c);
                    //visualisation infos (only stored in first Element
                    if (readLayoutInfo)
                    {
                        if (cuInfo["cellX"] != null && cuInfo["cellZ"] != null)
                        {
                            int posX = cuInfo["cellX"].As<int>();
                            int posZ = cuInfo["cellZ"].As<int>();
                            master.SetGridInfo(posX, posZ);
                        }
                        else
                        {
                            isIslandsLayouted = false;
                        }
                    }
                }
                else
                {
                    int prevId = -1;
                    if (cuInfo["id(cuprev)"] != null)
                    {
                        prevId = cuInfo["id(cuprev)"].As<int>();
                    }
                    else
                    {
                        Debug.LogWarning("In Commit " + c.GetNeoId() + " found compUnit with id " + compUnit.GetNeoId() + " that hasn't tag \"New\" but also no predecessor");
                    }
                    if (prevId != -1)
                    {
                        CompilationUnit prevCompUnit;
                        prevCompUnitDict.TryGetValue(prevId, out prevCompUnit);
                        if (prevCompUnit != null)
                        {
                            compUnit.AddPrevious(b, prevCompUnit, true);
                            compUnit.SetMaster(prevCompUnit.GetMaster(), c);
                            prevCompUnitDict.Remove(prevId);
                        }
                        else
                        {
                            Debug.LogWarning("In Commit " + c.GetNeoId() + " found compUnit with id " + compUnit.GetNeoId() + " that has predecessor with id " + prevId + " but this element cannot be found in local dataset");
                        }
                    }
                }
                //history info just for merge branch cases
                if (nextCompUnitDict != null)
                {
                    if (cuInfo["id(pnext)"] != null)
                    {
                        int nextId = -1;
                        if (cuInfo["id(cunext)"] != null)
                        {
                            nextId = cuInfo["id(cunext)"].As<int>();
                        }
                        if (nextId != -1)
                        {
                            CompilationUnit nextCompUnit;
                            nextCompUnitDict.TryGetValue(nextId, out nextCompUnit);
                            if (nextCompUnit != null)
                            {
                                compUnit.AddNext(b, nextCompUnit, true);
                            }
                            else
                            {
                                Debug.LogWarning("In Commit " + c.GetNeoId() + " found compUnit with id " + compUnit.GetNeoId() + " that has successor with id " + nextId + " but this element cannot be found in local dataset");
                            }
                        }
                    }
                }
                if ((i + 1) % 10 == 0)
                {
                    yield return null;
                }
            }

            if (printInfo)
            {
                Debug.Log("Found " + package.getCompilationUnits().Count + " CompUnits in Package " + package.getName());
            }
            yield return null;
        }

        #endregion


    }
}
