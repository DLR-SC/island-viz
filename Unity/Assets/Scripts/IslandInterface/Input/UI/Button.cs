using System;
using IslandEvents;
using IslandInterface.Selection;
using UnityEngine;
using UnityEngine.Events;
// using Valve.VR.InteractionSystem;

namespace IslandViz.Core.UI
{
    public class Button : MonoBehaviour
    {
        public UnityEvent OnButtonPressed;

        public UnityEvent OnHighlight;
        public UnityEvent OnDehighlight;

        // Start is called before the first frame update
        void OnEnable()
        {
            InteractionEvents.OnGameObjectSelected += OnSelected;
            InteractionEvents.OnGameObjectHitStart += OnHighlighted;
            InteractionEvents.OnGameObjectHitEnd += OnDehighlighted;
        }


        private void OnSelected(GameObject gameObject)
        {
            if (gameObject != this.gameObject)
            {
                return;
            }

            OnButtonPressed?.Invoke();
        }

        private void OnHighlighted(Collider collider, Transform hand)
        {
            if (collider.gameObject != this.gameObject)
            {
                return;
            }

            OnHighlight?.Invoke();
        }

        private void OnDehighlighted(Collider collider, Transform hand)
        {
            if (collider.gameObject != this.gameObject)
            {
                return;
            }

            OnDehighlight?.Invoke();
        }

        private void OnDisable()
        {
            OnDehighlight?.Invoke();

            InteractionEvents.OnGameObjectSelected -= OnSelected;
            InteractionEvents.OnGameObjectHitStart -= OnHighlighted;
            InteractionEvents.OnGameObjectHitEnd -= OnDehighlighted;
        }

    }
}
