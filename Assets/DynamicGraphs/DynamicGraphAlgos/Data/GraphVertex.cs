using UnityEngine;
//using OsgiViz.Island;


namespace DynamicGraphAlgoImplementation
{
    public class GraphVertex
    {
        //private CartographicIsland island;
        private Vector3 position;
        private string name;
        
        public GraphVertex(string n)
        {
            name = n;
            position = Vector3.negativeInfinity;
        }

        public string getName()
        {
            return name;
        }

       public virtual Vector3 getPosition()
        {
            return position;
        }
        /*public CartographicIsland getIsland()
        {
            return island;
        }*/

        public void setPosition(Vector3 newPos)
        {
            if (float.IsNaN(newPos.x) || float.IsPositiveInfinity(newPos.x) || float.IsNegativeInfinity(newPos.x))
            {
                Debug.Log("StopHere x");
            }
            if (float.IsNaN(newPos.z) || float.IsPositiveInfinity(newPos.z) || float.IsNegativeInfinity(newPos.z))
            {
                Debug.Log("StopHere z");
            }
            position = newPos;
        }

        public virtual float getRadius()
        {
            //TODO
            //return this.getIsland().getRadius();
            return 0.05f;
        }

        public Vector2 getXZPosAs2D()
        {
            return new Vector2(position.x, position.z);
        }
        /*public void setIsland(CartographicIsland i)
        {
            island = i;
        }*/
    }
}