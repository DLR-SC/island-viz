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

public class DynamicGraphCalculation : MonoBehaviour
{

    [SerializeField]
    private Text taskTextfield;
    [SerializeField]
    private Text statusTextfield;
    [SerializeField]
    private Text loadingDotsTextfield;

    private Project project;

    private Dictionary<BundleMaster, MasterVertex> masterDict;
    private Dictionary<BundleElement, HistoryGraphVertex> elementDict;
    private Dictionary<MasterVertex, Dictionary<MasterVertex, MasterEdge>> masterEdgeDict;

    // Start is called before the first frame update
    void Start()
    {
        Neo4J database = GameObject.Find("DatabaseObject").GetComponent<DatabaseAccess>().GetDatabase();
        Neo4JWriterGraph.SetDatabase(database);

        project = GameObject.Find("DataObject").GetComponent<OSGi_Project_Script>().GetProject();

        masterDict = new Dictionary<BundleMaster, MasterVertex>();
        elementDict = new Dictionary<BundleElement, HistoryGraphVertex>();
        masterEdgeDict = new Dictionary<MasterVertex, Dictionary<MasterVertex, MasterEdge>>();

        taskTextfield.text = "";
        statusTextfield.text = "";
        StartCoroutine(GraphMain());

    }

    // Update is called once per frame
    void Update()
    {
        loadingDotsTextfield.color = new Color(loadingDotsTextfield.color.r, loadingDotsTextfield.color.g, loadingDotsTextfield.color.b, Mathf.PingPong(Time.time, 1));
    }

    private IEnumerator GraphMain()
    {
        List<Branch> branchList = project.GetBranches().Values.ToList<Branch>();
        branchList.Sort();
        foreach (Branch branch in branchList)
        {
            taskTextfield.text = "For branch " + branch.GetName() + " (" + branch.GetCommits(false).Count + " Commits)\n" 
                + "Create Graph from Project Datastructure";
            HistoryGraph historyGraph = new HistoryGraph(null);

            yield return FillHistoryGraphForBranch(historyGraph, branch);

            if (historyGraph.getFrameCount() == 0)
            {
                statusTextfield.text = "Branch needs no layout";
                continue;
            }

            taskTextfield.text = "For branch " + branch.GetName() + " (" + branch.GetCommits(false).Count + " Commits)\n"
                + "Layout Commits";
            yield return Layout(historyGraph);

            yield return SetPositionsToBundleAndWriteDB(branch);
        }

        yield return null;
        SceneManager.LoadScene(4);

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

    private float VertexRadiusCaclulation(BundleElement b, Commit c)
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
            foreach (BundleElement bundle in commitList[index].GetBundles())
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
                    BundleElement predBundle = bundle.GetPrevious(b);
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
                foreach (BundleElement bundle in commitList[index].GetBundles())
                {
                    //Collect import betwenn Bundles based on bundles importing packages
                    Dictionary<BundleElement, int> importCount = new Dictionary<BundleElement, int>();
                    foreach (PackageElement importedPackage in bundle.GetImportedPackages())
                    {
                        BundleElement parentBundle = importedPackage.GetParentBundle();
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
                    foreach (KeyValuePair<BundleElement, int> kvp in importCount)
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

                if ((index + 1) / (float)totalCommitsToLoad >= 1f)
                {
                    statusTextfield.text = "100% completed";
                }
                else if ((index + 1) / (float)totalCommitsToLoad > 0.8f)
                {
                    statusTextfield.text = "80% completed";
                }
                else if ((index + 1) / (float)totalCommitsToLoad > 0.6f)
                {
                    statusTextfield.text = "60% completed";
                }
                else if ((index + 1) / (float)totalCommitsToLoad > 0.4f)
                {
                    statusTextfield.text = "40% completed";
                }
                else if ((index + 1) / (float)totalCommitsToLoad > 0.2f)
                {
                    statusTextfield.text = "20% completed";
                }

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
            statusTextfield.text = ((i/(float)hG.getFrameCount())*100).ToString("0.00")+"% completed";
            yield return historyGraphManager.LayoutFrame(i);
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
        foreach (KeyValuePair<BundleElement, HistoryGraphVertex> kvp in elementDict)
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
