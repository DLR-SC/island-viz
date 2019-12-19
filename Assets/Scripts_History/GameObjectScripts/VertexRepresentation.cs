using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DynamicGraphAlgoImplementation;
public class VertexRepresentation : MonoBehaviour {

    MasterVertex master;
    GameObject representation;
    PlaneMainController controller;

    private int currentFrameIndex;

    // Use this for initialization
    void Start () {
        representation.GetComponent<VertexScript>().Initialise(controller, this);
        //representation.GetComponent<Renderer>().material.SetColor("TC", new Color(master.GetR(), master.GetG(), master.GetB()));
        /*HistoryGraphVertex firstHVertex;
        if ((firstHVertex = master.GetHistoryGraphVertexAtIndex(0)) != null)
        {
            Vector3 pos = firstHVertex.getPosition();
            pos.y = 0.05f;
            representation.transform.position = pos;
            representation.SetActive(true);
        }
        else
        {
            representation.SetActive(false);
        }*/

    }

    public void Initialize(MasterVertex master, PlaneMainController pmc)
    {
        this.controller = pmc;
        this.master = master;
        HistoryGraphVertex firstPos;
        Vector3 initPos;
        
        if ((firstPos = master.GetHistoryGraphVertexAtIndex(0)) != null)
        {
            initPos = firstPos.getPosition();
            initPos.y = 0.05f;
        }
        else {
            initPos = Vector3.zero;
            initPos.y = 0.05f;
        }
        representation = gameObject.transform.GetChild(0).gameObject;
        representation.gameObject.name = "VertexReal " + master.getName();
        float radius = master.getRadius();
        representation.transform.localScale = new Vector3(2 * radius, 0.1f, 2 * radius);
        representation.GetComponent<SphereCollider>().radius = radius + 0.5f;


    }
	
	// Update is called once per frame
	void Update () {
        //nur in Fällen 1, 3, -1, -3, -9(InitialesEnable)
        int stateMachineState = controller.GetStateMachineState();
        if (stateMachineState == -9)
        {
            if(!representation.activeSelf && master.ExistsHistoryGraphVertexAtIndex(0))
            {
                Vector3 pos = master.GetHistoryGraphVertexAtIndex(0).getPosition();
                pos.y = 0.05f;
                representation.transform.position = pos;
                representation.SetActive(true);
                controller.NoticeVertexEnabled();
                currentFrameIndex = 0;
            }
        }
        if(stateMachineState==1 || stateMachineState == 3 || stateMachineState == -1 || stateMachineState == -3)
        {
            HistoryGraphVertex historyVertex = master.GetHistoryGraphVertexAtIndex(controller.GetCurrentFrameIndex());

            if(representation.activeSelf && (stateMachineState == 1 || stateMachineState == -1) && historyVertex == null)
            {
                //State1 ist Deaktivierungssequenz bei History-Schritt-Vorwärts (-1 bei Rückwärts)
                representation.SetActive(false);
                controller.NoticeVertexDisabled();
                currentFrameIndex = controller.GetCurrentFrameIndex();
            }else if(!representation.activeSelf && (stateMachineState == 3 || stateMachineState == -3) && historyVertex != null)
            {
                //State3 ist Aktivierungssequenz bei History-Schritt-Vorwärts (-3 bei Rückwärts)
                Vector3 pos = historyVertex.getPosition();
               
                pos.y = 0.05f;
                try
                {
                    representation.transform.position = pos;
                }
                catch
                {
                    Debug.LogError("stop here");
                }
                representation.SetActive(true);
                controller.NoticeVertexEnabled();
            }

        }		
	}

    public Vector3 GetTarget()
    {
        HistoryGraphVertex hV = master.GetHistoryGraphVertexAtIndex(controller.GetCurrentFrameIndex());
        if(hV == null)
        {
            throw new System.Exception("Nullpointer At Vertex Representation GetTarget()");
        }
        Vector3 target = hV.getPosition();
        target.y = 0.05f;
        return target;
    }

    public Vector3 GetColorVector()
    {
        Vector3 colorV = new Vector3(master.GetR(), master.GetG(), master.GetB());
        return colorV;
    }

    public string GetVertexName()
    {
        return master.getName();
    }

    public bool ShallMove()
    {
        if(currentFrameIndex == controller.GetCurrentFrameIndex())
        {
            //Schon an FrameAngepasst -> nicht mehr bewegen
            return false;
        }

        return true;

    }

    public void NoticeReachedPosition()
    {
        currentFrameIndex = controller.GetCurrentFrameIndex();
    }
}
