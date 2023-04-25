using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class TestingInputSystem : MonoBehaviour
{
    private Rigidbody playerRigidbody;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        //////// 여기부터!!  영상 16:00 부터 보면 됨.  아래 줄 제거 후 진행해도 무방해보임
        playerInput.onActionTriggered += PlayerInput_onActionTriggered;
    }

    private void PlayerInput_onActionTriggered(InputAction.CallbackContext context)
    {
        Debug.Log(context);
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
