using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OsgiViz.Core;


namespace OsgiViz.SideThreadConstructors
{

    public class SideThreadConstructor {

        public string projectModelFile = GlobalVar.projectmodelPath;
        private JsonObjConstructor jConstructor;
        private OsgiProjectConstructor osgiConstructor;
        private IslandStructureConstructor isConstructor;
        private Graph_Layout_Constructor bdConstructor;
        private Status status;

        //Todo Implement Multicore Support
        public SideThreadConstructor()
        {
            jConstructor = new JsonObjConstructor();
            jConstructor.Construct(projectModelFile, advanceConstruction);

            osgiConstructor = new OsgiProjectConstructor();
            isConstructor = new IslandStructureConstructor(1, 2, 8);
            bdConstructor = new Graph_Layout_Constructor();
            status = Status.Working;
        }

        //This method is called when a constructor finishes its task,
        //It handles the sequential execution of order dependant construction tasks
        private void advanceConstruction()
        {
            //Start OsgiProject construction
            if (jConstructor.getStatus() == Status.Finished)
            {
                jConstructor.setStatus(Status.Idle);
                osgiConstructor.Construct(jConstructor.getJsonModel(), advanceConstruction);
            }

            //Start IslandStructure construction
            if (osgiConstructor.getStatus() == Status.Finished)
            {
                Debug.Log("Project has a total of " + osgiConstructor.getProject().getNumberOfCUs() + " compilation units!");
                osgiConstructor.setStatus(Status.Idle);
                isConstructor.Construct(osgiConstructor.getProject(), advanceConstruction);
            }
            //Start the spatial distribution of islands
            if (isConstructor.getStatus() == Status.Finished)
            {
                isConstructor.setStatus(Status.Idle);
                Vector3 minBounds = new Vector3(-10.5f, 1.31f, -10.5f);
                Vector3 maxBounds = new Vector3(10.5f, 1.31f, 10.5f);
                //bdConstructor.ConstructRndLayout(osgiConstructor.getProject().getDependencyGraph(), advanceConstruction, minBounds, maxBounds, 0.075f, 10000);
                bdConstructor.ConstructFDLayout(osgiConstructor.getProject(), advanceConstruction, 0.25f, 70000);
            }
            //Wrap it up!
            if (bdConstructor.getStatus() == Status.Finished)
            {
                bdConstructor.setStatus(Status.Idle);
                status = Status.Finished;
            }

        }

        public OsgiProjectConstructor getOsgiProjectConstructor()
        {
            return osgiConstructor;
        }

        public Graph_Layout_Constructor getIslandDistributionConstructor()
        {
            return bdConstructor;
        }

        public IslandStructureConstructor getIslandStructureConstructor()
        {
            return isConstructor;
        }

        public Status getStatus()
        {
            return status;
        }

        public void setStatus(Status newStatus)
        {
            status = newStatus;
        }

    }

}