using IslandData.SystemData;
using IslandInterface.Input;
using Unity.Entities;
using UnityEngine;

namespace IslandEvents
{
    public static class InteractionEvents
    {
        /// <summary>
        /// Called when a button is pressed, released or touched.
        /// </summary>
        public static ButtonEvent OnButtonEvent { get; set; }

        
        /// <summary>
        /// Called when a controller ray cast starts hitting a entity.
        /// </summary>
        public static RaycastEntityHit OnEntityHitChanged { get; set; }
        
        /// <summary>
        /// Called when a controller ray cast hits a GameObject.
        /// </summary>
        public static RaycastGameObjectHit OnGameObjectHitStart { get; set; }
        
        /// <summary>
        /// Called when a controller ray cast hits a GameObject.
        /// </summary>
        public static RaycastGameObjectHit OnGameObjectHitEnd { get; set; }
        
        
        /// <summary>
        /// Called when an island was selected/highlighted or deselected/dehighlighted.
        /// </summary>
        public static EntitySelection OnEntitySelected { get; set; }
        
        /// <summary>
        /// Called when any other object was selected or deselected.
        /// </summary>
        public static GameObjectSelection OnGameObjectSelected { get; set; }
        
        
        /// <summary>
        /// Called when an island element was highlighted/dehighlighted.
        /// </summary>
        public static HighlightEvent OnIslandElementHighlighted { get; set; }

        /// <summary>
        /// Called when an island element was selected/deselected.
        /// </summary>
        public static SelectEvent OnIslandElementSelected { get; set; }
        
        
        
        
        
        
        /// <summary>
        /// This is called when a button is pressed, released or touched.
        /// </summary>
        /// <param name="button">The button that was pressed, released or touched.</param>
        /// <param name="type">Weather the button was pressed, released or touched.</param>
        /// <param name="hand">The hand that pressed the button.</param>
        public delegate void ButtonEvent(Button button, PressType type, Transform hand);
        
        /// <summary>
        /// Delegate of OnRaycastHitsEntityStart & OnRaycastHitsEntityEnd event.
        /// </summary>
        /// <param name="newHit">Entity that is hit.</param>
        /// <param name="oldHit">Entity that was hit before.</param>
        /// <param name="hand">The hand that exited the trigger.</param>
        public delegate void RaycastEntityHit(Entity newHit, Entity oldHit, int hand);
        
        /// <summary>
        /// Called when a controller raycast hits a GameObject. This is mostly meant for UI Elements.
        /// </summary>
        /// <param name="collider">Collider component of the GameObject.</param>
        /// <param name="hand">The hand that exited the trigger.</param>
        public delegate void RaycastGameObjectHit(Collider collider, Transform hand);
        
        /// <summary>
        /// Delegate of the OnSelected event.
        /// </summary>
        /// <param name="entity">The entity that was selected.</param>
        /// <param name="selected">True = select/highlight, false = deselect/dehighlight.</param>
        public delegate void EntitySelection(Entity entity, bool selected);
        
        /// <summary>
        /// Called when any other GameObject with a collider was selected.
        /// Use this event for custom additional components.
        /// </summary>
        /// <param name="go">The object that was selected.</param>
        public delegate void GameObjectSelection(GameObject go);
        
        /// <summary>
        /// Delegate of the OnSelect or OnHighlight events.
        /// </summary>
        /// <param name="islandElement">The associate island Element of the selected entity.</param>
        /// <param name="entity">The entity that was selected.</param>
        /// <param name="highlighted">True = select/highlight, false = deselect/dehighlight.</param>
        public delegate void HighlightEvent(IIslandSystemElement islandElement, bool highlighted, int handID);

        /// <summary>
        /// Delegate of the OnSelect events.
        /// </summary>
        /// <param name="islandElement">The associate island Element of the selected entity.</param>
        /// <param name="entity">The entity that was selected.</param>
        /// <param name="selected">True = select/highlight, false = deselect/dehighlight.</param>
        public delegate void SelectEvent(IIslandSystemElement islandElement, bool selected);
    }
}