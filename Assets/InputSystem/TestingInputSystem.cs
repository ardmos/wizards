using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class TestingInputSystem : MonoBehaviour
{
    private Rigidbody playerRigidbody;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();

        PlayerInputActions playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += Jump;
        
    }

    public void Jump(InputAction.CallbackContext context) {
        Debug.Log(context);
        if (context.performed)
        {
            Debug.Log("Jump! " + context.phase);
            playerRigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }
    }
}
