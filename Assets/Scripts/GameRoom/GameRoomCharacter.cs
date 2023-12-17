using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoomCharacter : MonoBehaviour
{
    private void OnDisable()
    {
        Destroy(gameObject);
    }
}
