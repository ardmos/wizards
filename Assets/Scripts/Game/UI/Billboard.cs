using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform camera;

    private void Start()
    {
        camera = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + camera.forward);
    }
}
