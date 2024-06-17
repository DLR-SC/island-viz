using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
using IslandViz.Core;
using System.Collections;
using IslandData.SystemData;
using IslandEntities.Construction;
using IslandInterface.Input;
using IslandInterface.Selection;
using IslandLevelOfDetail;
using IslandVis.ECS_Components;

namespace StaticIslandNamesComponent
{
    /// <summary>
    /// This class is attached to all IslandNamePrefabs of the StaticIslandNames plugin and handles the movement of the island name ui elements.
    /// </summary>
    public class StaticIslandName : MonoBehaviour
    {
        // ################
        // Public
        // ################
        public bool Pin { get; private set; }
        public IIslandSystemElement _islandElement { get; private set; }
        public StaticIslandNames.NameType _selectionType { get; private set; }
        public Entity _target { private set; get; } // The island target this island name is following.



        // These values are defined in the Prefab.
        public GameObject NameParent;
        public Text TopName;
        public Text Name;
        public Text Line;
        public AlwaysLookAtTarget AlwaysLookAtTarget;
        public GameObject ButtonClose;
        public GameObject ButtonPin;
        public GameObject ButtonZoom;
        public GameObject ButtonImport;
        public GameObject ButtonExport;
        public GameObject ButtonStats;
        public GameObject ButtonCode;

        // ################
        // Private
        // ################

        public float heightIndex = 1f; // The current height index of this island name.


        private bool initiated = false;

        private Transform _playerCamera;

        private int _playerHandID;


        // ################
        // Initiation
        // ################

        public void Init(IIslandSystemElement islandElement, StaticIslandNames.NameType selectionType, int playerHand, bool pin)
        {
            //Debug.Log("Init - " + oSGiElement.Name + " - " + selectionType.ToString());

            AlwaysLookAtTarget.Target = Camera.main.transform;
            _target = islandElement.Entity;
            _islandElement = islandElement;
            _selectionType = selectionType;
            Pin = pin;

            _playerHandID = playerHand;

            ResolveNames();

            if (Pin && _selectionType == StaticIslandNames.NameType.Select) // TODO add UI indicator for Highlight
            {
                UpdatePinUI();
            }

            heightIndex = StaticIslandNames.Instance.GetHeightIndex(this);

            _playerCamera = PlayerManager.Instance.PlayerCamera.transform;

            // This removes flickering at start.
            UpdatePosition();
            AlwaysLookAtTarget.ForceUpdate();

            initiated = true;

            StartCoroutine(StartAnimation(1f)); // 0.1f

        }

        private void ResolveNames() // TODO Remove Buttons depending on type
        {
            Name.text = _islandElement.Name;
            TopName.text = _islandElement.GetLabels()[0];

            //if (_selectionType == StaticIslandNames.NameType.Select && _islandElement is Island island)
            //{
            //    if (((Bundle)_islandElement).Imports.Count == 0)
            //    {
            //        ButtonImport.SetActive(false);
            //    }
            //    if (((Bundle)_islandElement).Exports.Count == 0)
            //    {
            //        ButtonExport.SetActive(false);
            //    }
            //    //ButtonCode.SetActive(false);
            //    //ButtonStats.SetActive(false);
            //}
            //else if (_islandElement.GetType() == typeof(Package))
            //{
            //    TopName.text = "Package";

            //    if (_selectionType == StaticIslandNames.NameType.Select)
            //    {
            //        if (((Package)_islandElement).Imports.Count == 0)
            //        {
            //            ButtonImport.SetActive(false);
            //        }
            //        if (((Package)_islandElement).Imports.Count == 0) // TODO
            //        {
            //            ButtonExport.SetActive(false);
            //        }
            //        ButtonCode.SetActive(false);
            //        //ButtonStats.SetActive(false);
            //    }
            //}
            //else if (_islandElement.GetType() == typeof(CompilationUnit))
            //{
            //    TopName.text = "CompilationUnit";

            //    if (_selectionType == StaticIslandNames.NameType.Select)
            //    {
            //        //if (((CompilationUnit)oSGiElement).Imports.Count == 0)
            //        //{
            //        //    ButtonImport.SetActive(false);
            //        //}
            //        //if (((Package)oSGiElement).Exports.Count == 0) // TODO
            //        //{
            //        //    ButtonExport.SetActive(false);
            //        //}

            //        ButtonExport.SetActive(false);
            //        ButtonImport.SetActive(false);
            //        ButtonCode.SetActive(false);
            //        ButtonStats.SetActive(false);
            //    }
            //}
        }



        private void Update()
        {
            if (!initiated)
                return;

            UpdateScale();

            UpdatePosition();
        }



        private void UpdatePosition()
        {
            float yPos = IslandConstructor.IslandHeight + StaticIslandNames.Instance.yOffset + (heightIndex * StaticIslandNames.Instance.VerticalTextOffset);
            float3 entityPosition = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalToWorld>(_target).Position;
            
            float3 entityCenterOffset = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<CenterOffsetData>(_target).Value;

            transform.position = new Vector3(entityPosition.x + entityCenterOffset.x, yPos + entityCenterOffset.y, entityPosition.z + entityCenterOffset.z);
        }

        private void UpdateScale ()
        {
            transform.localScale = Vector3.one / 3f * _playerCamera.position.y / 20f;
        }

        // ################
        // UI Buttons
        // ################

        //public void OnButtonShowImportDependencies()
        //{
        //    // Create relationship entites.

        //    RelationshipConstructor.SpawnRelationships(_oSGiElement, RelationshipConstructor.RelationshipType.Import);

        //    // Center view.

        //    if (_oSGiElement.GetType() == typeof(Bundle) && ((Bundle)_oSGiElement).Imports.Count > 0 )
        //    {
        //        List<Entity> islandEntities = new List<Entity>();

        //        islandEntities.Add(_target);

        //        Bundle bundle = (Bundle)_oSGiElement;

        //        foreach (var importBundle in bundle.Imports)
        //        {
        //            if (importBundle.Key.Entity != Entity.Null)
        //                islandEntities.Add(importBundle.Key.Entity);
        //        }

        //        IslandTransformation.Instance.CenterIslands(islandEntities.ToArray());
        //    }
        //}

        //public void OnButtonShowExportDependencies()
        //{
        //    // Create relationship entites.

        //    RelationshipConstructor.SpawnRelationships(_oSGiElement, RelationshipConstructor.RelationshipType.Export);

        //    // Center view.

        //    if (_oSGiElement.GetType() == typeof(Bundle) && ((Bundle)_oSGiElement).Exports.Count > 0)
        //    {
        //        List<Entity> islandEntities = new List<Entity>();

        //        islandEntities.Add(_target);

        //        Bundle bundle = (Bundle)_oSGiElement;

        //        foreach (var exportBundle in bundle.Exports)
        //        {
        //            if (exportBundle.Entity != Entity.Null)
        //                islandEntities.Add(exportBundle.Entity);
        //        }

        //        IslandTransformation.Instance.CenterIslands(islandEntities.ToArray());
        //    }
        //}

        public void OnButtonPin()
        {
            Pin = !Pin;

            if (Pin)
            {
                StaticIslandNames.Instance.PinName(_islandElement);
            }
            else
            {
                StaticIslandNames.Instance.UnPinName(_islandElement);
            }

            UpdatePinUI();
        }

        public void OnButtonShowCode()
        {

        }

        public void OnButtonShowDetails()
        {

        }

        public void OnButtonZoomIn()
        {
            IslandTransformation.Instance.CenterIslands(_target, IslandLOD.Region);
        }

        public void OnButtonClose()
        {
            //StaticIslandNames.Instance.RemoveStaticName(this);

            IslandInteraction.Instance.Deselect(_islandElement);
        }


        private void UpdatePinUI ()
        {
            if (Pin)
            {
                ButtonPin.GetComponent<Image>().color = Color.yellow;
            }
            else
            {
                ButtonPin.GetComponent<Image>().color = ButtonClose.GetComponent<Image>().color;
            }
        }

        // ################
        // Start Animation
        // ################

        IEnumerator StartAnimation (float speed)
        {
            float value = 0;

            Vector3 endpos = transform.localPosition;
            Vector3 startpos = transform.localPosition - Vector3.up * 0.5f;

            while (value <= 1)
            {
                transform.localPosition = Vector3.Lerp(startpos, endpos, value);
                value += speed;
                yield return new WaitForFixedUpdate();
            }

            transform.localPosition = endpos;
        }


        // ################
        // Getter & Setter
        // ################

        public float GetHeightIndex()
        {
            return heightIndex;
        }
        public void SetHeightIndex(int index)
        {
            heightIndex = index;
            UpdatePosition();
        }
    }
}
