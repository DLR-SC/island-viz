using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexScript : MonoBehaviour {

    public Vector3 nextTarget;

    Rigidbody rb;
    Vector3 currentVelo;
    float lastChangeTime;
    PlaneMainController controller;
    VertexRepresentation master;
    bool actualTarget;
    Vector3 target_;

    // Use this for initialization
    void Start () {
        rb = gameObject.GetComponent<Rigidbody>();
        lastChangeTime = Time.time;
        currentVelo = new Vector3(Random.Range(-5.0f, 5.0f), 0f, Random.Range(-5.0f, 5.0f));
        actualTarget = false;
	}
    public void Initialise(PlaneMainController pmc, VertexRepresentation master)
    {
        this.controller = pmc;
        this.master = master;
        Vector3 colorV = master.GetColorVector();
        gameObject.GetComponent<Renderer>().material.color = new Color(colorV.x, colorV.y, colorV.z);
    }

    // Update is called once per frame
    void Update () {
        if(controller.GetStateMachineState()==2 || controller.GetStateMachineState() == -2)
        {
            if (master.ShallMove())
            {
                if (!actualTarget)
                {
                    actualTarget = true;
                    target_ = master.GetTarget();
                    nextTarget = target_;
                    //Debug.LogAssertion("Neues Target ist " + target_.x + ", " + target_.y + ", " + target_.z);
                }
                //Vector3 target = master.GetTarget();
                Vector3 direction = target_ - gameObject.transform.position;
                if (direction.magnitude >= 0.1)
                {
                    try
                    {
                        if (direction.magnitude >= 1.0)
                        {
                            rb.velocity = 2.0f * direction.normalized;
                        }
                        else
                        {
                            rb.velocity = direction.normalized;
                        }
                    }
                    catch
                    {
                        Debug.Log("Exception");
                    }
                }
                else
                {
                    master.NoticeReachedPosition();
                    controller.NoticeVertexMoved();
                    actualTarget = false;
                }
            }

        }
   	}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("VertexReal"))
        {
            Debug.Log("CollisionEnter " + gameObject.name + ", " + collision.gameObject.name);
        }
        
    }
    private void OnCollisionStay(Collision collision)
    {
        Vector3 dirToOther = collision.gameObject.transform.position;
        Vector3 dirRight = Vector3.Cross(Vector3.up, dirToOther);
        rb.AddForce(5 * (dirRight - dirToOther).normalized);
    }


    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag.Equals("VertexReal"))
        {
            Debug.Log("CollisionExit " + gameObject.name + ", " + collision.gameObject.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("VertexReal"))
        {
            Vector3 velo = rb.velocity;
            Vector3 repForceDir;
            if (velo.magnitude != 0)
            {
                repForceDir = Vector3.Cross(Vector3.up, velo);
            }
            else
            {
                Vector3 dir = other.gameObject.transform.position - gameObject.transform.position;
                repForceDir = Vector3.Cross(Vector3.up, dir);
            }
           
            gameObject.GetComponent<ConstantForce>().force = 10.0f * repForceDir.normalized;
            Debug.Log("Trigger Enter " + gameObject.name + ", " + other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("VertexReal"))
        {
            gameObject.GetComponent<ConstantForce>().force = Vector3.zero;
            Debug.Log("TriggerExit " + gameObject.name + ", " + other.gameObject.name);
        }
    }

    /*private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag.Equals("VertexReal"))
        {
            Vector3 forceDirection = Vector3.Cross(rb.velocity, Vector3.up);
            rb.AddForce(forceDirection.normalized);
        }
    }*/

    private void OnMouseEnter()
    {
        Debug.Log("Vertex Nr. " + master.GetVertexName());
    }

}
