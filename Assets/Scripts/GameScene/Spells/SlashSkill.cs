using Unity.Netcode;
using UnityEngine;

public class SlashSkill : NetworkBehaviour
{
    public const byte SFX_SHOOTING = 1;
    public const byte SFX_HIT = 2;

    [SerializeField] private SpellInfo skillInfo;
    [SerializeField] private GameObject hitVFXPrefab;

    // 스킬 자동파괴 설정
    public void SetSelfDestroy()
    {
        // 4. 스킬 오브젝트 제거
        Destroy(gameObject, GetComponent<ParticleSystem>().main.duration);
    }

    // 스킬 상세값 설정
    public virtual void InitSkillInfoDetail(SpellInfo spellInfoFromServer)
    {
        if (IsClient) return;

        skillInfo = new SpellInfo(spellInfoFromServer);
    }

    // 1. Hit(충돌) 인식
    private void OnCollisionEnter(Collision collision)
    {
        // 서버에서만 처리.
        if (IsClient) return;

        // 충돌을 중복 처리하는것을 방지하기 위한 처리
        GetComponent<Collider>().enabled = false;

        Collider collider = collision.collider;

        // 충돌한게 공격마법일 경우
        if (collider.CompareTag("AttackSpell"))
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

            byte damage = (byte)skillInfo.level;
            // 플레이어 피격을 서버에서 처리
            player.PlayerGotHitOnServer(damage, player);
        }
        // 기타 오브젝트 충돌
        else
        {
            Debug.Log($"{skillInfo.spellName}이(가) {collider.name}와(과) Hit!");
        }

        // 2. 충돌지점에 Hit VFX 재생
        HitVFX(collision);

        // 3. Hit SFX 재생
        PlaySFX(SFX_HIT);

        // 4. 스킬 오브젝트 제거
        Destroy(gameObject, 0.2f);
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

    // 3. 해당 슬래쉬 스킬의 다양한 SFX 재생 ( 시전: 1, Hit: 2 )
    public void PlaySFX(byte state)
    {
        if (SoundManager.Instance == null) return;

        SoundManager.Instance.PlayKnightSkillSFXClientRPC(skillInfo.spellName, state);
    }

}
