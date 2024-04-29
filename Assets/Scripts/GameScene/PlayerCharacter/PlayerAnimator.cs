using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;
/// <summary>
/// �������� �� �÷��̾� ������Ʈ���� �����ϴ� ��ũ��Ʈ �Դϴ�.
/// ���� ���� ��� �ִϸ��̼� ��Ʈ��.
/// </summary>
public class PlayerAnimator : NetworkBehaviour
{
    public WizardMaleAnimState playerAttackAnimState;

    // �Ʒ� ��� String�� ���� ��ũ��Ʈ ���� �������ֱ�
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
        // �Ķ������ �̸��� ����Ͽ� �ؽ� �� ���
        return Animator.StringToHash(parameterName);
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
    /// Wizard_male�� �ִϸ��̼�
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
