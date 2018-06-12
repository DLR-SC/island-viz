using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OsgiViz
{
    public class PdaInspectable : MonoBehaviour
    {

        private Pda pdaDevice;

        void Awake()
        {
            GameObject pdaGO = GameObject.FindGameObjectWithTag("PDA");
            if(pdaGO == null)
                throw new Exception("Scene does not contain a gameobject with the tag 'PDA'!");
            pdaDevice = pdaGO.GetComponent<Pda>();
            if(pdaDevice == null)
                throw new Exception("The pda gameobject does not have a pda component attatched!");
        }

        // Use this for initialization
        void Start()
        {

        }

        public void sendContentToPda(String content)
        {
            pdaDevice.setInspectContent(content);
        }

    }
}
