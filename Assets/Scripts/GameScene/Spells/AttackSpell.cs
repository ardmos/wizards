using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ���� ������ �������� ��ɵ��� �����ϴ� Ŭ����
/// </summary>
public abstract class AttackSpell : NetworkBehaviour
{
    [SerializeField] protected SpellInfo spellInfo;

    [SerializeField] protected GameObject muzzleVFXPrefab;
    [SerializeField] protected GameObject hitVFXPrefab;
    [SerializeField] protected List<GameObject> trails;

    // ���� �浹�� �Ӽ� ���
    public abstract SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell);

    /// <summary>
    /// 2. CollisionEnter �浹 ó�� (���� ���� ���)
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // ���������� ó��.
        if (IsClient) return;

        Debug.Log($"���� �浹! ����:{gameObject.name}, �浹ü:{collision.gameObject.name}");

        // �浹�� �ߺ� ó���ϴ°��� �����ϱ� ���� ó��
        GetComponent<Collider>().enabled = false;

        Collider collider = collision.collider;
        ulong spellOwnerClientId = GetSpellInfo().ownerPlayerClientId;

        // �浹�Ѱ� ���ݸ����� ���, � ������ ��Ƴ����� ��꿡 ��
        if (collider.CompareTag("AttackSpell") || collider.CompareTag("AttackSkill"))
        {
            SpellHitHandlerOnServer(collider);
        }
        // �浹�Ѱ� �÷��̾��� ���, �÷��̾��� �ǰ� ����� �ش� �÷��̾��� SpellManager �˸��ϴ�. 
        else if (collider.CompareTag("Player"))
        {
            if (GetSpellInfo() == null)
            {
                //Debug.Log("AttackSpell Info is null");
                return;
            }

            PlayerServer player = collider.GetComponent<PlayerServer>();
            if (player == null)
            {
                //Debug.LogError("Player is null!");
                return;
            }

            sbyte damage = (sbyte)GetSpellInfo().level;
            // �÷��̾� �ǰ��� �������� ó��
            player.PlayerGotHitOnServer(damage, spellOwnerClientId);
        }
        // ��Ÿ ������Ʈ �浹
        else
        {
            //Debug.Log($"{collider.name} Hit!");
        }

        // Ȥ�� �����ڰ� Casting ���¿����� Cooltime ���·� �Ѱ��ֱ� 
        NetworkClient networkClient = NetworkManager.ConnectedClients[spellOwnerClientId];
        SpellManagerServerWizard spellManagerServerWizard = networkClient.PlayerObject.GetComponent<SpellManagerServerWizard>();
        if (spellManagerServerWizard.GetSpellInfo(GetSpellInfo().spellName).spellState==SpellState.Aiming)
        {
            //Debug.Log($"ĳ���� ���¿��� �����߽��ϴ�!!! ��ų ���¸� �����մϴ�. spellState:{spellManagerServerWizard.GetSpellInfo(GetSpellInfo().spellName).spellState} -> ");
            int spellIndex = spellManagerServerWizard.GetSpellIndexBySpellName(GetSpellInfo().spellName);
            if(spellIndex != -1)
            {
                spellManagerServerWizard.UpdatePlayerSpellState((ushort)spellIndex, SpellState.Cooltime);
                spellManagerServerWizard.playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
                //Debug.Log($"{spellManagerServerWizard.GetSpellInfo(GetSpellInfo().spellName).spellState}.");
            }
            else Debug.Log($"���� �ε����� ã�� ���߽��ϴ�.");
        }

        // ���� �浹 ���� ���
        PlaySFX(SFX_Type.Hit);

        // ���� ȿ�� VFX
        HitVFX(GetHitVFXPrefab(), collision);

        Destroy(gameObject, 0.2f);
    }

    // �浹�Ѱ� Spell�� ���, ���簣 �浹 ��� ó���� ���� �����մϴ�
    // ó������� collisionHandlingResult ������ �����ص״ٰ� DestroyParticle�ÿ� castSpell ��ŵ�ϴ�.
    private void SpellHitHandlerOnServer(Collider collider)
    {
        // �浹 ó����� ����
        SpellInfo collisionHandlingResult = null;

        // �ϴ� ���� ����
        gameObject.GetComponent<Rigidbody>().isKinematic = true;

        SpellInfo thisSpell = GetSpellInfo();
        SpellInfo opponentsSpell = collider.GetComponent<AttackSpell>().GetSpellInfo();

        collisionHandlingResult = CollisionHandling(thisSpell, opponentsSpell);

        //Debug.Log($"Spell���� �浹! ourSpell: name{thisSpell.spellName}, lvl{thisSpell.level}, owner{thisSpell.ownerPlayerClientId}  // " +
            //$"opponentsSpell : name{opponentsSpell.spellName}, lvl{opponentsSpell.level}, owner{opponentsSpell.ownerPlayerClientId}");

        // ���糢�� �浹�ؼ� �츮 ������ �̰��� �� ��� ����� ���� �浹 ��ġ�� ���ο� ���� ����. 
        if (collisionHandlingResult.level > 0)
        {
            //Debug.Log($"our spell is win! generate spell.name:{collisionHandlingResult.spellName}, spell.level :{collisionHandlingResult.level}, spell.owner: {collisionHandlingResult.ownerPlayerClientId} ");
            SpawnSpellObjectOnServer(collisionHandlingResult, transform);
        }
    }

    // ����ȿ�� VFX
    public void HitVFX(GameObject hitVFXPrefab, Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;

        if (hitVFXPrefab != null)
        {
            //Debug.Log($"hitVFXPrefab is Not null");
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

    /// <summary>
    /// ���� �����ϱ�.
    /// Spell���� �浹 ���� ȣ��Ǵ� �޼ҵ� �Դϴ�.
    /// �÷��̾ ĳ������ ������ �߻��ϴ� �޼ҵ�� �Ʒ� ServerRPC �޼ҵ� �Դϴ�.
    /// </summary>
    /// �ʿ��Ѱ�.
    ///     1. ���� Transform
    ///     2. �� �߻�ü SpellName
    public void SpawnSpellObjectOnServer(SpellInfo spellInfo, Transform spawnPosition)
    {
        // ������ ���� �߻�ü ��ġ��Ű��
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(spellInfo.spellName), spawnPosition.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo);
        //Debug.Log($"SpawnSpellObjectOnServer!! skillInfo.ownerClientId : {spellInfo.ownerPlayerClientId}, name:{spellInfo.spellName}, lvl:{spellInfo.level}");

        spellObject.transform.SetParent(GameManager.Instance.transform);

        // ���� �߻�ü ���� �����ϱ�
        spellObject.transform.forward = spawnPosition.forward;

        // ���� �߻�
        float moveSpeed = spellInfo.moveSpeed;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);
    }

    // ���� �󼼰� ����
    public virtual void InitSpellInfoDetail(SpellInfo spellInfoFromServer)
    {
        if (IsClient) return;

        spellInfo = new SpellInfo(spellInfoFromServer);
    }

    public virtual void Shoot(Vector3 force, ForceMode forceMode)
    {
        if (IsClient) return;

        GetComponent<Rigidbody>().AddForce(force, forceMode);
        // ���� �߻� ���� ���
        PlaySFX(SFX_Type.Shooting);
    }

    public SpellInfo GetSpellInfo()
    {
        return spellInfo;
    }
    public GameObject GetMuzzleVFXPrefab()
    {
        return muzzleVFXPrefab;
    }
    public GameObject GetHitVFXPrefab()
    {
        return hitVFXPrefab;
    }
    public List<GameObject> GetTrails()
    {
        return trails;
    }

    public void PlaySFX(SFX_Type sFX_Type)
    {
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, sFX_Type, transform);
    }
}
