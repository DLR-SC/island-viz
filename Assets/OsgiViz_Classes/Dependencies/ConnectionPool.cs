using OSGI_Datatypes.OrganisationElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OsgiViz.Relations
{
    public struct IDPair
    {
        public int ID_ConnectionPoint_A;
        public int ID_ConnectionPoint_B;
        public IDPair(int a, int b)
        {
            ID_ConnectionPoint_A = a;
            ID_ConnectionPoint_B = b;
        }
    }

    //Bidirectional ConnectionPool for storing all of the Dependency arrows and Service connections
    public class ConnectionPool : MonoBehaviour
    {
        private Dictionary<IDPair, GameObject> pool;


        void Start()
        {
            pool = new Dictionary<IDPair, GameObject>();
            IslandVizInteraction.Instance.OnNewCommit += ResetConnectionPool;

        }

        public GameObject getConnection(IDPair key)
        {
            GameObject result;
            bool check = pool.TryGetValue(key, out result);
            if (check == false)
            {
                IDPair reverseConnection = new IDPair(key.ID_ConnectionPoint_B, key.ID_ConnectionPoint_A);
                check = pool.TryGetValue(reverseConnection, out result);
            }
            return result;
        }

        public void AddConnection(IDPair key, GameObject connection)
        {
            bool check = pool.ContainsKey(key);
            IDPair reverseConnection = new IDPair(key.ID_ConnectionPoint_B, key.ID_ConnectionPoint_A);
            bool reverseCheck = pool.ContainsKey(reverseConnection);

            if (check == false && reverseCheck == false)
                pool.Add(key, connection);

            return;
        }

        public void ResetConnectionPool(Commit oldCommit, Commit newCommit)
        {
            pool = new Dictionary<IDPair, GameObject>();
        }
    }

}