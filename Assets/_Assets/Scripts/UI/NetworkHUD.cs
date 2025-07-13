using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    public class NetworkHUD : MonoBehaviour
    {
        [SerializeField] private Button m_Host;
        [SerializeField] private Button m_Client;

        public static event Action OnStart;

        private void OnEnable()
        {
            m_Host.onClick.AddListener(CreateHost);
            m_Client.onClick.AddListener(CreateClient);
        }

        private void CreateClient()
        {
            OnStart?.Invoke();
            NetworkManager.Singleton.StartClient();
            gameObject.SetActive(false);
        }

        private void CreateHost()
        {
            OnStart?.Invoke();
            NetworkManager.Singleton.StartHost();
            gameObject.SetActive(false);
        }
    }
}