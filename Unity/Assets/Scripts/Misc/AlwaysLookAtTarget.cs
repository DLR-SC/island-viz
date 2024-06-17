using System.Collections;
using System.Collections.Generic;
using IslandInterface.Input;
using UnityEngine;

/// <summary>
/// Attach this class to a GameObject to make it always look at a target Transform. The GameObject will only rotate on the y axis.
/// </summary>
public class AlwaysLookAtTarget : MonoBehaviour
{
    public Transform Target;
    public bool Invert;

    public bool X;
    public bool Y;

    private Quaternion rotation;

    private void Start()
    {
        if (Target == null)
        {
            Target = Camera.main.transform;
        }
    }

    public void ForceUpdate ()
    {
        FixedUpdate();
    }

    private void FixedUpdate()
    {
        Vector3 targetPos = Target.position - Target.forward * 0.2f;

        rotation = Quaternion.LookRotation((Invert ? -1f : 1f) * Vector3.Normalize(targetPos - transform.position));
        rotation.y = Y ? rotation.y : 0f;
        rotation.x = X ? rotation.x : 0f;
        rotation.z = 0f;

        if (PlayerManager.Instance.Type == PlayerManager.PlayerType.Desktop || Mathf.Abs(Mathf.Abs(rotation.y) - Mathf.Abs(transform.rotation.y)) > 0.1f)
        {
            transform.rotation = rotation;
        }
    }
}