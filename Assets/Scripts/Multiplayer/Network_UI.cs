using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Network_UI : MonoBehaviour
{
    [SerializeField] private Button m_ServerButton;
    [SerializeField] private Button m_HostButton;
    [SerializeField] private Button m_ClientButton;

    private void Start()
    {
        m_ServerButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });
        m_HostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        m_ClientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }
}
