using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;
/// <summary>
/// 서버상의 각 플레이어 오브젝트에서 동작하는 스크립트 입니다.
/// 서버 권한 방식 애니메이션 컨트롤.
/// </summary>
public class PlayerAnimator : NetworkBehaviour
{
    public WizardMaleAnimState playerAttackAnimState;

    // 아래 상수 String들 따로 스크립트 만들어서 정리해주기
    public int k_IS_WALKING;
    public int k_IS_GAMEOVER;
    public int k_IS_VICTORY;

    // Wizard_Male
    public int k_IS_CASTING_ATTACK_MAGIC;
    public int k_IS_CASTING_DEFENSIVE_MAGIC;

    // Knight_Male
    public int k_IS_ATTACK_VERTICAL_REAY;
    public int k_IS_ATTACK_VERTICAL;
    public int k_IS_ATTACK_WHIRLWIND_REAY;
    public int k_IS_ATTACK_WHIRLWIND;
    public int k_IS_Dash;


    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        k_IS_WALKING = GetAnimatorParameterID("IsWalking");
        k_IS_GAMEOVER = GetAnimatorParameterID("IsGameOver");
        k_IS_VICTORY = GetAnimatorParameterID("IsVictory");
        k_IS_CASTING_ATTACK_MAGIC = GetAnimatorParameterID("IsCastingAttackMagic");
        k_IS_CASTING_DEFENSIVE_MAGIC = GetAnimatorParameterID("IsCastingDefensiveMagic");
        k_IS_ATTACK_VERTICAL_REAY = GetAnimatorParameterID("IsAttackVerticalReady");
        k_IS_ATTACK_VERTICAL = GetAnimatorParameterID("IsAttackVertical");
        k_IS_ATTACK_WHIRLWIND_REAY = GetAnimatorParameterID("IsAttackWhirlwindReady");
        k_IS_ATTACK_WHIRLWIND = GetAnimatorParameterID("IsAttackWhirlwind");
        k_IS_Dash = GetAnimatorParameterID("IsDash");
    }

    public override void OnNetworkSpawn()
    {
        //GameMultiplayer.Instance.OnPlayerAttackAnimStateChanged += OnPlayerAttackAnimStateChanged;
        GameMultiplayer.Instance.OnPlayerMoveAnimStateChanged += OnPlayerMoveAnimStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        //GameMultiplayer.Instance.OnPlayerAttackAnimStateChanged -= OnPlayerAttackAnimStateChanged;
        GameMultiplayer.Instance.OnPlayerMoveAnimStateChanged -= OnPlayerMoveAnimStateChanged;
    }

    private int GetAnimatorParameterID(string parameterName)
    {
        // 파라미터의 이름을 사용하여 해시 값 얻기
        return Animator.StringToHash(parameterName);
    }

    /// <summary>
    /// 서버에서 동작하는 메소드 입니다.
    /// 게임 내 접속중인(서버에 존재하는) 플레이어들중 MoveAnimState가 변경된 플레이어가 있을 시 호출되는 리스너 입니다.
    /// MoveAnimState가 변경된 서버상의 플레이어 오브젝트에게 애니메이션을 바꾸라고 알려줍니다.(서버권한방식 애니메이션 변경)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">PlayerAnimState가 변경된 플레이어의 clientId와 PlayerMoveAnimState를 담고있습니다.</param>
    private void OnPlayerMoveAnimStateChanged(object sender, System.EventArgs e)
    {
        PlayerMoveAnimStateEventData eventData = (PlayerMoveAnimStateEventData)e;
        NetworkClient networkClient = NetworkManager.ConnectedClients[eventData.clientId];
        networkClient.PlayerObject.GetComponentInChildren<PlayerAnimator>().UpdatePlayerMoveAnimationOnServer(eventData.playerMoveAnimState);
        //Debug.Log($"Player{eventData.clientId} MoveAnimation OnPlayerMoveAnimStateChanged: {eventData.playerMoveAnimState}");
    }
    private void UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState playerMoveAnimState)
    {
        switch(playerMoveAnimState)
        {
            case PlayerMoveAnimState.Idle:
                animator.SetBool(k_IS_WALKING, false);
                break;
            case PlayerMoveAnimState.Walking:
                animator.SetBool(k_IS_WALKING, true);
                break;
            case PlayerMoveAnimState.GameOver:
                animator.SetBool(k_IS_WALKING, false);
                animator.SetTrigger(k_IS_GAMEOVER);
                break;               
        }
    }

    /// <summary>
    /// Wizard_male용 애니메이션
    /// </summary>
    /// <param name="wizardMaleAnimState"></param>
    public void UpdateWizardMaleAnimationOnServer(WizardMaleAnimState wizardMaleAnimState)
    {
        switch (wizardMaleAnimState)
        {
            case WizardMaleAnimState.Idle:
                animator.SetBool(k_IS_CASTING_ATTACK_MAGIC, false);
                animator.SetBool(k_IS_CASTING_DEFENSIVE_MAGIC, false);
                break;
            case WizardMaleAnimState.CastingAttackMagic:
                animator.SetBool(k_IS_CASTING_ATTACK_MAGIC, true);
                animator.SetBool(k_IS_CASTING_DEFENSIVE_MAGIC, false);
                break;
            case WizardMaleAnimState.ShootingMagic:
                animator.SetBool(k_IS_CASTING_ATTACK_MAGIC, false);
                animator.SetBool(k_IS_CASTING_DEFENSIVE_MAGIC, false);
                break;
            case WizardMaleAnimState.CastingDefensiveMagic:
                animator.SetBool(k_IS_CASTING_ATTACK_MAGIC, false);
                animator.SetBool(k_IS_CASTING_DEFENSIVE_MAGIC, true);
                break;
        }
    }

    public void UpdateKnightMaleAnimationOnServer(KnightMaleAnimState knightMaleAnimState)
    {
        switch (knightMaleAnimState)
        {
            case KnightMaleAnimState.Idle:
                animator.SetBool(k_IS_ATTACK_VERTICAL_REAY, false);
                animator.SetBool(k_IS_ATTACK_WHIRLWIND_REAY, false);
                break;
            case KnightMaleAnimState.AttackVerticalReady:
                animator.SetBool(k_IS_ATTACK_VERTICAL_REAY, true);
                break;
            case KnightMaleAnimState.AttackVertical:
                animator.SetBool(k_IS_ATTACK_VERTICAL_REAY, false);
                animator.SetTrigger(k_IS_ATTACK_VERTICAL);
                break;
            case KnightMaleAnimState.AttackWhirlwindReady:
                animator.SetBool(k_IS_ATTACK_WHIRLWIND_REAY, true);
                break;
            case KnightMaleAnimState.AttackWhirlwind:
                animator.SetBool(k_IS_ATTACK_WHIRLWIND_REAY, false);
                animator.SetTrigger(k_IS_ATTACK_WHIRLWIND);
                break;
            case KnightMaleAnimState.Dash:
                animator.SetTrigger(k_IS_Dash);
                break;
        }
    }
}
