using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this class to a GameObject to make it always look at a target Transform. The GameObject will only rotate on the y axis.
/// </summary>
public class AlwaysLookAtTarget : MonoBehaviour
{
    public Transform Target; 
    public bool Invert;

    private Quaternion rotation;

    private void FixedUpdate()
    {
        rotation = Quaternion.LookRotation((Invert ? -1f : 1f) * Vector3.Normalize(Target.position - transform.position));
        rotation.x = 0f;
        rotation.z = 0f;

        if ( Mathf.Abs(Mathf.Abs(rotation.y) - Mathf.Abs(transform.rotation.y)) > 0.2f)
        {
            transform.rotation = rotation;
        }
    }
}
