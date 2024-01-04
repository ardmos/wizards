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
    /// SpellState ����� ����� State�� �°� �ִϸ��̼��� ��������ִ� �޼ҵ� �Դϴ�.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">State�� ����� SpellIndex�� �ֽ��ϴ�.</param>
    private void OnSpellStateChanged(object sender, System.EventArgs e)
    {
        SpellState spellState = spellController.GetSpellStateFromSpellIndexOnServer(((SpellStateEventData)e).clientId, ((SpellStateEventData)e).spellIndex);
        switch(spellState) 
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
        Debug.Log($"PlayerAnimator OnSpellStateChanged: {spellState}");
    }

    private void Update()
    {
        // Update ���� EventHandler�� ����ؼ� �����غ��� 

        //Debug.Log($"is walking? {player.IsWalking()}");
        animator.SetBool(IS_WALKING, player.IsWalking());
        // �ִϸ��̼� ������ �ʿ��մϴ�. �ܰ� ����. 1.ĳ���� 2.�߻�
        //animator.SetBool(IS_ATTACK1, player.IsAttack1());
    }


}
