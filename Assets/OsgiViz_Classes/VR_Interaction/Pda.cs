using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OsgiViz.Core;

namespace OsgiViz
{
    public class Pda : MonoBehaviour
    {

        public List<PdaPage> pages;
        private PdaPage currentPage;
        private String currentInspectContent;

        private GameObject observer;

        void Awake()
        {
            if (pages == null)
                throw new Exception("Pda has no Pages defined!");

            currentPage = null;

            observer = GameObject.FindGameObjectWithTag("Observer");
        }

        void Start()
        {
            currentInspectContent = "";
        }

        public void setInspectContent(String newContent)
        {
            currentInspectContent = newContent;
            switchToInspectPage();
        }

        
        // Update is called once per frame
        void Update () {
            
            float dotProd = Vector3.Dot(observer.transform.forward, -transform.up);
            bool val = (dotProd > 0);
            
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                gameObject.transform.GetChild(i).gameObject.SetActive(val);
            }  
            
        }
        

        public void switchToSettingsPage()
        {
            //Debug.Log("Switching to inspection page.");
            PdaPage settingsPage = pages.Find(x => x.pageType == PdaPageType.Settings);
            if (currentPage == null)
            {
                currentPage = GameObject.Instantiate(settingsPage.gameObject, transform).GetComponent<PdaPage>();
            }
            else if (currentPage.pageType != PdaPageType.Settings)
            {
                GameObject.Destroy(currentPage.gameObject);
                currentPage = GameObject.Instantiate(settingsPage.gameObject, transform).GetComponent<PdaPage>();
            }
        }

        public void switchToInspectPage()
        {
            PdaPage inspectPage = pages.Find(x => x.pageType == PdaPageType.Inspect);
            if (currentPage == null)
            {
                currentPage = GameObject.Instantiate(inspectPage.gameObject, transform).GetComponent<PdaPage>();
            }
            else if (currentPage.pageType != PdaPageType.Inspect)
            {
                GameObject.Destroy(currentPage.gameObject);
                currentPage = GameObject.Instantiate(inspectPage.gameObject, transform).GetComponent<PdaPage>();
            }

            TMPro.TMP_Text[] textComponents = currentPage.gameObject.GetComponentsInChildren<TMPro.TMP_Text>();
            TMPro.TMP_Text textField = null;
            foreach(TMPro.TMP_Text textComp in textComponents)
            {
                if(textComp.gameObject.name == "Content")
                    textField = textComp;
            }
            if(textField != null)
                textField.SetText(currentInspectContent);

        }

    }
}
