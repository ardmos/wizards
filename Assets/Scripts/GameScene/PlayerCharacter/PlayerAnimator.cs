using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// �������� �� �÷��̾� ������Ʈ���� �����ϴ� ��ũ��Ʈ �Դϴ�.
/// ���� ���� ��� �ִϸ��̼� ��Ʈ��.
/// </summary>
public class PlayerAnimator : NetworkBehaviour
{
    public WizardMaleAnimState playerAttackAnimState;

    // �Ʒ� ��� String�� ���� ��ũ��Ʈ ���� �������ֱ�
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
    /// �������� �����ϴ� �޼ҵ� �Դϴ�.
    /// ���� �� ��������(������ �����ϴ�) �÷��̾���� MoveAnimState�� ����� �÷��̾ ���� �� ȣ��Ǵ� ������ �Դϴ�.
    /// MoveAnimState�� ����� �������� �÷��̾� ������Ʈ���� �ִϸ��̼��� �ٲٶ�� �˷��ݴϴ�.(�������ѹ�� �ִϸ��̼� ����)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">PlayerAnimState�� ����� �÷��̾��� clientId�� PlayerMoveAnimState�� ����ֽ��ϴ�.</param>
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
        /// AttackAnimState�� �°� �ִϸ��̼��� ��������ִ� �޼ҵ� �Դϴ�.
        /// AttackAnimState�� ����� �������� �÷��̾� ������Ʈ���� �ִϸ��̼��� �ٲٶ�� �˷��ݴϴ�.(�������ѹ�� �ִϸ��̼� ����)
        /// </summary>
        private void OnPlayerAttackAnimStateChanged(object sender, System.EventArgs e)
        {
            PlayerAttackAnimStateEventData eventData = (PlayerAttackAnimStateEventData)e;
            NetworkClient networkClient = NetworkManager.ConnectedClients[eventData.clientId];
            networkClient.PlayerObject.GetComponentInChildren<PlayerAnimator>().UpdateSpellAnimationOnServer(eventData.playerAttackAnimState);
            //Debug.Log($"{nameof(OnPlayerAttackAnimStateChanged)} Player{eventData.clientId} AttackAnimation OnPlayerAttackAnimStateChanged: {eventData.playerAttackAnimState}");
        }*/

    /// <summary>
    /// Wizard_male�� �ִϸ��̼�
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
