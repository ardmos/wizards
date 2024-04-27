using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 서버상의 각 플레이어 오브젝트에서 동작하는 스크립트 입니다.
/// 서버 권한 방식 애니메이션 컨트롤.
/// </summary>
public class PlayerAnimator : NetworkBehaviour
{
    public WizardMaleAnimState playerAttackAnimState;

    // 아래 상수 String들 따로 스크립트 만들어서 정리해주기
    private const int IS_WALKING = 0;
    private const int IS_GAMEOVER = 1;
    private const int IS_VICTORY = 2;

    // Wizard_Male
    private const int IS_CASTING_ATTACK_MAGIC = 4;
    private const int IS_CASTING_DEFENSIVE_MAGIC = 3;

    // Knight_Male
    private const int IS_ATTACK1 = 4;
    private const int IS_ATTACK2 = 5;
    private const int IS_Dash = 3;


    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        //GameMultiplayer.Instance.OnPlayerAttackAnimStateChanged += OnPlayerAttackAnimStateChanged;
        GameMultiplayer.Instance.OnPlayerMoveAnimStateChanged += OnPlayerMoveAnimStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        //GameMultiplayer.Instance.OnPlayerAttackAnimStateChanged -= OnPlayerAttackAnimStateChanged;
        GameMultiplayer.Instance.OnPlayerMoveAnimStateChanged -= OnPlayerMoveAnimStateChanged;
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
                animator.SetBool(IS_WALKING, false);
                break;
            case PlayerMoveAnimState.Walking:
                animator.SetBool(IS_WALKING, true);
                break;
            case PlayerMoveAnimState.GameOver:
                animator.SetBool(IS_WALKING, false);
                animator.SetTrigger(IS_GAMEOVER);
                break;               
        }
    }

    /*    /// <summary>
        /// AttackAnimState에 맞게 애니메이션을 실행시켜주는 메소드 입니다.
        /// AttackAnimState가 변경된 서버상의 플레이어 오브젝트에게 애니메이션을 바꾸라고 알려줍니다.(서버권한방식 애니메이션 변경)
        /// </summary>
        private void OnPlayerAttackAnimStateChanged(object sender, System.EventArgs e)
        {
            PlayerAttackAnimStateEventData eventData = (PlayerAttackAnimStateEventData)e;
            NetworkClient networkClient = NetworkManager.ConnectedClients[eventData.clientId];
            networkClient.PlayerObject.GetComponentInChildren<PlayerAnimator>().UpdateSpellAnimationOnServer(eventData.playerAttackAnimState);
            //Debug.Log($"{nameof(OnPlayerAttackAnimStateChanged)} Player{eventData.clientId} AttackAnimation OnPlayerAttackAnimStateChanged: {eventData.playerAttackAnimState}");
        }*/

    /// <summary>
    /// Wizard_male용 애니메이션
    /// </summary>
    /// <param name="wizardMaleAnimState"></param>
    public void UpdateWizardMaleAnimationOnServer(WizardMaleAnimState wizardMaleAnimState)
    {
        switch (wizardMaleAnimState)
        {
            case WizardMaleAnimState.Idle:
                animator.SetBool(IS_CASTING_ATTACK_MAGIC, false);
                animator.SetBool(IS_CASTING_DEFENSIVE_MAGIC, false);
                break;
            case WizardMaleAnimState.CastingAttackMagic:
                animator.SetBool(IS_CASTING_ATTACK_MAGIC, true);
                animator.SetBool(IS_CASTING_DEFENSIVE_MAGIC, false);
                break;
            case WizardMaleAnimState.ShootingMagic:
                animator.SetBool(IS_CASTING_ATTACK_MAGIC, false);
                animator.SetBool(IS_CASTING_DEFENSIVE_MAGIC, false);
                break;
            case WizardMaleAnimState.CastingDefensiveMagic:
                animator.SetBool(IS_CASTING_ATTACK_MAGIC, false);
                animator.SetBool(IS_CASTING_DEFENSIVE_MAGIC, true);
                break;
        }
    }

    public void UpdateKnightMaleAnimationOnServer(KnightMaleAnimState knightMaleAnimState)
    {
        switch (knightMaleAnimState)
        {
            case KnightMaleAnimState.Idle:
                animator.SetBool(IS_ATTACK1, false);
                animator.SetBool(IS_ATTACK2, false);
                animator.SetBool(IS_Dash, false);
                break;
            case KnightMaleAnimState.Attack1:
                animator.SetBool(IS_ATTACK1, true);
                animator.SetBool(IS_ATTACK2, false);
                animator.SetBool(IS_Dash, false);
                break;
            case KnightMaleAnimState.Attack2:
                animator.SetBool(IS_ATTACK1, false);
                animator.SetBool(IS_ATTACK2, true);
                animator.SetBool(IS_Dash, false);
                break;
            case KnightMaleAnimState.Dash:
                animator.SetBool(IS_ATTACK1, false);
                animator.SetBool(IS_ATTACK2, false);
                animator.SetBool(IS_Dash, true);
                break;
        }
    }
}
