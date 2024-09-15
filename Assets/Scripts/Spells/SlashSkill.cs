using Unity.Netcode;
using UnityEngine;

public class SlashSkill : NetworkBehaviour
{
    [SerializeField] private SpellInfo skillInfo;
    [SerializeField] private GameObject hitVFXPrefab;
    [SerializeField] private ParticleSystem particleSystemMain;
    //[SerializeField] private Collider mCollider;

    [Header("AI�� �ǰݵ��� �� Ÿ������ ������ ������ ������ �÷��̾� ������Ʈ")]
    public GameObject skillOwnerObject;

    // ��ų �ڵ��ı� ����
    public void SetSelfDestroy()
    {
        // 4. ��ų ������Ʈ ����
        Destroy(gameObject, particleSystemMain.main.duration);
    }

    // ��ų �󼼰� ����
    public virtual void InitSkillInfoDetail(SpellInfo spellInfoFromServer, GameObject skillOwnerObject)
    {
        if (!IsServer) return;
        //Debug.Log($"Slash Skill InitSkillInfoDetail ");

        skillInfo = new SpellInfo(spellInfoFromServer);
        this.skillOwnerObject = skillOwnerObject;

        // Layer ����
        LayerMask shooterLayer;
        switch (skillInfo.ownerPlayerClientId)
        {
            case 0:
                gameObject.layer = LayerMask.NameToLayer("Attack Skill Player0");
                shooterLayer = LayerMask.NameToLayer("Player0");
                break;
            case 1:
                gameObject.layer = LayerMask.NameToLayer("Attack Skill Player1");
                shooterLayer = LayerMask.NameToLayer("Player1");
                break;
            case 2:
                gameObject.layer = LayerMask.NameToLayer("Attack Skill Player2");
                shooterLayer = LayerMask.NameToLayer("Player2");
                break;
            case 3:
                gameObject.layer = LayerMask.NameToLayer("Attack Skill Player3");
                shooterLayer = LayerMask.NameToLayer("Player3");
                break;
            default:
                shooterLayer = LayerMask.NameToLayer("Player");
                break;
        }

        // �÷��̾� ���� Layer�� �浹üũ���� �����մϴ�
        Physics.IgnoreLayerCollision(gameObject.layer, shooterLayer, true);
    }

    // 1. Hit(�浹) �ν�
    private void OnCollisionEnter(Collision collision)
    {
        // ���������� ó��.
        if (IsClient) return;
        // ������ �ڽ��� �浹���� ����
        if (collision.collider.gameObject.layer == gameObject.layer)
        {
            Debug.Log("�ڽŰ� �浹");
            return;
        }

        // �浹�� �ߺ� ó���ϴ°��� �����ϱ� ���� ó��
        GetComponent<Collider>().enabled = false;

        Collider collider = collision.collider;

        // �浹�Ѱ� ���ݸ����� ���
        if (collider.CompareTag("AttackSpell") || collider.CompareTag("AttackSkill"))
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

            PlayerHPManagerServer player = collider.GetComponent<PlayerHPManagerServer>();
            if (player == null)
            {
                Debug.LogError("Player is null!");
                return;
            }

            sbyte damage = (sbyte)skillInfo.damage;
            // �÷��̾� �ǰ��� �������� ó��
            player.TakeDamage(damage, GetSkillInfo().ownerPlayerClientId);
            // ���� �������� ȭ�� ��鸲 ȿ�� ����
            SpellOwnersCameraShakeEffect();
        }
        // AI�÷��̾��� ��� ó��
        else if (collider.CompareTag("AI"))
        {
            if (skillInfo == null) return;

            // WizardRukeAI Ȯ��.  ���� �ٸ� AI�߰� �� ����.           
            if (collider.TryGetComponent<WizardRukeAIHPManagerServer>(out WizardRukeAIHPManagerServer aiPlayer))
            {
                sbyte damage = (sbyte)skillInfo.damage;
                // �÷��̾� �ǰ��� �������� ó��
                aiPlayer.TakingDamageWithCameraShake(damage, GetSkillInfo().ownerPlayerClientId, skillOwnerObject);
            }
        }
        // ��Ÿ ������Ʈ �浹
        else
        {
            //Debug.Log($"{skillInfo.spellName}��(��) {collider.name}��(��) Hit!");
            return;
        }

        // 2. �浹������ Hit VFX ���
        HitVFX(collision);

        // 3. Hit SFX ���
        PlaySFX(SFX_Type.Hit);

        // 4. ��ų ������Ʈ ���� <-- SetSelfDestroy()�� ���� ���ְ��ֽ��ϴ�.
        //Destroy(gameObject, 0.2f);
    }

    // 2. �浹������ Hit VFX ���
    private void HitVFX(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        float positionYAdjustment = 1f;
        float positionZAdjustment = -1f;
        Vector3 pos = new Vector3(contact.point.x, contact.point.y + positionYAdjustment, contact.point.z + positionZAdjustment);

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

    // 3. �ش� ������ ��ų�� �پ��� SFX ���
    public void PlaySFX(SFX_Type sFX_Type)
    {
        //SoundManager.Instance?.PlayKnightSkillSFXClientRPC(skillInfo.spellName, sFX_Type);
    }

    public SpellInfo GetSkillInfo()
    {
        return skillInfo;
    }

    private void SpellOwnersCameraShakeEffect()
    {
        // ���� �����ڰ� Player���� Ȯ�� �� ī�޶� ����ũ 
        if (skillOwnerObject.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            playerClient.ActivateHitCameraShakeClientRPC();
        }
    }
}
