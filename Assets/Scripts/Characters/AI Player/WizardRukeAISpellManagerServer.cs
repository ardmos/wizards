using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static ComponentValidator;

public class WizardRukeAISpellManagerServer : MonoBehaviour
{
    // SpellInfoList�� �ε��� 0~2������ ���ݸ���, 3�� ���� �Դϴ�.
    public const byte DEFENCE_SPELL_INDEX_DEFAULT = 3;

    public WizardRukeAIServer wizardRukeAIServer;

    public PlayerAnimator playerAnimator;

    [Header("���� �߻� ��ġ ����")]
    public Transform muzzlePos_Normal;
    public Transform muzzlePos_AoE;

    [Header("���� ��ų ����")]
    private List<SpellInfo> playerOwnedSpellInfoListOnServer = new List<SpellInfo>();
    private float[] restTimeCurrentSpellArrayOnClient = new float[4];
    private GameObject playerCastingSpell;

    #region ��Ÿ�� 
    private void Update()
    {
        // ��Ÿ�� ����
        for (ushort spellIndex = 0; spellIndex < playerOwnedSpellInfoListOnServer.Count; spellIndex++)
        {
            Cooltime(spellIndex);
        }
    }

    private void Cooltime(ushort spellIndex)
    {
        if (playerOwnedSpellInfoListOnServer[spellIndex] == null) return;
        // ��Ÿ�� ����
        if (playerOwnedSpellInfoListOnServer[spellIndex].spellState == SpellState.Cooltime)
        {
            restTimeCurrentSpellArrayOnClient[spellIndex] += Time.deltaTime;
            if (restTimeCurrentSpellArrayOnClient[spellIndex] >= playerOwnedSpellInfoListOnServer[spellIndex].coolTime)
            {
                restTimeCurrentSpellArrayOnClient[spellIndex] = 0f;
                // ���⼭ ������ Ready�� �ٲ� State�� ���� �ϸ� �������� �ٽ� �ݹ��ؼ� �� Ŭ���̾�Ʈ ������Ʈ�� State�� Ready�� �ٲ�� �ϴµ�, �� ���̿� �����̰� �־
                // ���⼭ �� �� Ŭ���̾�Ʈ�� State�� �ٲ��ְ� ������ ���� ���ش�.
                playerOwnedSpellInfoListOnServer[spellIndex].spellState = SpellState.Ready;
                UpdatePlayerSpellState(spellIndex, SpellState.Ready);
            }          
        }
    }
    #endregion

    #region Attack Spell Cast&Fire

    /// <summary>
    /// Blizzard ��ų ���� ĳ���� �޼���
    /// </summary>
    /// <param name="spellIndex"></param>
    public void CastBlizzard()
    {
        if (playerOwnedSpellInfoListOnServer[2].spellState != SpellState.Ready) return;
        if (!ValidateComponent(GameAssetsManager.Instance, "WizardRukeAISpellManagerServer GameAssetsManager.Instance ������ �ȵǾ��ֽ��ϴ�.")) return;
        if (!ValidateComponent(muzzlePos_AoE, "WizardRukeAISpellManagerServer muzzlePos_AoE ������ �ȵǾ��ֽ��ϴ�.")) return;
        if (!ValidateComponent(playerAnimator, "WizardRukeAISpellManagerServer playerAnimator ������ �ȵǾ��ֽ��ϴ�.")) return;

        // ���� ǥ�� ������Ʈ ����
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(SpellName.BlizzardLv1_Ready), muzzlePos_AoE.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        spellObject.transform.SetParent(transform);
        // ������ �߻�ü ��ġ��Ű��
        spellObject.transform.localPosition = muzzlePos_AoE.localPosition;

        // �÷��̾ �����ִ� ����� �߻�ü�� �ٶ󺸴� ���� ��ġ��Ű��
        spellObject.transform.forward = transform.forward;

        // �÷��̾ �������� ������ �����ϱ�
        playerCastingSpell = spellObject;

        // �ش� �÷��̾��� ���� SpellState ������Ʈ
        UpdatePlayerSpellState(2, SpellState.Casting);

        // ĳ���� �ִϸ��̼� ����
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingAttackMagic);
    }


    public void FireBlizzard()
    {
        if (playerOwnedSpellInfoListOnServer[2].spellState != SpellState.Casting) return;
        if (!ValidateComponent(GameAssetsManager.Instance, "WizardRukeAISpellManagerServer GameAssetsManager.Instance ������ �ȵǾ��ֽ��ϴ�.")) return;
        if (!ValidateComponent(muzzlePos_AoE, "WizardRukeAISpellManagerServer muzzlePos_AoE ������ �ȵǾ��ֽ��ϴ�.")) return;
        if (!ValidateComponent(wizardRukeAIServer, "WizardRukeAISpellManagerServer wizardRukeAIServer ������ �ȵǾ��ֽ��ϴ�.")) return;
        if (!ValidateComponent(playerAnimator, "WizardRukeAISpellManagerServer playerAnimator ������ �ȵǾ��ֽ��ϴ�.")) return;

        // 1. �������� ����ǥ�� ������Ʈ ����
        Destroy(playerCastingSpell);
        // 2. ���ڵ� ��ų ����Ʈ������Ʈ ����
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(SpellName.BlizzardLv1), muzzlePos_AoE.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        if (spellObject.TryGetComponent<AoESpell>(out var aoESpell))
        {
            aoESpell.SetOwner(wizardRukeAIServer.GetClientID(), gameObject);
            aoESpell.InitAoESpell(GetSpellInfo(2));
        }

        // �ش� SpellState ������Ʈ
        UpdatePlayerSpellState(2, SpellState.Cooltime);
        spellObject.transform.SetParent(MultiplayerGameManager.Instance.transform);

        // �߻� �ִϸ��̼� ����
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
    }


    /// <summary>
    /// ���� ���� �������ֱ�. ĳ���� ���� ( NetworkObject�� Server������ ���� �����մϴ� )
    /// </summary>
    public void CastSpell(ushort spellIndex)
    {
        if (playerOwnedSpellInfoListOnServer[spellIndex].spellState != SpellState.Ready) return;
        if (!ValidateComponent(GameAssetsManager.Instance, "WizardRukeAISpellManagerServer GameAssetsManager.Instance ������ �ȵǾ��ֽ��ϴ�.")) return;
        if (!ValidateComponent(muzzlePos_AoE, "WizardRukeAISpellManagerServer muzzlePos_AoE ������ �ȵǾ��ֽ��ϴ�.")) return;
        if (!ValidateComponent(wizardRukeAIServer, "WizardRukeAISpellManagerServer wizardRukeAIServer ������ �ȵǾ��ֽ��ϴ�.")) return;
        if (!ValidateComponent(SoundManager.Instance, "WizardRukeAISpellManagerServer SoundManager.Instance ������ �ȵǾ��ֽ��ϴ�.")) return;
        if (!ValidateComponent(playerAnimator, "WizardRukeAISpellManagerServer playerAnimator ������ �ȵǾ��ֽ��ϴ�.")) return;

        // �߻�ü ������Ʈ ����
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(GetSpellInfo(spellIndex).spellName), muzzlePos_Normal.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        // �߻�ü ���� �ʱ�ȭ ���ֱ�
        SpellInfo spellInfo = new SpellInfo(GetSpellInfo(spellIndex));
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo, gameObject);
        // ȣ�� �����̶�� ȣ�� ������ ������ ��� & �ӵ� ����
        if (spellObject.TryGetComponent<HomingMissile>(out var ex))
        {
            ex.SetOwner(wizardRukeAIServer.GetClientID(), gameObject);
            ex.SetSpeed(spellInfo.moveSpeed);
        }
        spellObject.transform.SetParent(transform);
        // ������ �߻�ü ��ġ��Ű��
        spellObject.transform.localPosition = muzzlePos_Normal.localPosition;

        // ���� ���� SFX ����
        SoundManager.Instance.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Aiming, transform);

        // �÷��̾ �����ִ� ����� �߻�ü�� �ٶ󺸴� ���� ��ġ��Ű��
        spellObject.transform.forward = transform.forward;

        // �÷��̾ �������� ������ �����ϱ�
        playerCastingSpell = spellObject;

        // �ش� �÷��̾��� ���� SpellState ������Ʈ
        UpdatePlayerSpellState(spellIndex, SpellState.Casting);

        // ĳ���� �ִϸ��̼� ����
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingAttackMagic);
    }

    /// <summary>
    /// �÷��̾��� ��û���� ���� ĳ�������� ������ �߻��ϴ� �޼ҵ� �Դϴ�.
    /// </summary>
    public void FireSpell(ushort spellIndex)
    {
        if (playerOwnedSpellInfoListOnServer[spellIndex].spellState != SpellState.Casting) return;
        if (!ValidateComponent(MultiplayerGameManager.Instance, "WizardRukeAISpellManagerServer MultiplayerGameManager.Instance ������ �ȵǾ��ֽ��ϴ�.")) return;
        if (!ValidateComponent(playerCastingSpell, "WizardRukeAISpellManagerServer playerCastingSpell ������ �ȵǾ��ֽ��ϴ�.")) return;
        if (!ValidateComponent(SoundManager.Instance, "WizardRukeAISpellManagerServer SoundManager.Instance ������ �ȵǾ��ֽ��ϴ�.")) return;
        if (!ValidateComponent(playerAnimator, "WizardRukeAISpellManagerServer playerAnimator ������ �ȵǾ��ֽ��ϴ�.")) return;

        // �ش� SpellState ������Ʈ
        UpdatePlayerSpellState(spellIndex, SpellState.Cooltime);

        playerCastingSpell.transform.SetParent(MultiplayerGameManager.Instance.transform);
        float moveSpeed = 0;
        if (playerCastingSpell.TryGetComponent<AttackSpell>(out AttackSpell attackSpell))
        {
            moveSpeed = attackSpell.GetSpellInfo().moveSpeed;

            // ȣ�� �����̶�� ȣ�� ���� ó��
            if (attackSpell.TryGetComponent<HomingMissile>(out var ex)) ex.StartHoming();
            // ��ġ ����
            else if (moveSpeed == 0)
            {
                // ��ġ ������ �ƹ��͵� ���ص� �˴ϴ�.
            }
            // ���� �߻� (�⺻ ���� ���� ����)
            else
            {
                attackSpell.Shoot(playerCastingSpell.transform.forward * moveSpeed, ForceMode.Impulse);
            }

            // �߻� SFX ���� 
            SpellInfo spellInfo = new SpellInfo(GetSpellInfo(spellIndex));
            SoundManager.Instance.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Shooting, transform);

            // ���� VFX
            MuzzleVFX(attackSpell.GetMuzzleVFXPrefab(), GetComponentInChildren<MuzzlePos>().transform);

            // �߻� �ִϸ��̼� ����
            playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
        }   


    }
    #endregion

    #region Defence Spell Cast
    /// <summary>
    /// ��� ���� ����
    /// </summary>
    /// <param name="player"></param>
    public void StartActivateDefenceSpell()
    {
        if (playerOwnedSpellInfoListOnServer[DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Ready) return;
        if (!ValidateComponent(GameAssetsManager.Instance, "WizardRukeAISpellManagerServer GameAssetsManager.Instance ������ �ȵǾ��ֽ��ϴ�.")) return;
        if (!ValidateComponent(playerAnimator, "WizardRukeAISpellManagerServer playerAnimator ������ �ȵǾ��ֽ��ϴ�.")) return;

        // ���� ����
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(GetSpellInfo(DEFENCE_SPELL_INDEX_DEFAULT).spellName), transform.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>()?.Spawn();
        spellObject.GetComponent<DefenceSpell>()?.InitSpellInfoDetail(GetSpellInfo(DEFENCE_SPELL_INDEX_DEFAULT));
        spellObject.transform.SetParent(transform);
        spellObject.transform.localPosition = Vector3.zero;
        spellObject.GetComponent<DefenceSpell>()?.Activate();

        // ���� ���� ���� ���
        spellObject.GetComponent<DefenceSpell>()?.PlaySFX(SFX_Type.Aiming);

        // �ش� SpellState ������Ʈ
        UpdatePlayerSpellState(DEFENCE_SPELL_INDEX_DEFAULT, SpellState.Cooltime);

        // �ִϸ��̼� ����
        StartCoroutine(StartAndResetAnimState(spellObject.GetComponent<DefenceSpell>().GetSpellInfo().lifetime));

        // ��� ���� ó��
        tag = "Invincible";
    }

    IEnumerator StartAndResetAnimState(float lifeTime)
    {
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingDefensiveMagic);
        yield return new WaitForSeconds(lifeTime);

        // ���� ����
        tag = "AI";

        // �÷��̾� ĳ���Ͱ� Casting �ִϸ��̼����� �ƴ� ��쿡�� Idle�� ����
        if (!playerAnimator.playerAttackAnimState.Equals(WizardMaleAnimState.CastingAttackMagic))
        {
            playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.Idle);
        }
    }
    #endregion

    #region Spell VFX
    // ���� VFX 
    public void MuzzleVFX(GameObject muzzleVFXPrefab, Transform muzzleTransform)
    {
        if (!ValidateComponent(muzzleVFXPrefab, "WizardRukeAISpellManagerServer muzzleVFXPrefab ������ �ȵǾ��ֽ��ϴ�.")) return;
        GameObject muzzleVFX = Instantiate(muzzleVFXPrefab, muzzleTransform.position, Quaternion.identity);
        if (!ValidateComponent(muzzleVFX, "WizardRukeAISpellManagerServer muzzleVFX ������ �ȵǾ��ֽ��ϴ�.")) return;
        var particleSystem = muzzleVFX.GetComponent<ParticleSystem>();
        if (!ValidateComponent(particleSystem, "WizardRukeAISpellManagerServer particleSystem ������ �ȵǾ��ֽ��ϴ�.")) return;

        muzzleVFX.GetComponent<NetworkObject>().Spawn();
        muzzleVFX.transform.position = muzzleTransform.position;
        muzzleVFX.transform.forward = muzzleTransform.forward;
        particleSystem = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();

        Destroy(muzzleVFX, particleSystem.main.duration);
    }
    #endregion

    #region SpellInfo
    public void UpdatePlayerSpellState(ushort spellIndex, SpellState spellState)
    {
        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;    
    }

    /// <summary>
    /// Server������ ������ SpellInfo ����Ʈ �ʱ�ȭ �޼ҵ� �Դϴ�.
    /// �÷��̾� ���� ������ ȣ��˴ϴ�.
    /// </summary>
    public void InitAIPlayerSpellInfoArrayOnServer(SpellName[] skillNames)
    {
        if (!ValidateComponent(SpellSpecifications.Instance, "WizardRukeAISpellManagerServer SpellSpecifications.Instance ������ �ȵǾ��ֽ��ϴ�.")) return;

        List<SpellInfo> playerSpellInfoList = new List<SpellInfo>();
        foreach (SpellName spellName in skillNames)
        {
            SpellInfo spellInfo = new SpellInfo(SpellSpecifications.Instance.GetSpellDefaultSpec(spellName));
            spellInfo.ownerPlayerClientId = wizardRukeAIServer.GetClientID();
            playerSpellInfoList.Add(spellInfo);
        }

        playerOwnedSpellInfoListOnServer = playerSpellInfoList;
    }

    public List<SpellInfo> GetSpellInfoList()
    {
        return playerOwnedSpellInfoListOnServer;
    }

    /// <summary>
    /// ���� client�� �������� Ư�� ������ ������ �˷��ִ� �޼ҵ� �Դϴ�.  
    /// </summary>
    /// <param name="spellName">�˰���� ������ �̸�</param>
    /// <returns></returns>
    public SpellInfo GetSpellInfo(SpellName spellName)
    {
        foreach (SpellInfo spellInfo in playerOwnedSpellInfoListOnServer)
        {
            if (spellInfo.spellName == spellName)
            {
                return spellInfo;
            }
        }

        return null;
    }

    public SpellInfo GetSpellInfo(ushort spellIndex)
    {
        if (playerOwnedSpellInfoListOnServer.Count <= spellIndex) return null;
        return playerOwnedSpellInfoListOnServer[spellIndex];
    }

    public void SetSpellInfo(ushort spellIndex, SpellInfo newSpellInfo)
    {
        if (playerOwnedSpellInfoListOnServer.Count <= spellIndex) return;
        playerOwnedSpellInfoListOnServer[spellIndex] = newSpellInfo;
    }

    /// <summary>
    /// -1�� ��ȯ�ϸ� ��ã�Ҵٴ� ��
    /// </summary>
    /// <param name="skillName"></param>
    /// <returns></returns>
    public int GetSpellIndexBySpellName(SpellName skillName)
    {
        int index = -1;

        for (int i = 0; i < playerOwnedSpellInfoListOnServer.Count; i++)
        {
            if (playerOwnedSpellInfoListOnServer[i].spellName.Equals(skillName))
            {
                index = i;
                break;
            }
        }
        return index;
    }
    #endregion
}
