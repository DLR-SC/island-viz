using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ignoreHovering = Valve.VR.InteractionSystem.IgnoreHovering;

namespace OsgiViz
{

    [RequireComponent(typeof(ignoreHovering))]
    public class HierarchicalComponent : MonoBehaviour{


        public HierarchicalComponent parentComponent;
        public List<HierarchicalComponent> childrenComponents;
        public float subdivisionDistanceSquared = 4f;
        public bool isSplit;

        private GameObject observer;
        private Valve.VR.InteractionSystem.IgnoreHovering ignoreHover;
        
        void Awake()
        {
            //Comment out for manual child selection in editor
            childrenComponents = new List<HierarchicalComponent>();
            isSplit = false;
            observer = GameObject.FindGameObjectWithTag("Observer");
        }

	    // Use this for initialization
	    void Start () {
            ignoreHover = GetComponent<ignoreHovering>();
            ignoreHover.enabled = false;
	    }

        
        void Update()
        {
            float distanceSquared = (observer.transform.position - transform.position).sqrMagnitude;
            if (distanceSquared < subdivisionDistanceSquared && !isSplit)
                split();
            else if (distanceSquared >= subdivisionDistanceSquared && isSplit)
                merge();
        }
        
         
        private void split()
        {
            if (childrenComponents.Count == 0)
                return;

            foreach (HierarchicalComponent hc in childrenComponents)
            {
                hc.gameObject.SetActive(true);
            }
            disableAllComponents(gameObject);
            ignoreHover.enabled = true;
            isSplit = true;
        }

        private void merge()
        {
            foreach (HierarchicalComponent hc in childrenComponents)
            {
                hc.gameObject.SetActive(false);
            }
            enableAllComponents(gameObject);
            ignoreHover.enabled = false;
            isSplit = false;
        }

        public void setParentComponent(HierarchicalComponent parent)
        {
            parentComponent = parent;
        }

        private void SetComponentActive(Component component, bool value)
        {
            if (component == null) return;
            if (component is Renderer)
            {
                (component as Renderer).enabled = value;
            }
            
            else if (component is Collider)
            {
                (component as Collider).enabled = value;
            }
            
            else if (component is Behaviour && !(component is HierarchicalComponent))
            {
                (component as Behaviour).enabled = value;
            }
            else if (component is LineRenderer)
            {
                (component as LineRenderer).enabled = value;
            }
            else
            {
                //Debug.Log("Don't know how to enable/disable " + component.GetType().Name);
            }

        }

        public void enableAllComponents(GameObject obj)
        {
            Component[] components = obj.GetComponents<Component>();
            foreach (Component comp in components)
                SetComponentActive(comp, true);

        }

        public void disableAllComponents(GameObject obj)
        {
            Component[] components = obj.GetComponents<Component>();
            foreach (Component comp in components)
                SetComponentActive(comp, false);

        }


    }
}
