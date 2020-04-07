using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Text;
using System;
using UnityEngine;
using OsgiViz.Core;


namespace OsgiViz.SideThreadConstructors
{

    public class JsonObjConstructor
    {

        private callbackMethod cb;
        private JSONObject jsonObj;
        private string fileToLoad;
        Thread _thread;
        private Status status;

        public JsonObjConstructor()
        {
            jsonObj = null;
            status = Status.Idle;
        }

        //Public method to load a JSONObject from a file in a separate thread
        public void Construct(string filePath, callbackMethod m)
        {
            fileToLoad = filePath;
            cb = m;
            _thread = new Thread(Load);
            _thread.Start();
        }

        //Access the loaded JSONObject
        public JSONObject getJsonModel()
        {
            return jsonObj;
        }

        public Status getStatus()
        {
            return status;
        }

        public void setStatus(Status newStatus)
        {
            status = newStatus;
        }

        private void Load()
        {

            string dataAsString = "";

            try
            {
                status = Status.Working;
                StreamReader theReader = new StreamReader(fileToLoad, Encoding.Default);
                using (theReader)
                {
                    dataAsString = theReader.ReadToEnd();
                    theReader.Close();
                }

                Debug.Log("Finished reading file. Starting the construction of a JSONObject from the string data.");
                jsonObj = JSONObject.Create(dataAsString);
                status = Status.Finished;
                Debug.Log("Finished constructing the JSONObject. SUCCESS");
                cb();
            }

            catch (Exception e)
            {
                Debug.Log(e.Message);
                status = Status.Failed;
                Debug.Log("Finished constructing the JSONObject. FAILURE");
            }

        }
    }

}

