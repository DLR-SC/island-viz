using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DynamicGraphAlgoImplementation;
using DynamicGraphAlgoImplementation.HistoryGraphManager;
using System.Linq;

public class PlaneMainController : MonoBehaviour
{
    //-----History-Datatypes
    public enum LayoutOption
    {
        Master,
        HistoryForce
    };
    HistoryGraph historyGraph;
    HistoryGraphManager historyGraphManager;

    //-----Public Variables for User-Configuration
    public LayoutOption layoutOpt;
    [Range(1, 10)]
    public int sequenceLength;
    [Range(4, 30)]
    public int nrOfStartVertices;
    [Range(0, 10)]
    public int maxNrOfAppearVperFrame;
    public GameObject vertexRepresentationPrefab;

    //-----Internal Variables of State Machine
    private int currentFrameIndex;
    private int stateMachineState;
    private int nrOfVerticesToBeDisabled;
    private int nrOfVerticesToBeMoved;
    private int nrOfVerticesToBeEnabled;

    public bool LayoutFinished;
    public bool StartFinished;

    // Use this for initialization
    private void Awake()
    {
        Debug.Log("Create HistoryGraph");
        Builder_HistoryGraph builder = new Builder_HistoryGraph("PucksPlaceHistory");
        Debug.Log("Instantiate HistoryGraphManager");
        builder.setVertexGenerationParameters(nrOfStartVertices, maxNrOfAppearVperFrame);
        historyGraph = builder.BuildHistoryGraph(sequenceLength);
        float planeRadius = gameObject.transform.localScale.x * 5;
        switch (layoutOpt)
        {
            case LayoutOption.HistoryForce:
                historyGraphManager = ScriptableObject.CreateInstance<HistoryGraphManager_History>();
                //Doppelter Code HistoryGraphManager.Set... nötig, da HistoryGraph vor Init gesetzt sein muss, aber verschiedene Inits
                historyGraphManager.SetHistoryGraph(historyGraph);
                historyGraphManager.SetPlaneMainController(this);
                historyGraphManager.Init(planeRadius - 2.1f, 1.0f, 8.0f, 0.3f, 2000, 32, 4);
                break;
            case LayoutOption.Master:
                historyGraphManager = ScriptableObject.CreateInstance<HistoryGraphManager_Master>();
                //Doppelter Code HistoryGraphManager.Set... nötig, da HistoryGraph vor Init gesetzt sein muss, aber verschiedene Inits
                historyGraphManager.SetHistoryGraph(historyGraph);
                historyGraphManager.SetPlaneMainController(this);
                historyGraphManager.Init(planeRadius - 2.1f, 1.0f, 4.0f, 0.3f, 1000, planeRadius - 2.1f);
                break;
        }
        //Als PositionDebu
        LayoutFinished = false;
        StartFinished = false;
        StartCoroutine(Layouting());
        //historyGraph.WriteHistoryGraphInDotFormatToFileSystem("C:\\Users\\heid_el\\DotFiles", true, true, false);
        //historyGraph.WriteHistoryGraphInDotFormatToFileSystem("C:\\Users\\heidm\\Studium\\Masterarbeit\\dynamic-graph-positioning-unity\\DynamicGraphAnimation\\DotFiles", true, true, false);


        //Standardfall
        /*historyGraphManager.LayoutFrame(0);
        if (sequenceLength > 1)
        {
            historyGraphManager.LayoutFrame(1);
        }
        inspectLayoutData();*/
        

        currentFrameIndex = 0;
        stateMachineState = -10;
        Debug.Log("Finished PlaneMainController Start-Routine");
    }

    IEnumerator Layouting()
    {
        yield return historyGraphManager.LayoutFrame(sequenceLength - 1);
        LayoutFinished = true;
    }

    void inspectLayoutData()
    {
        for(int i = 0; i<historyGraph.getFrameCount(); i++)
        {
            List<HistoryGraphVertex> vertexList = historyGraph.getFrameAt(i).GetVertices().ToList<HistoryGraphVertex>();
                //Vertices as List<HistoryGraphVertex>;
        }
    }

    void Start()
    {
        //historyGraphManager.InstantiateVertexRepresentations(vertexRepresentationPrefab);
        //Debug.Log("Finished VertexRepresentation Instantiation");
    }
    void Starting()
    {
        historyGraphManager.InstantiateVertexRepresentations(vertexRepresentationPrefab);
        Debug.Log("Finished VertexRepresentation Instantiation");
    }

    // Update is called once per frame
    void Update()
    {
        if (!LayoutFinished)
        {
            return;
        }
        if (!StartFinished)
        {
            Starting();
            StartFinished = true;
            return;
        }

        if (stateMachineState == -10)
        {
            Debug.Log("FirstStep Enable Initial Config");
            nrOfVerticesToBeEnabled = historyGraphManager.NrOfVerticesToBeEnabledInFirstStep();
            stateMachineState = -9;
        }
        if (stateMachineState == 0)
        {
            //backwards in History
            if (Input.GetKey("left"))
            {
                Debug.Log("Noticed KeyBoard Input step Backwards");
                if (currentFrameIndex > 0)
                {
                    Vector3Int changeInfo = historyGraphManager.NrOfVertexChanges("backward");
                    nrOfVerticesToBeDisabled = changeInfo.x;
                    nrOfVerticesToBeMoved = changeInfo.y;
                    nrOfVerticesToBeEnabled = changeInfo.z;

                    currentFrameIndex--;
                    if (nrOfVerticesToBeDisabled > 0)
                    {
                        stateMachineState = -1;
                    }
                    else if (nrOfVerticesToBeMoved > 0)
                    {
                        stateMachineState = -2;
                    }
                    else if (nrOfVerticesToBeEnabled > 0)
                    {
                        stateMachineState = -3;
                    }
                }
                else
                {
                    Debug.LogWarning("Already showing first frame, no step backwards possible");
                }
            }

            //forwards in History
            else if (Input.GetKey("right"))
            {
                Debug.Log("Noticed KeyBoard Input step Forwards");
                if (currentFrameIndex < historyGraph.getFrameCount() - 1)
                {
                    Vector3Int changeInfo = historyGraphManager.NrOfVertexChanges("forward");
                    nrOfVerticesToBeDisabled = changeInfo.x;
                    nrOfVerticesToBeMoved = changeInfo.y;
                    nrOfVerticesToBeEnabled = changeInfo.z;

                    currentFrameIndex++;
                    if (nrOfVerticesToBeDisabled > 0)
                    {
                        stateMachineState = 1;
                    }
                    else if (nrOfVerticesToBeMoved > 0)
                    {
                        stateMachineState = 2;
                    }
                    else if (nrOfVerticesToBeEnabled > 0)
                    {
                        stateMachineState = 3;
                    }
                    //TODO: Layout im Standardfall
                    //historyGraphManager.LayoutFrame(currentFrameIndex + 1);
                }
                else
                {
                    Debug.LogWarning("No next step forwards available");
                }
                //Debug.Log("Statemachine Change " + stateMachineState + " ToChangeValues " + nrOfVerticesToBeDisabled + " " + nrOfVerticesToBeMoved + " " + nrOfVerticesToBeEnabled);
            }
        }
       
    }


    //-----getter for state machine
    public int GetCurrentFrameIndex()
    {
        return currentFrameIndex;
    }
    public int GetStateMachineState()
    {
        return stateMachineState;
    }

    //-----callback notice for state machine
    public void NoticeVertexDisabled()
    {
        nrOfVerticesToBeDisabled--;
        if (nrOfVerticesToBeDisabled == 0)
        {
            if (stateMachineState == 1)
            {
                if (nrOfVerticesToBeMoved > 0)
                    stateMachineState = 2;
                else if (nrOfVerticesToBeEnabled > 0)
                    stateMachineState = 3;
                else
                    stateMachineState = 0;
            }
            else if (stateMachineState == -1)
            {
                if (nrOfVerticesToBeMoved > 0)
                    stateMachineState = -2;
                else if (nrOfVerticesToBeEnabled > 0)
                    stateMachineState = -3;
                else
                    stateMachineState = 0;
            }
            //Debug.Log("Disabling Finished " + stateMachineState);

        }
    }
    public void NoticeVertexMoved()
    {
        nrOfVerticesToBeMoved--;
        if (nrOfVerticesToBeMoved == 0)
        {
            if (stateMachineState == 2)
            {
                if (nrOfVerticesToBeEnabled > 0)
                    stateMachineState = 3;
                else
                    stateMachineState = 0;
            }
            else if (stateMachineState == -2)
            {
                if (nrOfVerticesToBeEnabled > 0)
                    stateMachineState = -3;
                else
                    stateMachineState = 0;
            }
            //Debug.Log("MovingPhasse Finished " + stateMachineState);
        }
    }
    public void NoticeVertexEnabled()
    {
        nrOfVerticesToBeEnabled--;
        if (nrOfVerticesToBeEnabled == 0)
        {
            stateMachineState = 0;
            //Debug.Log("Finished Enable, StateMachine is " + stateMachineState);
        }
    }
}
