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
    private const string IS_WALKING = "IsWalking";
    private const string IS_CASTING_ATTACK_MAGIC = "IsCastingAttackMagic";
    private const string IS_CASTING_DEFENSIVE_MAGIC = "IsCastingDefensiveMagic";
    private const string IS_GAMEOVER = "IsGameOver";

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        GameMultiplayer.Instance.OnPlayerAttackAnimStateChanged += OnPlayerAttackAnimStateChanged;
        GameMultiplayer.Instance.OnPlayerMoveAnimStateChanged += OnPlayerMoveAnimStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        GameMultiplayer.Instance.OnPlayerAttackAnimStateChanged -= OnPlayerAttackAnimStateChanged;
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

    /// <summary>
    /// AttackAnimState�� �°� �ִϸ��̼��� ��������ִ� �޼ҵ� �Դϴ�.
    /// AttackAnimState�� ����� �������� �÷��̾� ������Ʈ���� �ִϸ��̼��� �ٲٶ�� �˷��ݴϴ�.(�������ѹ�� �ִϸ��̼� ����)
    /// </summary>
    private void OnPlayerAttackAnimStateChanged(object sender, System.EventArgs e)
    {
        PlayerAttackAnimStateEventData eventData = (PlayerAttackAnimStateEventData)e;
        NetworkClient networkClient = NetworkManager.ConnectedClients[eventData.clientId];
        networkClient.PlayerObject.GetComponentInChildren<PlayerAnimator>().UpdateSpellAnimationOnServer(eventData.playerAttackAnimState);
        //Debug.Log($"{nameof(OnPlayerAttackAnimStateChanged)} Player{eventData.clientId} AttackAnimation OnPlayerAttackAnimStateChanged: {eventData.playerAttackAnimState}");
    }

    private void UpdateSpellAnimationOnServer(PlayerAttackAnimState playerAttackAnimState)
    {
        switch (playerAttackAnimState)
        {
            case PlayerAttackAnimState.Idle:
                animator.SetBool(IS_CASTING_ATTACK_MAGIC, false);
                animator.SetBool(IS_CASTING_DEFENSIVE_MAGIC, false);
                break;
            case PlayerAttackAnimState.CastingAttackMagic:
                animator.SetBool(IS_CASTING_ATTACK_MAGIC, true);
                animator.SetBool(IS_CASTING_DEFENSIVE_MAGIC, false);
                break;
            case PlayerAttackAnimState.ShootingMagic:
                animator.SetBool(IS_CASTING_ATTACK_MAGIC, false);
                animator.SetBool(IS_CASTING_DEFENSIVE_MAGIC, false);
                break;
            case PlayerAttackAnimState.CastingDefensiveMagic:
                animator.SetBool(IS_CASTING_ATTACK_MAGIC, false);
                animator.SetBool(IS_CASTING_DEFENSIVE_MAGIC, true);
                break;
            default:
                break;
        }
    }
}
