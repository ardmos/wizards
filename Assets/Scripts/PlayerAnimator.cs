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

        // 이제 여기서 Player Walking이랑 GameOver 구독해서 애니메이션 실행해주면 됨.
        GameMultiplayer.Instance.OnPlayerMoveAnimStateChanged += OnPlayerMoveAnimStateChanged;
    }

    /// <summary>
    /// 게임 내 접속중인(서버에 존재하는) 플레이어들중 MoveAnimState가 변경된 플레이어가 있을 시 호출되는 리스너 입니다.
    /// MoveAnimState가 변경된 서버상의 플레이어 오브젝트에게 애니메이션을 바꾸라고 알려줍니다.(서버권한방식 애니메이션 변경)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">PlayerAnimState가 변경된 플레이어의 clientId와 PlayerMoveAnimState를 담고있습니다.</param>
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
    /// SpellState 변경시 변경된 State에 맞게 애니메이션을 실행시켜주는 메소드 입니다.
    /// SpellState가 변경된 서버상의 플레이어 오브젝트에게 애니메이션을 바꾸라고 알려줍니다.(서버권한방식 애니메이션 변경)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">SpellState가 변경된 플레이어의 clientId와 SpellIndex가 있습니다.</param>
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
                // Casting 애니메이션 시작
                animator.SetBool(IS_CASTING, true);
                break;
            case SpellState.Ready:
            case SpellState.Cooltime:
                // 기본 애니메이션으로 복귀.
                animator.SetBool(IS_CASTING, false);
                break;
        }        
    }

    private void Update()
    {
        // Update 말고 EventHandler를 사용해서 구현해보자 

        //Debug.Log($"is walking? {player.IsWalking()}");
        //animator.SetBool(IS_WALKING, player.IsWalking());
        // 애니메이션 변경이 필요합니다. 단계 구분. 1.캐스팅 2.발사
        //animator.SetBool(IS_ATTACK1, player.IsAttack1());
    }


}
