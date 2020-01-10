using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OsgiViz.Core;
using OsgiViz.Unity.Island;

namespace StaticIslandNamesComponent
{
    /// <summary>
    /// This class is attached to all IslandNamePrefabs of the StaticIslandNames component and handles the movement of the island name ui elements.
    /// </summary>
    public class StaticIslandName : MonoBehaviour
    {
        // ################
        // Public
        // ################

        // These values are defined in the Prefab.
        public GameObject NameParent;
        public Text Name;
        public Text Line;
        public AlwaysLookAtTarget AlwaysLookAtTarget;

        // ################
        // Private
        // ################

        private Transform target; // The island target this island name is following.
        private IslandGO targetIsland;
        public float heightIndex = 1f; // The current height index of this island name.

        private float yPosition; // The current y-position of this island name.

        private bool isIsland; // TODO remove or extend?
        private IslandVizInteraction.SelectionType selectionType;
        private bool initiated = false;



        // ################
        // Initiation
        // ################

        public void Init(Transform target, string name, IslandVizInteraction.SelectionType selectionType)
        {
            AlwaysLookAtTarget.Target = Camera.main.transform;
            Name.text = name;
            this.selectionType = selectionType;
            this.target = target;
            targetIsland = GetIslandFromTransform(target);

            if (targetIsland == null)
            {
                Debug.LogError("StaticIslandName was attached to a none island object!");
            }

            heightIndex = StaticIslandNames.Instance.GetHeightIndex(this);

            isIsland = target.GetComponent<IslandGO>() != null;

            initiated = true;
        }


        // ################
        // Fixed Update
        // ################

        private void FixedUpdate()
        {
            if (!initiated)
                return;

            if (targetIsland.Visible)
            {
                if (IslandVizVisualization.Instance.CurrentZoomLevel != ZoomLevel.Near)
                {
                    yPosition = GlobalVar.hologramTableHeight + 0.05f + (heightIndex * StaticIslandNames.Instance.VerticalTextOffset); // GlobalVar.hologramTableHeight + Mathf.Clamp(Vector3.Distance(transform.position, Camera.main.transform.position) / 8f, 0.1f, 3f) + GlobalVar.CurrentZoom * 2f;
                    transform.position = new Vector3(target.position.x, yPosition, target.position.z);
                }
                else // ZoomLevel.Near
                {
                    if (target.GetComponent<IslandGO>())
                    {
                        yPosition = GlobalVar.hologramTableHeight + 0.1f + GlobalVar.CurrentZoom * 2f;
                        transform.position = new Vector3(target.position.x, yPosition, target.position.z);
                    }
                    else if (target.GetComponent<Region>())
                    {
                        yPosition = GlobalVar.hologramTableHeight + 0.1f + GlobalVar.CurrentZoom + heightIndex * StaticIslandNames.Instance.VerticalTextOffset;
                        transform.position = new Vector3(target.GetComponent<MeshCollider>().bounds.center.x, yPosition, target.GetComponent<MeshCollider>().bounds.center.z);
                    }
                    else
                    {
                        yPosition = GlobalVar.hologramTableHeight + 0.1f + GlobalVar.CurrentZoom + heightIndex * StaticIslandNames.Instance.VerticalTextOffset;
                        transform.position = new Vector3(target.position.x, yPosition, target.position.z);
                    }
                }
            }
        }


        // ################
        // Helper Functions
        // ################

        public IslandGO GetIslandFromTransform (Transform transform)
        {
            if (transform.GetComponent<IslandGO>())
            {
                return transform.GetComponent<IslandGO>();
            }
            else if (transform.GetComponent<Region>())
            {
                return transform.GetComponent<Region>().getParentIsland();
            }
            else if (transform.GetComponent<Building>())
            {
                return transform.parent.GetComponent<Region>().getParentIsland();
            }
            else return null;
        }


        // ################
        // Getter & Setter
        // ################

        public Transform GetTarget()
        {
            return target;
        }
        public IslandGO GetTargetIsland ()
        {
            return targetIsland;
        }
        public float GetHeightIndex()
        {
            return heightIndex;
        }
        public void SetHeightIndex(int index)
        {
            heightIndex = index;
        }
        public IslandVizInteraction.SelectionType GetSelectionType ()
        {
            return selectionType;
        }
        
    }
}
