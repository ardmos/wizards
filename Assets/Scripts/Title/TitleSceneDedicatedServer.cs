using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSceneDedicatedServer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if DEDICATED_SERVER
        Debug.Log("DEDICATED_SERVER 5.2");
#endif
    }
}
