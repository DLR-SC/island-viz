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
        private float heightIndex = 1f; // The current height index of this island name.

        private float yPosition; // The current y-position of this island name.

        private bool isIsland; // TODO remove or extend?
        private bool initiated = false;



        // ################
        // Initiation
        // ################

        public void Init(Transform target, string name)
        {
            AlwaysLookAtTarget.Target = Camera.main.transform;
            Name.text = name;
            this.target = target;
            targetIsland = target.GetComponent<IslandGO>();

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
                    yPosition = GlobalVar.hologramTableHeight + 0.075f + heightIndex * StaticIslandNames.Instance.VerticalTextOffset;
                    transform.position = new Vector3(target.position.x, yPosition, target.position.z);
                }
                else
                {
                    if (isIsland)
                    {
                        yPosition = GlobalVar.hologramTableHeight + 0.2f + GlobalVar.CurrentZoom * 2f;
                        transform.position = new Vector3(target.position.x, yPosition, target.position.z);
                    }
                    else
                    {
                        yPosition = GlobalVar.hologramTableHeight + 0.2f + GlobalVar.CurrentZoom + heightIndex * StaticIslandNames.Instance.VerticalTextOffset;
                        transform.position = new Vector3(target.position.x, yPosition, target.position.z);
                    }
                }
            }
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
    }
}
