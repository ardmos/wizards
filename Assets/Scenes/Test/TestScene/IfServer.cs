using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class IfServer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtClientCount;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log($"I'm Server!");
            if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
            {
                Debug.Log($"There Is Two Client");
                LoadingSceneManager.LoadNetwork(LoadingSceneManager.Scene.GameRoomScene);
            }
            txtClientCount.text = NetworkManager.Singleton.ConnectedClients.Count.ToString();
        }
        else txtClientCount.text = "I'm Client";
    }
}
