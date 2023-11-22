using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ServerStartUp : MonoBehaviour
{
    private const string internalServerIp = "0.0.0.0";
    // Default port 7777
    private ushort serverPort = 7777;

    // Start is called before the first frame update
    void Start()
    {
        bool server = false;
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++) {
            if (args[i] == "-dedicatedServer")
            {
                server = true;
            }
            if (args[i] == "-port" && (i + 1 < args.Length))
            {
                serverPort = (ushort)int.Parse(args[i + 1]);
            }
        }

        if (server)
        {
            StartServer();
        }
    }

    private void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(internalServerIp, serverPort);

        NetworkManager.Singleton.StartServer();
    }

}
