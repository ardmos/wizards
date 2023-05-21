using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ���� ���� 
/// ��Ʈ��ũ ������Ʈ������ Awake�� Start ����ؼ� �ȵ�.  ��� OnNetworkSpawn ���
/// </summary>

public class PlayerNetwork : NetworkBehaviour
{
    // NetworkVariable �� �����ϰ� Ŭ���̾�Ʈ�� ���� ������ �����ϴ�. ���Ѽ����� ����. 
    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData {
            _int = 56,
            _bool = true
        }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        // ��Ʈ��ũ ������Ʈ������ string ��� FixedString ���. ����� ���������. ���⼱ 128����Ʈ. 1ĳ���� 1����Ʈ 
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
        

        // ������ ������ ĳ���͸� �� ��ũ��Ʈ�� �����̵��� üũ
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
