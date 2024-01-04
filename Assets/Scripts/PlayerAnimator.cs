using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private const string IS_WALKING = "IsWalking";
    private const string IS_CASTING = "IsCasting";
    private const string IS_GAMEOVER = "IsGameOver";

    [SerializeField] private Player player;
    [SerializeField] private SpellController spellController;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();


        spellController.OnSpellStateChanged += OnSpellStateChanged;
        Debug.Log("PlayerAnimator Awake!");
    }

    /// <summary>
    /// SpellState 변경시 변경된 State에 맞게 애니메이션을 실행시켜주는 메소드 입니다.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">State가 변경된 SpellIndex가 있습니다.</param>
    private void OnSpellStateChanged(object sender, System.EventArgs e)
    {
        SpellState spellState = spellController.GetSpellStateFromSpellIndexOnServer(((SpellStateEventData)e).clientId, ((SpellStateEventData)e).spellIndex);
        switch(spellState) 
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
        Debug.Log($"PlayerAnimator OnSpellStateChanged: {spellState}");
    }

    private void Update()
    {
        // Update 말고 EventHandler를 사용해서 구현해보자 

        //Debug.Log($"is walking? {player.IsWalking()}");
        animator.SetBool(IS_WALKING, player.IsWalking());
        // 애니메이션 변경이 필요합니다. 단계 구분. 1.캐스팅 2.발사
        //animator.SetBool(IS_ATTACK1, player.IsAttack1());
    }


}
