using UnityEngine;

namespace IslandInterface.Input
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;

        public PlayerType Type;
        public Transform[] Hands;

        public Camera PlayerCamera;



        public enum PlayerType
        {
            Desktop,
            VR,
            AR
        }


        void Awake()
        {
            Instance = this;
        }
    }

}
