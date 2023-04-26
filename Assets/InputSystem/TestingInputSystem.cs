using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class TestingInputSystem : MonoBehaviour
{
    private Rigidbody playerRigidbody;
    private PlayerInputActions playerInputActions;

    public float movementSpeed = 1f;
    public float jumpPower = 5f;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += Jump;
    }

    private void FixedUpdate()
    {
        Movement();
    }

    public void Movement()
    {
        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        playerRigidbody.AddForce(new Vector3(inputVector.x, 0, inputVector.y) * movementSpeed, ForceMode.Force);
    }

    public void Jump(InputAction.CallbackContext context) {
        Debug.Log(context);
        if (context.performed)
        {
            Debug.Log("Jump! " + context.phase);
            playerRigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }
}
