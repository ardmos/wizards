using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 삭제 가능 
/// </summary>

public class PlayerNetwork : NetworkBehaviour
{
    // NetworkVariable 로 간단하게 클라이언트간 변수 공유가 가능하다. 권한설정도 가능
    private NetworkVariable<int> randomNumber =  new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    private void Update()
    {
        Debug.Log(OwnerClientId + "; randomNumber: " + randomNumber.Value);

        // 접속한 유저의 캐릭터만 현 스크립트로 움직이도록 체크
        if (!IsOwner) return;  

        // 
        if (Input.GetKeyDown(KeyCode.T))
        {
            randomNumber.Value = Random.Range(0, 100);
        }

        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}
