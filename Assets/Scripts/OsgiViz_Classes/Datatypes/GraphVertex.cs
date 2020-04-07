using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickGraph;
using OsgiViz.Island;

namespace OsgiViz.Relations
{
    public class GraphVertex {

        private CartographicIsland island;
        private Vector3 position;
        private string name;

        public GraphVertex(string n)
        {
            name = n;
            position = new Vector3(0, 0, 0);
        }

        public string getName()
        {
            return name;
        }
        public Vector3 getPosition()
        {
            return position;
        }
        public CartographicIsland getIsland()
        {
            return island;
        }

        public void setPosition(Vector3 newPos)
        {
            position = newPos;
        }
        public void setIsland(CartographicIsland i)
        {
            island = i;
        }

    }

}
