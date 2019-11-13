using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        transform.rotation = rotation;
    }
}
