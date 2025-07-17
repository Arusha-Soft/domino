using UnityEngine;

namespace Project.Core
{
    public class TempDominoPoint : MonoBehaviour
    {
        public bool UpIsConnected {  get; private set; }
        public bool DownIsConnected {  get; private set; }

        public TempDominoPoint SetUpIsConnected(bool isConnected)
        {
            UpIsConnected = isConnected;
            return this;
        }

        public TempDominoPoint SetDownIsConnected(bool isConnected)
        {
            DownIsConnected = isConnected;
            return this;
        }

        public void Clear()
        {
            UpIsConnected = false;
            DownIsConnected = false;
        }
    }
}