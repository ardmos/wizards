using Unity.Netcode;
using UnityEngine;

public class SlashSkill : NetworkBehaviour
{
    [SerializeField] private SpellInfo skillInfo;
    [SerializeField] private GameObject hitVFXPrefab;
    [SerializeField] private ParticleSystem particleSystemMain;
    [SerializeField] private Collider mCollider;

    // 스킬 자동파괴 설정
    public void SetSelfDestroy()
    {
        // 4. 스킬 오브젝트 제거
        Destroy(gameObject, particleSystemMain.main.duration);
    }

    // 스킬 상세값 설정
    public virtual void InitSkillInfoDetail(SpellInfo spellInfoFromServer)
    {
        if (IsClient) return;

        skillInfo = new SpellInfo(spellInfoFromServer);

        // 플레이어 Layer 설정
        switch (skillInfo.ownerPlayerClientId)
        {
            case 0:
                gameObject.layer = LayerMask.NameToLayer("Player0");
                break;
            case 1:
                gameObject.layer = LayerMask.NameToLayer("Player1");
                break;
            case 2:
                gameObject.layer = LayerMask.NameToLayer("Player2");
                break;
            case 3:
                gameObject.layer = LayerMask.NameToLayer("Player3");
                break;
            default:
                break;
        }

        // 플레이어 본인 Layer는 충돌체크에서 제외합니다
        Physics.IgnoreLayerCollision(mCollider.gameObject.layer, gameObject.layer, true);
    }

    // 1. Hit(충돌) 인식
    private void OnCollisionEnter(Collision collision)
    {
        // 서버에서만 처리.
        if (IsClient) return;
        // 시전자 자신은 충돌차리 안함
        if (collision.collider.gameObject.layer == gameObject.layer)
        {
            Debug.Log("자신과 충돌");
            return;
        }

        // 충돌을 중복 처리하는것을 방지하기 위한 처리
        mCollider.enabled = false;
       
        Collider collider = collision.collider;

        // 충돌한게 공격마법일 경우
        if (collider.CompareTag("AttackSpell") || collider.CompareTag("AttackSkill"))
        {
            Debug.Log($"{skillInfo.spellName}이(가) 공격마법{collider.name}와(과) 충돌했습니다.");
        }
        // 충돌한게 플레이어일 경우, 플레이어의 피격 사실을 해당 플레이어의 SpellManager 알립니다. 
        else if (collider.CompareTag("Player"))
        {
            if (skillInfo == null)
            {
                Debug.Log("AttackSpell Info is null");
                return;
            }

            PlayerServer player = collider.GetComponent<PlayerServer>();
            if (player == null)
            {
                Debug.LogError("Player is null!");
                return;
            }

            sbyte damage = (sbyte)skillInfo.level;
            // 플레이어 피격을 서버에서 처리
            player.PlayerGotHitOnServer(damage, GetSkillInfo().ownerPlayerClientId);
        }
        // 기타 오브젝트 충돌
        else
        {
            //Debug.Log($"{skillInfo.spellName}이(가) {collider.name}와(과) Hit!");
            return;
        }

        // 2. 충돌지점에 Hit VFX 재생
        HitVFX(collision);

        // 3. Hit SFX 재생
        PlaySFX(SFX_Type.Hit);

        // 4. 스킬 오브젝트 제거 <-- SetSelfDestroy()로 따로 해주고있습니다.
        //Destroy(gameObject, 0.2f);
    }

    // 2. 충돌지점에 Hit VFX 재생
    private void HitVFX(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;

        if (hitVFXPrefab != null)
        {
            var hitVFX = Instantiate(hitVFXPrefab, pos, rot) as GameObject;
            hitVFX.GetComponent<NetworkObject>().Spawn();
            var particleSystem = hitVFX.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                particleSystem = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
            }
            Destroy(hitVFX, particleSystem.main.duration);
        }
        else Debug.Log($"hitVFXPrefab is null");
    }

    // 3. 해당 슬래쉬 스킬의 다양한 SFX 재생
    public void PlaySFX(SFX_Type sFX_Type)
    {
        //SoundManager.Instance?.PlayKnightSkillSFXClientRPC(skillInfo.spellName, sFX_Type);
    }

    public SpellInfo GetSkillInfo()
    {
        return skillInfo;
    }
}
