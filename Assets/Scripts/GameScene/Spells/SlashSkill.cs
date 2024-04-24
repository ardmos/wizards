using Unity.Netcode;
using UnityEngine;

public class SlashSkill : NetworkBehaviour
{
    public const byte SFX_SHOOTING = 1;
    public const byte SFX_HIT = 2;

    [SerializeField] private SpellInfo skillInfo;
    [SerializeField] private GameObject hitVFXPrefab;

    // ��ų �ڵ��ı� ����
    public void SetSelfDestroy()
    {
        // 4. ��ų ������Ʈ ����
        Destroy(gameObject, GetComponent<ParticleSystem>().main.duration);
    }

    // ��ų �󼼰� ����
    public virtual void InitSkillInfoDetail(SpellInfo spellInfoFromServer)
    {
        if (IsClient) return;

        skillInfo = new SpellInfo(spellInfoFromServer);
    }

    // 1. Hit(�浹) �ν�
    private void OnCollisionEnter(Collision collision)
    {
        // ���������� ó��.
        if (IsClient) return;

        // �浹�� �ߺ� ó���ϴ°��� �����ϱ� ���� ó��
        GetComponent<Collider>().enabled = false;

        Collider collider = collision.collider;

        // �浹�Ѱ� ���ݸ����� ���
        if (collider.CompareTag("AttackSpell"))
        {
            Debug.Log($"{skillInfo.spellName}��(��) ���ݸ���{collider.name}��(��) �浹�߽��ϴ�.");
        }
        // �浹�Ѱ� �÷��̾��� ���, �÷��̾��� �ǰ� ����� �ش� �÷��̾��� SpellManager �˸��ϴ�. 
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
            // �÷��̾� �ǰ��� �������� ó��
            player.PlayerGotHitOnServer(damage, player);
        }
        // ��Ÿ ������Ʈ �浹
        else
        {
            Debug.Log($"{skillInfo.spellName}��(��) {collider.name}��(��) Hit!");
        }

        // 2. �浹������ Hit VFX ���
        HitVFX(collision);

        // 3. Hit SFX ���
        PlaySFX(SFX_HIT);

        // 4. ��ų ������Ʈ ����
        Destroy(gameObject, 0.2f);
    }

    // 2. �浹������ Hit VFX ���
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

    // 3. �ش� ������ ��ų�� �پ��� SFX ��� ( ����: 1, Hit: 2 )
    public void PlaySFX(byte state)
    {
        if (SoundManager.Instance == null) return;

        SoundManager.Instance.PlayKnightSkillSFXClientRPC(skillInfo.spellName, state);
    }

}
