using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ArchitectureElements;
using DynamicGraphAlgoImplementation;
using Assets;
using System.Linq;
using GraphBasics;
using DynamicGraphAlgoImplementation.HistoryGraphManager;
using UnityEngine.UI;
using Neo4JDriver;
using OsgiViz.SoftwareArtifact;

public class DynamicGraphCalculation : MonoBehaviour
{

    public static DynamicGraphCalculation Instance { get { return instance; } }
    private static DynamicGraphCalculation instance; // The instance of this class.

    private Project project;

    private Dictionary<BundleMaster, MasterVertex> masterDict;
    private Dictionary<Bundle, HistoryGraphVertex> elementDict;
    private Dictionary<MasterVertex, Dictionary<MasterVertex, MasterEdge>> masterEdgeDict;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        Neo4J database = GameObject.Find("DatabaseObject").GetComponent<DatabaseAccess>().GetDatabase();
        Neo4JWriterGraph.SetDatabase(database);

        masterDict = new Dictionary<BundleMaster, MasterVertex>();
        elementDict = new Dictionary<Bundle, HistoryGraphVertex>();
        masterEdgeDict = new Dictionary<MasterVertex, Dictionary<MasterVertex, MasterEdge>>();


    }

    public IEnumerator GraphMain()
    {
        IslandVizUI.Instance.UpdateLoadingScreenUI("Calculating Island Positions", ""); // Update UI.

        project = GameObject.Find("DataObject").GetComponent<OSGi_Project_Script>().GetProject();

        List<Branch> branchList = project.GetBranches().Values.ToList<Branch>();
        branchList.Sort();
        foreach (Branch branch in branchList)
        {
            HistoryGraph historyGraph = new HistoryGraph(null);

            yield return FillHistoryGraphForBranch(historyGraph, branch);

            if (historyGraph.getFrameCount() == 0)
            {
                continue;
            }
            yield return Layout(historyGraph);

            yield return SetPositionsToBundleAndWriteDB(branch);
        }

        yield return null;
    }

    #region Create_dynamic-graph_datastructure_of_project_datastructure
    private IEnumerator FillHistoryGraphForBranch(HistoryGraph hG, Branch b)
    {
        if (b.GetCommits(false).Count == 0)
        {
            hG = null;
            yield break;
        }
        if (Constants.layoutAlgo == Constants.LayoutOption.HistoryForce)
        {
            yield return FillHistoryGraphForBranch_HistoryForce(hG, b);
        }
        else
        {
            yield return FillHistoryGraphForBranch_Master(hG, b);
        }
    }

    private float VertexRadiusCaclulation(Bundle b, Commit c)
    {
        int maxRingTotal = b.GetMaster().GetGrid().GetOuterAssignedTotal(c);
        int maxRingSegment = b.GetMaster().GetGrid().GetOuterAssignedFirstTwoSixths(c);

        if (maxRingTotal < 1)
        {
            maxRingTotal = 1;
        }
        if (maxRingSegment < 1)
        {
            maxRingSegment = 1;
        }

        float radiusTotal = Constants.GetRadiusFromRing(maxRingTotal);
        float radiusSegment = Constants.GetRadiusFromRing(maxRingSegment);

        float colliderDim = 3f;

        if (radiusSegment + 3 <= radiusTotal)
        {
            colliderDim = radiusTotal;
        }
        else
        {
            colliderDim = radiusSegment + 3;
        }
        return colliderDim+8;
    }

    private IEnumerator FillHistoryGraphForBranchCore(HistoryGraph hG, Branch b, List<Commit> commitList, int firstCommitToLoad, int firstCommitToLayout)
    {
        int totalCommitsToLoad = commitList.Count - firstCommitToLoad;
        for (int index = firstCommitToLoad; index < commitList.Count; index++)
        {
            DirectedGraph<HistoryGraphVertex, HistoryGraphEdge> currentGraph = new DirectedGraph<HistoryGraphVertex, HistoryGraphEdge>();
            int currentMaxImport = 0;
            int bundleCounter = 0;

            //Vertex Centered
            foreach (Bundle bundle in commitList[index].GetBundles())
            {
                //create new HistoryGraphVertex for Bundle and add position if possible
                bundleCounter++;
                HistoryGraphVertex vertex = new HistoryGraphVertex(null, false);
                elementDict.Add(bundle, vertex);
                //radius calculation
                float radius = VertexRadiusCaclulation(bundle, commitList[index]);
                vertex.SetRadius(radius);


                Vector2 pos;
                if ((pos = bundle.GetPosition()) != Vector2.negativeInfinity)
                {
                    vertex.setPosition(new Vector3(pos.x, 0, pos.y));
                }
                //create mastervertex if necessary and add vertex to it
                BundleMaster bundleMaster = bundle.GetMaster();
                MasterVertex vertexMaster;
                masterDict.TryGetValue(bundleMaster, out vertexMaster);
                if (vertexMaster == null)
                {
                    vertexMaster = new MasterVertex(null);
                    hG.AddMasterVertex(vertexMaster);
                }
                vertex.setMaster(vertexMaster);

                //if commit is first commit in branch bundle might have a predecessor in parent branch that is already layouted
                //if commit is not the first to be loaded, bundle might have a predecessor that is relevant
                if (index == 0 || index > firstCommitToLoad)
                {
                    Bundle predBundle = bundle.GetPrevious(b);
                    if (predBundle != null)
                    {
                        HistoryGraphVertex predVertex;
                        elementDict.TryGetValue(predBundle, out predVertex);
                        if (predVertex == null)
                        {
                            predVertex = new HistoryGraphVertex(null, false);
                            Vector2 predpos;
                            if ((predpos = bundle.GetPosition()) != Vector2.negativeInfinity)
                            {
                                vertex.setPosition(new Vector3(predpos.x, 0, predpos.y));
                            }
                            predVertex.setMaster(vertexMaster);
                        }
                        predVertex.SetNext(vertex);
                        vertex.SetPrevious(predVertex);
                    }
                }

                //if commit has to be layouted add Vertex to graph
                if (index >= firstCommitToLayout)
                {
                    currentGraph.AddVertex(vertex);
                }
                if (bundleCounter % 5 == 0)
                {
                    yield return null;
                }
            }
            //if commit has to be layouted add Edges
            if (index >= firstCommitToLayout)
            {
                foreach (Bundle bundle in commitList[index].GetBundles())
                {
                    //Collect import betwenn Bundles based on bundles importing packages
                    Dictionary<Bundle, int> importCount = new Dictionary<Bundle, int>();
                    foreach (Package importedPackage in bundle.getImportedPackages())
                    {
                        Bundle parentBundle = importedPackage.getBundle();
                        if (importCount.ContainsKey(parentBundle))
                        {
                            importCount[parentBundle]++;
                        }
                        else
                        {
                            importCount.Add(parentBundle, 1);
                        }
                    }
                    //write Edges to Graphs
                    foreach (KeyValuePair<Bundle, int> kvp in importCount)
                    {
                        HistoryGraphVertex source = elementDict[bundle];
                        HistoryGraphVertex target = elementDict[kvp.Key];
                        HistoryGraphEdge importEdge = new HistoryGraphEdge(source, target, false);
                        importEdge.setWeight(kvp.Value);
                        if (kvp.Value > currentMaxImport)
                        {
                            currentMaxImport = kvp.Value;
                        }
                        currentGraph.AddEdge(importEdge);

                        MasterEdge edgeMaster;
                        if (!masterEdgeDict.ContainsKey(source.getMaster()))
                        {
                            masterEdgeDict.Add(source.getMaster(), new Dictionary<MasterVertex, MasterEdge>());
                        }
                        if (masterEdgeDict[source.getMaster()].ContainsKey(target.getMaster()))
                        {
                            edgeMaster = masterEdgeDict[source.getMaster()][target.getMaster()];
                        }
                        else
                        {
                            edgeMaster = new MasterEdge(source.getMaster(), target.getMaster());
                            masterEdgeDict[source.getMaster()].Add(target.getMaster(), edgeMaster);
                            hG.AddMasterEdge(edgeMaster);
                        }
                        edgeMaster.incrementWeight(kvp.Value);
                        importEdge.setMaster(edgeMaster);

                    }
                    yield return null;
                }
                //only add graph if it has to be layouted
                hG.addNextFrame(currentGraph);

            }
            yield return null;
        }
    }

    private IEnumerator FillHistoryGraphForBranch_HistoryForce(HistoryGraph hG, Branch b)
    {
        //Find first not layouted commit in branch
        List<Commit> commitList = b.GetCommits(true);
        int i;
        for (i = 0; i < commitList.Count; i++)
        {
            if (!commitList[i].IsGraphLayouted())
            {
                break;
            }
        }
        if (i >= commitList.Count)
        {
            //all commits already layouted
            hG = null;
            yield break;
        }
        int firstCommitToLayout = i;
        int firstCommitToLoad = i - Constants.historyForceLayoutDepth;
        if (firstCommitToLoad < 0)
        {
            firstCommitToLoad = 0;
        }
        yield return FillHistoryGraphForBranchCore(hG, b, commitList, firstCommitToLoad, firstCommitToLayout);

    }
    private IEnumerator FillHistoryGraphForBranch_Master(HistoryGraph hG, Branch b)
    {
        //Find first not layouted commit in branch
        List<Commit> commitList = b.GetCommits(true);
        int i;
        for (i = 0; i < commitList.Count; i++)
        {
            if (!commitList[i].IsGraphLayouted())
            {
                break;
            }
        }
        if (i >= commitList.Count)
        {
            //all commits already layouted
            hG = null;
            yield break;
        }
        //For Master Layout always everything needs to be loaded and layouted
        yield return FillHistoryGraphForBranchCore(hG, b, commitList, 0, 0);
    }

    #endregion

    private IEnumerator Layout(HistoryGraph hG)
    {
        IslandHistoryGraphManager historyGraphManager;
        float planeRadius = Constants.planeRadius;
        switch (Constants.layoutAlgo)
        {

            case Constants.LayoutOption.HistoryForce:
                historyGraphManager = new Island_HistoryGraphManager_History(hG);
                yield return historyGraphManager.Init(planeRadius - 2.1f, 1.0f, 8.0f, 0.3f, 2000, 16, 4);
                break;
            case Constants.LayoutOption.Master:
                historyGraphManager = new Island_HistoryGraphManager_Master(hG);
                yield return historyGraphManager.Init(planeRadius - 2.1f, 1.0f, 4.0f, 0.3f, 1000, planeRadius - 2.1f);
                break;
            default:
                yield break;
        }

        for(int i = 0; i<hG.getFrameCount(); i++)
        {
            yield return historyGraphManager.LayoutFrame(i);
            IslandVizUI.Instance.UpdateLoadingScreenUI("Calculating Island Positions", (100*i/hG.getFrameCount()).ToString("0.00")+"%"); // Update UI.

        }

        //TODO hier rausschreiben der Layouts
        yield return null;
    }



    private IEnumerator SetPositionsToBundleAndWriteDB(Branch branch)
    {
        List<Dictionary<string, object>> parameterList = new List<Dictionary<string, object>>();
        yield return SetPositionToBundleCollectData(parameterList);

        if (Constants.writeNewValuesToDB)
        {
            yield return Neo4JWriterGraph.WritePositionsToDB(parameterList);
            yield return Neo4JWriterGraph.SetCommitsLayouted(branch.GetCommits(false));
        }
    }

    private IEnumerator SetPositionToBundleCollectData(List<Dictionary<string, object>> parameterList)
    {
        int counter = 0;
        foreach (KeyValuePair<Bundle, HistoryGraphVertex> kvp in elementDict)
        {
            counter++;
            Vector3 pos = kvp.Value.getPosition();
            kvp.Key.SetPosition(pos.x, pos.z);
            parameterList.Add(new Dictionary<string, object> { { "id", kvp.Key.GetNeoId() }, { "posX", pos.x }, { "posZ", pos.z } });
            if (counter % 20 == 0)
            {
                yield return null;
            }
        }

    }
}
