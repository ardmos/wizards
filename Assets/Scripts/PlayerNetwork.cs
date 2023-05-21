using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 삭제 가능 
/// 네트워크 오브젝트에서는 Awake나 Start 사용해선 안됨.  대신 OnNetworkSpawn 사용
/// </summary>

public class PlayerNetwork : NetworkBehaviour
{
    // NetworkVariable 로 간단하게 클라이언트간 변수 공유가 가능하다. 권한설정도 가능. 
    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData {
            _int = 56,
            _bool = true
        }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        // 네트워크 오브젝트에서는 string 대신 FixedString 사용. 사이즈를 정해줘야함. 여기선 128바이트. 1캐릭터 1바이트 
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            //throw new System.NotImplementedException();
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + "; randomNumber: " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
        };
    }

    private void Update()
    {
        

        // 접속한 유저의 캐릭터만 현 스크립트로 움직이도록 체크
        if (!IsOwner) return;  

        // 
        if (Input.GetKeyDown(KeyCode.T))
        {
            randomNumber.Value = new MyCustomData
            {
                _int = 10,
                _bool = false,
                message = "new message!!!!"
                
            };
        }

/*        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;*/
    }
}
