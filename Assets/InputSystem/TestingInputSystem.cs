using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingInputSystem : MonoBehaviour
{
    private Rigidbody playerRigidbody;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();    
    }

    public void Jump() {
        Debug.Log("Jump!");
        playerRigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse);
    }
}
