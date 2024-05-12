using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoESpell : MonoBehaviour
{
    [SerializeField] private ulong _shooterClientID;
    [SerializeField] private LayerMask _shooterLayer;

    public void SetOwner(ulong shooterClientID)
    {

        _shooterClientID = shooterClientID;

        // 플레이어 Layer 설정
        switch (_shooterClientID)
        {
            case 0:
                gameObject.layer = LayerMask.NameToLayer("Attack Magic Player0");
                _shooterLayer = LayerMask.NameToLayer("Player0");
                break;
            case 1:
                gameObject.layer = LayerMask.NameToLayer("Attack Magic Player1");
                _shooterLayer = LayerMask.NameToLayer("Player1");
                break;
            case 2:
                gameObject.layer = LayerMask.NameToLayer("Attack Magic Player2");
                _shooterLayer = LayerMask.NameToLayer("Player2");
                break;
            case 3:
                gameObject.layer = LayerMask.NameToLayer("Attack Magic Player3");
                _shooterLayer = LayerMask.NameToLayer("Player3");
                break;
            default:
                _shooterLayer = LayerMask.NameToLayer("Player");
                break;
        }
        
        // 플레이어 본인 Layer는 충돌체크에서 제외합니다
        Physics.IgnoreLayerCollision(gameObject.layer, _shooterLayer, true);
    }
}
