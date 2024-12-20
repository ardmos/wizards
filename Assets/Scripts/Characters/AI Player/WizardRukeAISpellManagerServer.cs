using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static ComponentValidator;

public class WizardRukeAISpellManagerServer : MonoBehaviour
{
    // SpellInfoList의 인덱스 0~2까지는 공격마법, 3은 방어마법 입니다.
    public const byte DEFENCE_SPELL_INDEX_DEFAULT = 3;

    public WizardRukeAIServer wizardRukeAIServer;

    public PlayerAnimator playerAnimator;

    [Header("마법 발사 위치 설정")]
    public Transform muzzlePos_Normal;
    public Transform muzzlePos_AoE;

    [Header("보유 스킬 관리")]
    private List<SpellInfo> playerOwnedSpellInfoListOnServer = new List<SpellInfo>();
    private float[] restTimeCurrentSpellArrayOnClient = new float[4];
    private GameObject playerCastingSpell;

    #region 쿨타임 
    private void Update()
    {
        // 쿨타임 관리
        for (ushort spellIndex = 0; spellIndex < playerOwnedSpellInfoListOnServer.Count; spellIndex++)
        {
            Cooltime(spellIndex);
        }
    }

    private void Cooltime(ushort spellIndex)
    {
        if (playerOwnedSpellInfoListOnServer[spellIndex] == null) return;
        // 쿨타임 관리
        if (playerOwnedSpellInfoListOnServer[spellIndex].spellState == SpellState.Cooltime)
        {
            restTimeCurrentSpellArrayOnClient[spellIndex] += Time.deltaTime;
            if (restTimeCurrentSpellArrayOnClient[spellIndex] >= playerOwnedSpellInfoListOnServer[spellIndex].coolTime)
            {
                restTimeCurrentSpellArrayOnClient[spellIndex] = 0f;
                // 여기서 서버에 Ready로 바뀐 State를 보고 하면 서버에서 다시 콜백해서 현 클라이언트 오브젝트의 State가 Ready로 바뀌긴 하는데, 그 사이에 딜레이가 있어서
                // 여기서 한 번 클라이언트의 State를 바꿔주고 서버에 보고 해준다.
                playerOwnedSpellInfoListOnServer[spellIndex].spellState = SpellState.Ready;
                UpdatePlayerSpellState(spellIndex, SpellState.Ready);
            }          
        }
    }
    #endregion

    #region Attack Spell Cast&Fire

    /// <summary>
    /// Blizzard 스킬 전용 캐스팅 메서드
    /// </summary>
    /// <param name="spellIndex"></param>
    public void CastBlizzard()
    {
        if (playerOwnedSpellInfoListOnServer[2].spellState != SpellState.Ready) return;
        if (!ValidateComponent(GameAssetsManager.Instance, "WizardRukeAISpellManagerServer GameAssetsManager.Instance 설정이 안되어있습니다.")) return;
        if (!ValidateComponent(muzzlePos_AoE, "WizardRukeAISpellManagerServer muzzlePos_AoE 설정이 안되어있습니다.")) return;
        if (!ValidateComponent(playerAnimator, "WizardRukeAISpellManagerServer playerAnimator 설정이 안되어있습니다.")) return;

        // 범위 표시 오브젝트 생성
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(SpellName.BlizzardLv1_Ready), muzzlePos_AoE.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        spellObject.transform.SetParent(transform);
        // 포구에 발사체 위치시키기
        spellObject.transform.localPosition = muzzlePos_AoE.localPosition;

        // 플레이어가 보고있는 방향과 발사체가 바라보는 방향 일치시키기
        spellObject.transform.forward = transform.forward;

        // 플레이어가 시전중인 마법에 저장하기
        playerCastingSpell = spellObject;

        // 해당 플레이어의 마법 SpellState 업데이트
        UpdatePlayerSpellState(2, SpellState.Casting);

        // 캐스팅 애니메이션 실행
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingAttackMagic);
    }


    public void FireBlizzard()
    {
        if (playerOwnedSpellInfoListOnServer[2].spellState != SpellState.Casting) return;
        if (!ValidateComponent(GameAssetsManager.Instance, "WizardRukeAISpellManagerServer GameAssetsManager.Instance 설정이 안되어있습니다.")) return;
        if (!ValidateComponent(muzzlePos_AoE, "WizardRukeAISpellManagerServer muzzlePos_AoE 설정이 안되어있습니다.")) return;
        if (!ValidateComponent(wizardRukeAIServer, "WizardRukeAISpellManagerServer wizardRukeAIServer 설정이 안되어있습니다.")) return;
        if (!ValidateComponent(playerAnimator, "WizardRukeAISpellManagerServer playerAnimator 설정이 안되어있습니다.")) return;

        // 1. 시전중인 범위표시 오브젝트 제거
        Destroy(playerCastingSpell);
        // 2. 블리자드 스킬 이펙트오브젝트 생성
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(SpellName.BlizzardLv1), muzzlePos_AoE.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        if (spellObject.TryGetComponent<AoESpell>(out var aoESpell))
        {
            aoESpell.SetOwner(wizardRukeAIServer.GetClientID(), gameObject);
            aoESpell.InitAoESpell(GetSpellInfo(2));
        }

        // 해당 SpellState 업데이트
        UpdatePlayerSpellState(2, SpellState.Cooltime);
        spellObject.transform.SetParent(MultiplayerGameManager.Instance.transform);

        // 발사 애니메이션 실행
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
    }


    /// <summary>
    /// 공격 마법 생성해주기. 캐스팅 시작 ( NetworkObject는 Server에서만 생성 가능합니다 )
    /// </summary>
    public void CastSpell(ushort spellIndex)
    {
        if (playerOwnedSpellInfoListOnServer[spellIndex].spellState != SpellState.Ready) return;
        if (!ValidateComponent(GameAssetsManager.Instance, "WizardRukeAISpellManagerServer GameAssetsManager.Instance 설정이 안되어있습니다.")) return;
        if (!ValidateComponent(muzzlePos_AoE, "WizardRukeAISpellManagerServer muzzlePos_AoE 설정이 안되어있습니다.")) return;
        if (!ValidateComponent(wizardRukeAIServer, "WizardRukeAISpellManagerServer wizardRukeAIServer 설정이 안되어있습니다.")) return;
        if (!ValidateComponent(SoundManager.Instance, "WizardRukeAISpellManagerServer SoundManager.Instance 설정이 안되어있습니다.")) return;
        if (!ValidateComponent(playerAnimator, "WizardRukeAISpellManagerServer playerAnimator 설정이 안되어있습니다.")) return;

        // 발사체 오브젝트 생성
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(GetSpellInfo(spellIndex).spellName), muzzlePos_Normal.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        // 발사체 스펙 초기화 해주기
        SpellInfo spellInfo = new SpellInfo(GetSpellInfo(spellIndex));
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo, gameObject);
        // 호밍 마법이라면 호밍 마법에 소유자 등록 & 속도 설정
        if (spellObject.TryGetComponent<HomingMissile>(out var ex))
        {
            ex.SetOwner(wizardRukeAIServer.GetClientID(), gameObject);
            ex.SetSpeed(spellInfo.moveSpeed);
        }
        spellObject.transform.SetParent(transform);
        // 포구에 발사체 위치시키기
        spellObject.transform.localPosition = muzzlePos_Normal.localPosition;

        // 마법 생성 SFX 실행
        SoundManager.Instance.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Aiming, transform);

        // 플레이어가 보고있는 방향과 발사체가 바라보는 방향 일치시키기
        spellObject.transform.forward = transform.forward;

        // 플레이어가 시전중인 마법에 저장하기
        playerCastingSpell = spellObject;

        // 해당 플레이어의 마법 SpellState 업데이트
        UpdatePlayerSpellState(spellIndex, SpellState.Casting);

        // 캐스팅 애니메이션 실행
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingAttackMagic);
    }

    /// <summary>
    /// 플레이어의 요청으로 현재 캐스팅중인 마법을 발사하는 메소드 입니다.
    /// </summary>
    public void FireSpell(ushort spellIndex)
    {
        if (playerOwnedSpellInfoListOnServer[spellIndex].spellState != SpellState.Casting) return;
        if (!ValidateComponent(MultiplayerGameManager.Instance, "WizardRukeAISpellManagerServer MultiplayerGameManager.Instance 설정이 안되어있습니다.")) return;
        if (!ValidateComponent(playerCastingSpell, "WizardRukeAISpellManagerServer playerCastingSpell 설정이 안되어있습니다.")) return;
        if (!ValidateComponent(SoundManager.Instance, "WizardRukeAISpellManagerServer SoundManager.Instance 설정이 안되어있습니다.")) return;
        if (!ValidateComponent(playerAnimator, "WizardRukeAISpellManagerServer playerAnimator 설정이 안되어있습니다.")) return;

        // 해당 SpellState 업데이트
        UpdatePlayerSpellState(spellIndex, SpellState.Cooltime);

        playerCastingSpell.transform.SetParent(MultiplayerGameManager.Instance.transform);
        float moveSpeed = 0;
        if (playerCastingSpell.TryGetComponent<AttackSpell>(out AttackSpell attackSpell))
        {
            moveSpeed = attackSpell.GetSpellInfo().moveSpeed;

            // 호밍 마법이라면 호밍 시작 처리
            if (attackSpell.TryGetComponent<HomingMissile>(out var ex)) ex.StartHoming();
            // 설치 마법
            else if (moveSpeed == 0)
            {
                // 설치 마법은 아무것도 안해도 됩니다.
            }
            // 마법 발사 (기본 직선 비행 마법)
            else
            {
                attackSpell.Shoot(playerCastingSpell.transform.forward * moveSpeed, ForceMode.Impulse);
            }

            // 발사 SFX 실행 
            SpellInfo spellInfo = new SpellInfo(GetSpellInfo(spellIndex));
            SoundManager.Instance.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Shooting, transform);

            // 포구 VFX
            MuzzleVFX(attackSpell.GetMuzzleVFXPrefab(), GetComponentInChildren<MuzzlePos>().transform);

            // 발사 애니메이션 실행
            playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
        }   


    }
    #endregion

    #region Defence Spell Cast
    /// <summary>
    /// 방어 마법 시전
    /// </summary>
    /// <param name="player"></param>
    public void StartActivateDefenceSpell()
    {
        if (playerOwnedSpellInfoListOnServer[DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Ready) return;
        if (!ValidateComponent(GameAssetsManager.Instance, "WizardRukeAISpellManagerServer GameAssetsManager.Instance 설정이 안되어있습니다.")) return;
        if (!ValidateComponent(playerAnimator, "WizardRukeAISpellManagerServer playerAnimator 설정이 안되어있습니다.")) return;

        // 마법 시전
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(GetSpellInfo(DEFENCE_SPELL_INDEX_DEFAULT).spellName), transform.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>()?.Spawn();
        spellObject.GetComponent<DefenceSpell>()?.InitSpellInfoDetail(GetSpellInfo(DEFENCE_SPELL_INDEX_DEFAULT));
        spellObject.transform.SetParent(transform);
        spellObject.transform.localPosition = Vector3.zero;
        spellObject.GetComponent<DefenceSpell>()?.Activate();

        // 마법 생성 사운드 재생
        spellObject.GetComponent<DefenceSpell>()?.PlaySFX(SFX_Type.Aiming);

        // 해당 SpellState 업데이트
        UpdatePlayerSpellState(DEFENCE_SPELL_INDEX_DEFAULT, SpellState.Cooltime);

        // 애니메이션 실행
        StartCoroutine(StartAndResetAnimState(spellObject.GetComponent<DefenceSpell>().GetSpellInfo().lifetime));

        // 잠시 무적 처리
        tag = "Invincible";
    }

    IEnumerator StartAndResetAnimState(float lifeTime)
    {
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingDefensiveMagic);
        yield return new WaitForSeconds(lifeTime);

        // 무적 해제
        tag = "AI";

        // 플레이어 캐릭터가 Casting 애니메이션중이 아닐 경우에만 Idle로 변경
        if (!playerAnimator.playerAttackAnimState.Equals(WizardMaleAnimState.CastingAttackMagic))
        {
            playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.Idle);
        }
    }
    #endregion

    #region Spell VFX
    // 포구 VFX 
    public void MuzzleVFX(GameObject muzzleVFXPrefab, Transform muzzleTransform)
    {
        if (!ValidateComponent(muzzleVFXPrefab, "WizardRukeAISpellManagerServer muzzleVFXPrefab 설정이 안되어있습니다.")) return;
        GameObject muzzleVFX = Instantiate(muzzleVFXPrefab, muzzleTransform.position, Quaternion.identity);
        if (!ValidateComponent(muzzleVFX, "WizardRukeAISpellManagerServer muzzleVFX 설정이 안되어있습니다.")) return;
        var particleSystem = muzzleVFX.GetComponent<ParticleSystem>();
        if (!ValidateComponent(particleSystem, "WizardRukeAISpellManagerServer particleSystem 설정이 안되어있습니다.")) return;

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
    /// Server측에서 보유한 SpellInfo 리스트 초기화 메소드 입니다.
    /// 플레이어 최초 생성시 호출됩니다.
    /// </summary>
    public void InitAIPlayerSpellInfoArrayOnServer(SpellName[] skillNames)
    {
        if (!ValidateComponent(SpellSpecifications.Instance, "WizardRukeAISpellManagerServer SpellSpecifications.Instance 설정이 안되어있습니다.")) return;

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
    /// 현재 client가 보유중인 특정 마법의 정보를 알려주는 메소드 입니다.  
    /// </summary>
    /// <param name="spellName">알고싶은 마법의 이름</param>
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
    /// -1을 반환하면 못찾았다는 뜻
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
