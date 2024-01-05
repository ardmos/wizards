using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    private const string IS_WALKING = "IsWalking";
    private const string IS_CASTING = "IsCasting";
    private const string IS_GAMEOVER = "IsGameFinished";

    [SerializeField] private Player player;
    [SerializeField] private SpellController spellController;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();


        spellController.OnSpellStateChanged += OnSpellStateChanged;
        //Debug.Log("PlayerAnimator Awake!");

        // ���� ���⼭ Player Walking�̶� GameOver �����ؼ� �ִϸ��̼� �������ָ� ��.
        GameMultiplayer.Instance.OnPlayerMoveAnimStateChanged += OnPlayerMoveAnimStateChanged;
    }

    /// <summary>
    /// ���� �� ��������(������ �����ϴ�) �÷��̾���� MoveAnimState�� ����� �÷��̾ ���� �� ȣ��Ǵ� ������ �Դϴ�.
    /// MoveAnimState�� ����� �������� �÷��̾� ������Ʈ���� �ִϸ��̼��� �ٲٶ�� �˷��ݴϴ�.(�������ѹ�� �ִϸ��̼� ����)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">PlayerAnimState�� ����� �÷��̾��� clientId�� PlayerMoveAnimState�� ����ֽ��ϴ�.</param>
    private void OnPlayerMoveAnimStateChanged(object sender, System.EventArgs e)
    {
        PlayerAnimStateEventData eventData = (PlayerAnimStateEventData)e;
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
                animator.SetBool(IS_GAMEOVER, false);
                break;
            case PlayerMoveAnimState.Walking:
                animator.SetBool(IS_WALKING, true);
                animator.SetBool(IS_GAMEOVER, false);
                break;
            case PlayerMoveAnimState.GameOver:
                animator.SetBool(IS_GAMEOVER, true);
                break;
               
        }
    }

    /// <summary>
    /// SpellState ����� ����� State�� �°� �ִϸ��̼��� ��������ִ� �޼ҵ� �Դϴ�.
    /// SpellState�� ����� �������� �÷��̾� ������Ʈ���� �ִϸ��̼��� �ٲٶ�� �˷��ݴϴ�.(�������ѹ�� �ִϸ��̼� ����)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">SpellState�� ����� �÷��̾��� clientId�� SpellIndex�� �ֽ��ϴ�.</param>
    private void OnSpellStateChanged(object sender, System.EventArgs e)
    {
        SpellStateEventData eventData = (SpellStateEventData)e;     
        NetworkClient networkClient = NetworkManager.ConnectedClients[eventData.clientId];
        networkClient.PlayerObject.GetComponentInChildren<PlayerAnimator>().UpdateSpellAnimationOnServer(eventData.spellState);
        Debug.Log($"Player{eventData.clientId} SpellAnimation OnSpellStateChanged: {eventData.spellState}");
    }
    private void UpdateSpellAnimationOnServer(SpellState spellState)
    {
        switch (spellState)
        {
            case SpellState.Casting:
                // Casting �ִϸ��̼� ����
                animator.SetBool(IS_CASTING, true);
                break;
            case SpellState.Ready:
            case SpellState.Cooltime:
                // �⺻ �ִϸ��̼����� ����.
                animator.SetBool(IS_CASTING, false);
                break;
        }        
    }

    private void Update()
    {
        // Update ���� EventHandler�� ����ؼ� �����غ��� 

        //Debug.Log($"is walking? {player.IsWalking()}");
        //animator.SetBool(IS_WALKING, player.IsWalking());
        // �ִϸ��̼� ������ �ʿ��մϴ�. �ܰ� ����. 1.ĳ���� 2.�߻�
        //animator.SetBool(IS_ATTACK1, player.IsAttack1());
    }


}
