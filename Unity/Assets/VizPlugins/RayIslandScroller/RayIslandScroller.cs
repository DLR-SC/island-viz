using System.Collections;
using IslandEvents;
using IslandInterface.Input;
using IslandInterface.Selection;
using IslandPlugins;
using UnityEngine;
using IslandTransformationSystem = IslandVis.ECS_Systems.IslandTransformationSystem;

public class RayIslandScroller : IslandPluginBase
{
    private GameObject _tableCollider;

    private bool _highlighted;
    private bool _scrolling;
    private Vector3 _lastHandPosition;

    public override IEnumerator Init()
    {
        InteractionEvents.OnGameObjectHitStart += Highlight;
        InteractionEvents.OnGameObjectHitEnd += Dehighlight;
        InteractionEvents.OnButtonEvent += ButtonEvent;

        _tableCollider = GameObject.Find("Water");

        _highlighted = true;

        yield return null;
    }


    public void Highlight (UnityEngine.Collider collider, Transform hand)
    {
        //Debug.Log(collider.gameObject.name);

        if (collider.gameObject == _tableCollider)
        {
            _highlighted = true;
        }
            
    }

    public void Dehighlight (UnityEngine.Collider collider, Transform hand)
    {
        if (collider.gameObject == _tableCollider)
        {
            _highlighted = false;
        }            
    }

    public void ButtonEvent (Button button, PressType type, Transform hand)
    {
        if (!_highlighted || button != Button.Trigger)
            return;

        if (type == PressType.PressDown)
        {
            StartCoroutine(Scroll(hand));
        }
        else if (type == PressType.PressUp)
        {
            _scrolling = false;
        }
    }

    IEnumerator Scroll (Transform hand)
    {
        if (GameObjectRaycaster.Instance.Raycast(hand, out RaycastHit hit))
        {
            _lastHandPosition = hit.point;
            _scrolling = true;
        }
        else
        {
            _scrolling = false;
        }

        while (_scrolling && GameObjectRaycaster.Instance.Raycast(hand, out hit))
        {
            IslandTransformationSystem.Translate(GetHandVelocity(hit.point));
            _lastHandPosition = hit.point;
            yield return null;
        }
    }

    private Vector3 GetHandVelocity (Vector3 handPos)
    {
        Vector3 velocity = _lastHandPosition - handPos;
        velocity.y = 0f;
        velocity *= -1f;
        return velocity;
    }

}
