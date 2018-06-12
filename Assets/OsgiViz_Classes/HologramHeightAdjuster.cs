using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OsgiViz.Core;

namespace OsgiViz
{
    public class HologramHeightAdjuster : MonoBehaviour
    {

        //This component checks if the GlobalVar.hologramHeight was changed and adjusts the linked objects height accordingly.
        private float previousHeight;
        // Use this for initialization
        void Start()
        {
            previousHeight = GlobalVar.hologramTableHeight;
            adjustHologramHeight();
        }

        void Update()
        {
            if (GlobalVar.hologramTableHeight != previousHeight)
                adjustHologramHeight();
        }

        private void adjustHologramHeight()
        {
            Vector3 tempPos = transform.position;
            tempPos.y = GlobalVar.hologramTableHeight;
            previousHeight = GlobalVar.hologramTableHeight;
            transform.position = tempPos;
            
        }

    }
}
